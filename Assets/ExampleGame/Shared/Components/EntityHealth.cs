using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Networking.Transport;

namespace Shared.Components
{
    [GenerateAuthoringComponent]
    [PrivateSnapshot]
    public struct EntityHealth : ISnapshotComponent<EntityHealth>, IComponentData
    {
        public int Value;

        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in EntityHealth baseSnapshot)
        {
            if (Value == baseSnapshot.Value)
            {
                writer.WriteRawBits(0, 1);
            }
            else
            {
                writer.WriteRawBits(1, 1);
                int delta = Value - baseSnapshot.Value;
                writer.WritePackedInt(delta, compressionModel);
            }
        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel,
            in EntityHealth baseSnapshot)
        {
            if (reader.ReadRawBits(1) == 0)
            {
                Value = baseSnapshot.Value;
            }
            else
            {
                Value = reader.ReadPackedInt(compressionModel);
                Value += baseSnapshot.Value;
            }
        }
    }
}