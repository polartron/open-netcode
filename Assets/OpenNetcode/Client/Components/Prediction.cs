using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace OpenNetcode.Client.Components
{
    [InternalBufferCapacity(TimeConfig.TicksPerSecond)]
    public struct Prediction<T> : IBufferElementData
        where T : unmanaged, INetworkedComponent
    {
        public int Tick;
        public T Value;

        public bool Compare(T other)
        {
            return Value.Hash() == other.Hash();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}