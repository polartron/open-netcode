using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Utils
{
    public unsafe struct IncomingPacket
    {
        public DataStreamReader Reader;
        public NetworkConnection Connection;
    }
    
    public unsafe struct PacketArrayWrapper
    {
        public DataStreamReader Reader;
        public void* Pointer;
        public int Length;
        public int InternalId;
        public Allocator Allocator;

        public NativeArray<T> GetArray<T>() where T : unmanaged
        {    
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(Pointer, Length, Allocator);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif    
            return array;
        }
    }
}