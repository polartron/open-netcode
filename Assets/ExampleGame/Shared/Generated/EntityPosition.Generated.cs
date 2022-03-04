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

            writer.WriteRawBits(Convert.ToUInt32(!Test.Equals(baseSnapshot.Test)), 1);
            if(!Test.Equals(baseSnapshot.Test)) Test.Write(ref writer, compressionModel, baseSnapshot.Test);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in EntityPosition baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Value = baseSnapshot.Value;
            else
                Value.Read(ref reader, compressionModel, baseSnapshot.Value);

            if (reader.ReadRawBits(1) == 0)
                Test = baseSnapshot.Test;
            else
                Test.Read(ref reader, compressionModel, baseSnapshot.Test);

        }

        public bool Equals(EntityPosition other)
        {
            bool equals = true;
            //<equals>
            equals = equals && Value.Equals(other.Value);
            equals = equals && Test.Equals(other.Test);
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
                hash = hash * 23 + Test.GetHashCode();
                return hash;
            }
        }
    }
}
