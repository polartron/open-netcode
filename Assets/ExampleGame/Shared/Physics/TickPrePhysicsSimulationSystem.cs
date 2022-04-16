using ExampleGame.Shared.Movement.Components;
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
    [UpdateBefore(typeof(BuildPhysicsWorld))]
    public partial class TickPrePhysicsSimulationSystem : SystemBase
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
            PrePhysicsSimulationJob job = new PrePhysicsSimulationJob()
            {
                FloatingOrigin = GetSingleton<FloatingOrigin>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(),
                PhysicsVelocityHandle = GetComponentTypeHandle<PhysicsVelocity>(),
                EntityPositionHandle = GetComponentTypeHandle<EntityPosition>(true),
                EntityVelocityHandle = GetComponentTypeHandle<EntityVelocity>(true)
            };

            Dependency = job.ScheduleParallel(_physicsQuery, Dependency);
            Dependency.Complete();
        }

        [BurstCompile]
        public struct PrePhysicsSimulationJob : IJobChunk
        {
            public FloatingOrigin FloatingOrigin;

            public ComponentTypeHandle<PhysicsVelocity> PhysicsVelocityHandle;
            public ComponentTypeHandle<Translation> TranslationHandle;
            [ReadOnly] public ComponentTypeHandle<EntityPosition> EntityPositionHandle;
            [ReadOnly] public ComponentTypeHandle<EntityVelocity> EntityVelocityHandle;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var translations = chunk.GetNativeArray(TranslationHandle);
                var physicsVelocities = chunk.GetNativeArray(PhysicsVelocityHandle);
                var entityPositions = chunk.GetNativeArray(EntityPositionHandle);
                var entityVelocities = chunk.GetNativeArray(EntityVelocityHandle);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var entityVelocity = entityVelocities[i];

                    physicsVelocities[i] = new PhysicsVelocity()
                    {
                        Linear = entityVelocity.Linear.ToUnityVector3(),
                        Angular = entityVelocity.Angular.ToUnityVector3()
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