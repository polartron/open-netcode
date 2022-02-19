using System;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace OpenNetcode.Client.Systems
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderLast = true)]
    public class TickPredictionSystem<TPrediction, TInput> : SystemBase
        where TPrediction : unmanaged, INetworkedComponent
        where TInput : unmanaged, INetworkedComponent
    {
        public Action<Entity, int> OnRollback;
        private TickSystem _tickSystem;

        protected override void OnCreate()
        {
            _tickSystem = World.GetExistingSystem<TickSystem>();
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var clientData = GetSingleton<ClientData>();
            Entity clientEntity = clientData.LocalPlayer;

            if (clientEntity == Entity.Null)
                return;
            
            int tick = GetSingleton<TickData>().Value;    
            TInput input = EntityManager.GetComponentData<TInput>(clientEntity);

            var resultBuffer = EntityManager.GetBuffer<SnapshotBufferElement<TPrediction>>(clientEntity);
            var resultElement = resultBuffer[clientData.LastReceivedSnapshotTick % resultBuffer.Length];

            if (resultElement.Tick != clientData.LastReceivedSnapshotTick)
                return;
            
            var predictedMoves = EntityManager.GetBuffer<PredictedMove<TPrediction, TInput>>(clientEntity);
            int predictedIndex = resultElement.Tick % predictedMoves.Length;
            var predicted = predictedMoves[predictedIndex];
            
            if (resultElement.Tick != predicted.Tick)
                return;
    
            if (!predicted.Compare(resultElement.Value))
            {
                OnRollback?.Invoke(clientEntity, predicted.Tick);
                
                TInput inputBeforeRollback = input;
                
                predictedMoves[predictedIndex] = new PredictedMove<TPrediction, TInput>()
                {
                    Input = predicted.Input,
                    Prediction = resultElement.Value,
                    Tick = predicted.Tick
                };

                //Perform rollback

                int ticksToPredict = tick - predicted.Tick;
                
                for (int j = 1; j < ticksToPredict; j++)
                {
                    int index = (predictedIndex + j) % predictedMoves.Length;

                    PredictedMove<TPrediction, TInput> replay = predictedMoves[index];

                    if (replay.Tick >= tick)
                        break;
                    
                    int predictedTick = resultElement.Tick + j;
                    
                    SetSingleton(new TickData()
                    {
                        Value = predictedTick
                    });
                    
                    EntityManager.SetComponentData(clientEntity, replay.Input);
                    _tickSystem.StepSimulation();
                }
                
                //Set values back to before rollback
                
                SetSingleton(new TickData()
                {
                    Value = tick
                });
                
                EntityManager.SetComponentData(clientEntity, inputBeforeRollback);
            }
        }
    }
}


