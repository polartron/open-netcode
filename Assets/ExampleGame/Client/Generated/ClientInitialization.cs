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

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    public static class ClientInitialization
    {
        public static void Initialize(in World clientWorld, in GameObject localPlayerPrefab, in NetworkedPrefabs networkedPrefabs)
        {
            TickSystem tickSystem = clientWorld.AddSystem(new TickSystem(TimeConfig.TicksPerSecond, (long) (Time.time * 1000f)));
            
            var blobAssetStore = new BlobAssetStore();
            Entity entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(localPlayerPrefab,
                GameObjectConversionSettings.FromWorld(clientWorld, blobAssetStore));
            blobAssetStore.Dispose();

            var entity = clientWorld.EntityManager.Instantiate(entityPrefab);

#if UNITY_EDITOR
            clientWorld.EntityManager.SetName(entity, "Client Entity");
#endif

            clientWorld.EntityManager.AddComponent<ClientEntityTag>(entity);
            clientWorld.EntityManager.AddComponent<SimulatedEntity>(entity);
            
            clientWorld.EntityManager.CreateEntity(ComponentType.ReadOnly<TickData>(), ComponentType.ReadOnly<ClientData>(), ComponentType.ReadOnly<RoundTripTime>());
            tickSystem.SetSingleton(new ClientData()
            {
                LocalPlayer = entity
            });

            clientWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(tickSystem);
            clientWorld.AddSystem(new FloatingOriginSystem(new float3(0f, 0f, 0f)));
            clientWorld.AddSystem(new NetworkedPrefabSystem(networkedPrefabs, false));

            ClientNetworkSystem clientNetworkSystem = clientWorld.AddSystem(new ClientNetworkSystem());
        
            tickSystem.AddPreSimulationSystem(new TickClientReceiveSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickReceiveClientInfoSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickReceiveResultSystem(clientNetworkSystem));
            tickSystem.AddPostSimulationSystem(new TickClientSendSystem(clientNetworkSystem));
            
            tickSystem.AddPreSimulationSystem(new TickClientSnapshotSystem(clientNetworkSystem));
            tickSystem.AddPreSimulationSystem(new TickApplySnapshotSystem());
            tickSystem.AddPreSimulationSystem(new TickPredictionSystem());
            tickSystem.AddPreSimulationSystem(new TickInputSystem(clientNetworkSystem));
            tickSystem.AddSimulationSystem(new TickSavePredictionSystem());
            tickSystem.AddPostSimulationSystem(new TickClearEventsSystem());
        }
    }
}
