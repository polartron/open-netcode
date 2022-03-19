using OpenNetcode.Client.Components;
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
    [UpdateInGroup(typeof(TickSimulationSystemGroup), OrderLast = true)]
    public partial class TickSavePredictionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;
            int index = tick % TimeConfig.TicksPerSecond;
            Entity clientEntity = GetSingleton<ClientData>().LocalPlayer;
            
            //<template:predicted>
            //var ##TYPELOWER##Prediction = EntityManager.GetComponentData<##TYPE##>(clientEntity);
            //var ##TYPELOWER##Predictions = EntityManager.GetBuffer<Prediction<##TYPE##>>(clientEntity);
            //
            //##TYPELOWER##Predictions[index] = new Prediction<##TYPE##>()
            //{
            //    Tick = tick,
            //    Value = ##TYPELOWER##Prediction
            //};
            //</template>
//<generated>
            var entityVelocityPrediction = EntityManager.GetComponentData<EntityVelocity>(clientEntity);
            var entityVelocityPredictions = EntityManager.GetBuffer<Prediction<EntityVelocity>>(clientEntity);
            
            entityVelocityPredictions[index] = new Prediction<EntityVelocity>()
            {
                Tick = tick,
                Value = entityVelocityPrediction
            };
            var entityPositionPrediction = EntityManager.GetComponentData<EntityPosition>(clientEntity);
            var entityPositionPredictions = EntityManager.GetBuffer<Prediction<EntityPosition>>(clientEntity);
            
            entityPositionPredictions[index] = new Prediction<EntityPosition>()
            {
                Tick = tick,
                Value = entityPositionPrediction
            };
//</generated>
            
            
        }
    }
}
