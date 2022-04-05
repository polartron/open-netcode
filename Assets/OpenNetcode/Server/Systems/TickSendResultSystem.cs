using System;
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
    public partial class TickSendResultSystem : SystemBase
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

            if (tick % (TimeConfig.TicksPerSecond / 2) != 0)
            {
                return;
            }
            
            NetworkCompressionModel compressionModel = _compressionModel;
            NativeMultiHashMap<int, PacketArrayWrapper> packets = _server.SendPackets;
            float elapsedTime = (float) Time.ElapsedTime;
            
            Entities.WithAll<PlayerControlledTag>().ForEach((in InputTimeData inputTimeData, in ServerNetworkedEntity networkEntity, in Entity entity) =>
            {
                bool loss = inputTimeData.ProcessedTick != tick;
                

                DataStreamWriter writer = new DataStreamWriter(20, Allocator.Temp);
                Packets.WritePacketType(PacketType.Result, ref writer);
                writer.WritePackedUInt((uint) inputTimeData.LatestReceivedTick, compressionModel);
                writer.WritePackedFloat(elapsedTime, compressionModel);
                writer.WritePackedFloat((float)(elapsedTime - inputTimeData.ArrivedTime), compressionModel);
                writer.WriteRawBits(Convert.ToUInt32(loss), 1);
                packets.Add(networkEntity.OwnerNetworkId, Packets.WrapPacket(writer));
            }).Run();
        }
    }
}
