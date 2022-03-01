using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Components
{
    public partial struct WeaponInput : ISnapshotComponent<WeaponInput>, IEquatable<WeaponInput>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in WeaponInput baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(!WeaponType.Equals(baseSnapshot.WeaponType)), 1);
            if(!WeaponType.Equals(baseSnapshot.WeaponType)) WeaponType.Write(ref writer, compressionModel, baseSnapshot.WeaponType);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in WeaponInput baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                WeaponType = baseSnapshot.WeaponType;
            else
                WeaponType.Read(ref reader, compressionModel, baseSnapshot.WeaponType);

        }

        public bool Equals(WeaponInput other)
        {
            bool equals = true;
            //<equals>
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is WeaponInput other && Equals(other);
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
