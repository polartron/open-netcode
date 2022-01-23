using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Messages
{
    public struct ClientInfoMessage
    {
        public static bool Write(int entityId, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            Packets.WritePacketType(PacketType.ClientInfo, ref writer);
            writer.WritePackedUInt((uint) entityId, compressionModel);
            writer.WritePackedUInt((uint) entityId, compressionModel);
            writer.WritePackedUInt((uint) entityId, compressionModel);
            writer.WritePackedUInt((uint) entityId, compressionModel);
            return writer.HasFailedWrites;
        }

        public static int Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel)
        {
            Packets.ReadPacketType(ref reader);
            int entityId = (int) reader.ReadPackedUInt(compressionModel);
            return entityId;
        }
    }
}