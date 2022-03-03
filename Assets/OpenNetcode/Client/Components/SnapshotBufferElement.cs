using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace OpenNetcode.Client.Components
{
    [InternalBufferCapacity(TimeConfig.SnapshotsPerSecond)]
    public struct SnapshotBufferElement<T> : IBufferElementData where T : unmanaged
    {
        public int Tick;
        public T Value;
    }
}