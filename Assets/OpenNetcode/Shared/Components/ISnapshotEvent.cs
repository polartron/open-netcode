using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Components
{
    public interface ISnapshotEvent<T> where T : unmanaged
    {
        void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel);
        void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel);
    }
}
