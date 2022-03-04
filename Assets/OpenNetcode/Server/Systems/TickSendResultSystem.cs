using System;
using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
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
            var tick = GetSingleton<TickData>();

            if (tick.Value % (TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond) != 0)
            {
                return;
            }
            
            NetworkCompressionModel compressionModel = _compressionModel;
            NativeMultiHashMap<int, PacketArrayWrapper> packets = _server.SendPackets;
            double elapsedTime = Time.ElapsedTime;
            
            Entities.WithAll<PlayerControlledTag>().ForEach((ref DynamicBuffer<ProcessedInput> processedInputs, in ServerNetworkedEntity networkEntity, in Entity entity) =>
            {
                DataStreamWriter writer = new DataStreamWriter(20, Allocator.Temp);

                Packets.WritePacketType(PacketType.Result, ref writer);
                writer.WritePackedUInt((uint) tick.Value, compressionModel);
                writer.WritePackedUInt((uint) processedInputs.Length, compressionModel);

                ProcessedInput lastValidInput = default;
                
                foreach (ProcessedInput input in processedInputs)
                {
                    writer.WriteRawBits(Convert.ToUInt32(input.HasInput), 1);
                    if (input.HasInput)
                    {
                        lastValidInput = input;
                    }
                }

                if (lastValidInput.HasInput)
                {
                    writer.WriteRawBits(1, 1);
                    uint processedTime = (uint) ((elapsedTime - lastValidInput.ArrivedTime) * 1000f);
                    writer.WritePackedUInt(processedTime, compressionModel);
                }
                else
                {
                    writer.WriteRawBits(0, 1);
                }

                packets.Add(networkEntity.OwnerNetworkId, Packets.WrapPacket(writer));
                
                processedInputs.Clear();
            }).Run();
        }
    }
}
