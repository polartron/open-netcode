using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Coordinates;
using Shared.Time;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace OpenNetcode.Client
{
    public static class ClientInitialization
    {
        public static void Initialize<TPrediction, TInput, TResult>(in World clientWorld, in Entity localPlayer, in NetworkedPrefabs networkedPrefabs)
            where TPrediction : unmanaged, INetworkedComponent
            where TInput : unmanaged, INetworkedComponent
            where TResult : unmanaged, IResultMessage<TPrediction>
        {
            var predictedMoves = clientWorld.EntityManager.AddBuffer<PredictedMove<TPrediction, TInput>>(localPlayer);
            
            for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            {
                predictedMoves.Add(default);
            }

            TickSystem tickSystem = clientWorld.AddSystem(new TickSystem(TimeConfig.TicksPerSecond, (long) (Time.time * 1000f)));
            clientWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(tickSystem);
            clientWorld.AddSystem(new FloatingOriginSystem(new float3(0f, 0f, 0f)));
            clientWorld.AddSystem(new NetworkedPrefabSystem(networkedPrefabs, false));
        
            clientWorld.EntityManager.CreateEntity(ComponentType.ReadOnly<TickData>(), ComponentType.ReadOnly<ClientData>(), ComponentType.ReadOnly<RoundTripTime>());

            tickSystem.SetSingleton(new ClientData()
            {
                LocalPlayer = localPlayer
            });

            IClientNetworkSystem clientNetworkSystem = clientWorld.AddSystem(new ClientNetworkSystem());
        
            tickSystem.AddPreSimulationSystem(new TickClientReceiveSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickReceiveClientInfoSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickReceiveResultSystem(clientNetworkSystem));
            tickSystem.AddPostSimulationSystem(new TickClientSendSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickSendInputSystem<TPrediction, TInput, TResult>(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickPredictionSystem<TPrediction, TInput>());
            tickSystem.AddSimulationSystem(new TickSavePredictedResultSystem<TPrediction, TInput, TResult>());
            
            
        }
    }
}
