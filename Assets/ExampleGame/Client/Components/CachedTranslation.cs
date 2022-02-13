using Unity.Entities;
using Unity.Mathematics;

namespace ExampleGame.Client.Components
{
    public struct CachedTranslation : IComponentData
    {
        public float3 Value;
    }
}
