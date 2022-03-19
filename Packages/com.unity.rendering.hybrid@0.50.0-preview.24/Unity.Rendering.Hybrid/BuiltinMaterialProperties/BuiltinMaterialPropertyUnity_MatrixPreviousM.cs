using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    // Previous matrix components always exist, to prevent compilation errors,
    // but they become material properties only when V2 is enabled
#if SRP_10_0_0_OR_NEWER
    [MaterialProperty("unity_MatrixPreviousM", MaterialPropertyFormat.Float4x4, 4 * 4 * 3)]
#else
    [MaterialProperty("unity_MatrixPreviousM", MaterialPropertyFormat.Float4x4)]
#endif
    public struct BuiltinMaterialPropertyUnity_MatrixPreviousM : IComponentData
    {
        public float4x4 Value;
    }
}
