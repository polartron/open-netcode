using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Networking.Transport;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [BurstCompile]
    unsafe struct PrivateSnapshotJob : IJobChunk
    {
        [NativeDisableUnsafePtrRestriction] public void* PacketArray;
        [ReadOnly] public int PacketsLength;
        public NativeHashMap<int, PacketArrayWrapper>.ParallelWriter Results;
        [ReadOnly] public int Tick;
        public NativeHashSet<int>.ParallelWriter ActiveAreasList;
        public NativeMultiHashMap<int, PlayerSnapshot>.ParallelWriter PlayersInArea;
        [ReadOnly] public NetworkCompressionModel CompressionModel;
        [ReadOnly] public ComponentDataFromEntity<NetworkedPrefab> NetworkedPrefabFromEntityHandle;
        [ReadOnly] public ComponentTypeHandle<ServerNetworkedEntity> ServerNetworkedEntityHandle;
        [ReadOnly] public ComponentTypeHandle<SpatialHash> SpatialHashHandle;
        public ComponentTypeHandle<PlayerBaseLine> PlayerBaseLineHandle;
        [ReadOnly] public BufferTypeHandle<PrivateSnapshotObserver> PrivateSnapshotObserverHandle;
        //<template:publicsnapshot>
        //[ReadOnly] public ComponentDataFromEntity<##TYPE##> ##TYPE##Components;
        //</template>
//<generated>
        [ReadOnly] public ComponentDataFromEntity<EntityPosition> EntityPositionComponents;
        [ReadOnly] public ComponentDataFromEntity<EntityVelocity> EntityVelocityComponents;
        [ReadOnly] public ComponentDataFromEntity<PathComponent> PathComponentComponents;
//</generated>
        //<template:privatesnapshot>
        //[ReadOnly] public ComponentDataFromEntity<##TYPE##> ##TYPE##Components;
        //</template>
//<generated>
        [ReadOnly] public ComponentDataFromEntity<EntityHealth> EntityHealthComponents;
//</generated>
            
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var playerBaselines = chunk.GetNativeArray(PlayerBaseLineHandle);
            var serverNetworkedEntities = chunk.GetNativeArray(ServerNetworkedEntityHandle);
            var privateSnapshotObservers = chunk.GetBufferAccessor(PrivateSnapshotObserverHandle);
            var spatialHashes = chunk.GetNativeArray(SpatialHashHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                var playerBaseLine = playerBaselines[i];
                    
                if (playerBaseLine.Version == 0) // Client haven't received client info yet
                    return;

                var serverNetworkedEntity = serverNetworkedEntities[i];
                var privateSnapshotObserver = privateSnapshotObservers[i];
                var spatialHash = spatialHashes[i];

                PlayerSnapshot playerSnapshot = new PlayerSnapshot();
                playerSnapshot.PlayerId = serverNetworkedEntity.OwnerNetworkId;

                if (spatialHash.h0 != playerBaseLine.LastHash)
                {
                    playerBaseLine.LastHash = spatialHash.h0;
                    playerBaseLine.ExpectedVersion = playerBaseLine.ExpectedVersion + 1;
                    playerBaselines[i] = playerBaseLine;
                }

                if (playerBaseLine.Version != playerBaseLine.ExpectedVersion)
                {
                    playerSnapshot.SnapshotIndex = 0;
                }
                else
                {
                    playerSnapshot.SnapshotIndex = playerBaseLine.BaseLine;
                }

                PlayersInArea.Add(spatialHash.h0, playerSnapshot);
                ActiveAreasList.Add(spatialHash.h0);

                var writer = new DataStreamWriter(1024, Allocator.Temp);

                Packets.WritePacketType(PacketType.PrivateSnapshot, ref writer);
                writer.WritePackedUInt((uint) Tick, CompressionModel);
                writer.WritePackedUInt((uint) playerBaseLine.ExpectedVersion, CompressionModel);
                writer.WritePackedUInt((uint) privateSnapshotObserver.Length, CompressionModel);

                for (int j = 0; j < privateSnapshotObserver.Length; j++)
                {
                    var target = privateSnapshotObserver[j];
                    var targetEntity = target.Entity;
                    var networkedPrefab = NetworkedPrefabFromEntityHandle[targetEntity];

                    writer.WritePackedUInt((uint) targetEntity.Index, CompressionModel);
                    writer.WritePackedInt(networkedPrefab.Index, CompressionModel);
                    int updateMask = target.ComponentInterestMask;
                    writer.WritePackedInt(updateMask, CompressionModel);

                    //<template:publicsnapshot>
                    //if ((updateMask & (1 << ##INDEX##)) != 0 && ##TYPE##Components.HasComponent(targetEntity))
                    //{
                    //    var ##TYPELOWER## = ##TYPE##Components[targetEntity];
                    //    ##TYPELOWER##.WriteSnapshot(ref writer, CompressionModel, default);
                    //}
                    //</template>
//<generated>
                    if ((updateMask & (1 << 0)) != 0 && EntityPositionComponents.HasComponent(targetEntity))
                    {
                        var entityPosition = EntityPositionComponents[targetEntity];
                        entityPosition.WriteSnapshot(ref writer, CompressionModel, default);
                    }
                    if ((updateMask & (1 << 1)) != 0 && EntityVelocityComponents.HasComponent(targetEntity))
                    {
                        var entityVelocity = EntityVelocityComponents[targetEntity];
                        entityVelocity.WriteSnapshot(ref writer, CompressionModel, default);
                    }
                    if ((updateMask & (1 << 2)) != 0 && PathComponentComponents.HasComponent(targetEntity))
                    {
                        var pathComponent = PathComponentComponents[targetEntity];
                        pathComponent.WriteSnapshot(ref writer, CompressionModel, default);
                    }
//</generated>
                        
                    //<template:privatesnapshot>
                    //if ((updateMask & (1 << ##INDEX##)) != 0 && ##TYPE##Components.HasComponent(targetEntity))
                    //{
                    //    var ##TYPELOWER## = ##TYPE##Components[targetEntity];
                    //    ##TYPELOWER##.WriteSnapshot(ref writer, CompressionModel, default);
                    //}
                    //</template>
//<generated>
                    if ((updateMask & (1 << 3)) != 0 && EntityHealthComponents.HasComponent(targetEntity))
                    {
                        var entityHealth = EntityHealthComponents[targetEntity];
                        entityHealth.WriteSnapshot(ref writer, CompressionModel, default);
                    }
//</generated>
                }
                    
                    
                NativeArray<PacketArrayWrapper> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<PacketArrayWrapper>(
                    PacketArray, PacketsLength, Allocator.TempJob);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, AtomicSafetyHandle.Create());
#endif
                PacketArrayWrapper wrapper = UnsafeUtility.ReadArrayElement<PacketArrayWrapper>(array.GetUnsafePtr(), firstEntityIndex + i);
                var bytesArray = writer.AsNativeArray();
                UnsafeUtility.MemMove(wrapper.Pointer, bytesArray.GetUnsafePtr(), bytesArray.Length);
                    
                Results.TryAdd(serverNetworkedEntity.OwnerNetworkId, new PacketArrayWrapper()
                {
                    Pointer = wrapper.Pointer,
                    Length = writer.Length,
                    Allocator = Allocator.Invalid,
                    InternalId = serverNetworkedEntity.OwnerNetworkId
                });
            }
        }
    }
}