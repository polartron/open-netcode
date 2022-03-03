using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Client.Generated
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderLast = true)]
    [DisableAutoCreation]
    public class TickApplySnapshotSystem : SystemBase
    {
        private TickSystem _tickSystem;
        
        //<template:publicsnapshot>
        //private EntityQuery _##TYPELOWER##Query;
        //</template>
//<generated>
        private EntityQuery _entityPositionQuery;
//</generated>
        //<template:private>
        //private EntityQuery _##TYPELOWER##Query;
        //</template>

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            
            //<template:publicsnapshot>
            //_entityPositionQuery = GetEntityQuery(
            //    ComponentType.Exclude<Prediction<##TYPE##>>(),
            //    ComponentType.ReadOnly<SnapshotBufferElement<##TYPE##>>(),
            //    ComponentType.ReadWrite<##TYPE##>());
            //</template>
//<generated>
            _entityPositionQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<EntityPosition>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityPosition>>(),
                ComponentType.ReadWrite<EntityPosition>());
//<generated>
            //<template:privatesnapshot>
            //_entityPositionQuery = GetEntityQuery(
            //    ComponentType.Exclude<Prediction<##TYPE##>>(),
            //    ComponentType.ReadOnly<SnapshotBufferElement<##TYPE##>>(),
            //    ComponentType.ReadWrite<##TYPE##>());
            //</template>
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var roundTripTime = GetSingleton<RoundTripTime>();
            double rttHalf = roundTripTime.Value / 2;
            double tickFloat = _tickSystem.TickFloat;
            double tickServer = tickFloat - (rttHalf + TimeConfig.CommandBufferLengthMs) / 1000f * TimeConfig.TicksPerSecond;
            double tickFrom = tickServer - TimeConfig.TicksPerSecond * Mathf.Min(0.1f, 1f / TimeConfig.SnapshotsPerSecond);

            //<template:publicsnapshot>
            //ApplyFromBufferJob<##TYPE##> ##TYPELOWER##Job = new ApplyFromBufferJob<##TYPE##>()
            //{
            //    Tick = (int) tickFrom,
            //    BufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<##TYPE##>>(true),
            //    ComponentDataFromEntity = GetComponentTypeHandle<##TYPE##>()
            //};
            //Dependency = ##TYPELOWER##Job.ScheduleParallel(_##TYPELOWER##Query, Dependency);
            //</template>
//<generated>
            ApplyFromBufferJob<EntityPosition> entityPositionJob = new ApplyFromBufferJob<EntityPosition>()
            {
                Tick = (int) tickFrom,
                BufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityPosition>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityPosition>()
            };
            Dependency = entityPositionJob.ScheduleParallel(_entityPositionQuery, Dependency);
//</generated>
            //<template:privatesnapshot>
            //ApplyFromBufferJob<##TYPE##> ##TYPELOWER##Job = new ApplyFromBufferJob<##TYPE##>()
            //{
            //    Tick = (int) tickFrom,
            //    BufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<##TYPE##>>(true),
            //    ComponentDataFromEntity = GetComponentTypeHandle<##TYPE##>()
            //};
            //Dependency = ##TYPELOWER##Job.ScheduleParallel(_##TYPELOWER##Query, Dependency);
            //</template>

            Dependency.Complete();
        }

        [BurstCompile]
        private struct ApplyFromBufferJob<T> : IJobChunk where T : unmanaged, IComponentData
        {
            public int Tick;
            public ComponentTypeHandle<T> ComponentDataFromEntity;
            public BufferTypeHandle<SnapshotBufferElement<T>> BufferFromEntity;
        
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var components = chunk.GetNativeArray(ComponentDataFromEntity);
                var buffers = chunk.GetBufferAccessor(BufferFromEntity);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var buffer = buffers[i];
                    var element = buffer[Tick % TimeConfig.SnapshotsPerSecond];
                    if (element.Tick == Tick)
                    {
                        components[i] = element.Value;
                    }
                }
            }
        }
    }
}
