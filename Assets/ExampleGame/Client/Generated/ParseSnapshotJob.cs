using System;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Time;
using Shared.Generated;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    [BurstCompile]
    public unsafe struct ParsePublicSnapshotJob : IJob
    {
        public NativeArray<bool> Success;
        public EntityCommandBuffer EntityCommandBuffer;
        public NativeHashMap<int, EntityArchetype> EntityArchetypes;
        public NativeHashMap<int, Entity> Prefabs;
        public ClientData ClientData;
        [ReadOnly] public NativeArray<byte> Snapshot;
        public NetworkCompressionModel CompressionModel;
        public ClientArea Area;
        public NativeHashMap<int, ClientEntitySnapshot> SnapshotEntities;
        public NativeHashMap<int, ClientEntitySnapshot> ObservedEntities;
        
        //<template>
        //public BufferFromEntity<SnapshotBufferElement<##TYPE##>> ##TYPE##Buffer;
        //</template>
//<generated>
        public BufferFromEntity<SnapshotBufferElement<EntityPosition>> EntityPositionBuffer;
        public BufferFromEntity<SnapshotBufferElement<EntityVelocity>> EntityVelocityBuffer;
//</generated>
        //<privatetemplate>
        //public BufferFromEntity<SnapshotBufferElement<##TYPE##>> ##TYPE##Buffer;
        //</privatetemplate>
//<generated>
        public BufferFromEntity<SnapshotBufferElement<EntityHealth>> EntityHealthBuffer;
//</generated>

        //<events>
        //public BufferFromEntity<SnapshotBufferElement<##TYPE##>> ##TYPE##Buffer;
        //</events>
//<generated>
        public BufferFromEntity<SnapshotBufferElement<BumpEvent>> BumpEventBuffer;
//</generated>
        private struct ComponentBuffers
        {
            private void* Pointers;
            private void* Lenghts;

            public ComponentBuffers(int size)
            {
                Pointers = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<IntPtr>() * size, UnsafeUtility.AlignOf<IntPtr>(), Allocator.Temp);
                Lenghts = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>() * size, UnsafeUtility.AlignOf<int>(), Allocator.Temp);
            }

            public void Set(void* pointer, int length, int index)
            {
                UnsafeUtility.WriteArrayElement<IntPtr>(Pointers, index, (IntPtr) pointer);
                UnsafeUtility.WriteArrayElement<int>(Lenghts, index, length);
            }

            public NativeArray<T> GetBufferArray<T>(int index)
                where T : unmanaged
            {
                var array = NativeArrayUnsafeUtility
                    .ConvertExistingDataToNativeArray<T>(
                        (void*) UnsafeUtility.ReadArrayElement<IntPtr>(Pointers, index),
                        UnsafeUtility.ReadArrayElement<int>(Lenghts, index), Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
                return array;
            }
        }


        public void Execute()
        {
            var reader = new DataStreamReader(Snapshot);
            Packets.ReadPacketType(ref reader);
            int snapshotIndex = (int) reader.ReadPackedUInt(CompressionModel); // SnapshotIndex
            int hash = reader.ReadPackedInt(CompressionModel); // Hash
            int baseLine = (int) reader.ReadPackedUInt(CompressionModel); // BaseLine
            int tick = snapshotIndex * (TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond);

            var baseEntities = Area.ClientEntitySnapshotBaseLine.GetBaseline(baseLine);

            //<template>
            //var base##TYPE## = Area.##TYPE##BaseLine.GetBaseline(baseLine);
            //</template>
//<generated>
            var baseEntityPosition = Area.EntityPositionBaseLine.GetBaseline(baseLine);
            var baseEntityVelocity = Area.EntityVelocityBaseLine.GetBaseline(baseLine);
//</generated>

            NativeHashSet<ActiveEntity> activeEntities = new NativeHashSet<ActiveEntity>(5000, Allocator.Temp);

            for (var i = 0; i < baseEntities.Length; i++)
            {
                var baseEntity = baseEntities[i];

                activeEntities.Add(new ActiveEntity()
                {
                    Index = baseEntity.ServerId,
                    HasBase = true,
                    BaseEntity = baseEntity,
                    Type = baseEntity.Type
                });
            }

            int removedOrAddedCount = (int) reader.ReadPackedUInt(CompressionModel); // Removed Length
            int lastIndex = 0;
            int lastType = 0;

            var entityComponentBuffers =
                new NativeHashMap<int, ComponentBuffers>(SnapshotEntities.Count(), Allocator.Temp);

            for (int i = 0; i < removedOrAddedCount; i++)
            {
                bool added = Convert.ToBoolean(reader.ReadRawBits(1));

                if (added)
                {
                    if (Convert.ToBoolean(reader.ReadRawBits(1)))
                    {
                        lastType = reader.ReadPackedInt(CompressionModel);
                    }

                    lastIndex = (int) (lastIndex + reader.ReadPackedUInt(CompressionModel));

                    activeEntities.Add(new ActiveEntity()
                    {
                        Index = lastIndex,
                        HasBase = false,
                        Type = lastType
                    });

                    // Create entity

                    AddToSnapshot(CreateSnapshotEntity(ref entityComponentBuffers, lastIndex, lastType));
                }
                else // removed
                {
                    int index = (int) (lastIndex + reader.ReadPackedUInt(CompressionModel));
                    lastIndex = index;

                    if (activeEntities.Contains(index))
                    {
                        activeEntities.Remove(index);

                        if (SnapshotEntities.ContainsKey(index))
                        {
                            var serverEntitySnapshot = SnapshotEntities[index];
                            SnapshotEntities.Remove(serverEntitySnapshot.ServerId);
                            
                            if (!ObservedEntities.ContainsKey(serverEntitySnapshot.ServerId) &&
                                ClientData.LocalPlayerServerEntityId != serverEntitySnapshot.ServerId)
                            {
                                EntityCommandBuffer.DestroyEntity(serverEntitySnapshot.Entity);
                            }
                        }
                    }
                }
            }

            var activeEntitiesList = activeEntities.ToNativeArray(Allocator.Temp);
            activeEntitiesList.Sort();
            var serverEntitiesArray = SnapshotEntities.GetKeyArray(Allocator.Temp);
            serverEntitiesArray.Sort();

            for (int i = 0; i < serverEntitiesArray.Length; i++)
            {
                var index = serverEntitiesArray[i];

                if (!activeEntities.Contains(index))
                {
                    var serverEntitySnapshot = SnapshotEntities[index];
                    SnapshotEntities.Remove(serverEntitySnapshot.ServerId);
                    
                    if (!ObservedEntities.ContainsKey(serverEntitySnapshot.ServerId) &&
                        ClientData.LocalPlayerServerEntityId != serverEntitySnapshot.ServerId)
                    {
                        EntityCommandBuffer.DestroyEntity(serverEntitySnapshot.Entity);
                    }
                }
            }

            // Read changed

            for (int i = 0; i < activeEntitiesList.Length; i++)
            {
                var activeEntity = activeEntitiesList[i];

                if (!SnapshotEntities.ContainsKey(activeEntity.Index))
                {
                    // TODO: Figure out why I have to re-create the entity.
                    // This shouldn't happen.
                    AddToSnapshot(CreateSnapshotEntity(ref entityComponentBuffers, activeEntity.Index, activeEntity.Type));
                }

                ClientEntitySnapshot serverEntity = SnapshotEntities[activeEntity.Index];
                bool hasCreatedBuffer = entityComponentBuffers.ContainsKey(serverEntity.ServerId);
                ComponentBuffers componentBuffers = hasCreatedBuffer ? entityComponentBuffers[serverEntity.ServerId] : default;

                //<template>
                //if (!ParseComponent<##TYPE##>(tick, ##INDEX##, activeEntity.BaseEntity.##TYPE##Index, ref componentBuffers,
                //    hasCreatedBuffer, serverEntity, activeEntity, ref reader,
                //    base##TYPE##, ref ##TYPE##Buffer, CompressionModel))
                //{
                //    Success[0] = false;
                //    return;
                //}
                //</template>
//<generated>
                if (!ParseComponent<EntityPosition>(tick, 0, activeEntity.BaseEntity.EntityPositionIndex, ref componentBuffers,
                    hasCreatedBuffer, serverEntity, activeEntity, ref reader,
                    baseEntityPosition, ref EntityPositionBuffer, CompressionModel))
                {
                    Success[0] = false;
                    return;
                }
                if (!ParseComponent<EntityVelocity>(tick, 1, activeEntity.BaseEntity.EntityVelocityIndex, ref componentBuffers,
                    hasCreatedBuffer, serverEntity, activeEntity, ref reader,
                    baseEntityVelocity, ref EntityVelocityBuffer, CompressionModel))
                {
                    Success[0] = false;
                    return;
                }
//</generated>

                if (Convert.ToBoolean(reader.ReadRawBits(1))) // We have events
                {
                    int eventMaskBits = 0;
                    //<template>
                    //eventMaskBits = ##EVENTMASKBITS##;
                    //</template>
//<generated>
                    eventMaskBits = 1;
                    eventMaskBits = 1;
//</generated>
                    int eventMask = (int) reader.ReadRawBits(eventMaskBits); // What type of events?

                    //<events>
                    //if (!ParseEvent<##TYPE##>(eventMask, ##INDEXOFFSET##, tick, ##INDEX##, ref componentBuffers, hasCreatedBuffer,
                    //    serverEntity,
                    //    ref reader, ref ##TYPE##Buffer, CompressionModel))
                    //{
                    //    Success[0] = false;
                    //    return;
                    //}
                    //</events>
//<generated>
                    if (!ParseEvent<BumpEvent>(eventMask, 2, tick, 0, ref componentBuffers, hasCreatedBuffer,
                        serverEntity,
                        ref reader, ref BumpEventBuffer, CompressionModel))
                    {
                        Success[0] = false;
                        return;
                    }
//</generated>
                }
            }

            if (!reader.HasFailedReads)
            {
                Success[0] = true;
            }
        }
        
        static bool ParseEvent<T>(int eventMask, int arrayIndex, int tick, int index, ref ComponentBuffers componentBuffers, bool hasCreatedBuffer, in ClientEntitySnapshot serverEntity, ref DataStreamReader reader, ref BufferFromEntity<SnapshotBufferElement<T>> theBuffer, in NetworkCompressionModel compressionModel) where T : unmanaged, ISnapshotComponent<T>
        {
            if ((eventMask & (1 << index)) != 0)
            {
                int length = (int) reader.ReadRawBits(SnapshotSettings.MaxEventsBufferLength);

                for (int b = 0; b < length; b++)
                {
                    T bumpEvent = new T();
                    bumpEvent.ReadSnapshot(ref reader, compressionModel, default);
                    
                    if (reader.HasFailedReads)
                    {
                        return false;
                    }
                    
                    NativeArray<SnapshotBufferElement<T>> buffer = hasCreatedBuffer
                        ? componentBuffers.GetBufferArray<SnapshotBufferElement<T>>(arrayIndex)
                        : theBuffer[serverEntity.Entity].ToNativeArray(Allocator.Temp);
                            
                    buffer[tick % buffer.Length] = new SnapshotBufferElement<T>()
                    {
                        Value = bumpEvent,
                        Tick = tick
                    };
                    
                    if (!hasCreatedBuffer)
                    {
                        theBuffer[serverEntity.Entity].CopyFrom(buffer);
                    }
                }
            }

            return true;
        }

        private static bool ParseComponent<T>(int tick, int index, int baselineIndex, ref ComponentBuffers componentBuffers, bool hasCreatedBuffer, in ClientEntitySnapshot serverEntity, in ActiveEntity activeEntity, ref DataStreamReader reader, in NativeSlice<T> baseline, ref BufferFromEntity<SnapshotBufferElement<T>> theBuffer, in NetworkCompressionModel compressionModel) where T : unmanaged, ISnapshotComponent<T>
        {
            if ((serverEntity.ComponentMask & (1 << index)) != 0)
            {
                T component = new T();
                T baseComponent = activeEntity.HasBase
                    ? baseline[baselineIndex]
                    : default;
                component.ReadSnapshot(ref reader, compressionModel, baseComponent);

                if (reader.HasFailedReads)
                {
                    return false;
                }

                NativeArray<SnapshotBufferElement<T>> buffer = hasCreatedBuffer
                    ? componentBuffers.GetBufferArray<SnapshotBufferElement<T>>(index)
                    : theBuffer[serverEntity.Entity].ToNativeArray(Allocator.Temp);

                int length = buffer.Length;

                buffer[tick % length] = new SnapshotBufferElement<T>()
                {
                    Value = component,
                    Tick = tick
                };
                    
                if (!hasCreatedBuffer)
                {
                    theBuffer[serverEntity.Entity].CopyFrom(buffer);
                }
            }

            return true;
        }

        private void AddToSnapshot(in ClientEntitySnapshot snapshot)
        {
            if (!SnapshotEntities.ContainsKey(snapshot.ServerId))
                SnapshotEntities.Add(snapshot.ServerId, snapshot);
        }

        private ClientEntitySnapshot CreateSnapshotEntity(ref NativeHashMap<int, ComponentBuffers> entityCommandBuffers, int index, int type)
        {
            if (SnapshotEntities.TryGetValue(index, out var snapshotEntity))
            {
                return snapshotEntity;
            }

            if (ObservedEntities.TryGetValue(index, out var observedEntity))
            {
                return observedEntity;
            }

            Entity entity;
            bool created = false;

            EntityArchetype archetype = EntityArchetypes[type];
            Entity prefab = Prefabs[type];

            if (index == ClientData.LocalPlayerServerEntityId)
            {
                Debug.Log($"<color=green> Returning existing local entity with ID = {index}</color>");
                entity = ClientData.LocalPlayer;
            }
            else
            {
                entity = EntityCommandBuffer.Instantiate(prefab);
                EntityCommandBuffer.RemoveComponent<Prefab>(entity);
                created = true;
            }

            EntityCommandBuffer.AddComponent(entity, new ServerEntity()
            {
                ServerIndex = index
            });

            int componentBufferLength = 0;
            //<template>
            //componentBufferLength = ##COMPONENTBUFFERLENGTH##;
            //</template>
//<generated>
            componentBufferLength = 3;
            componentBufferLength = 3;
//</generated>
            var componentBuffers = new ComponentBuffers(componentBufferLength);
            
            //<template>
            //if (created)
            //{
            //    var ##TYPELOWER##Buffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<##TYPE##>>(entity);
            //    for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
            //        ##TYPELOWER##Buffer.Add(default);
            //    var array = ##TYPELOWER##Buffer.AsNativeArray();
            //    componentBuffers.Set(array.GetUnsafePtr(), array.Length, ##INDEX##);
            //}
            //else
            //{
            //    var array = ##TYPE##Buffer[entity].AsNativeArray();
            //    componentBuffers.Set(array.GetUnsafePtr(), array.Length, ##INDEX##);
            //}
            //</template>
//<generated>
            if (created)
            {
                var entityPositionBuffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<EntityPosition>>(entity);
                for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
                    entityPositionBuffer.Add(default);
                var array = entityPositionBuffer.AsNativeArray();
                componentBuffers.Set(array.GetUnsafePtr(), array.Length, 0);
            }
            else
            {
                var array = EntityPositionBuffer[entity].AsNativeArray();
                componentBuffers.Set(array.GetUnsafePtr(), array.Length, 0);
            }
            if (created)
            {
                var entityVelocityBuffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<EntityVelocity>>(entity);
                for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
                    entityVelocityBuffer.Add(default);
                var array = entityVelocityBuffer.AsNativeArray();
                componentBuffers.Set(array.GetUnsafePtr(), array.Length, 1);
            }
            else
            {
                var array = EntityVelocityBuffer[entity].AsNativeArray();
                componentBuffers.Set(array.GetUnsafePtr(), array.Length, 1);
            }
//</generated>
            //<privatetemplate>
            //if (created)
            //{
            //    var ##TYPELOWER##Buffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<##TYPE##>>(entity);
            //    for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
            //        ##TYPELOWER##Buffer.Add(default);
            //}
            //</privatetemplate>
//<generated>
            if (created)
            {
                var entityHealthBuffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<EntityHealth>>(entity);
                for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
                    entityHealthBuffer.Add(default);
            }
//</generated>
            //<events>
            //if (created)
            //{
            //    var ##TYPELOWER##Buffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<##TYPE##>>(entity);
            //    for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
            //        ##TYPELOWER##Buffer.Add(default);
            //    var array = ##TYPELOWER##Buffer.AsNativeArray();
            //    componentBuffers.Set(array.GetUnsafePtr(), array.Length, ##INDEXOFFSET## + ##INDEX##);
            //}
            //else
            //{
            //    var array = ##TYPE##Buffer[entity].AsNativeArray();
            //    componentBuffers.Set(array.GetUnsafePtr(), array.Length,##INDEXOFFSET## + ##INDEX##);
            //}
            //</events>
//<generated>
            if (created)
            {
                var bumpEventBuffer = EntityCommandBuffer.AddBuffer<SnapshotBufferElement<BumpEvent>>(entity);
                for (int i = 0; i < TimeConfig.SnapshotsPerSecond; i++)
                    bumpEventBuffer.Add(default);
                var array = bumpEventBuffer.AsNativeArray();
                componentBuffers.Set(array.GetUnsafePtr(), array.Length, 2 + 0);
            }
            else
            {
                var array = BumpEventBuffer[entity].AsNativeArray();
                componentBuffers.Set(array.GetUnsafePtr(), array.Length,2 + 0);
            }
//</generated>

            entityCommandBuffers[index] = componentBuffers;

            int mask = 0;
            var componentTypes = archetype.GetComponentTypes(Allocator.Temp);

            //<template>
            //if (componentTypes.Contains(ComponentType.ReadWrite<##TYPE##>()))
            //{
            //    mask = mask | (1 << ##INDEX##);
            //}
            //</template>
//<generated>
            if (componentTypes.Contains(ComponentType.ReadWrite<EntityPosition>()))
            {
                mask = mask | (1 << 0);
            }
            if (componentTypes.Contains(ComponentType.ReadWrite<EntityVelocity>()))
            {
                mask = mask | (1 << 1);
            }
//</generated>
            //<privatetemplate>
            //if (componentTypes.Contains(ComponentType.ReadWrite<##TYPE##>()))
            //{
            //    mask = mask | (1 << ##INDEX##);
            //}
            //</privatetemplate>
//<generated>
            if (componentTypes.Contains(ComponentType.ReadWrite<EntityHealth>()))
            {
                mask = mask | (1 << 2);
            }
//</generated>

            var snapshot = new ClientEntitySnapshot()
            {
                Entity = entity,
                ServerId = index,
                ComponentMask = mask
            };

            return snapshot;
        }
        
        private struct ActiveEntity : IComparable<ActiveEntity>, IEquatable<ActiveEntity>
        {
            public int Index;
            public bool HasBase;
            public int Type;
            public ClientEntitySnapshot BaseEntity;

            public bool Equals(ActiveEntity other)
            {
                return Index == other.Index;
            }

            public override bool Equals(object obj)
            {
                return obj is ActiveEntity other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Index;
            }

            public int CompareTo(ActiveEntity other)
            {
                return Index.CompareTo(other.Index);
            }

            public static implicit operator ActiveEntity(int index)
            {
                return new ActiveEntity()
                {
                    Index = index
                };
            }
        }
    }
}
