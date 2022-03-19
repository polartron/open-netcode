using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_ProbeVolumeWorldToObject", MaterialPropertyFormat.Float4x4)] public struct BuiltinMaterialPropertyUnity_ProbeVolumeWorldToObject : IComponentData { public float4x4 Value; }
}
