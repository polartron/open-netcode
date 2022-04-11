using System;
using System.Collections.Generic;
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
using OpenNetcode.Shared.Components;
using Unity.Collections;

//</generated>

namespace Client.Generated
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderLast = true)]
    [DisableAutoCreation]
    public partial class TickApplySnapshotSystem : SystemBase
    {
        private TickSystem _tickSystem;
        
        //<template:publicsnapshot>
        //private EntityQuery _##TYPELOWER##Query;
        //</template>
//<generated>
        private EntityQuery _entityVelocityQuery;
        private EntityQuery _entityPositionQuery;
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

        private NativeHashMap<Entity, EntityPosition> _entityPositionUpdates;
        private NativeHashMap<Entity, EntityVelocity> _entityVelocityUpdates;
        private NativeHashMap<Entity, PathComponent> _pathComponentUpdates;
        private NativeHashMap<Entity, EntityHealth> _entityHealthUpdates;
        private NativeHashMap<Entity, BumpEvent> _bumpEventUpdates;

        private Dictionary<Entity, Action<EntityPosition>> _entityPositionUpdateListeners = new Dictionary<Entity, Action<EntityPosition>>();
        private Dictionary<Entity, Action<EntityVelocity>> _entityVelocityUpdateListeners = new Dictionary<Entity, Action<EntityVelocity>>();
        private Dictionary<Entity, Action<PathComponent>> _pathComponentUpdateListeners = new Dictionary<Entity, Action<PathComponent>>();
        private Dictionary<Entity, Action<EntityHealth>> _entityHealthUpdateListeners = new Dictionary<Entity, Action<EntityHealth>>();
        private Dictionary<Entity, Action<BumpEvent>> _bumpEventUpdateListeners = new Dictionary<Entity, Action<BumpEvent>>();

        public void RegisterListener<T>(Entity entity, Func<T> listener) where T : IComponentData
        {
            if (typeof(T) == typeof(EntityPosition))
            {
                if(!_entityPositionUpdateListeners.ContainsKey(entity))
                    _entityPositionUpdateListeners.Add(entity, default);
                
                _entityPositionUpdateListeners[entity] += position =>
                {
                    listener.Invoke();
                };
            }
            else if (typeof(T) == typeof(EntityVelocity))
            {
                if(!_entityVelocityUpdateListeners.ContainsKey(entity))
                    _entityVelocityUpdateListeners.Add(entity, default);
                
                _entityVelocityUpdateListeners[entity] += position =>
                {
                    listener.Invoke();
                };
            }
            else if (typeof(T) == typeof(PathComponent))
            {
                if(!_pathComponentUpdateListeners.ContainsKey(entity))
                    _pathComponentUpdateListeners.Add(entity, default);
                
                _pathComponentUpdateListeners[entity] += position =>
                {
                    listener.Invoke();
                };
            }
            else if (typeof(T) == typeof(EntityHealth))
            {
                if(!_entityHealthUpdateListeners.ContainsKey(entity))
                    _entityHealthUpdateListeners.Add(entity, default);
                
                _entityHealthUpdateListeners[entity] += position =>
                {
                    listener.Invoke();
                };
            }
            else if (typeof(T) == typeof(BumpEvent))
            {
                if(!_bumpEventUpdateListeners.ContainsKey(entity))
                    _bumpEventUpdateListeners.Add(entity, default);
                
                _bumpEventUpdateListeners[entity] += position =>
                {
                    listener.Invoke();
                };
            }
        }
        
        protected override void OnDestroy()
        {
            _entityPositionUpdates.Dispose();
            _entityVelocityUpdates.Dispose();
            base.OnDestroy();
        }

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
            _entityVelocityQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<EntityVelocity>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityVelocity>>(),
                ComponentType.ReadWrite<EntityVelocity>());
            _entityPositionQuery = GetEntityQuery(
                ComponentType.Exclude<Prediction<EntityPosition>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityPosition>>(),
                ComponentType.ReadWrite<EntityPosition>());
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


            _entityPositionUpdates = new NativeHashMap<Entity, EntityPosition>(1000, Allocator.Persistent);
            _entityVelocityUpdates = new NativeHashMap<Entity, EntityVelocity>(1000, Allocator.Persistent);
            _pathComponentUpdates = new NativeHashMap<Entity, PathComponent>(1000, Allocator.Persistent);
            _entityHealthUpdates = new NativeHashMap<Entity, EntityHealth>(1000, Allocator.Persistent);
            _bumpEventUpdates = new NativeHashMap<Entity, BumpEvent>(1000, Allocator.Persistent);

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            _entityPositionUpdates.Clear();
            _entityVelocityUpdates.Clear();
            _pathComponentUpdates.Clear();
            _entityHealthUpdates.Clear();
            
            double rttHalf = _tickSystem.RttHalf / 2;
            double tickFloat = _tickSystem.TickFloat;
            double tickServer = tickFloat - (rttHalf + TimeConfig.CommandBufferLengthMs) / 1000f * TimeConfig.TicksPerSecond;
            double tickFrom = tickServer - TimeConfig.TicksPerSecond * Mathf.Min(0.1f, 1f / TimeConfig.SnapshotsPerSecond);

            //<template:publicsnapshot>
            //ApplyFromBufferJob<##TYPE##> ##TYPELOWER##Job = new ApplyFromBufferJob<##TYPE##>()
            //{
            //    Tick = (int) tickFrom,
            //    SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<##TYPE##>>(true),
            //    ComponentDataFromEntity = GetComponentTypeHandle<##TYPE##>()
            //};
            //Dependency = ##TYPELOWER##Job.ScheduleParallel(_##TYPELOWER##Query, Dependency);
            //</template>
//<generated>
            ApplyFromBufferJob<EntityVelocity> entityVelocityJob = new ApplyFromBufferJob<EntityVelocity>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityVelocity>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityVelocity>(),
                Updates = _entityVelocityUpdates.AsParallelWriter(),
                EntityTypeHandle = GetEntityTypeHandle()
            };
            Dependency = entityVelocityJob.ScheduleParallel(_entityVelocityQuery, Dependency);
            ApplyFromBufferJob<EntityPosition> entityPositionJob = new ApplyFromBufferJob<EntityPosition>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityPosition>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityPosition>(),
                Updates = _entityPositionUpdates.AsParallelWriter(),
                EntityTypeHandle = GetEntityTypeHandle()
            };
            Dependency = entityPositionJob.ScheduleParallel(_entityPositionQuery, Dependency);
            ApplyFromBufferJob<PathComponent> pathComponentJob = new ApplyFromBufferJob<PathComponent>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<PathComponent>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<PathComponent>(),
                Updates = _pathComponentUpdates.AsParallelWriter(),
                EntityTypeHandle = GetEntityTypeHandle()
            };
            Dependency = pathComponentJob.ScheduleParallel(_pathComponentQuery, Dependency);
//</generated>
            //<template:privatesnapshot>
            //ApplyFromBufferJob<##TYPE##> ##TYPELOWER##Job = new ApplyFromBufferJob<##TYPE##>()
            //{
            //    Tick = (int) tickFrom,
            //    SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<##TYPE##>>(true),
            //    ComponentDataFromEntity = GetComponentTypeHandle<##TYPE##>(),
            //    Updates = _##TYPELOWERUpdates
            //};
            //Dependency = ##TYPELOWER##Job.ScheduleParallel(_##TYPELOWER##Query, Dependency);
            //</template>
//<generated>
            ApplyFromBufferJob<EntityHealth> entityHealthJob = new ApplyFromBufferJob<EntityHealth>()
            {
                Tick = (int) tickFrom,
                SnapshotBufferFromEntity = GetBufferTypeHandle<SnapshotBufferElement<EntityHealth>>(true),
                ComponentDataFromEntity = GetComponentTypeHandle<EntityHealth>(),
                Updates = _entityHealthUpdates.AsParallelWriter(),
                EntityTypeHandle = GetEntityTypeHandle()
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
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;
            public BufferTypeHandle<SnapshotBufferElement<T>> SnapshotBufferFromEntity;
            public NativeHashMap<Entity, T>.ParallelWriter Updates;
        
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var components = chunk.GetNativeArray(ComponentDataFromEntity);
                var snapshotBuffers = chunk.GetBufferAccessor(SnapshotBufferFromEntity);
                var entities = chunk.GetNativeArray(EntityTypeHandle);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var buffer = snapshotBuffers[i];
                    var element = buffer[Tick % TimeConfig.SnapshotsPerSecond];
                    var entity = entities[i];
                    if (element.Tick == Tick)
                    {
                        components[i] = element.Value;
                        Updates.TryAdd(entity, element.Value);
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
