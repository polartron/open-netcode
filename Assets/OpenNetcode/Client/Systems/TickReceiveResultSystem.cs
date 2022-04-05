using System;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace OpenNetcode.Client.Systems
{
    public interface ITickReceiveResultSystem
    {
    }
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    public partial class TickReceiveResultSystem : SystemBase, ITickReceiveResultSystem
    {
        private struct SentTime
        {
            public int Tick;
            public float Time;
        }
        
        private IClientNetworkSystem _client;
        private TickSystem _tickSystem;
        private NativeHashMap<int, SentTime> _sentInputs;
        private float _dilationMs;
        private NetworkCompressionModel _compressionModel;

        private NativeList<double> _timeOffsets;
        private bool _timeSet;
        private int _timeOffsetIndex = 0;

        public TickReceiveResultSystem(IClientNetworkSystem client)
        {
            _client = client;
        }

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            _sentInputs = new NativeHashMap<int, SentTime>(TimeConfig.TicksPerSecond, Allocator.Persistent);
            _timeOffsets = new NativeList<double>(10, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _sentInputs.Dispose();
            _timeOffsets.Dispose();
        }

        public void AddSentInput(int tick, float sentTime)
        {
            _sentInputs[tick % _sentInputs.Capacity] = new SentTime()
            {
                Tick = tick,
                Time = sentTime
            };
        }

        protected override void OnUpdate()
        {
            foreach (var packet in _client.ReceivedPackets)
            {
                switch ((PacketType) packet.Key)
                {
                    case PacketType.Result:
                        DataStreamReader reader = packet.Value.Reader;
                        Packets.ReadPacketType(ref reader);
                        
                        int resultTick = (int) reader.ReadPackedUInt(_compressionModel);
                        double serverTime = reader.ReadPackedFloat(_compressionModel);
                        float processedTime = reader.ReadPackedFloat(_compressionModel);
                        bool hasLostInput = Convert.ToBoolean(reader.ReadRawBits(1));

                        if (_sentInputs.TryGetValue(resultTick % _sentInputs.Capacity, out SentTime sentTime))
                        {
                            if (sentTime.Tick != resultTick)
                            {
                                break;
                            }
                            
                            double rttMs = (Time.ElapsedTime - sentTime.Time) * 1000f;
                            rttMs -= (processedTime * 1000f);
                            float rttHalf = (float) rttMs / 2;
                            _tickSystem.SetRttHalf(rttHalf);

                            if (hasLostInput)
                            {
                                Debug.LogWarning("The server didn't receive input in time. Possible spike in latency.");
                            }
                            
                            if (!_timeSet || hasLostInput)
                            {
                                double time = serverTime * 1000f;
                                time += rttHalf;
                                time += TimeConfig.CommandBufferLengthMs * 2;
                                _tickSystem.SetTime(time);

                                _timeSet = true;
                            }
                        }

                        break;
                }
            }
        }
    }
}
