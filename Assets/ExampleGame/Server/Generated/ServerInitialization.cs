using OpenNetcode.Server.Systems;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Coordinates;
using Unity.Entities;
using Unity.Mathematics;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    public static class ServerInitialization
    {
        public static void Initialize(in World serverWorld,in NetworkedPrefabs networkedPrefabs)
        {
            var entityManager = serverWorld.EntityManager;
            
            var tickSystem = serverWorld.AddSystem(new TickSystem(TimeConfig.TicksPerSecond, (long) (UnityEngine.Time.time * 1000f)));
            serverWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(tickSystem);
            serverWorld.AddSystem(new FloatingOriginSystem(float3.zero));
            serverWorld.AddSystem(new NetworkedPrefabSystem(networkedPrefabs, true));
            
            entityManager.CreateEntity(ComponentType.ReadOnly<TickData>());

            ServerNetworkSystem server = serverWorld.AddSystem(new ServerNetworkSystem());
            
            tickSystem.AddPreSimulationSystem(new TickServerReceiveSystem(server));
            tickSystem.AddPostSimulationSystem(new TickSendResultSystem(server));
            tickSystem.AddPostSimulationSystem(new TickServerSendSystem(server));
            
            tickSystem.AddPostSimulationSystem(new TickServerSnapshotSystem(server));
            tickSystem.AddPreSimulationSystem(new TickInputBufferSystem(server));
        }
    }
}