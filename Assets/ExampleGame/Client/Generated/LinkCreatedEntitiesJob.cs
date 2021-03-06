using OpenNetcode.Client.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    [BurstCompile]
    public struct LinkCreatedEntities : IJob
    {
        [ReadOnly] public ComponentDataFromEntity<ServerEntity> ServerEntityComponents;
        [ReadOnly] public NativeArray<Entity> Entities;

        public NativeHashMap<int, ClientEntitySnapshot> SnapshotEntities;
        public NativeHashMap<int, ClientEntitySnapshot> ObservedEntities;

        public void Execute()
        {
            for (int i = 0; i < Entities.Length; i++)
            {
                var entity = Entities[i];
                var serverEntity = ServerEntityComponents[entity];

                if (SnapshotEntities.TryGetValue(serverEntity.ServerIndex, out var snapshotEntity))
                {
                    if (snapshotEntity.Entity.Index < 0)
                    {
                        snapshotEntity.Entity = entity;
                        SnapshotEntities[serverEntity.ServerIndex] = snapshotEntity;
                    }
                }

                if (ObservedEntities.TryGetValue(serverEntity.ServerIndex, out var observedEntity))
                {
                    if (observedEntity.Entity.Index < 0)
                    {
                        observedEntity.Entity = entity;
                        ObservedEntities[serverEntity.ServerIndex] = observedEntity;
                    }
                }
            }
        }
    }
}
