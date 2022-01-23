using Unity.Entities;
using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Components
{
    public interface INetworkedComponent : IComponentData
    {
        public void Write(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel);
        public void Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel);
        public int Hash();
    }
}