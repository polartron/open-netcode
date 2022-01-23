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
        private IClientNetworkSystem _client;
        private TickSystem _tickSystem;
        private NativeHashMap<int, float> _sentInputs;
        private float _dilationMs;
        private NetworkCompressionModel _compressionModel;

        public TickReceiveResultSystem(IClientNetworkSystem client)
        {
            _client = client;
        }

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            _sentInputs = new NativeHashMap<int, float>(TimeConfig.TicksPerSecond, Allocator.Persistent);
            
           // _profilerRoundTripTime = ProfilerRecorder.StartNew(ProfilerCategory.Network, "Round Trip Time");
        }

        protected override void OnDestroy()
        {
            _sentInputs.Dispose();
            //_profilerRoundTripTime.Dispose();
        }

        public void AddSentInput(int tick, float sentTime)
        {
            _sentInputs[tick % _sentInputs.Capacity] = sentTime;
        }

        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;
            
            foreach (var packet in _client.ReceivedPackets)
            {
                switch ((PacketType) packet.Key)
                {
                    case PacketType.Result:
                        var data = packet.Value.GetArray<byte>();
                        DataStreamReader reader = new DataStreamReader(data);
                        Packets.ReadPacketType(ref reader);
                        int resultTick = (int) reader.ReadPackedUInt(_compressionModel);
                        int processedInputs = (int) reader.ReadRawBits(3);

                        int lastValidInput = 0;
                        int missedInputs = 0;
                        for (int i = 0; i < processedInputs; i++)
                        {
                            if (reader.ReadRawBits(1) == 1)
                            {
                                lastValidInput = i;
                            }
                            else
                            {
                                missedInputs++;
                            }
                        }

                        if (reader.ReadRawBits(1) == 1)
                        {
                            int offset = (processedInputs - (lastValidInput + 1));
                            int processedTick = resultTick - offset;
                            int processedTime = (int) reader.ReadPackedUInt(_compressionModel);
                            
                            if (_sentInputs.TryGetValue(processedTick % _sentInputs.Capacity, out float sentTime))
                            {
                                //Calculate round trip time
                                double rttMs = (Time.ElapsedTime - sentTime) * 1000f;
                                rttMs -= processedTime;
                                
                                float rttHalf = (float) rttMs / 2;
                                
                                SetSingleton(new RoundTripTime()
                                {
                                    Value = (float) rttMs
                                });

                                double serverTimeMs = ((float) processedTick / TimeConfig.TicksPerSecond) * 1000f;

                                //No sudden time changes. Slowly move towards predicted time. 

                                double tickerTime = _tickSystem.TickerTime;
                
                                if (tickerTime < serverTimeMs || tickerTime > serverTimeMs + 1000)
                                {
                                    _tickSystem.SetTime(serverTimeMs + 1000);
                                }
                                else
                                {
                                    if (missedInputs > 0)
                                    {
                                        //We missed input. Dilate hard.
                                        _dilationMs += (TimeConfig.FixedDeltaTime * 100f);
                                        _dilationMs = Mathf.Clamp(_dilationMs, 0, TimeConfig.MaxDilationMs);
                                    }
                                    else
                                    {
                                        //We got input. Slowly remove dilation.
                                        _dilationMs -= (TimeConfig.FixedDeltaTime * 2f);
                                        _dilationMs = Mathf.Clamp(_dilationMs, 0, TimeConfig.MaxDilationMs);
                                    }

                                    double predictedTimeMs = serverTimeMs + rttHalf + TimeConfig.CommandBufferLengthMs + _dilationMs;
                                    float lerpedTime = Mathf.Lerp((float) tickerTime, (float) predictedTimeMs, TimeConfig.FixedDeltaTime);
                                    _tickSystem.SetTime(lerpedTime);
                                }
                            }
                        }

                        break;
                }
            }
        }
    }
}
