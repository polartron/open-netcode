using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_DynamicLightmapST"       , MaterialPropertyFormat.Float4)] public struct BuiltinMaterialPropertyUnity_DynamicLightmapST : IComponentData { public float4   Value; }
}
