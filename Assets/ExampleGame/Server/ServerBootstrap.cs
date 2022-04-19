using System.Reflection;
using ExampleGame.Server.Generated;
using ExampleGame.Server.Systems;
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Movement.Systems;
using OpenNetcode.Server;
using OpenNetcode.Server.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Systems;
using Server.Generated;
using Shared;
using Unity.Entities;
using UnityEngine;

namespace ExampleGame.Server
{
    public class ServerBootstrap : IWorldBootstrap
    {
        public static World World;
        public string Name => "Server World";

        public bool Initialize()
        {
            World = SharedBootstrap.CreateWorld(Name);

            var networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
            ServerInitialization.Initialize(World, networkedPrefabs);

            var tickSystem = World.GetExistingSystem<TickSystem>();
            tickSystem.AddPreSimulationSystem(new ServerGuestAuthentication(World.GetExistingSystem<ServerNetworkSystem>()));
            tickSystem.AddPreSimulationSystem(new TickDevServer());

            //Simulation
            tickSystem.AddSimulationSystem(new TickMovementSystem());
            tickSystem.AddSimulationSystem(new TopDownSpatialHashingSystem());
            
            
            // Disable rendering on server
            World.GetExistingSystem<PresentationSystemGroup>().Enabled = false;
            
            SourceConsole.SourceConsole.AddAssembly(Assembly.GetExecutingAssembly());
            SourceConsole.SourceConsole.RefreshCommands();

            return true;
        }
    }
}
