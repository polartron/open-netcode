using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Components
{
    public partial struct BumpEvent : ISnapshotComponent<BumpEvent>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            //<write>
        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in BumpEvent baseSnapshot)
        {
            //<read>
        }
    }
}
