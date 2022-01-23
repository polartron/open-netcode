using Unity.Entities;
using Unity.Mathematics;

namespace Client.Components
{
    public struct CachedTranslation : IComponentData
    {
        public float3 Value;
    }
}
