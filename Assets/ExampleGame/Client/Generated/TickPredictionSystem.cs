using System;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderLast = true)]
    public partial class TickPredictionSystem : SystemBase
    {
        private TickSystem _tickSystem;

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            
            Entity clientEntity = GetSingleton<ClientData>().LocalPlayer;
            
            //<template:predicted>
            //if (EntityManager.HasComponent<##TYPE##>(clientEntity))
            //{
            //    var buffer = EntityManager.AddBuffer<Prediction<##TYPE##>>(clientEntity);
            //    for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            //    {
            //        buffer.Add(default);
            //    }
            //}
            //</template>
//<generated>
            if (EntityManager.HasComponent<EntityVelocity>(clientEntity))
            {
                var buffer = EntityManager.AddBuffer<Prediction<EntityVelocity>>(clientEntity);
                for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
                {
                    buffer.Add(default);
                }
            }
            if (EntityManager.HasComponent<EntityPosition>(clientEntity))
            {
                var buffer = EntityManager.AddBuffer<Prediction<EntityPosition>>(clientEntity);
                for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
                {
                    buffer.Add(default);
                }
            }
//</generated>
            
            base.OnCreate();
        }
        
        public void Rollback(Entity entity, int tick)
        {
            //<template:publicsnapshot>
            //DoRollback<##TYPE##>(entity, tick);
            //</template>
//<generated>
            DoRollback<EntityVelocity>(entity, tick);
            DoRollback<EntityPosition>(entity, tick);
            DoRollback<PathComponent>(entity, tick);
//</generated>
        }
        
        private void DoRollback<T>(in Entity entity, int tick) where T : unmanaged, IComponentData
        {
            if (!EntityManager.HasComponent<SnapshotBufferElement<T>>(entity))
                return;

            var buffer = EntityManager.GetBuffer<SnapshotBufferElement<T>>(entity);
            var component = buffer[tick % buffer.Length];

            if (component.Tick == tick)
            {
                EntityManager.SetComponentData(entity, component.Value);
            }
        }

        private bool IsPredictionError<T>(in ClientData clientData, in Entity entity, out int errorTick) where T : unmanaged, IEquatable<T>
        {
            var resultBuffer = EntityManager.GetBuffer<SnapshotBufferElement<T>>(entity);
            var resultElement = resultBuffer[clientData.LastReceivedSnapshotTick % resultBuffer.Length];
            errorTick = resultElement.Tick;

            if (resultElement.Tick != clientData.LastReceivedSnapshotTick)
            {
                return false;
            }
            
            var predictions = EntityManager.GetBuffer<Prediction<T>>(entity);
            int predictedIndex = resultElement.Tick % predictions.Length;
            var predicted = predictions[predictedIndex];
            
            if (resultElement.Tick != predicted.Tick)
            {
                return false;
            }

            if (!predicted.Value.Equals(resultElement.Value))
            {
                predictions[predictedIndex] = new Prediction<T>()
                {
                    Value = resultElement.Value,
                    Tick = predicted.Tick
                };
                
                return true;
            }
            
            return false;
        }

        protected override void OnUpdate()
        {
            var clientData = GetSingleton<ClientData>();
            int tick = GetSingleton<TickData>().Value;
            Entity clientEntity = clientData.LocalPlayer;

            if (clientEntity == Entity.Null)
                return;

            bool rollback = false;
            int rollbackFromTick = 0;
            
            //<template:predicted>
            //if (IsPredictionError<##TYPE##>(clientData, clientEntity, out int ##TYPELOWER##ErrorTick))
            //{
            //    rollbackFromTick = ##TYPELOWER##ErrorTick;
            //    rollback = true;
            //}
            //</template>
//<generated>
            if (IsPredictionError<EntityVelocity>(clientData, clientEntity, out int entityVelocityErrorTick))
            {
                rollbackFromTick = entityVelocityErrorTick;
                rollback = true;
            }
            if (IsPredictionError<EntityPosition>(clientData, clientEntity, out int entityPositionErrorTick))
            {
                rollbackFromTick = entityPositionErrorTick;
                rollback = true;
            }
//</generated>
            
            int predictedIndex = rollbackFromTick % TimeConfig.TicksPerSecond;

            if (rollback)
            {
                Rollback(clientEntity, rollbackFromTick);
                int rollbackTicks = tick - rollbackFromTick;
                
                //<template:input>
                //var ##TYPELOWER##Cached = EntityManager.GetComponentData<##TYPE##>(clientEntity);
                //var ##TYPELOWER##Saved = EntityManager.GetBuffer<SavedInput<##TYPE##>>(clientEntity);
                //</template>
//<generated>
                var weaponInputCached = EntityManager.GetComponentData<WeaponInput>(clientEntity);
                var weaponInputSaved = EntityManager.GetBuffer<SavedInput<WeaponInput>>(clientEntity);
                var movementInputCached = EntityManager.GetComponentData<MovementInput>(clientEntity);
                var movementInputSaved = EntityManager.GetBuffer<SavedInput<MovementInput>>(clientEntity);
//</generated>
                
                for (int i = 1; i < rollbackTicks; i++)
                {
                    SetSingleton(new TickData()
                    {
                        Value = rollbackFromTick + i
                    });
                    
                    int index = (predictedIndex + i) % TimeConfig.TicksPerSecond;
                    
                    //<template:input>
                    //EntityManager.SetComponentData(clientEntity, ##TYPELOWER##Saved[index].Value);
                    //</template>
//<generated>
                    EntityManager.SetComponentData(clientEntity, weaponInputSaved[index].Value);
                    EntityManager.SetComponentData(clientEntity, movementInputSaved[index].Value);
//</generated>
                    
                    _tickSystem.StepSimulation();
                }
                
                SetSingleton(new TickData()
                {
                    Value = tick
                });
                
                //<template:input>
                //EntityManager.SetComponentData(clientEntity, ##TYPELOWER##Cached);
                //</template>
//<generated>
                EntityManager.SetComponentData(clientEntity, weaponInputCached);
                EntityManager.SetComponentData(clientEntity, movementInputCached);
//</generated>
            }
        }
    }
}


