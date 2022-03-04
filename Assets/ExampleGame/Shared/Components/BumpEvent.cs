using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Networking.Transport;

namespace ExampleGame.Shared.Components
{
    [GenerateAuthoringComponent]
    [InternalBufferCapacity(4)]
    [PublicEvent]
    public partial struct BumpEvent : IBufferElementData
    {
        public ushort SoundIndex;
    }
}
