using ExampleGame.Client.Systems;
using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Time;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ExampleGame.Shared.Physics;
using OpenNetcode.Shared;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    public static class ClientInitialization
    {
        public static void Initialize(in World clientWorld, in NetworkedPrefabs networkedPrefabs)
        {
            TickSystem tickSystem = clientWorld.AddSystem(new TickSystem(TimeConfig.TicksPerSecond, (long) (Time.time * 1000f)));
            
            var blobAssetStore = new BlobAssetStore();
            Entity entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(networkedPrefabs.ClientPlayer,
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

            ClientNetworkSystem client = clientWorld.AddSystem(new ClientNetworkSystem());
            tickSystem.AddPreSimulationSystem(new TickClientPrefabSystem(networkedPrefabs, client));
            tickSystem.AddPreSimulationSystem(new TickClientReceiveSystem(client));
            tickSystem.AddPreSimulationSystem(new TickReceiveClientInfoSystem(client));
            tickSystem.AddPreSimulationSystem(new TickReceiveResultSystem(client));
            tickSystem.AddPostSimulationSystem(new TickClientSendSystem(client));
            
            tickSystem.AddPreSimulationSystem(new TickClientSnapshotSystem(client));
            tickSystem.AddPreSimulationSystem(new TickApplySnapshotSystem());
            tickSystem.AddPreSimulationSystem(new TickPredictionSystem());
            tickSystem.AddPreSimulationSystem(new TickInputSystem(client));
            tickSystem.AddSimulationSystem(new TickSavePredictionSystem());
            tickSystem.AddPostSimulationSystem(new TickClearEventsSystem());
            
            tickSystem.AddSimulationSystem(new TickPrePhysicsSimulationSystem());
            tickSystem.AddSimulationSystem(new TickPostPhysicsSimulationSystem());
        }
    }
}
