using System;
using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace ExampleGame.Client.Generated
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderLast = true)]
    public class TickPredictionSystem : SystemBase
    {
        private TickSystem _tickSystem;

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            base.OnCreate();
        }
        
        public void Rollback(Entity entity, int tick)
        {
            //<template>
            //DoRollback<##TYPE##>(entity, tick);
            //</template>
//<generated>
            DoRollback<EntityPosition>(entity, tick);
            DoRollback<EntityVelocity>(entity, tick);
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

        private bool IsPredictionError<T>(in ClientData clientData, in Entity entity, out DynamicBuffer<Prediction<T>> predictions, out SnapshotBufferElement<T> result) where T : unmanaged, INetworkedComponent
        {
            predictions = default;
            var resultBuffer = EntityManager.GetBuffer<SnapshotBufferElement<T>>(entity);
            var resultElement = resultBuffer[clientData.LastReceivedSnapshotTick % resultBuffer.Length];
            result = resultElement;

            if (resultElement.Tick != clientData.LastReceivedSnapshotTick)
            {
                return false;
            }
            
            predictions = EntityManager.GetBuffer<Prediction<T>>(entity);
            int predictedIndex = resultElement.Tick % predictions.Length;
            var predicted = predictions[predictedIndex];
            
            if (resultElement.Tick != predicted.Tick)
            {
                return false;
            }

            if (!predicted.Compare(resultElement.Value))
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
            
            if (IsPredictionError<EntityPosition>(clientData, clientEntity, out var predictions, out var result))
            {
                rollbackFromTick = result.Tick;
                rollback = true;
            }
            
            int predictedIndex = rollbackFromTick % TimeConfig.TicksPerSecond;

            if (rollback)
            {
                Rollback(clientEntity, rollbackFromTick);
                
                int rollbackTicks = tick - rollbackFromTick;
                
                var characterInputCached = EntityManager.GetComponentData<CharacterInput>(clientEntity);
                var characterInputSaved = EntityManager.GetBuffer<SavedInput<CharacterInput>>(clientEntity);
                
                for (int i = 1; i < rollbackTicks; i++)
                {
                    SetSingleton(new TickData()
                    {
                        Value = rollbackFromTick + i
                    });
                    
                    int index = (predictedIndex + i) % TimeConfig.TicksPerSecond;
                    
                    EntityManager.SetComponentData(clientEntity, characterInputSaved[index].Value);
                    
                    _tickSystem.StepSimulation();
                }
                
                SetSingleton(new TickData()
                {
                    Value = tick
                });
                
                EntityManager.SetComponentData(clientEntity, characterInputCached);
            }
        }
    }
}


