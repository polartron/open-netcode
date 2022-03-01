using OpenNetcode.Shared.Attributes;
using Unity.Entities;

namespace ExampleGame.Shared.Components
{
    [GenerateAuthoringComponent]
    [NetworkedInput]
    public partial struct WeaponInput : IComponentData
    {
        public int WeaponType;
    }
}
