using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct PathComponent : ISnapshotComponent<PathComponent>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in PathComponent baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(From != baseSnapshot.From), 1);
            if(From != baseSnapshot.From) From.Write(ref writer, compressionModel, baseSnapshot.From);

            writer.WriteRawBits(Convert.ToUInt32(To != baseSnapshot.To), 1);
            if(To != baseSnapshot.To) To.Write(ref writer, compressionModel, baseSnapshot.To);

            writer.WriteRawBits(Convert.ToUInt32(Start != baseSnapshot.Start), 1);
            if(Start != baseSnapshot.Start) Start.Write(ref writer, compressionModel, baseSnapshot.Start);

            writer.WriteRawBits(Convert.ToUInt32(Stop != baseSnapshot.Stop), 1);
            if(Stop != baseSnapshot.Stop) Stop.Write(ref writer, compressionModel, baseSnapshot.Stop);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in PathComponent baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                From = baseSnapshot.From;
            else
                From.Read(ref reader, compressionModel, baseSnapshot.From);

            if (reader.ReadRawBits(1) == 0)
                To = baseSnapshot.To;
            else
                To.Read(ref reader, compressionModel, baseSnapshot.To);

            if (reader.ReadRawBits(1) == 0)
                Start = baseSnapshot.Start;
            else
                Start.Read(ref reader, compressionModel, baseSnapshot.Start);

            if (reader.ReadRawBits(1) == 0)
                Stop = baseSnapshot.Stop;
            else
                Stop.Read(ref reader, compressionModel, baseSnapshot.Stop);

        }
    }
}
