using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Networking.Transport;

namespace ExampleGame.Shared.Components
{
    [InternalBufferCapacity(4)]
    [PublicEvent]
    public struct BumpEvent : ISnapshotComponent<BumpEvent>, IBufferElementData
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            
        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            
        }
    }
}
