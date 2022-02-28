using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;

namespace ExampleGame.Client.Generated
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickSimulationSystemGroup), OrderLast = true)]
    public class TickSavePredictionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;
            int index = tick % TimeConfig.TicksPerSecond;
            Entity clientEntity = GetSingleton<ClientData>().LocalPlayer;
            
            var prediction = EntityManager.GetComponentData<EntityPosition>(clientEntity);
            var predictedMoves = EntityManager.GetBuffer<Prediction<EntityPosition>>(clientEntity);
            
            predictedMoves[index] = new Prediction<EntityPosition>()
            {
                Tick = tick,
                Value = prediction
            };
        }
    }
}
