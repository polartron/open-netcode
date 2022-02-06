using ExampleGame.Server.Systems;
using OpenNetcode.Movement.Components;
using OpenNetcode.Movement.Systems;
using OpenNetcode.Server;
using OpenNetcode.Server.Systems;
using OpenNetcode.Shared.Systems;
using Shared;
using Unity.Entities;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(TickInputBufferSystem<CharacterInput>.UpdatePlayerInputJob))]
namespace Server
{
    public class ServerBootstrap : IWorldBootstrap
    {
        public static World World;
        public string Name => "Server World";
        
        public bool Initialize()
        {
            World = SharedBootstrap.CreateWorld(Name);

            var networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
            ServerInitialization.Initialize<CharacterInput>(World, networkedPrefabs);
            var tickSystem = World.GetExistingSystem<TickSystem>();
            tickSystem.AddPostSimulationSystem(new Server.Generated.TickServerSnapshotSystem(World.GetExistingSystem<ServerNetworkSystem>()));

            var networkedPrefabSystem = World.GetExistingSystem<NetworkedPrefabSystem>();

            var player = Resources.Load<GameObject>("Prefabs/Server/Server Player");
            var monster = Resources.Load<GameObject>("Prefabs/Server/Server Monster");
            
            World.AddSystem(new ServerEntitySystem()
            {
                Player = networkedPrefabSystem.GetEntityFromPrefab(player),
                Monster = networkedPrefabSystem.GetEntityFromPrefab(monster),
            });

            tickSystem.AddPreSimulationSystem(new ServerGuestAuthentication(World.GetExistingSystem<ServerNetworkSystem>()));
            tickSystem.AddPreSimulationSystem(new TickDevServer());

            //Simulation
            tickSystem.AddSimulationSystem(new TickMovementSystem());
            tickSystem.AddSimulationSystem(new SpatialHashingSystem());

            return true;
        }
    }
}
