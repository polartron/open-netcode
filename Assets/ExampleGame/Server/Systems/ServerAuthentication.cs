using OpenNetcode.Server.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
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

        public ServerGuestAuthentication(IServerNetworkSystem server)
        {
            _server = server;
        }
        
        protected override void OnStartRunning()
        {
            _serverEntitySystem = World.GetExistingSystem<ServerEntitySystem>();
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
                    NativeArray<byte> array = wrapper.GetArray<byte>();
                    DataStreamReader reader = new DataStreamReader(array);
                    LoginMessage.Read(ref loginMessages, ref reader, wrapper.InternalId);

                } while (receivedMessages.TryGetNextValue(out wrapper, ref iterator));
            }

            foreach (var login in loginMessages)
            {
                DataStreamWriter writer = new DataStreamWriter(50, Allocator.Temp);
                
                if (login.Value.Guest)
                {
                    Debug.Log("Guest logging in");
                    Entity entity = _serverEntitySystem.SpawnPlayer(new Vector3(login.Key, 0, login.Key) + new Vector3(1, 0, 1), login.Key);
                    ClientInfoMessage.Write(entity.Index, ref writer, _compressionModel);
                    _server.Send(login.Key, Packets.WrapPacket(writer));
                }
                else
                {
                    //Implement authentication
                }
            }
        }
    }
}