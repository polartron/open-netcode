using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct EntityVelocity : ISnapshotComponent<EntityVelocity>, IEquatable<EntityVelocity>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in EntityVelocity baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(!Value.Equals(baseSnapshot.Value)), 1);
            if(!Value.Equals(baseSnapshot.Value)) Value.Write(ref writer, compressionModel, baseSnapshot.Value);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in EntityVelocity baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Value = baseSnapshot.Value;
            else
                Value.Read(ref reader, compressionModel, baseSnapshot.Value);

        }

        public bool Equals(EntityVelocity other)
        {
            bool equals = true;
            //<equals>
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityVelocity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                //<hash>
                return hash;
            }
        }
    }
}
