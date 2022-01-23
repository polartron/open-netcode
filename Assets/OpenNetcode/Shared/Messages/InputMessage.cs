using OpenNetcode.Shared.Components;
using Unity.Collections;
using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Messages
{
    public struct InputMessage<T> where T : unmanaged, INetworkedComponent
    {
        public T Input;
        public int Tick;
        public int LastReceivedSnapshotTick;
        public int Version;
        
        public static bool Write(ref NativeArray<T> inputs, int tick, int lastReceivedSnapshotTick, int version, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            Packets.WritePacketType(PacketType.Input, ref writer);
            writer.WriteRawBits((uint) inputs.Length, 3);
            writer.WritePackedUInt((uint) tick, compressionModel);
            writer.WritePackedUInt((uint) lastReceivedSnapshotTick, compressionModel);
            writer.WritePackedUInt((uint) version, compressionModel);
            
            int lastWrittenHashCode = default(T).Hash();

            for (int i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];

                int hash = input.Hash();
                if (hash == lastWrittenHashCode)
                {
                    writer.WriteRawBits(1, 1);
                }
                else
                {
                    writer.WriteRawBits(0, 1);
                    input.Write(ref writer, compressionModel);
                    lastWrittenHashCode = hash;
                }
            }
            
            return !writer.HasFailedWrites;
        }
        
        public static bool Read(ref NativeMultiHashMap<int, InputMessage<T>> inputs, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, int internalId)
        {
            Packets.ReadPacketType(ref reader);
            int count = (int) reader.ReadRawBits(3);
            uint tick = reader.ReadPackedUInt(compressionModel);
            int lastReceivedSnapshotTick = (int) reader.ReadPackedUInt(compressionModel);
            int version = (int) reader.ReadPackedUInt(compressionModel);

            T component = new T();
            
            for (int i = 0; i < count; i++)
            {
                uint repeat = reader.ReadRawBits(1);

                if (repeat == 1)
                {
                    if (!reader.HasFailedReads)
                    {
                        inputs.Add(internalId, new InputMessage<T>()
                        {
                            Tick = (int) tick - i,
                            Input = component,
                            LastReceivedSnapshotTick = lastReceivedSnapshotTick,
                            Version = version
                        });
                    }
                }
                else
                {
                    component.Read(ref reader, compressionModel);

                    if (!reader.HasFailedReads)
                    {
                        inputs.Add(internalId, new InputMessage<T>()
                        {
                            Tick = (int) tick - i,
                            Input = component,
                            LastReceivedSnapshotTick = lastReceivedSnapshotTick,
                            Version = version
                        });
                    }
                }
            }

            return !reader.HasFailedReads;
        }
    }
}
