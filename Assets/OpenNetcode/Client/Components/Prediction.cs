using System;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace OpenNetcode.Client.Components
{
    [InternalBufferCapacity(TimeConfig.TicksPerSecond)]
    public struct Prediction<T> : IBufferElementData
        where T : unmanaged, IEquatable<T>
    {
        public int Tick;
        public T Value;
    }
}