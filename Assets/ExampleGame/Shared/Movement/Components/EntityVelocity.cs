using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Collections;
using Unity.Entities;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    [PublicSnapshot]
    [Predict]
    public partial struct EntityVelocity : IComponentData
    {
        public GameUnits Linear;
        public GameUnits Angular;
    }
}