using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct EntityPosition : ISnapshotComponent<EntityPosition>, IEquatable<EntityPosition>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in EntityPosition baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(!Value.Equals(baseSnapshot.Value)), 1);
            if(!Value.Equals(baseSnapshot.Value)) Value.Write(ref writer, compressionModel, baseSnapshot.Value);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in EntityPosition baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Value = baseSnapshot.Value;
            else
                Value.Read(ref reader, compressionModel, baseSnapshot.Value);

        }

        public bool Equals(EntityPosition other)
        {
            bool equals = true;
            //<equals>
            equals = equals && Value.Equals(other.Value);
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                //<hash>
                hash = hash * 23 + Value.GetHashCode();
                return hash;
            }
        }
    }
}
