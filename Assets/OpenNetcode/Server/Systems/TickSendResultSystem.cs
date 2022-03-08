using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using OpenNetcode.Shared.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

namespace OpenNetcode.Server.Systems
{
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(TickServerSendSystem))]
    [DisableAutoCreation]
    public unsafe class TickSendResultSystem : SystemBase
    {
        private IServerNetworkSystem _server;
        private NetworkCompressionModel _compressionModel;

        public TickSendResultSystem(IServerNetworkSystem server)
        {
            _server = server;
        }
        
        protected override void OnCreate()
        {
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            base.OnCreate();
        }
        
        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;

            if (tick % (TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond) != 0)
            {
                return;
            }
            
            NetworkCompressionModel compressionModel = _compressionModel;
            NativeMultiHashMap<int, PacketArrayWrapper> packets = _server.SendPackets;
            float timeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
            
            Entities.WithAll<PlayerControlledTag>().ForEach((in InputTimeData inputTimeData, in DynamicBuffer<ProcessedInput> processedInputs, in ServerNetworkedEntity networkEntity, in Entity entity) =>
            {
                DataStreamWriter writer = new DataStreamWriter(20, Allocator.Temp);
                Packets.WritePacketType(PacketType.Result, ref writer);
                writer.WritePackedUInt((uint) inputTimeData.Tick, compressionModel);
                writer.WritePackedUInt((uint) tick, compressionModel);
                writer.WritePackedFloat((float)(timeSinceStartup - inputTimeData.ArrivedTime), compressionModel);
                packets.Add(networkEntity.OwnerNetworkId, Packets.WrapPacket(writer));
            }).Run();
        }
    }
}
