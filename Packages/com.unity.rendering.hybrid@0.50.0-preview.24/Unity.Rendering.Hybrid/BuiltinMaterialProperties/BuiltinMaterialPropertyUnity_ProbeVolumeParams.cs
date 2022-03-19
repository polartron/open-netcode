using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_ProbeVolumeParams"       , MaterialPropertyFormat.Float4)] public struct BuiltinMaterialPropertyUnity_ProbeVolumeParams : IComponentData { public float4   Value; }
}
