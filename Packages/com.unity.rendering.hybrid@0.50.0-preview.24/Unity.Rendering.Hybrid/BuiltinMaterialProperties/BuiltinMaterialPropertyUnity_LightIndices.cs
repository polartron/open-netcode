using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_LightIndices", MaterialPropertyFormat.Float2x4)]
    public struct BuiltinMaterialPropertyUnity_LightIndices : IComponentData
    {
        public float4 Value0;
        public float4 Value1;
    }
}
