using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace ExampleGame.Shared.Physics
{
    [UpdateInGroup(typeof(TickSimulationSystemGroup))]
    [DisableAutoCreation]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public partial class TickPostPhysicsSimulationSystem : SystemBase
    {
        private EntityQuery _physicsQuery;

        protected override void OnCreate()
        {
            _physicsQuery = GetEntityQuery(
                ComponentType.ReadOnly<PhysicsWorldIndex>(),
                ComponentType.ReadWrite<PhysicsVelocity>(),
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadOnly<EntityPosition>(),
                ComponentType.ReadOnly<EntityVelocity>());

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            PostPhysicsSimulationJob job = new PostPhysicsSimulationJob()
            {
                FloatingOrigin = GetSingleton<FloatingOrigin>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(true),
                PhysicsVelocityHandle = GetComponentTypeHandle<PhysicsVelocity>(true),
                EntityPositionHandle = GetComponentTypeHandle<EntityPosition>(),
                EntityVelocityHandle = GetComponentTypeHandle<EntityVelocity>()
            };

            Dependency = job.ScheduleParallel(_physicsQuery, Dependency);
            Dependency.Complete();
        }

        [BurstCompile]
        public struct PostPhysicsSimulationJob : IJobChunk
        {
            public FloatingOrigin FloatingOrigin;

            [ReadOnly] public ComponentTypeHandle<PhysicsVelocity> PhysicsVelocityHandle;
            [ReadOnly] public ComponentTypeHandle<Translation> TranslationHandle;
            public ComponentTypeHandle<EntityPosition> EntityPositionHandle;
            public ComponentTypeHandle<EntityVelocity> EntityVelocityHandle;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var translations = chunk.GetNativeArray(TranslationHandle);
                var physicsVelocities = chunk.GetNativeArray(PhysicsVelocityHandle);
                var entityPositions = chunk.GetNativeArray(EntityPositionHandle);
                var entityVelocities = chunk.GetNativeArray(EntityVelocityHandle);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var physicsVelocity = physicsVelocities[i];
                    
                    entityVelocities[i] = new EntityVelocity()
                    {
                        Linear = GameUnits.FromUnityVector3(physicsVelocity.Linear),
                        Angular = GameUnits.FromUnityVector3(physicsVelocity.Angular)
                    };

                    translations[i] = new Translation()
                    {
                        Value = FloatingOrigin.GetUnityVector(entityPositions[i].Value)
                    };
                }
            }
        }
    }
}