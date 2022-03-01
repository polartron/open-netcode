using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Components
{
    public partial struct WeaponInput : ISnapshotComponent<WeaponInput>
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
    }
}
