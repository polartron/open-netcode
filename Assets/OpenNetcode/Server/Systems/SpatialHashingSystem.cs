using OpenNetcode.Server.Components;
using OpenNetcode.Shared.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace OpenNetcode.Server.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickSimulationSystemGroup), OrderFirst = true)]
    public class SpatialHashingSystem : SystemBase
    {
        private EntityQuery _entityQuery;
        
        protected override void OnCreate()
        {
            _entityQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadWrite<SpatialHash>());
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            SpatialHashingGridJob spatialHashingGridJob = new SpatialHashingGridJob()
            {
                TranslationsHandle = GetComponentTypeHandle<Translation>(true),
                SpatialHashHandle = GetComponentTypeHandle<SpatialHash>()
            };

            Dependency = spatialHashingGridJob.Schedule(_entityQuery, Dependency);
            Dependency.Complete();
        }
        
        [BurstCompile]
        private struct SpatialHashingGridJob : IJobEntityBatch
        {
            private static readonly int AreaSize = 10;
            private static readonly int HashSegments = 1000;
            
            [ReadOnly] public ComponentTypeHandle<Translation> TranslationsHandle;
            public ComponentTypeHandle<SpatialHash> SpatialHashHandle;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var translations = batchInChunk.GetNativeArray(TranslationsHandle);
                var spatialHashes = batchInChunk.GetNativeArray(SpatialHashHandle);

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var translation = translations[i].Value;
                    spatialHashes[i] = new SpatialHash()
                    {
                        Value = (int) (math.floor(translation.x / AreaSize) + HashSegments * math.floor(translation.z / AreaSize))
                    };
                }
            }
        }
    }
}
