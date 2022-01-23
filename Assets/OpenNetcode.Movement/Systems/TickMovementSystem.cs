using System;
using OpenNetcode.Movement.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared;
using Shared.Coordinates;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace OpenNetcode.Movement.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickSimulationSystemGroup))]
    public class TickMovementSystem : SystemBase
    {
        private EntityQuery _movingEntitiesQuery;

        protected override void OnCreate()
        {
            _movingEntitiesQuery = GetEntityQuery(
                ComponentType.ReadWrite<Translation>(), 
                ComponentType.ReadWrite<EntityVelocity>(),
                ComponentType.ReadWrite<EntityPosition>(),
                ComponentType.ReadOnly<MovementConfig>(),
                ComponentType.ReadOnly<CharacterInput>());
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var floatingOrigin = GetSingleton<FloatingOrigin>();
            
            MovementJob job = new MovementJob()
            {
                floatingOrigin = floatingOrigin,
                TranslationTypeHandle = GetComponentTypeHandle<Translation>(),
                CharacterPositionTypeHandle = GetComponentTypeHandle<EntityPosition>(),
                CharacterVelocityTypeHandle = GetComponentTypeHandle<EntityVelocity>(),
                PlayerInputTypeHandle = GetComponentTypeHandle<CharacterInput>(true),
                MovementConfigTypeHandle = GetComponentTypeHandle<MovementConfig>(true),
            };

            Dependency = job.Schedule(_movingEntitiesQuery, Dependency);
            Dependency.Complete();
        }

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

                    Movement.CalculateVelocity(ref velocity, movementConfig, playerInput, TimeConfig.FixedDeltaTime);
                    Movement.Move(ref position, velocity, TimeConfig.FixedDeltaTime);

                    entityVelocity.Value = GameUnits.FromUnityVector3(velocity);
                    entityVelocities[i] = entityVelocity;

                    entityPosition.Value = floatingOrigin.GetGameUnits(position);
                    entityPositions[i] = entityPosition;
                    
                    translation.Value = floatingOrigin.GetUnityVector(entityPosition.Value);;
                    translations[i] = translation;
                }
            }
        }
    }
}