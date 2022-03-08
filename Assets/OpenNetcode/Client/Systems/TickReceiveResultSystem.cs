using System;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.Profiling;
using UnityEngine;

namespace OpenNetcode.Client.Systems
{
    public interface ITickReceiveResultSystem
    {
    }
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    public class TickReceiveResultSystem : SystemBase, ITickReceiveResultSystem
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

        private NativeArray<float> _timeOffsets;
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
            _timeOffsets = new NativeArray<float>(TimeConfig.TicksPerSecond, Allocator.Persistent);
            
           // _profilerRoundTripTime = ProfilerRecorder.StartNew(ProfilerCategory.Network, "Round Trip Time");
        }

        protected override void OnDestroy()
        {
            _sentInputs.Dispose();
            //_profilerRoundTripTime.Dispose();
        }

        public void AddSentInput(int tick, float sentTime)
        {
            _sentInputs[tick % _sentInputs.Capacity] = new SentTime()
            {
                Tick = tick,
                Time = sentTime
            };
        }

        private float GetAverageTimeOffset()
        {
            float offset = 0f;

            for (int i = 0; i < _timeOffsets.Length; i++)
            {
                offset += _timeOffsets[i];
            }

            offset /= _timeOffsets.Length;

            return offset;
        }

        protected override void OnUpdate()
        {
            foreach (var packet in _client.ReceivedPackets)
            {
                switch ((PacketType) packet.Key)
                {
                    case PacketType.Result:
                        var data = packet.Value.GetArray<byte>();
                        DataStreamReader reader = new DataStreamReader(data);
                        Packets.ReadPacketType(ref reader);
                        
                        int resultTick = (int) reader.ReadPackedUInt(_compressionModel);
                        int serverTick = (int) reader.ReadPackedUInt(_compressionModel);
                        float processedTime = reader.ReadPackedFloat(_compressionModel);
                        
                        double serverTimeMs = ((float) serverTick / TimeConfig.TicksPerSecond) * 1000f;
                        
                        if (_sentInputs.TryGetValue(resultTick % _sentInputs.Capacity, out SentTime sentTime))
                        {
                            if (sentTime.Tick != resultTick)
                            {
                                break;
                            }
                            
                            //Calculate round trip time
                            double rttMs = (Time.ElapsedTime - sentTime.Time) * 1000f;
                            rttMs -= (processedTime * 1000f);
                            float rttHalf = (float) rttMs / 2;
                            
                            SetSingleton(new RoundTripTime()
                            {
                                Value = (float) rttMs
                            });

                            //No sudden time changes. Slowly move towards predicted time. 
                            double tickerTime = _tickSystem.TickerTime;
            
                            if (tickerTime < serverTimeMs || tickerTime > serverTimeMs + 1000)
                            {
                                _tickSystem.SetTime(serverTimeMs + rttHalf + TimeConfig.CommandBufferLengthMs);
                            }
                            else
                            {
                                double predictedTimeMs = serverTimeMs + rttHalf + TimeConfig.CommandBufferLengthMs + _dilationMs;
                                float lerpedTime = Mathf.Lerp((float) tickerTime, (float) predictedTimeMs, TimeConfig.FixedDeltaTime);
                                _tickSystem.SetTime(lerpedTime);
                            }
                        }

                        break;
                }
            }
        }
    }
}
