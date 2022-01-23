using Unity.Entities;

namespace OpenNetcode.Shared.Components
{
    [InternalBufferCapacity(10)]
    public struct ProcessedInput : IBufferElementData
    {
        public bool HasInput;
        public double ArrivedTime;
    }
}
