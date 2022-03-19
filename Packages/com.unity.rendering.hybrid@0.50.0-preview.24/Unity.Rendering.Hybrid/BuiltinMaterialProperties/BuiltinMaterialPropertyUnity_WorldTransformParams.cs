using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_WorldTransformParams", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_WorldTransformParams : IComponentData
    {
        public float4 Value;
    }

    [MaterialProperty("unity_WorldTransformParams", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_WorldTransformParams_Shared : IHybridSharedComponentFloat4Override
    {
        public float4 Value;

        public float4 GetFloat4OverrideData()
        {
            return Value;
        }
    }
}
