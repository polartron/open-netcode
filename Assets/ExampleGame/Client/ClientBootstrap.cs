using Client.Generated;
using ExampleGame.Client.Components;
using ExampleGame.Client.Generated;
using ExampleGame.Client.Systems;
using ExampleGame.Shared.Components;
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Movement.Systems;
using OpenNetcode.Client;
using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[assembly: RegisterGenericComponentType(typeof(Prediction<EntityPosition>))]
namespace ExampleGame.Client
{
    public class ClientBootstrap : IWorldBootstrap
    {
        public static World World;
        public string Name => "Client World";

        public bool Initialize()
        {
            World = SharedBootstrap.CreateWorld(Name);

            NetworkedPrefabs networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Local Player");
            
            EntityManager entityManager = World.EntityManager;

            Entity clientEntity = CreateLocalPlayer(ref entityManager, playerPrefab);
            ClientInitialization.Initialize<EntityPosition, CharacterInput>(World, clientEntity, networkedPrefabs);
            var tickSystem = World.GetExistingSystem<TickSystem>();
            tickSystem.AddPreSimulationSystem(new TickClientSnapshotSystem<EntityPosition, CharacterInput>(
                World.GetExistingSystem<ClientNetworkSystem>()));
            tickSystem.AddPreSimulationSystem(new TickPredictionSystem());
            tickSystem.AddPreSimulationSystem(new TickInputSystem(World.GetExistingSystem<ClientNetworkSystem>()));
            tickSystem.AddSimulationSystem(new TickSavePredictionSystem());

            SharedBootstrap.AddSystem<SimulationSystemGroup>(World, new PlayerInputSystem());
            SharedBootstrap.AddSystem<SimulationSystemGroup>(World, new InterpolationSystem());

            //Pre
            tickSystem.AddPreSimulationSystem(new TickDevClient(World.GetExistingSystem<ClientNetworkSystem>()));

            //Simulation
            tickSystem.AddSimulationSystem(new TickMovementSystem());
        
            return true;
        }

        private Entity CreateLocalPlayer(ref EntityManager entityManager, in GameObject prefab)
        {
            var blobAssetStore = new BlobAssetStore();
            Entity entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab,
                GameObjectConversionSettings.FromWorld(World, blobAssetStore));

            var entity = entityManager.Instantiate(entityPrefab);

#if UNITY_EDITOR
            entityManager.SetName(entity, "Client Entity");
#endif


            entityManager.AddComponent<Translation>(entity);
            entityManager.AddComponent<ClientEntityTag>(entity);

            Debug.Log($"<color=green> Created client entity with ID = {entity.Index}</color>");
            var entityPositionBuffer = entityManager.AddBuffer<SnapshotBufferElement<EntityPosition>>(entity);
            for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            {
                entityPositionBuffer.Add(default);
            }

            var entityVelocityBuffer = entityManager.AddBuffer<SnapshotBufferElement<EntityVelocity>>(entity);
            for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            {
                entityVelocityBuffer.Add(default);
            }

            var entityHealthBuffer = entityManager.AddBuffer<SnapshotBufferElement<EntityHealth>>(entity);
            for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            {
                entityHealthBuffer.Add(default);
            }
            
            var entityPositionPrediction = entityManager.AddBuffer<Prediction<EntityPosition>>(entity);
            for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            {
                entityPositionPrediction.Add(default);
            }
            
            var characterInputSave = entityManager.AddBuffer<SavedInput<CharacterInput>>(entity);
            for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            {
                characterInputSave.Add(default);
            }


            blobAssetStore.Dispose();

            return entity;
        }
    }
}