using OpenNetcode.Server.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using Server.Generated;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Server.Systems
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickServerReceiveSystem))]
    [DisableAutoCreation]
    public partial class ServerGuestAuthentication : SystemBase
    {
        private readonly IServerNetworkSystem _server;
        private ServerEntitySystem _serverEntitySystem;
        private NetworkCompressionModel _compressionModel;
        private TickServerPrefabSystem _tickServerPrefabSystem;

        public ServerGuestAuthentication(IServerNetworkSystem server)
        {
            _server = server;
        }
        
        protected override void OnStartRunning()
        {
            _serverEntitySystem = World.GetExistingSystem<ServerEntitySystem>();
            _tickServerPrefabSystem = World.GetExistingSystem<TickServerPrefabSystem>();
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        {
            var receivedMessages = _server.ReceivePackets;
            NativeHashMap<int, LoginMessage> loginMessages = new NativeHashMap<int, LoginMessage>(100, Allocator.Temp);

            if(receivedMessages.TryGetFirstValue((int) PacketType.Login, out var wrapper, out var iterator))
            {
                do
                {
                    DataStreamReader reader = wrapper.Reader;
                    LoginMessage.Read(ref loginMessages, ref reader, wrapper.Connection.InternalId);

                } while (receivedMessages.TryGetNextValue(out wrapper, ref iterator));
            }

            foreach (var login in loginMessages)
            {
                DataStreamWriter writer = new DataStreamWriter(1000, Allocator.Temp);
                
                if (login.Value.Guest)
                {
                    Debug.Log("Guest logging in");

                    Entity entity = _tickServerPrefabSystem.SpawnPrefab("Player");
                    ServerInitialization.InitializePlayerEntity(EntityManager, entity, login.Key);
                    ClientInfoMessage.Write(entity.Index, ref writer, _compressionModel);
                    _server.Send(login.Key, Packets.WrapPacket(writer));
                    _tickServerPrefabSystem.SendNetworkedPrefabs(login.Key);
                }
                else
                {
                    //Implement authentication
                }
            }
        }
    }
}