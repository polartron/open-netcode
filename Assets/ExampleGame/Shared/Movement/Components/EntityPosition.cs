using OpenNetcode.Shared.Attributes;
using Shared.Coordinates;
using Unity.Entities;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    [PublicSnapshot]
    [Predict]
    public partial struct EntityPosition : IComponentData
    {
        public GameUnits Value;
        public int Test;
    }
}