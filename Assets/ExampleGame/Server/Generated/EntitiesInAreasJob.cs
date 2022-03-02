using OpenNetcode.Server.Components;
using OpenNetcode.Shared.Components;
using Shared.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [BurstCompile]
    struct AddEntitiesToAreas : IJobEntityBatch
    {
        [ReadOnly] public ComponentTypeHandle<SpatialHash> SpatialHashHandle;
        [ReadOnly] public ComponentTypeHandle<NetworkedPrefab> NetworkedPrefabHandle;
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public NativeHashSet<int> ActiveAreas;
        [WriteOnly] public NativeMultiHashMap<int, ServerEntitySnapshot>.ParallelWriter EntitiesInAreas;

        //<template:publicsnapshot>
        //[ReadOnly] public ComponentDataFromEntity<##TYPE##> ##TYPE##Components;
        //</template>
//<generated>
        [ReadOnly] public ComponentDataFromEntity<EntityPosition> EntityPositionComponents;
        [ReadOnly] public ComponentDataFromEntity<EntityVelocity> EntityVelocityComponents;
        [ReadOnly] public ComponentDataFromEntity<PathComponent> PathComponentComponents;
//</generated>
        //<template:publicevent>
        //[ReadOnly] public BufferFromEntity<##TYPE##> ##TYPE##BufferFromEntity;
        //</template>
//<generated>
        [ReadOnly] public BufferFromEntity<BumpEvent> BumpEventBufferFromEntity;
//</generated>
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var networkedPrefabs = batchInChunk.GetNativeArray(NetworkedPrefabHandle);
            var entities = batchInChunk.GetNativeArray(EntityTypeHandle);
            var spatialHashes = batchInChunk.GetNativeArray(SpatialHashHandle);

            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var networkedPrefab = networkedPrefabs[i];
                var entity = entities[i];
                var spatialHash = spatialHashes[i];
                    
                ServerEntitySnapshot serverEntitySnapshot = new ServerEntitySnapshot();
    
                serverEntitySnapshot.Entity = entity;
                serverEntitySnapshot.PrefabType = networkedPrefab.Index;

                int componentMask = 0;

                //<template:publicsnapshot>
                //if (##TYPE##Components.HasComponent(entity))
                //{
                //    componentMask = componentMask | (1 << ##INDEX##);
                //}
                //</template>
//<generated>
                if (EntityPositionComponents.HasComponent(entity))
                {
                    componentMask = componentMask | (1 << 0);
                }
                if (EntityVelocityComponents.HasComponent(entity))
                {
                    componentMask = componentMask | (1 << 1);
                }
                if (PathComponentComponents.HasComponent(entity))
                {
                    componentMask = componentMask | (1 << 2);
                }
//</generated>
                serverEntitySnapshot.ComponentMask = componentMask;

                int eventMask = 0;

                //<template:publicevent>
                //if (##TYPE##BufferFromEntity.HasComponent(entity) && ##TYPE##BufferFromEntity[entity].Length > 0)
                //{
                //    eventMask = eventMask | (1 << ##INDEX##);
                //}
                //</template>
//<generated>
                if (BumpEventBufferFromEntity.HasComponent(entity) && BumpEventBufferFromEntity[entity].Length > 0)
                {
                    eventMask = eventMask | (1 << 0);
                }
//</generated>

                serverEntitySnapshot.EventMask = eventMask;

                // Add entities to be included in the 8 areas around this one
                // XXX
                // X X
                // XXX

                if (ActiveAreas.Contains(spatialHash.h0)) EntitiesInAreas.Add(spatialHash.h0, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h1)) EntitiesInAreas.Add(spatialHash.h1, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h2)) EntitiesInAreas.Add(spatialHash.h2, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h3)) EntitiesInAreas.Add(spatialHash.h3, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h4)) EntitiesInAreas.Add(spatialHash.h4, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h5)) EntitiesInAreas.Add(spatialHash.h5, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h6)) EntitiesInAreas.Add(spatialHash.h6, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h7)) EntitiesInAreas.Add(spatialHash.h7, serverEntitySnapshot);
                if (ActiveAreas.Contains(spatialHash.h8)) EntitiesInAreas.Add(spatialHash.h8, serverEntitySnapshot);
            }
        }
    }
}