using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Entities;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    [PublicSnapshot]
    public partial struct PathComponent : IComponentData
    {
        public GameUnits From;
        public GameUnits To;
        public int Start;
        public int Stop;
    }
}