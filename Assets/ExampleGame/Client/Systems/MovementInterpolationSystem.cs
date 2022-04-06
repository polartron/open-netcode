using ExampleGame.Shared.Debugging;
using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TickSystem))]
    [DisableAutoCreation]
    public partial class MovementInterpolationSystem : SystemBase
    {
        private TickSystem _tickSystem;
        private EntityQuery _movingEntityQuery;
        private EntityQuery _pathingEntityQuery;

        private Ticker _lerpTicker;
        private int _lastTick;
        
        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();

            _movingEntityQuery = GetEntityQuery(
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<CachedTranslation>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityPosition>>(),
                ComponentType.ReadOnly<SnapshotBufferElement<EntityVelocity>>(),
                ComponentType.Exclude<SimulatedEntity>());

            _pathingEntityQuery = GetEntityQuery(
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadOnly<PathComponent>(),
                ComponentType.Exclude<SimulatedEntity>());
            
            _lerpTicker = new Ticker(TimeConfig.TicksPerSecond, 0);
            
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var floatingOrigin = GetSingleton<FloatingOrigin>();
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

            int lastReceivedSnapshotTick = GetSingleton<ClientData>().LastReceivedSnapshotTick;

            if (_lastTick != lastReceivedSnapshotTick)
            {
                _lerpTicker.SetTime(((float) lastReceivedSnapshotTick / TimeConfig.TicksPerSecond) * 1000f);
                _lastTick = lastReceivedSnapshotTick;
            }

            double tickServer = _lerpTicker.TickFloat;
            double interpolationTime = (1f / TimeConfig.SnapshotsPerSecond) * TimeConfig.TicksPerSecond;
            double tickFrom = _lerpTicker.TickFloat - interpolationTime;
            
            DebugOverlay.AddTickElement("Interpolation From", new TickElement()
            {
                Color = Color.yellow,
                Tick = (int) tickFrom
            });
            
            DebugOverlay.AddTickElement("Estimated Server Tick", new TickElement()
            {
                Color = Color.yellow,
                Tick = (int) tickServer
            });
            

            MovementInterpolationJob movementInterpolationJob = new MovementInterpolationJob()
            {
                DeltaTime = Time.DeltaTime,
                FloatingOrigin = floatingOrigin,
                TickFrom = tickFrom,
                TickServer = tickServer,
                TranslationTypeHandle = GetComponentTypeHandle<Translation>(),
                CachedTranslationTypeHandle = GetComponentTypeHandle<CachedTranslation>(),
                EntityPositionBufferTypeHandle = GetBufferTypeHandle<SnapshotBufferElement<EntityPosition>>(true),
                EntityVelocityBufferTypeHandle = GetBufferTypeHandle<SnapshotBufferElement<EntityVelocity>>(true)
            };

            Dependency = movementInterpolationJob.Schedule(_movingEntityQuery, Dependency);

            PathInterpolationJob pathInterpolationJob = new PathInterpolationJob()
            {
                FloatingOrigin = floatingOrigin,
                TickFrom = tickFrom,
                TranslationTypeHandle = GetComponentTypeHandle<Translation>(),
                PathComponentTypeHandle = GetComponentTypeHandle<PathComponent>(true)
            };

            Dependency = pathInterpolationJob.Schedule(_pathingEntityQuery, Dependency);
            Dependency.Complete();
        }

        private struct PathInterpolationJob : IJobEntityBatch
        {
            [ReadOnly] public double TickFrom;
            [ReadOnly] public FloatingOrigin FloatingOrigin;
            [ReadOnly] public ComponentTypeHandle<PathComponent> PathComponentTypeHandle;
            public ComponentTypeHandle<Translation> TranslationTypeHandle;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var pathComponents = batchInChunk.GetNativeArray(PathComponentTypeHandle);
                var translations = batchInChunk.GetNativeArray(TranslationTypeHandle);

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var pathComponent = pathComponents[i];
                    
                    float il = Mathf.InverseLerp(pathComponent.Start, pathComponent.Stop, (float) TickFrom);
                    GameUnits position = GameUnits.Lerp(pathComponent.From, pathComponent.To, il);
                    translations[i] = new Translation()
                    {
                        Value = FloatingOrigin.GetUnityVector(position)
                    };
                }
            }
        }

        [BurstCompile]
        private struct MovementInterpolationJob : IJobEntityBatch
        {
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public double TickFrom;
            [ReadOnly] public double TickServer;

            [ReadOnly] public FloatingOrigin FloatingOrigin;
            
            [ReadOnly] public BufferTypeHandle<SnapshotBufferElement<EntityPosition>> EntityPositionBufferTypeHandle;
            [ReadOnly] public BufferTypeHandle<SnapshotBufferElement<EntityVelocity>> EntityVelocityBufferTypeHandle;
            public ComponentTypeHandle<Translation> TranslationTypeHandle;
            public ComponentTypeHandle<CachedTranslation> CachedTranslationTypeHandle;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var entityPositionBuffers = batchInChunk.GetBufferAccessor(EntityPositionBufferTypeHandle);
                var entityVelocityBuffers = batchInChunk.GetBufferAccessor(EntityVelocityBufferTypeHandle);
                var translations = batchInChunk.GetNativeArray(TranslationTypeHandle);
                var cachedTranslations = batchInChunk.GetNativeArray(CachedTranslationTypeHandle);
                
                float fromIndex = (float) TickFrom / ((float) TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond);

                for (int b = 0; b < batchInChunk.Count; b++)
                {
                    var positionBuffer = entityPositionBuffers[b];
                    var velocityBuffer = entityVelocityBuffers[b];

                    SnapshotBufferElement<EntityPosition> p1 = default;
                    SnapshotBufferElement<EntityVelocity> v1 = default;
                    
                    SnapshotBufferElement<EntityPosition> p2 = default;
                    SnapshotBufferElement<EntityVelocity> v2 = default;

                    p1 = positionBuffer[(Mathf.FloorToInt(fromIndex) % TimeConfig.SnapshotsPerSecond)];
                    v1 = velocityBuffer[(Mathf.FloorToInt(fromIndex) % TimeConfig.SnapshotsPerSecond)];
                    
                    p2 = positionBuffer[(Mathf.FloorToInt(fromIndex) + 1) % TimeConfig.SnapshotsPerSecond];
                    v2 = velocityBuffer[(Mathf.FloorToInt(fromIndex) + 1) % TimeConfig.SnapshotsPerSecond];
                    
                    Debug.DrawRay(FloatingOrigin.GetUnityVector(p1.Value.Value), Vector3.up * 2f, Color.yellow);
                    Debug.DrawRay(FloatingOrigin.GetUnityVector(p2.Value.Value), Vector3.up * 2f, Color.cyan);

                    Vector3 target = FloatingOrigin.GetUnityVector(p1.Value.Value);

                    if (p2.Tick > p1.Tick)
                    {
                        double lerpFrom = TickFrom - TimeConfig.TicksPerSecond * 0.1f;
                        
                        // Interpolate
                        float t = Mathf.InverseLerp(p1.Tick, p2.Tick, (float) lerpFrom);
                        target = FloatingOrigin.GetUnityVector(GameUnits.Lerp(p1.Value.Value, p2.Value.Value, t));
                        //Debug.DrawRay(target, Vector3.up, Color.white, 5f);
                    }
                    else
                    {
                        // Extrapolate
                        
                        float t = (float) (TickFrom - p1.Tick) / TimeConfig.TicksPerSecond;
                    
                        target = Extrapolate(FloatingOrigin.GetUnityVector(p1.Value.Value),
                            v1.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t);
                        //Debug.DrawRay(target, Vector3.up, Color.blue, 5f);
                    }
                    
                    //if (p1.Tick >= (int) TickFrom && p1.Tick < TickServer)
                    //{
                    //    var fromPosition = p2;
                    //    var fromVelocity = v2;
//
                    //    float t1 = (float) (TickFrom - fromPosition.Tick) / TimeConfig.TicksPerSecond;
                    //    float t2 = (float) (TickFrom - p1.Tick) / TimeConfig.TicksPerSecond;
//
                    //    var ex1 = Extrapolate(FloatingOrigin.GetUnityVector(fromPosition.Value.Value),
                    //        fromVelocity.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t1);
//
                    //    var ex2 = Extrapolate(FloatingOrigin.GetUnityVector(p1.Value.Value),
                    //        v1.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t2);
//
                    //    float il = Mathf.InverseLerp(fromPosition.Tick, p1.Tick, (float) TickFrom);
                    //    target = Vector3.Lerp(ex1, ex2, il);
                    //}
                    //else if(p1.Tick > 0)
                    //{
                    //    float t = (float) (TickFrom - p1.Tick) / TimeConfig.TicksPerSecond;
                    //
                    //    target = Extrapolate(FloatingOrigin.GetUnityVector(p1.Value.Value),
                    //        v1.Value.Value.ToUnityVector3(), new Vector3(0f, 0f, 0f), t);
                    //}

                    if (!cachedTranslations[b].IsSet && p1.Tick > 0)
                    {
                        translations[b] = new Translation()
                        {
                            Value = target
                        };
                        
                        cachedTranslations[b] = new CachedTranslation()
                        {
                            Value = target,
                            IsSet = true
                        };
                    }
                    else if (cachedTranslations[b].IsSet)
                    {
                        Vector3 from = cachedTranslations[b].Value;
                        Vector3 result = Vector3.Lerp(from, target, DeltaTime * 2f * math.length(FloatingOrigin.GetUnityVector(v1.Value.Value)));

                        translations[b] = new Translation()
                        {
                            Value = result
                        };

                        cachedTranslations[b] = new CachedTranslation()
                        {
                            Value = result,
                            IsSet = true
                        };
                        //Debug.DrawRay(result, Vector3.up, Color.green, 5f);
                    }
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
