using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct MovementInput : ISnapshotComponent<MovementInput>, IEquatable<MovementInput>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in MovementInput baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(!Move.Equals(baseSnapshot.Move)), 1);
            if(!Move.Equals(baseSnapshot.Move)) Move.Write(ref writer, compressionModel, baseSnapshot.Move);

            writer.WriteRawBits(Convert.ToUInt32(!Rotation.Equals(baseSnapshot.Rotation)), 1);
            if(!Rotation.Equals(baseSnapshot.Rotation)) Rotation.Write(ref writer, compressionModel, baseSnapshot.Rotation);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in MovementInput baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Move = baseSnapshot.Move;
            else
                Move.Read(ref reader, compressionModel, baseSnapshot.Move);

            if (reader.ReadRawBits(1) == 0)
                Rotation = baseSnapshot.Rotation;
            else
                Rotation.Read(ref reader, compressionModel, baseSnapshot.Rotation);

        }

        public bool Equals(MovementInput other)
        {
            bool equals = true;
            //<equals>
            equals = equals && Move.Equals(other.Move);
            equals = equals && Rotation.Equals(other.Rotation);
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is MovementInput other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                //<hash>
                hash = hash * 23 + Move.GetHashCode();
                hash = hash * 23 + Rotation.GetHashCode();
                return hash;
            }
        }
    }
}
