using OpenNetcode.Shared.Attributes;
using Unity.Entities;

namespace ExampleGame.Shared.Components
{
    [GenerateAuthoringComponent]
    public partial struct WeaponInput : IComponentData
    {
        public int WeaponType;
    }
}
