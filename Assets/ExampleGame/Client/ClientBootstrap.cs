using Shared;
using Unity.Entities;
using UnityEngine;
using Client.Generated;
using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared.Systems;
using ExampleGame.Client.Systems;
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Movement.Systems;
using UnityEngine.Scripting;

namespace ExampleGame.Client
{
    [Preserve]
    public class ClientBootstrap : IWorldBootstrap
    {
        public static World World;
        public string Name => "Client World";
        
        [Preserve]
        public ClientBootstrap()
        {
            Debug.Log("Client Bootstrap");
        }
        
        public bool Initialize()
        {
            World = SharedBootstrap.CreateWorld(Name);

            NetworkedPrefabs networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Local Player");
            ClientInitialization.Initialize(World, playerPrefab, networkedPrefabs);
            
            SharedBootstrap.AddSystem<SimulationSystemGroup>(World, new PlayerInputSystem());
            SharedBootstrap.AddSystem<SimulationSystemGroup>(World, new SoundSystem());
            SharedBootstrap.AddSystem<PresentationSystemGroup>(World, new MovementInterpolationSystem());

            var tickSystem = World.GetExistingSystem<TickSystem>();
            //Pre
            tickSystem.AddPreSimulationSystem(new TickDevClient(World.GetExistingSystem<ClientNetworkSystem>()));

            //Simulation
            tickSystem.AddSimulationSystem(new TickMovementSystem());
            tickSystem.AddPostSimulationSystem(new BumpEventSystem());
            
        
            return true;
        }
    }
}