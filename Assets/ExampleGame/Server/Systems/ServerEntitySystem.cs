using ExampleGame.Server.Components;
using ExampleGame.Shared.Components;
using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Server.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using Server.Generated;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ExampleGame.Server.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class ServerEntitySystem : SystemBase
    {
        internal Entity Player;
        internal Entity Monster;
        internal Entity PathingMonster;

        public Entity SpawnPlayer(Vector3 position, int ownerId)
        {
            var floatingOrigin = GetSingleton<FloatingOrigin>();

            Entity entity = EntityManager.Instantiate(Player);
            
#if UNITY_EDITOR
            EntityManager.SetName(entity, "Server Player");
#endif
            ServerInitialization.InitializePlayerEntity(EntityManager, entity, ownerId);
            
            EntityManager.SetComponentData(entity, new EntityPosition()
            {
                Value = floatingOrigin.GetGameUnits(position)
            });
            
            EntityManager.SetComponentData(entity, new Translation()
            {
                Value = position
            });

            return entity;
        }

        public void SpawnMonster(Vector3 position)
        {
            Entity entity = EntityManager.Instantiate(Monster);
#if UNITY_EDITOR
            EntityManager.SetName(entity, "Monster");
#endif
            EntityManager.AddComponent<SimulatedEntity>(entity);
            EntityManager.AddComponent<Translation>(entity);
            var floatingOrigin = GetSingleton<FloatingOrigin>();
            EntityManager.SetComponentData(entity, new EntityPosition()
            {
                Value = floatingOrigin.GetGameUnits(position)
            });
            
            EntityManager.AddComponent<ServerNetworkedEntity>(entity);
            EntityManager.SetComponentData(entity, new ServerNetworkedEntity()
            {
                OwnerNetworkId = -1
            });
            
            EntityManager.SetComponentData(entity, new Translation()
            {
                Value = position
            });
            
            EntityManager.AddComponent<SpatialHash>(entity);
            EntityManager.AddComponent<WanderingAiTag>(entity);

            EntityManager.AddBuffer<BumpEvent>(entity);
        }
        
        public void SpawnPathingMonster(Vector3 position)
        {
            Entity entity = EntityManager.Instantiate(PathingMonster);
#if UNITY_EDITOR
            EntityManager.SetName(entity, "Monster");
#endif
            EntityManager.AddComponent<Translation>(entity);
            EntityManager.AddComponent<SimulatedEntity>(entity);

            EntityManager.AddComponent<ServerNetworkedEntity>(entity);
            EntityManager.SetComponentData(entity, new ServerNetworkedEntity()
            {
                OwnerNetworkId = -1
            });
            
            EntityManager.SetComponentData(entity, new Translation()
            {
                Value = position
            });
            
            EntityManager.AddComponent<SpatialHash>(entity);
            EntityManager.AddComponent<WanderingAiTag>(entity);

            EntityManager.AddBuffer<BumpEvent>(entity);
        }

        protected override void OnUpdate()
        {
            
        }
    }
}
