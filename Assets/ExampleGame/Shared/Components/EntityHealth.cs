using OpenNetcode.Shared.Attributes;
using Unity.Entities;

namespace ExampleGame.Shared.Components
{
    [GenerateAuthoringComponent]
    [PrivateSnapshot]
    public partial struct EntityHealth : IComponentData
    {
        public int Value;
    }
}