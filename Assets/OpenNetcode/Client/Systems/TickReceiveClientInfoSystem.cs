using OpenNetcode.Shared;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace OpenNetcode.Client.Systems
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    [DisableAutoCreation]
    public partial class TickReceiveClientInfoSystem : SystemBase
    {
        private IClientNetworkSystem _clientNetworkSystem;
        private NetworkCompressionModel _compressionModel;

        public TickReceiveClientInfoSystem(IClientNetworkSystem clientNetworkSystem)
        {
            _clientNetworkSystem = clientNetworkSystem;
        }
        
        protected override void OnCreate()
        {
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            foreach (var packet in _clientNetworkSystem.ReceivedPackets)
            {
                switch ((PacketType) packet.Key)
                {
                    case PacketType.ClientInfo:
                    {
                        var data = packet.Value.GetArray<byte>();
                        var reader = new DataStreamReader(data);
                        int entityId = ClientInfoMessage.Read(ref reader, _compressionModel);

                        Debug.Log($"Local Server Id = {entityId}");
                        
                        var clientData = GetSingleton<ClientData>();
                        clientData.LocalPlayerServerEntityId = entityId;
                        clientData.Version = 1;
                        SetSingleton(clientData);
                        
                        break;
                    }
                }
            }
        }
    }
}
