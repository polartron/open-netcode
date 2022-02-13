using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct EntityVelocity : ISnapshotComponent<EntityVelocity>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in EntityVelocity baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(Value != baseSnapshot.Value), 1);
            if(Value != baseSnapshot.Value) Value.Write(ref writer, compressionModel, baseSnapshot.Value);

            writer.WriteRawBits(Convert.ToUInt32(CustomEnum != baseSnapshot.CustomEnum), 1);
            if(CustomEnum != baseSnapshot.CustomEnum) ((int) CustomEnum).Write(ref writer, compressionModel, (int) baseSnapshot.CustomEnum);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in EntityVelocity baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Value = baseSnapshot.Value;
            else
                Value.Read(ref reader, compressionModel, baseSnapshot.Value);

            if (reader.ReadRawBits(1) == 0)
                CustomEnum = baseSnapshot.CustomEnum;
            else
            {
                int temp = ((int) CustomEnum);
                temp.Read(ref reader, compressionModel, (int) baseSnapshot.CustomEnum);
                CustomEnum = (CustomEnum) temp;
            }
        }
    }
}
