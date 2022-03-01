using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct PathComponent : ISnapshotComponent<PathComponent>, IEquatable<PathComponent>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in PathComponent baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(!From.Equals(baseSnapshot.From)), 1);
            if(!From.Equals(baseSnapshot.From)) From.Write(ref writer, compressionModel, baseSnapshot.From);

            writer.WriteRawBits(Convert.ToUInt32(!To.Equals(baseSnapshot.To)), 1);
            if(!To.Equals(baseSnapshot.To)) To.Write(ref writer, compressionModel, baseSnapshot.To);

            writer.WriteRawBits(Convert.ToUInt32(!Start.Equals(baseSnapshot.Start)), 1);
            if(!Start.Equals(baseSnapshot.Start)) Start.Write(ref writer, compressionModel, baseSnapshot.Start);

            writer.WriteRawBits(Convert.ToUInt32(!Stop.Equals(baseSnapshot.Stop)), 1);
            if(!Stop.Equals(baseSnapshot.Stop)) Stop.Write(ref writer, compressionModel, baseSnapshot.Stop);

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

        public bool Equals(PathComponent other)
        {
            bool equals = true;
            //<equals>
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is PathComponent other && Equals(other);
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
