using OpenNetcode.Server.Systems;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Shared.Coordinates;
using Unity.Entities;
using Unity.Mathematics;

namespace OpenNetcode.Server
{
    public static class ServerInitialization
    {
        public static void Initialize<TInput>(in World serverWorld,in NetworkedPrefabs networkedPrefabs)
            where TInput : unmanaged, INetworkedComponent
        {
            var entityManager = serverWorld.EntityManager;
            
            var tickSystem = serverWorld.AddSystem(new TickSystem(TimeConfig.TicksPerSecond, (long) (UnityEngine.Time.time * 1000f)));
            serverWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(tickSystem);
            serverWorld.AddSystem(new FloatingOriginSystem(float3.zero));
            serverWorld.AddSystem(new NetworkedPrefabSystem(networkedPrefabs, true));
            
            entityManager.CreateEntity(ComponentType.ReadOnly<TickData>());

            IServerNetworkSystem server = serverWorld.AddSystem(new ServerNetworkSystem());
            
            tickSystem.AddPreSimulationSystem(new TickServerReceiveSystem(server));
            tickSystem.AddPreSimulationSystem(new TickInputBufferSystem<TInput>(server));
            tickSystem.AddPostSimulationSystem(new TickSendResultSystem(server));
            tickSystem.AddPostSimulationSystem(new TickServerSendSystem(server));
        }
    }
}
