using System;
using System.Collections.Generic;
using System.Linq;
using OpenNetcode.Shared.Time;
using Shared.Time;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Networking.Transport;

namespace OpenNetcode.Shared
{
    public unsafe struct BaseLines<T> : IDisposable where T : unmanaged
    {
        private readonly int _capacity;
        private readonly int _segments;
        
        [NativeDisableUnsafePtrRestriction] private void* _snapshotArrayPointer;
        [NativeDisableUnsafePtrRestriction] private void* _lengthsArrayPointer;

        private AtomicSafetyHandle _safetyHandle;

        public BaseLines(int length, int segments)
        {
            _segments = segments;
            _capacity = length * _segments;
            _snapshotArrayPointer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * _capacity, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            UnsafeUtility.MemClear(_snapshotArrayPointer, UnsafeUtility.SizeOf<T>() * _capacity);
            _lengthsArrayPointer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>() * _segments, UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
            UnsafeUtility.MemClear(_lengthsArrayPointer, UnsafeUtility.SizeOf<int>() * _segments);
            _safetyHandle = AtomicSafetyHandle.Create();
        }
        
        public void Dispose()
        {
            UnsafeUtility.Free(_snapshotArrayPointer, Allocator.Persistent);
            UnsafeUtility.Free(_lengthsArrayPointer, Allocator.Persistent);
            AtomicSafetyHandle.Release(_safetyHandle);
        }

        public NativeSlice<T> GetBaseline(int baseLine)
        {
            int index = baseLine % _segments;
            int chunkSize = _capacity / _segments;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(_safetyHandle);
#endif
            
            int length = UnsafeUtility.ReadArrayElement<int>(_lengthsArrayPointer, index);
            IntPtr target = (IntPtr) _snapshotArrayPointer + UnsafeUtility.SizeOf<T>() * index * chunkSize;
            
            if (baseLine == 0)
            {
                var emptySlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>((T*) target, UnsafeUtility.SizeOf<T>(), 0);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref emptySlice, _safetyHandle);
#endif
                return NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>((T*) target, UnsafeUtility.SizeOf<T>(), 0);
            }
            
            var slice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>((T*) target, UnsafeUtility.SizeOf<T>(), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref slice, _safetyHandle);
#endif
            return slice;
        }

        public void UpdateBaseline(NativeArray<T> snapshots, int baseLine, int snapshotsLength)
        {
            int index = baseLine % _segments;
            int chunkSize = _capacity / _segments;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(_safetyHandle);
#endif
            IntPtr target = (IntPtr) _snapshotArrayPointer + UnsafeUtility.SizeOf<T>() * index * chunkSize;
            UnsafeUtility.MemMove((void*) target, snapshots.GetUnsafePtr(), snapshotsLength * sizeof(T));
            UnsafeUtility.WriteArrayElement<int>(_lengthsArrayPointer, index, snapshotsLength);
        }
    }
}