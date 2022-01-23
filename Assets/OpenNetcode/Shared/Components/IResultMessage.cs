using Unity.Entities;

namespace OpenNetcode.Shared.Components
{
    public interface IResultMessage<T> : INetworkedComponent
    {
        public int Tick { get; }
        public bool HasInput { get; }
        public int ProcessedTimeMs { get; }
        public T Prediction { get; }
        
        public void Apply(in EntityManager entityManager, in Entity entity);
    }
}