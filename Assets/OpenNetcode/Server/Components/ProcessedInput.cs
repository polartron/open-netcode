using Unity.Entities;

namespace OpenNetcode.Server.Components
{
    public struct ProcessedInput : IComponentData
    {
        public bool HasInput;
        public double ArrivedTime;
    }
}
