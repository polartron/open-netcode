using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Components
{
    public partial struct BumpEvent : ISnapshotComponent<BumpEvent>, IEquatable<BumpEvent>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(!SoundIndex.Equals(baseSnapshot.SoundIndex)), 1);
            if(!SoundIndex.Equals(baseSnapshot.SoundIndex)) SoundIndex.Write(ref writer, compressionModel, baseSnapshot.SoundIndex);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                SoundIndex = baseSnapshot.SoundIndex;
            else
                SoundIndex.Read(ref reader, compressionModel, baseSnapshot.SoundIndex);

        }

        public bool Equals(BumpEvent other)
        {
            bool equals = true;
            //<equals>
            equals = equals && SoundIndex.Equals(other.SoundIndex);
            return equals;
        }

        public override bool Equals(object obj)
        {
            return obj is BumpEvent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                //<hash>
                hash = hash * 23 + SoundIndex.GetHashCode();
                return hash;
            }
        }
    }
}
