using System;
using ExampleGame.Shared.Components;
using OpenNetcode.Movement.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Shared.Generated;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [BurstCompile]
    internal unsafe struct MakeSnapshotJob : IJob
    {
        [NativeDisableUnsafePtrRestriction] public void* Buffers;
        [ReadOnly] public int BuffersLength;
        public NativeHashMap<int, PacketArrayWrapper>.ParallelWriter Results;
        [ReadOnly] public int JobIndex;
        [ReadOnly] public int Hash;
        [ReadOnly] public int SnapshotIndex;
        [ReadOnly] public int PlayerSnapshotIndex;
        [ReadOnly] public Area Area;
        [ReadOnly] public NetworkCompressionModel CompressionModel;
        [ReadOnly] public NativeMultiHashMap<int, ServerEntitySnapshot> PlayerSnapshots;

        //<template>
        //[ReadOnly] public ComponentDataFromEntity<##TYPE##> ##TYPE##FromEntity;
        //</template>
//<generated>
        [ReadOnly] public ComponentDataFromEntity<EntityPosition> EntityPositionFromEntity;
        [ReadOnly] public ComponentDataFromEntity<EntityVelocity> EntityVelocityFromEntity;
//</generated>
        //<events>
        //[ReadOnly] public BufferFromEntity<##TYPE##> ##TYPE##BufferFromEntity;
        //</events>
//<generated>
        [ReadOnly] public BufferFromEntity<BumpEvent> BumpEventBufferFromEntity;
//</generated>

        public void Execute()
        {
            NativeArray<ServerEntitySnapshot> entitySnapshots = new NativeArray<ServerEntitySnapshot>(PlayerSnapshots.CountValuesForKey(Hash), Allocator.Temp);
            ServerEntitySnapshot serverEntitySnapshot;
            NativeMultiHashMapIterator<int> iterator;

            int count = 0;
            if (PlayerSnapshots.TryGetFirstValue(Hash, out serverEntitySnapshot, out iterator))
            {
                do
                {
                    entitySnapshots[count] = serverEntitySnapshot;
                    count++;
                } while (PlayerSnapshots.TryGetNextValue(out serverEntitySnapshot, ref iterator));
            }

            entitySnapshots.Sort();

            MakeBaselines(ref Area, entitySnapshots, SnapshotIndex);

            var be = Area.EntitySnapshotBaseLine.GetBaseline(PlayerSnapshotIndex);
                
            NativeArray<PacketArrayWrapper> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<PacketArrayWrapper>(
                Buffers, BuffersLength, Allocator.TempJob);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
            PacketArrayWrapper wrapper = UnsafeUtility.ReadArrayElement<PacketArrayWrapper>(array.GetUnsafePtr(), JobIndex);
            PacketArrayWrapper deltaSnapshot = CreateSnapshot(wrapper, be, entitySnapshots);
            Results.TryAdd(JobIndex, deltaSnapshot);

            entitySnapshots.Dispose();
        }

        private void MakeBaselines(ref Area area, in NativeArray<ServerEntitySnapshot> entitySnapshots, int tick)
        {
            NativeArray<ServerEntitySnapshot> entities =
                new NativeArray<ServerEntitySnapshot>(entitySnapshots.Length, Allocator.Temp);
            //<template>
            //NativeArray<##TYPE##> ##TYPELOWER##Components = new NativeArray<##TYPE##>(entitySnapshots.Length, Allocator.Temp);
            //int ##TYPELOWER##Index = 0;
            //</template>
//<generated>
            NativeArray<EntityPosition> entityPositionComponents = new NativeArray<EntityPosition>(entitySnapshots.Length, Allocator.Temp);
            int entityPositionIndex = 0;
            NativeArray<EntityVelocity> entityVelocityComponents = new NativeArray<EntityVelocity>(entitySnapshots.Length, Allocator.Temp);
            int entityVelocityIndex = 0;
//</generated>

            for (int i = 0; i < entitySnapshots.Length; i++)
            {
                ServerEntitySnapshot snapshot = entitySnapshots[i];
                int mask = snapshot.ComponentMask;

                //<template>
                //if ((mask & (1 << ##INDEX##)) != 0)
                //{
                //    ##TYPELOWER##Components[##TYPELOWER##Index] = ##TYPE##FromEntity[snapshot.Entity];
                //    snapshot.##TYPE##Index = ##TYPELOWER##Index;
                //    ##TYPELOWER##Index++;
                //}
                //</template>
//<generated>
                if ((mask & (1 << 0)) != 0)
                {
                    entityPositionComponents[entityPositionIndex] = EntityPositionFromEntity[snapshot.Entity];
                    snapshot.EntityPositionIndex = entityPositionIndex;
                    entityPositionIndex++;
                }
                if ((mask & (1 << 1)) != 0)
                {
                    entityVelocityComponents[entityVelocityIndex] = EntityVelocityFromEntity[snapshot.Entity];
                    snapshot.EntityVelocityIndex = entityVelocityIndex;
                    entityVelocityIndex++;
                }
//</generated>

                entities[i] = snapshot;
            }

            area.EntitySnapshotBaseLine.UpdateBaseline(entities, tick, entitySnapshots.Length);
            //<template>
            //area.##TYPE##BaseLine.UpdateBaseline(##TYPELOWER##Components, tick, ##TYPELOWER##Index);
            //</template>
//<generated>
            area.EntityPositionBaseLine.UpdateBaseline(entityPositionComponents, tick, entityPositionIndex);
            area.EntityVelocityBaseLine.UpdateBaseline(entityVelocityComponents, tick, entityVelocityIndex);
//</generated>

            entities.Dispose();
        }

        private PacketArrayWrapper CreateSnapshot(in PacketArrayWrapper wrapper, in NativeSlice<ServerEntitySnapshot> baseSnapshots,
            in NativeArray<ServerEntitySnapshot> currentSnapshots)
        {
            SnapshotStaging staging = SnapshotStaging.Create(baseSnapshots, currentSnapshots, SnapshotSettings.MaxEntititesInArea);

            NativeArray<byte> byteArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(wrapper.Pointer, wrapper.Length, Allocator.TempJob);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref byteArray, AtomicSafetyHandle.Create());
#endif
            var writer = new DataStreamWriter(wrapper.Length, Allocator.Temp);

            Packets.WritePacketType(PacketType.PublicSnapshot, ref writer); // Type
            writer.WritePackedUInt((uint) SnapshotIndex, CompressionModel); // SnapshotIndex
            writer.WritePackedInt(Hash, CompressionModel); // Hash
            writer.WritePackedUInt((uint) PlayerSnapshotIndex, CompressionModel); // BaseLine
            writer.WritePackedUInt((uint) staging.RemovedOrAdded.Length, CompressionModel); // RemovedOrAdded Length

            int lastIndex = 0;
            int lastType = 0;

            for (int i = 0; i < staging.RemovedOrAdded.Length; i++)
            {
                var removeOrAdd = staging.RemovedOrAdded[i];

                if (removeOrAdd.Added)
                {
                    writer.WriteRawBits(1, 1); // Entity was added

                    if (removeOrAdd.Type == lastType)
                    {
                        writer.WriteRawBits(0, 1); // Type is the same
                    }
                    else
                    {
                        writer.WriteRawBits(1, 1); // Type has changed
                        writer.WritePackedInt(removeOrAdd.Type, CompressionModel); // Write new type
                        lastType = removeOrAdd.Type;
                    }
                }
                else
                {
                    writer.WriteRawBits(0, 1); // Entity was removed
                }

                writer.WritePackedUInt((uint) (removeOrAdd.Index - lastIndex), CompressionModel); // Index offset
                lastIndex = removeOrAdd.Index;
            }

            //<template>
            //var base##TYPELOWER## = Area.##TYPE##BaseLine.GetBaseline(PlayerSnapshotIndex);
            //</template>
//<generated>
            var baseentityPosition = Area.EntityPositionBaseLine.GetBaseline(PlayerSnapshotIndex);
            var baseentityVelocity = Area.EntityVelocityBaseLine.GetBaseline(PlayerSnapshotIndex);
//</generated>

            for (int i = 0; i < currentSnapshots.Length; i++)
            {
                var current = currentSnapshots[i];
                var updated = staging.Updated[i];
                int componentMask = current.ComponentMask;
                int eventMask = current.EventMask;

                //<template>
                //if ((componentMask & (1 << ##INDEX##)) != 0)
                //{
                //    ##TYPE## component = ##TYPE##FromEntity[current.Entity];
                //    ##TYPE## baseComponent = updated.Added ? default : base##TYPELOWER##[updated.Base.##TYPE##Index];
                //    component.WriteSnapshot(ref writer, CompressionModel, baseComponent);
                //}
                //</template>
//<generated>
                if ((componentMask & (1 << 0)) != 0)
                {
                    EntityPosition component = EntityPositionFromEntity[current.Entity];
                    EntityPosition baseComponent = updated.Added ? default : baseentityPosition[updated.Base.EntityPositionIndex];
                    component.WriteSnapshot(ref writer, CompressionModel, baseComponent);
                }
                if ((componentMask & (1 << 1)) != 0)
                {
                    EntityVelocity component = EntityVelocityFromEntity[current.Entity];
                    EntityVelocity baseComponent = updated.Added ? default : baseentityVelocity[updated.Base.EntityVelocityIndex];
                    component.WriteSnapshot(ref writer, CompressionModel, baseComponent);
                }
//</generated>

                writer.WriteRawBits(Convert.ToUInt32(eventMask != 0), 1); // Does this entity have events?

                if (eventMask != 0)
                {
                    writer.WriteRawBits((uint) eventMask, SnapshotSettings.EventMaskBits); // Event mask

                    //<events>
                    //if ((eventMask & (1 << ##INDEX##)) != 0)
                    //{
                    //    var buffer = ##TYPE##BufferFromEntity[current.Entity];
                    //    writer.WriteRawBits((uint) buffer.Length, SnapshotSettings.MaxEventsBufferLength);
                    //
                    //    for (int b = 0; b < buffer.Length; b++)
                    //    {
                    //        buffer[b].WriteSnapshot(ref writer, CompressionModel, default);
                    //    }
                    //}
                    //</events>
//<generated>
                    if ((eventMask & (1 << 0)) != 0)
                    {
                        var buffer = BumpEventBufferFromEntity[current.Entity];
                        writer.WriteRawBits((uint) buffer.Length, SnapshotSettings.MaxEventsBufferLength);
                    
                        for (int b = 0; b < buffer.Length; b++)
                        {
                            buffer[b].WriteSnapshot(ref writer, CompressionModel, default);
                        }
                    }
//</generated>
                }
            }
                
            staging.Dispose();

            var array = writer.AsNativeArray();
            UnsafeUtility.MemMove(wrapper.Pointer, array.GetUnsafePtr(), array.Length);
                
            return new PacketArrayWrapper()
            {
                Pointer = wrapper.Pointer,
                Length = writer.Length,
                Allocator = Allocator.Invalid
            };
        }
    }
}