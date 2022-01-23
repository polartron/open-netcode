using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Shared.Coordinates;
using Unity.Entities;
using Unity.Networking.Transport;

namespace OpenNetcode.Movement.Components
{
    [GenerateAuthoringComponent]
    [PublicSnapshot]
    public struct EntityVelocity : ISnapshotComponent<EntityVelocity>, INetworkedComponent
    {
        public GameUnits Value;

        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in EntityVelocity baseSnapshot)
        {
            if (Value == baseSnapshot.Value)
            {
                writer.WriteRawBits(0, 1);
            }
            else
            {
                writer.WriteRawBits(1, 1);
                GameUnits delta = Value - baseSnapshot.Value;
                delta.Write(ref writer, compressionModel);
            }
        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel,
            in EntityVelocity baseSnapshot)
        {
            if (reader.ReadRawBits(1) == 0)
            {
                Value = baseSnapshot.Value;
            }
            else
            {
                Value.Read(ref reader, compressionModel);
                Value += baseSnapshot.Value;
            }
        }

        public void Write(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            Value.Write(ref writer, compressionModel);
        }

        public void Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel)
        {
            Value.Read(ref reader, compressionModel);
        }

        public int Hash()
        {
            return Value.Hash();
        }
    }
}
