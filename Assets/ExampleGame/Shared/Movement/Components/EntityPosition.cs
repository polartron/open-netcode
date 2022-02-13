using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Shared.Coordinates;
using Unity.Entities;
using Unity.Networking.Transport;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    [PublicSnapshot]
    public partial struct EntityPosition : INetworkedComponent
    {
        public GameUnits Value;
        public int Test;

        public void Write(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            Value.Write(ref writer, compressionModel);
        }

        public void Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel)
        {
            Value.Read(ref reader, compressionModel);
        }

        public int Hash()
        {
            return Value.Hash();
        }
    }
}