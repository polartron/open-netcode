using OpenNetcode.Shared.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport;

namespace OpenNetcode.Shared
{
    public enum PacketType
    {
        Input,
        PublicSnapshot,
        PrivateSnapshot,
        Result,
        Login,
        ClientInfo,
        NetworkedPrefabs
    }

    public static class Packets
    {
        public static unsafe PacketArrayWrapper WrapPacket(in DataStreamWriter writer)
        {
            int length = (writer.LengthInBits / 8) + 1;
            NativeArray<byte> buffer = new NativeArray<byte>(length, Allocator.Temp);
            void* writerArrayPointer = writer.AsNativeArray().GetUnsafeReadOnlyPtr();
            
            return new PacketArrayWrapper()
            {
                Pointer = writerArrayPointer,
                Length = buffer.Length,
                Allocator = Allocator.Temp
            };
        }
        
        public static unsafe PacketArrayWrapper WrapPacket(in NativeArray<byte> bytes, int length)
        {
            NativeArray<byte> buffer = new NativeArray<byte>(length, Allocator.Temp);
            void* writerArrayPointer = bytes.GetUnsafeReadOnlyPtr();
            UnsafeUtility.MemMove(buffer.GetUnsafePtr(), writerArrayPointer, length);
            
            return new PacketArrayWrapper()
            {
                Pointer = writerArrayPointer,
                Length = buffer.Length
            };
        }
        
        public static PacketType ReadPacketType(ref DataStreamReader reader)
        {
            return (PacketType) reader.ReadRawBits(5);
        }

        public static void WritePacketType(PacketType type, ref DataStreamWriter writer)
        {
            writer.WriteRawBits((uint) type, 5);
        }
    }
}