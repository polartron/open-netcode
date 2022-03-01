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
        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            //<read>
        }

        public bool Equals(BumpEvent other)
        {
            bool equals = true;
            //<equals>
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
                return hash;
            }
        }
    }
}
