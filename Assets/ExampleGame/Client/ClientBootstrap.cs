using System;
using System.Reflection;
using Shared;
using Unity.Entities;
using UnityEngine;
using Client.Generated;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared.Systems;
using UnityEngine.Scripting;
using ExampleGame.Client.Systems;
using ExampleGame.Shared.Movement.Systems;

namespace ExampleGame.Client
{
    [Preserve]
    public class ClientBootstrap : IWorldBootstrap
    {
        public static World World;
        public string Name => "Client World";

        public bool Initialize()
        {
            World = InitializeWorld("Client World 1");
            return true;
        }
        
        public World InitializeWorld(string name)
        {
            var world = SharedBootstrap.CreateWorld(name);

            NetworkedPrefabs networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Local Player");
            ClientInitialization.Initialize(world, playerPrefab, networkedPrefabs);
            
            SharedBootstrap.AddSystem<SimulationSystemGroup>(world, new PlayerInputSystem());
            SharedBootstrap.AddSystem<SimulationSystemGroup>(world, new SoundSystem());
            SharedBootstrap.AddSystem<PresentationSystemGroup>(world, new MovementInterpolationSystem());

            var tickSystem = world.GetExistingSystem<TickSystem>();
            //Pre
            tickSystem.AddPreSimulationSystem(new TickDevClient(world.GetExistingSystem<ClientNetworkSystem>()));

            //Simulation
            tickSystem.AddSimulationSystem(new TickMovementSystem());
            tickSystem.AddPostSimulationSystem(new BumpEventSystem());
            tickSystem.AddPostSimulationSystem(new LinkToGameObjectSystem());
            
            SourceConsole.SourceConsole.AddAssembly(Assembly.GetExecutingAssembly());
            SourceConsole.SourceConsole.RefreshCommands();
            
            
            return world;
        }
    }
}


