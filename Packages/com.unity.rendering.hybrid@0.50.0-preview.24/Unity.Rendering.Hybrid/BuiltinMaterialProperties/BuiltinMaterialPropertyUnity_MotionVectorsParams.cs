using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("unity_MotionVectorsParams", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_MotionVectorsParams : IComponentData
    {
        public float4 Value;
    }

    [MaterialProperty("unity_MotionVectorsParams", MaterialPropertyFormat.Float4)]
    public struct BuiltinMaterialPropertyUnity_MotionVectorsParams_Shared : IHybridSharedComponentFloat4Override
    {
        public float4 Value;

        public float4 GetFloat4OverrideData()
        {
            return Value;
        }
    }
}
