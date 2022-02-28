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
        public static void Initialize<TPrediction, TInput>(in World clientWorld, in Entity localPlayer, in NetworkedPrefabs networkedPrefabs)
            where TPrediction : unmanaged, INetworkedComponent
            where TInput : unmanaged, INetworkedComponent
        {
            TickSystem tickSystem = clientWorld.AddSystem(new TickSystem(TimeConfig.TicksPerSecond, (long) (Time.time * 1000f)));
            clientWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(tickSystem);
            clientWorld.AddSystem(new FloatingOriginSystem(new float3(0f, 0f, 0f)));
            clientWorld.AddSystem(new NetworkedPrefabSystem(networkedPrefabs, false));
        
            clientWorld.EntityManager.CreateEntity(ComponentType.ReadOnly<TickData>(), ComponentType.ReadOnly<ClientData>(), ComponentType.ReadOnly<RoundTripTime>());

            tickSystem.SetSingleton(new ClientData()
            {
                LocalPlayer = localPlayer
            });

            ClientNetworkSystem clientNetworkSystem = clientWorld.AddSystem(new ClientNetworkSystem());
        
            tickSystem.AddPreSimulationSystem(new TickClientReceiveSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickReceiveClientInfoSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickReceiveResultSystem(clientNetworkSystem));
            tickSystem.AddPostSimulationSystem(new TickClientSendSystem(clientNetworkSystem));
        }
    }
}
