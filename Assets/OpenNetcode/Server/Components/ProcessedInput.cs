using Unity.Entities;

namespace OpenNetcode.Server.Components
{
    [InternalBufferCapacity(10)]
    public struct ProcessedInput : IBufferElementData
    {
        public bool HasInput;
        public double ArrivedTime;
    }
}
