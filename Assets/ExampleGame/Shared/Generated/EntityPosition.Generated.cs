using System;
using Unity.Networking.Transport;
using OpenNetcode.Shared.Components;

namespace ExampleGame.Shared.Movement.Components
{
    public partial struct EntityPosition : ISnapshotComponent<EntityPosition>
    {
        public void WriteSnapshot(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in EntityPosition baseSnapshot)
        {
            //<write>
            writer.WriteRawBits(Convert.ToUInt32(Value != baseSnapshot.Value), 1);
            if(!Value.Equals(baseSnapshot.Value)) Value.Write(ref writer, compressionModel, baseSnapshot.Value);

            writer.WriteRawBits(Convert.ToUInt32(Test != baseSnapshot.Test), 1);
            if(!Test.Equals(baseSnapshot.Test)) Test.Write(ref writer, compressionModel, baseSnapshot.Test);

        }

        public void ReadSnapshot(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in EntityPosition baseSnapshot)
        {
            //<read>
            if (reader.ReadRawBits(1) == 0)
                Value = baseSnapshot.Value;
            else
                Value.Read(ref reader, compressionModel, baseSnapshot.Value);

            if (reader.ReadRawBits(1) == 0)
                Test = baseSnapshot.Test;
            else
                Test.Read(ref reader, compressionModel, baseSnapshot.Test);

        }
    }
}
