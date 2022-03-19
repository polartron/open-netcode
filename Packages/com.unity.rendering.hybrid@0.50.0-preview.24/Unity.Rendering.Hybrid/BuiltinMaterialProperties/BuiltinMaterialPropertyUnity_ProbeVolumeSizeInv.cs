using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_ProbeVolumeSizeInv"      , MaterialPropertyFormat.Float4)] public struct BuiltinMaterialPropertyUnity_ProbeVolumeSizeInv : IComponentData { public float4   Value; }
}
