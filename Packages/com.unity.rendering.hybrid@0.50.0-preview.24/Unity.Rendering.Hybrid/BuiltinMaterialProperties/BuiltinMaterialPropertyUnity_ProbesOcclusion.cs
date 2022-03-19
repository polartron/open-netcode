using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    // This type is registered as a material property override manually.
    public struct BuiltinMaterialPropertyUnity_ProbesOcclusion : IComponentData { public float4   Value; }
}
