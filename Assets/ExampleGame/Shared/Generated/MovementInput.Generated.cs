using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct MovementInput : ISnapshotComponent<MovementInput>
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
    }
}
