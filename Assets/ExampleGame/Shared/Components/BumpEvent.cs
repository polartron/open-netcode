using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Networking.Transport;

namespace ExampleGame.Shared.Components
{
    [InternalBufferCapacity(4)]
    [PublicEvent]
    public partial struct BumpEvent : IBufferElementData
    {
    }
}
