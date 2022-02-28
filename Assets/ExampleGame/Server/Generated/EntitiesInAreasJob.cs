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
        //</events>
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
                //</events>
//<generated>
                if (BumpEventBufferFromEntity.HasComponent(entity) && BumpEventBufferFromEntity[entity].Length > 0)
                {
                    eventMask = eventMask | (1 << 0);
                }
//</generated>

                serverEntitySnapshot.EventMask = eventMask;

                int h0 = spatialHash.Value;
                if (ActiveAreas.Contains(h0)) EntitiesInAreas.Add(h0, serverEntitySnapshot);
                
                // Add entities to be included in the 8 areas around this one
                // XXX
                // X X
                // XXX
                
                int h1 = h0 + 1;
                int h2 = h0 - 1;
                int h3 = h0 + SpatialHashing.HashSegments;
                int h4 = h0 - SpatialHashing.HashSegments;
                int h5 = h3 + 1;
                int h6 = h3 - 1;
                int h7 = h0 + 1 - SpatialHashing.HashSegments;
                int h8 = h0 - 1 - SpatialHashing.HashSegments;
                
                if (ActiveAreas.Contains(h1)) EntitiesInAreas.Add(h1, serverEntitySnapshot);
                if (ActiveAreas.Contains(h2)) EntitiesInAreas.Add(h2, serverEntitySnapshot);
                if (ActiveAreas.Contains(h3)) EntitiesInAreas.Add(h3, serverEntitySnapshot);
                if (ActiveAreas.Contains(h4)) EntitiesInAreas.Add(h4, serverEntitySnapshot);
                if (ActiveAreas.Contains(h5)) EntitiesInAreas.Add(h5, serverEntitySnapshot);
                if (ActiveAreas.Contains(h6)) EntitiesInAreas.Add(h6, serverEntitySnapshot);
                if (ActiveAreas.Contains(h7)) EntitiesInAreas.Add(h7, serverEntitySnapshot);
                if (ActiveAreas.Contains(h8)) EntitiesInAreas.Add(h8, serverEntitySnapshot);
            }
        }
    }
}