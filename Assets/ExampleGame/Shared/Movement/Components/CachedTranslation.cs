using Unity.Entities;
using Unity.Mathematics;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    public struct CachedTranslation : IComponentData
    {
        public bool IsSet;
        public float3 Value;
    }
}
