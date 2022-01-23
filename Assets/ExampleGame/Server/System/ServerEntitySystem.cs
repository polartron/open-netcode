﻿using ExampleGame.Shared.Components;
using OpenNetcode.Movement.Components;
using OpenNetcode.Server.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using Server.Components;
using Server.Generated;
using Shared;
using Shared.Components;
using Shared.Coordinates;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Server.System
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(NetworkedPrefabSystem))]
    public class ServerEntitySystem : SystemBase
    {
        internal Entity Player;
        internal Entity Monster;

        public Entity SpawnPlayer(Vector3 position, int ownerId)
        {
            var floatingOrigin = GetSingleton<FloatingOrigin>();

            Entity entity = EntityManager.Instantiate(Player);
            
#if UNITY_EDITOR
            EntityManager.SetName(entity, "Server Player");
#endif
            
            EntityManager.SetComponentData(entity, new EntityPosition()
            {
                Value = floatingOrigin.GetGameUnits(position)
            });
            
            EntityManager.SetComponentData(entity, new Translation()
            {
                Value = position
            });

            EntityManager.AddComponent<SpatialHash>(entity);
            
            EntityManager.AddComponent<ServerNetworkedEntity>(entity);
            
            if (ownerId >= 0)
            {
                EntityManager.SetComponentData(entity, new ServerNetworkedEntity()
                {
                    OwnerNetworkId = ownerId
                });

                EntityManager.AddComponent<PlayerControlledTag>(entity);
                EntityManager.AddBuffer<ProcessedInput>(entity);
                EntityManager.AddComponent<PlayerBaseLine>(entity);
                EntityManager.SetComponentData(entity, new PlayerBaseLine()
                {
                    ExpectedVersion = 1
                });

                var buffer = EntityManager.AddBuffer<PrivateSnapshotObserver>(entity);

                int componentInterestMask = 0;
                componentInterestMask = PrivateSnapshotObserver.Observe<EntityHealth>(componentInterestMask);
                
                buffer.Add(new PrivateSnapshotObserver()
                {
                    Entity = entity,
                    ComponentInterestMask = componentInterestMask
                });
            }
            else
            {
                EntityManager.SetComponentData(entity, new ServerNetworkedEntity()
                {
                    OwnerNetworkId = 0
                });
            }

            return entity;
        }

        public void SpawnMonster(Vector3 position)
        {
            Entity entity = EntityManager.Instantiate(Monster);
#if UNITY_EDITOR
            EntityManager.SetName(entity, "Monster");
#endif
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

        protected override void OnUpdate()
        {
            
        }
    }
}
