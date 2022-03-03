using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

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
        private EntityQuery _entityVelocityQuery;
        private EntityQuery _pathComponentQuery;
//</generated>
        //<template:privatesnapshot>
        //private EntityQuery _##TYPELOWER##Query;
        //</template>
//<generated>
        private EntityQuery _entityHealthQuery;
//</generated>
        //<template:publicevent>
        //private EntityQuery _##TYPELOWER##Query;
        //</template>
//<generated>
        private EntityQuery _bumpEventQuery;
//</generated>

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            
            //<template:publicsnapshot>
            //_##TYPELOWER##Query = GetEntityQuery(
            //    ComponentType.Exclude<Prediction<##TYPE##>>(),
            //    ComponentType.ReadOnly<SnapshotBufferElement<##TYPE##>>(),
            //    ComponentType.ReadWrite<##TYPE##>());
            //</template>
//<generated>
            _entityPositionQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<EntityPosition>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityPosition>>(),
                ComponentType.ReadWrite<EntityPosition>());
            _entityVelocityQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<EntityVelocity>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityVelocity>>(),
                ComponentType.ReadWrite<EntityVelocity>());
            _pathComponentQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<PathComponent>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<PathComponent>>(),
                ComponentType.ReadWrite<PathComponent>());
//</generated>
            //<template:privatesnapshot>
            //_##TYPELOWER##Query = GetEntityQuery(
            //    ComponentType.Exclude<Prediction<##TYPE##>>(),
            //    ComponentType.ReadOnly<SnapshotBufferElement<##TYPE##>>(),
            //    ComponentType.ReadWrite<##TYPE##>());
            //</template>
//<generated>
            _entityHealthQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<EntityHealth>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityHealth>>(),
                ComponentType.ReadWrite<EntityHealth>());
//</generated>

            //<template:publicevent>
            //_##TYPELOWER##Query = GetEntityQuery(
            //    ComponentType.ReadOnly<SnapshotBufferElement<##TYPE##>>(),
            //    ComponentType.ReadWrite<##TYPE##>());
            //</template>
//<generated>
            _bumpEventQuery = GetEntityQuery(
                ComponentType.ReadOnly<SnapshotBufferElement<BumpEvent>>(),
                ComponentType.ReadWrite<BumpEvent>());
//</generated>

            
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
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityPosition>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityPosition>()
            };
            Dependency = entityPositionJob.ScheduleParallel(_entityPositionQuery, Dependency);
            ApplyFromBufferJob<EntityVelocity> entityVelocityJob = new ApplyFromBufferJob<EntityVelocity>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityVelocity>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityVelocity>()
            };
            Dependency = entityVelocityJob.ScheduleParallel(_entityVelocityQuery, Dependency);
            ApplyFromBufferJob<PathComponent> pathComponentJob = new ApplyFromBufferJob<PathComponent>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<PathComponent>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<PathComponent>()
            };
            Dependency = pathComponentJob.ScheduleParallel(_pathComponentQuery, Dependency);
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
//<generated>
            ApplyFromBufferJob<EntityHealth> entityHealthJob = new ApplyFromBufferJob<EntityHealth>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityHealth>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityHealth>()
            };
            Dependency = entityHealthJob.ScheduleParallel(_entityHealthQuery, Dependency);
//</generated>

            //<template:publicevent>
            //AddEventFromBufferJob<##TYPE##> ##TYPELOWER##Job = new AddEventFromBufferJob<##TYPE##>()
            //{
            //    Tick = (int) tickFrom,
            //    SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<##TYPE##>>(true),
            //    BufferFromEntity = GetBufferTypeHandle<##TYPE##>()
            //};
            //Dependency = ##TYPELOWER##Job.ScheduleParallel(_##TYPELOWER##Query, Dependency);
            //</template>
//<generated>
            AddEventFromBufferJob<BumpEvent> bumpEventJob = new AddEventFromBufferJob<BumpEvent>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<BumpEvent>>(true),
                BufferFromEntity = GetBufferTypeHandle<BumpEvent>()
            };
            Dependency = bumpEventJob.ScheduleParallel(_bumpEventQuery, Dependency);
//</generated>

            Dependency.Complete();
        }

        [BurstCompile]
        private struct ApplyFromBufferJob<T> : IJobChunk where T : unmanaged, IComponentData
        {
            public int Tick;
            public ComponentTypeHandle<T> ComponentDataFromEntity;
            public BufferTypeHandle<SnapshotBufferElement<T>> SnapshotBufferFromEntity;
        
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var components = chunk.GetNativeArray(ComponentDataFromEntity);
                var snapshotBuffers = chunk.GetBufferAccessor(SnapshotBufferFromEntity);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var buffer = snapshotBuffers[i];
                    var element = buffer[Tick % TimeConfig.SnapshotsPerSecond];
                    if (element.Tick == Tick)
                    {
                        components[i] = element.Value;
                    }
                }
            }
        }
        
        [BurstCompile]
        private struct AddEventFromBufferJob<T> : IJobChunk where T : unmanaged, IBufferElementData
        {
            public int Tick;
            public BufferTypeHandle<T> BufferFromEntity;
            public BufferTypeHandle<SnapshotBufferElement<T>> SnapshotBufferFromEntity;
        
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var buffers = chunk.GetBufferAccessor(BufferFromEntity);
                var snapshotBuffers = chunk.GetBufferAccessor(SnapshotBufferFromEntity);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var buffer = snapshotBuffers[i];
                    var element = buffer[Tick % TimeConfig.SnapshotsPerSecond];
                    
                    if (element.Tick == Tick)
                    {
                        buffers[i].Add(element.Value);
                    }
                }
            }
        }
    }
}
