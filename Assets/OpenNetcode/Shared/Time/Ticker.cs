using System;
using System.Collections.Generic;
using Shared.Time;
using UnityEngine;

namespace OpenNetcode.Shared.Time
{
    public interface ITickable
    {
        void OnTick(float deltaTime, int tick);
    }

    public struct TickerData
    {
        public int LastTick;
        public double TimeUpdated;
        public double Offset;
        public double TargetTime;

        public float DilationFrom;
        public float DilationTo;
    }

    public struct TickerConfig
    {
        public int TicksPerSecond;
        public double SmoothTimeMs;

        public float DilationAmountMs;
        public float DilationLengthMs;
        public AnimationCurve DilationCurve;

        public static TickerConfig Default = new TickerConfig()
        {
            DilationCurve = new AnimationCurve(new[]
            {
                new Keyframe(0, 0), new Keyframe(0.25f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f),
            }),
            DilationAmountMs = TimeConfig.CommandBufferLengthMs,
            DilationLengthMs = 5000,
            TicksPerSecond = 20,
            SmoothTimeMs = 500
        };
    }

    public class Ticker
    {
        private TickerData _tickerData;
        private readonly TickerConfig _tickerConfig;

        private List<ITickable> _tickables = new List<ITickable>();
        public float TickFloat => (float) GetTickFloat(_tickerData, _tickerConfig);

        public double Time
        {
            get
            {
                double timeInMs = TimeUtils.CurrentTimeInMs();
                double elapsed = timeInMs - _tickerData.TimeUpdated;
                double current = BaseTime(_tickerData, _tickerConfig, timeInMs) + elapsed;
                return current;
            }
        }
        
        public Ticker(int ticksPerSecond, long timeInMs, long smoothTimeMs = 0)
        {
            _tickerConfig = TickerConfig.Default;
            _tickerConfig.TicksPerSecond = ticksPerSecond;
            _tickerConfig.SmoothTimeMs = smoothTimeMs;

            _tickerData = new TickerData()
            {
                TargetTime = timeInMs, 
                Offset = 0, 
                TimeUpdated = TimeUtils.CurrentTimeInMs(), 
                DilationTo = 1
            };
            
        }

        public void Reset()
        {
            _tickerData = new TickerData()
            {
                TargetTime = TimeUtils.CurrentTimeInMs(), 
                Offset = 0, 
                TimeUpdated = TimeUtils.CurrentTimeInMs(), 
                DilationTo = 1
            };
            
        }

        public void Add(ITickable tickable)
        {
            _tickables.Add(tickable);
        }

        public void Remove(ITickable tickable)
        {
            _tickables.Remove(tickable);
        }

        public void Update()
        {
            double tickFloat = GetTickFloat(_tickerData, _tickerConfig);
            
            int ticksToSimulate = Mathf.Clamp((int) tickFloat - _tickerData.LastTick, 0, 10);
            int nextTick = _tickerData.LastTick + 1;

            for (int i = 0; i < ticksToSimulate; i++)
            {
                foreach (var fixedUpdate in _tickables)
                {
                    fixedUpdate.OnTick(1f / TimeConfig.TicksPerSecond, _tickerData.LastTick + 1 + i);
                }
            }

            _tickerData.LastTick = (int) tickFloat;
        }

        public void SetTime(double time)
        {
            SetTime(ref _tickerData, _tickerConfig, time);
        }

        public static void SetTime(ref TickerData data, in TickerConfig config, double time)
        {
            double timeInMs = TimeUtils.CurrentTimeInMs();
            double elapsed = (timeInMs - data.TimeUpdated);
            double current = BaseTime(data, config, timeInMs) + elapsed;

            data.Offset = time - current;
            data.TimeUpdated = timeInMs;
            data.TargetTime = time;
        }

        public static double GetTickFloat(in TickerData data, in TickerConfig config)
        {
            double timeInMs = TimeUtils.CurrentTimeInMs();
            double elapsed = timeInMs - data.TimeUpdated;
            double current = BaseTime(data, config, timeInMs) + elapsed;
            double seconds = current / 1000;

            double tick = seconds * config.TicksPerSecond;
            double dilation = GetDilationAmount(data, config, tick);

            return tick + dilation;
        }

        public static double GetDilationAmount(in TickerData data, in TickerConfig config, double tick)
        {
            double maxTick = config.DilationAmountMs / 1000f * config.TicksPerSecond;
            double time = tick / TimeConfig.TicksPerSecond * 1000f;
            float il = Mathf.InverseLerp(data.DilationFrom, data.DilationTo, (float) time);

            float value = config.DilationCurve.Evaluate(il);
            return value * maxTick;
        }

        private static double BaseTime(in TickerData data, in TickerConfig config, double timeInMs)
        {
            if (timeInMs < data.TimeUpdated + config.SmoothTimeMs)
            {
                double a = data.TimeUpdated;
                double b = data.TimeUpdated + config.SmoothTimeMs;
                double c = timeInMs;

                double inverseLerp = 1.0 - (Math.Abs(a - b) > double.Epsilon
                                         ? Mathf.Clamp01((float) ((c - a) / (b - a)))
                                         : 0.0f);

                return data.TargetTime + data.Offset * inverseLerp;
            }

            return data.TargetTime;
        }
    }
}