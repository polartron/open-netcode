using OpenNetcode.Server.Systems;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;
using Unity.Mathematics;
using OpenNetcode.Server.Components;
using OpenNetcode.Shared.Components;

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

        public static void InitializePlayerEntity(EntityManager entityManager, in Entity entity, int ownerId)
        {
            // Add components
            entityManager.AddComponent<SimulatedEntity>(entity);
            entityManager.AddComponent<SpatialHash>(entity);
            entityManager.AddComponent<ServerNetworkedEntity>(entity);
            entityManager.AddComponent<PlayerControlledTag>(entity);
            entityManager.AddBuffer<ProcessedInput>(entity);
            entityManager.AddComponent<PlayerBaseLine>(entity);
            entityManager.AddComponent<InputTimeData>(entity);
            var privateSnapshotObservers = entityManager.AddBuffer<PrivateSnapshotObserver>(entity);
            
            // Set data
            int componentInterestMask = 0;
            //<template:privatesnapshot>
            //componentInterestMask = PrivateSnapshotObserver.Observe<##TYPE##>(componentInterestMask);
            //</template>
//<generated>
            componentInterestMask = PrivateSnapshotObserver.Observe<EntityHealth>(componentInterestMask);
//</generated>
                
            privateSnapshotObservers.Add(new PrivateSnapshotObserver()
            {
                Entity = entity,
                ComponentInterestMask = componentInterestMask
            });
            
            entityManager.SetComponentData(entity, new ServerNetworkedEntity()
            {
                OwnerNetworkId = ownerId
            });
            
            entityManager.SetComponentData(entity, new PlayerBaseLine()
            {
                ExpectedVersion = 1
            });

            //<template:input>
            //if (entityManager.HasComponent<##TYPE##>(entity))
            //{
            //    var buffer = entityManager.AddBuffer<Received##TYPE##>(entity);
            //
            //    for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
            //    {
            //        buffer.Add(default);
            //    }
            //}
            //</template>
//<generated>
            if (entityManager.HasComponent<WeaponInput>(entity))
            {
                var buffer = entityManager.AddBuffer<ReceivedWeaponInput>(entity);
            
                for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
                {
                    buffer.Add(default);
                }
            }
            if (entityManager.HasComponent<MovementInput>(entity))
            {
                var buffer = entityManager.AddBuffer<ReceivedMovementInput>(entity);
            
                for (int i = 0; i < TimeConfig.TicksPerSecond; i++)
                {
                    buffer.Add(default);
                }
            }
//</generated>
        }
    }
}
