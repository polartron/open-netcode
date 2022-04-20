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
        
        public float ProcessedTime { get; private set; }
        public float ReceivedServerTick { get; private set; }

        public TickReceiveResultSystem(IClientNetworkSystem client)
        {
            _client = client;
        }

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            _sentInputs = new NativeHashMap<int, SentTime>(TimeConfig.TicksPerSecond * 10, Allocator.Persistent);
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
                        int serverTick = (int) reader.ReadPackedUInt(_compressionModel);
                        float processedTime = reader.ReadPackedFloat(_compressionModel);
                        bool hasLostInput = Convert.ToBoolean(reader.ReadRawBits(1));

                        ProcessedTime = processedTime * 1000f;
                        ReceivedServerTick = serverTick;

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
                                double time = ((float) serverTick / TimeConfig.TicksPerSecond) * 1000f;
                                time += TimeConfig.CommandBufferLengthMs;
                                _tickSystem.SetTime(time);
                                _timeSet = true;
                            }
                            else
                            {
                                double clientTime = ((float) serverTick / TimeConfig.TicksPerSecond) * 1000f;
                                clientTime += TimeConfig.CommandBufferLengthMs;
                                double offset = clientTime - _tickSystem.TickerTime;
                                _tickSystem.AdjustTime(offset);
                            }
                        }
                        else
                        {
                            if (!_timeSet || hasLostInput)
                            {
                                double time = ((float) serverTick / TimeConfig.TicksPerSecond) * 1000f;
                                time += TimeConfig.CommandBufferLengthMs;
                                _tickSystem.SetTime(time);
                                _tickSystem.SetRttHalf(500);
                            }
                        }

                        break;
                }
            }
        }
    }
}
