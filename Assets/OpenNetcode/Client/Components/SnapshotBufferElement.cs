using OpenNetcode.Shared.Components;
using Unity.Entities;

namespace OpenNetcode.Client.Components
{
    [InternalBufferCapacity(32)]
    public struct SnapshotBufferElement<T> : IBufferElementData where T : unmanaged
    {
        public int Tick;
        public T Value;
    }
}