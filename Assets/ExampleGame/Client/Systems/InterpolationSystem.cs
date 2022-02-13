using ExampleGame.Client.Components;
using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Coordinates;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TickSystem))]
    [DisableAutoCreation]
    public class InterpolationSystem : SystemBase
    {
        private TickSystem _tickSystem;

        private EntityQuery _query;
        
        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();

            _query = GetEntityQuery(
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityPosition>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityVelocity>>(),
                ComponentType.Exclude<ClientEntityTag>()
                );
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var floatingOrigin = GetSingleton<FloatingOrigin>();
            var roundTripTime = GetSingleton<RoundTripTime>();
            float deltaTime = Time.DeltaTime;

            int tick = _tickSystem.Tick;
            float fraction = _tickSystem.TickFloat % 1f;
            
            //Entities.WithAll<ClientEntityTag>().ForEach((ref CachedTranslation lerpedTransform, 
            //    ref Translation translation, in EntityPosition entityPosition, in Entity entity) =>
            //{
            //    var buffer = EntityManager.GetBuffer<PredictedMove<EntityPosition, CharacterInput>>(entity);
            //    var to = buffer[tick % buffer.Length];
            //    var from = buffer[(tick - 1 + buffer.Length) % buffer.Length];
//
//
            //    Vector3 toVector = floatingOrigin.GetUnityVector(to.Prediction.Value);
            //    Vector3 fromVector = floatingOrigin.GetUnityVector(from.Prediction.Value);
            //    Debug.DrawRay(toVector, Vector3.up, Color.green, 10f);
            //    Vector3 target = Vector3.Lerp(fromVector, toVector, fraction);
            //    translation.Value = target;
            //}).WithoutBurst().Run();
            
            double rttHalf = roundTripTime.Value / 2;
            double tickFloat = _tickSystem.TickFloat;
            double tickServer = tickFloat - (rttHalf + TimeConfig.CommandBufferLengthMs) / 1000f * TimeConfig.TicksPerSecond;
            double tickFrom = tickServer - Mathf.Max(1, 1f / TimeConfig.SnapshotsPerSecond);

            PlayerInterpolationJob playerInterpolationJob = new PlayerInterpolationJob()
            {
                FloatingOrigin = floatingOrigin,
                TickFrom = tickFrom,
                TickServer = tickServer,
                TranslationTypeHandle = GetComponentTypeHandle<Translation>(),
                EntityPositionBufferTypeHandle = GetBufferTypeHandle<SnapshotBufferElement<EntityPosition>>(true),
                EntityVelocityBufferTypeHandle = GetBufferTypeHandle<SnapshotBufferElement<EntityVelocity>>(true)
            };

            Dependency = playerInterpolationJob.Schedule(_query, Dependency);
            
            Dependency.Complete();
        }

        [BurstCompile]
        private struct PlayerInterpolationJob : IJobEntityBatch
        {
            [ReadOnly] public double TickFrom;
            [ReadOnly] public double TickServer;

            [ReadOnly] public FloatingOrigin FloatingOrigin;
            
            [ReadOnly] public BufferTypeHandle<SnapshotBufferElement<EntityPosition>> EntityPositionBufferTypeHandle;
            [ReadOnly] public BufferTypeHandle<SnapshotBufferElement<EntityVelocity>> EntityVelocityBufferTypeHandle;
            public ComponentTypeHandle<Translation> TranslationTypeHandle;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var entityPositionBuffers = batchInChunk.GetBufferAccessor(EntityPositionBufferTypeHandle);
                var entityVelocityBuffers = batchInChunk.GetBufferAccessor(EntityVelocityBufferTypeHandle);
                var translations = batchInChunk.GetNativeArray(TranslationTypeHandle);

                for (int b = 0; b < batchInChunk.Count; b++)
                {
                    var positionBuffer = entityPositionBuffers[b];
                    var velocityBuffer = entityVelocityBuffers[b];

                    SnapshotBufferElement<EntityPosition> p1 = default;
                    SnapshotBufferElement<EntityVelocity> v1 = default;
                    
                    for (int i = 0; i < positionBuffer.Length; i++)
                    {
                        var p = positionBuffer[i];
                        var v = velocityBuffer[i];

                        if (p.Tick > p1.Tick)
                        {
                            p1 = p;
                            v1 = v;
                        }
                    }

                    SnapshotBufferElement<EntityPosition> p2 = default;
                    SnapshotBufferElement<EntityVelocity> v2 = default;

                    for (int i = 0; i < positionBuffer.Length; i++)
                    {
                        var p = positionBuffer[i];
                        var v = velocityBuffer[i];

                        if (p.Tick > p2.Tick && p.Tick < p1.Tick)
                        {
                            p2 = p;
                            v2 = v;
                        }
                    }

                    Vector3 target;
                    if (p1.Tick > TickFrom && p1.Tick < TickServer && v1.Tick == p1.Tick)
                    {
                        var fromPosition = p2;
                        var fromVelocity = v2;
                        
                        float t1 = (float) (TickFrom - fromPosition.Tick) / TimeConfig.TicksPerSecond;
                        float t2 = (float) (TickFrom - p1.Tick) / TimeConfig.TicksPerSecond;

                        var ex1 = Extrapolate(FloatingOrigin.GetUnityVector(fromPosition.Value.Value),
                            fromVelocity.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t1);

                        var ex2 = Extrapolate(FloatingOrigin.GetUnityVector(p1.Value.Value),
                            v1.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t2);

                        float il = Mathf.InverseLerp(fromPosition.Tick, p1.Tick, (float) TickFrom);
                        target = Vector3.Lerp(ex1, ex2, il);
                        
                        //entityPosition.Value = FloatingOrigin.GetGameUnits(target);
                    }
                    else
                    {
                        float t = (float) (TickFrom - p1.Tick) / TimeConfig.TicksPerSecond;

                        target = Extrapolate(FloatingOrigin.GetUnityVector(p1.Value.Value),
                            v1.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t);

                       // entityPosition.Value = FloatingOrigin.GetGameUnits(target);
                    }

                    translations[b] = new Translation()
                    {
                        Value = target
                    };
                }
            }
        }
        
        public static Vector3 Extrapolate(Vector3 p0, Vector3 v0, Vector3 a0, float t)
        {
            return new Vector3(Extrapolate(p0.x, v0.x, a0.x, t), Extrapolate(p0.y, v0.y, a0.y, t), Extrapolate(p0.z, v0.z, a0.z, t));
        }

        private static float Extrapolate(float p0, float v0, float a0, float t)
        {
            return p0 + v0 * t + 0.5f * a0 * t * t;
        }
    }
}
