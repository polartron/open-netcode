using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace OpenNetcode.Client.Components
{
    [InternalBufferCapacity(TimeConfig.TicksPerSecond)]
    public struct SavedInput<T> : IBufferElementData
        where T : unmanaged, INetworkedComponent
    {
        public T Value;
    }
}