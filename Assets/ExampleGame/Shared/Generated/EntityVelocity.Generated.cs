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
            writer.WriteRawBits(Convert.ToUInt32(!Linear.Equals(baseSnapshot.Linear)), 1);
            if(!Linear.Equals(baseSnapshot.Linear)) Linear.Write(ref writer, compressionModel, baseSnapshot.Linear);

            writer.WriteRawBits(Convert.ToUInt32(!Angular.Equals(baseSnapshot.Angular)), 1);
            if(!Angular.Equals(baseSnapshot.Angular)) Angular.Write(ref writer, compressionModel, baseSnapshot.Angular);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in EntityVelocity baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Linear = baseSnapshot.Linear;
            else
                Linear.Read(ref reader, compressionModel, baseSnapshot.Linear);

            if (reader.ReadRawBits(1) == 0)
                Angular = baseSnapshot.Angular;
            else
                Angular.Read(ref reader, compressionModel, baseSnapshot.Angular);

        }

        public bool Equals(EntityVelocity other)
        {
            bool equals = true;
            //<equals>
            equals = equals && Linear.Equals(other.Linear);
            equals = equals && Angular.Equals(other.Angular);
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
                hash = hash * 23 + Linear.GetHashCode();
                hash = hash * 23 + Angular.GetHashCode();
                return hash;
            }
        }
    }
}
