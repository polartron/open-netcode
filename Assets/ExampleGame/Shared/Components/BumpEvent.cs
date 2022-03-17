using OpenNetcode.Shared.Attributes;
using Shared.Generated;
using Unity.Entities;

namespace ExampleGame.Shared.Components
{
    [InternalBufferCapacity(SnapshotSettings.MaxEventsBufferLength)]
    [PublicEvent]
    public partial struct BumpEvent : IBufferElementData
    {
        public ushort SoundIndex;
    }
}
