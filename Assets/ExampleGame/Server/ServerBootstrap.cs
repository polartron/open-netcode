using ExampleGame.Server.Systems;
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Movement.Systems;
using OpenNetcode.Server;
using OpenNetcode.Server.Systems;
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
            tickSystem.AddPostSimulationSystem(new TickServerSnapshotSystem(World.GetExistingSystem<ServerNetworkSystem>()));
            tickSystem.AddPreSimulationSystem(new TickInputBufferSystem(World.GetExistingSystem<ServerNetworkSystem>()));

            var networkedPrefabSystem = World.GetExistingSystem<NetworkedPrefabSystem>();

            var player = Resources.Load<GameObject>("Prefabs/Server/Server Player");
            var monster = Resources.Load<GameObject>("Prefabs/Server/Server Moving Monster");
            var pathingMonster = Resources.Load<GameObject>("Prefabs/Server/Server Pathing Monster");
            
            World.AddSystem(new ServerEntitySystem()
            {
                Player = networkedPrefabSystem.GetEntityFromPrefab(player),
                Monster = networkedPrefabSystem.GetEntityFromPrefab(monster),
                PathingMonster = networkedPrefabSystem.GetEntityFromPrefab(pathingMonster),
            });

            tickSystem.AddPreSimulationSystem(new ServerGuestAuthentication(World.GetExistingSystem<ServerNetworkSystem>()));
            tickSystem.AddPreSimulationSystem(new TickDevServer());

            //Simulation
            tickSystem.AddSimulationSystem(new TickMovementSystem());
            tickSystem.AddSimulationSystem(new TopDownSpatialHashingSystem());

            return true;
        }
    }
}
