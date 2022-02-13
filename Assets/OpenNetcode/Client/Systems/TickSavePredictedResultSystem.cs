using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace OpenNetcode.Client.Systems
{
    [UpdateInGroup(typeof(TickSimulationSystemGroup), OrderLast = true)]
    public class TickSavePredictedResultSystem<TPrediction, TInput> : SystemBase
        where TPrediction : unmanaged, INetworkedComponent
        where TInput : unmanaged, INetworkedComponent
    {
        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;
            
            Entity clientEntity = GetSingleton<ClientData>().LocalPlayer;
            TInput input = EntityManager.GetComponentData<TInput>(clientEntity);
            TPrediction prediction = EntityManager.GetComponentData<TPrediction>(clientEntity);
            var predictedMoves = EntityManager.GetBuffer<PredictedMove<TPrediction, TInput>>(clientEntity);
            
            int index = (tick) % predictedMoves.Length;

            predictedMoves[index] = new PredictedMove<TPrediction, TInput>()
            {
                Tick = tick,
                Input = input,
                Prediction = prediction
            };
        }
    }
}
