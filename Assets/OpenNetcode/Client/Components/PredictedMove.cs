using OpenNetcode.Shared.Components;
using Unity.Entities;

namespace OpenNetcode.Client.Components
{
    [InternalBufferCapacity(32)]
    public struct PredictedMove<TPrediction, TInput> : IBufferElementData, IPredictedMove<TPrediction>
        where TPrediction : unmanaged, IComponentData, INetworkedComponent
        where TInput : unmanaged, INetworkedComponent
    {
        public int Tick;
        public TPrediction Prediction;
        public TInput Input;

        public bool Compare(TPrediction other)
        {
            return Prediction.Hash() == other.Hash();
        }

        public override string ToString()
        {
            return Prediction.ToString();
        }
    }

    public interface IPredictedMove<T>
    {
        bool Compare(T other);
    }
}