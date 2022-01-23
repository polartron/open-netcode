using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Components
{
    public interface ISnapshotComponent<T> where T : unmanaged
    {
        void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel,
            in T baseSnapshot);
        
        void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel,
            in T baseSnapshot);
    }
}