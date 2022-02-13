using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Coordinates;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ExampleGame.Shared.Movement.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickSimulationSystemGroup))]
    public class TickMovementSystem : SystemBase
    {
        private EntityQuery _movingEntitiesQuery;
        private EntityQuery _pathingEntitiesQuery;

        protected override void OnCreate()
        {
            _movingEntitiesQuery = GetEntityQuery(
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<EntityVelocity>(),
                ComponentType.ReadWrite<EntityPosition>(),
                ComponentType.ReadOnly<MovementConfig>(),
                ComponentType.ReadOnly<CharacterInput>());

            _pathingEntitiesQuery = GetEntityQuery(
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<EntityPosition>(),
                ComponentType.ReadOnly<PathComponent>());

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var floatingOrigin = GetSingleton<FloatingOrigin>();
            int tick = GetSingleton<TickData>().Value;

            MovementJob movementJob = new MovementJob()
            {
                floatingOrigin = floatingOrigin,
                TranslationTypeHandle = GetComponentTypeHandle<Translation>(),
                CharacterPositionTypeHandle = GetComponentTypeHandle<EntityPosition>(),
                CharacterVelocityTypeHandle = GetComponentTypeHandle<EntityVelocity>(),
                PlayerInputTypeHandle = GetComponentTypeHandle<CharacterInput>(true),
                MovementConfigTypeHandle = GetComponentTypeHandle<MovementConfig>(true),
            };

            Dependency = movementJob.Schedule(_movingEntitiesQuery, Dependency);
            Dependency.Complete();
            
            PathJob pathJob = new PathJob()
            {
                floatingOrigin = floatingOrigin,
                TranslationTypeHandle = GetComponentTypeHandle<Translation>(),
                CharacterPositionTypeHandle = GetComponentTypeHandle<EntityPosition>(),
                PathComponentTypeHandle = GetComponentTypeHandle<PathComponent>(true),
                Tick = tick
            };

            Dependency = pathJob.Schedule(_pathingEntitiesQuery, Dependency);
            Dependency.Complete();
        }

        [BurstCompile]
        private struct PathJob : IJobEntityBatch
        {
            public ComponentTypeHandle<Translation> TranslationTypeHandle;
            public ComponentTypeHandle<EntityPosition> CharacterPositionTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PathComponent> PathComponentTypeHandle;
            [ReadOnly] public FloatingOrigin floatingOrigin;
            [ReadOnly] public int Tick;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var entityPositions = batchInChunk.GetNativeArray(CharacterPositionTypeHandle);
                var translations = batchInChunk.GetNativeArray(TranslationTypeHandle);
                var pathComponents = batchInChunk.GetNativeArray(PathComponentTypeHandle);

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var entityPosition = entityPositions[i];
                    var translation = translations[i];
                    var pathComponent = pathComponents[i];

                    float il = Mathf.InverseLerp(pathComponent.Start, pathComponent.Stop, Tick);
                    entityPosition.Value = GameUnits.Lerp(pathComponent.From, pathComponent.To, il);
                    translation.Value = floatingOrigin.GetUnityVector(entityPosition.Value);
                    entityPositions[i] = entityPosition;
                    translations[i] = translation;
                }
            }
        }

        [BurstCompile]
        private struct MovementJob : IJobEntityBatch
        {
            public ComponentTypeHandle<Translation> TranslationTypeHandle;
            public ComponentTypeHandle<EntityPosition> CharacterPositionTypeHandle;
            public ComponentTypeHandle<EntityVelocity> CharacterVelocityTypeHandle;
            [ReadOnly] public ComponentTypeHandle<MovementConfig> MovementConfigTypeHandle;
            [ReadOnly] public ComponentTypeHandle<CharacterInput> PlayerInputTypeHandle;
            [ReadOnly] public FloatingOrigin floatingOrigin;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var entityPositions = batchInChunk.GetNativeArray(CharacterPositionTypeHandle);
                var entityVelocities = batchInChunk.GetNativeArray(CharacterVelocityTypeHandle);
                var translations = batchInChunk.GetNativeArray(TranslationTypeHandle);
                var characterInputs = batchInChunk.GetNativeArray(PlayerInputTypeHandle);
                var movementConfigs = batchInChunk.GetNativeArray(MovementConfigTypeHandle);

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var entityVelocity = entityVelocities[i];
                    var entityPosition = entityPositions[i];
                    var movementConfig = movementConfigs[i];
                    var translation = translations[i];
                    var playerInput = characterInputs[i];

                    Vector3 velocity = entityVelocity.Value.ToUnityVector3();
                    Vector3 position = floatingOrigin.GetUnityVector(entityPosition.Value);

                    ExampleGame.Shared.Movement.Movement.CalculateVelocity(ref velocity, movementConfig, playerInput,
                        TimeConfig.FixedDeltaTime);
                    ExampleGame.Shared.Movement.Movement.Move(ref position, velocity, TimeConfig.FixedDeltaTime);

                    entityVelocity.Value = GameUnits.FromUnityVector3(velocity);
                    entityVelocities[i] = entityVelocity;

                    entityPosition.Value = floatingOrigin.GetGameUnits(position);
                    entityPositions[i] = entityPosition;

                    translation.Value = floatingOrigin.GetUnityVector(entityPosition.Value);
                    
                    translations[i] = translation;
                }
            }
        }
    }
}