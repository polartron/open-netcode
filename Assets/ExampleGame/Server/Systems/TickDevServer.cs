using ExampleGame.Shared.Components;
using OpenNetcode.Movement.Components;
using OpenNetcode.Shared.Systems;
using Server.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExampleGame.Server.Systems
{

    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    public class TickDevServer : SystemBase
    {
        protected override void OnCreate()
        {
            var spawner = World.GetExistingSystem<ServerEntitySystem>();
        
            for (int i = 0; i < 50; i++)
            {
                spawner.SpawnMonster(new Vector3(0, 0, 0) + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10)));
            }
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            double time = Time.ElapsedTime;

            float range = 20;
            
            Vector3 center = new Vector3(0, 0, 0);

            var entityManager = EntityManager;
            var ecb = new EntityCommandBuffer();
            
            Entities.ForEach((ref CharacterInput input, ref Translation translation, ref DynamicBuffer<BumpEvent> bumpEvents, in Entity entity, in WanderingAiTag wanderingAiTag) =>
            {
                if (input.Move.x == 0f && input.Move.y == 0f)
                {
                    input.Move = Random.insideUnitCircle;
                }
                
                if (translation.Value.x - center.x > range)
                {
                    bumpEvents.Add(new BumpEvent());
                    input.Move = new float2(-1f, input.Move.y);
                }
                else if (translation.Value.x - center.x < -range)
                {
                    bumpEvents.Add(new BumpEvent());
                    input.Move = new float2(1f, input.Move.y);
                }
            
                if (translation.Value.z - center.z > range)
                {
                    bumpEvents.Add(new BumpEvent());
                    input.Move = new float2(input.Move.x, -1f);
                }
                else if (translation.Value.z - center.z < -range)
                {
                    bumpEvents.Add(new BumpEvent());
                    input.Move = new float2(input.Move.x, 1f);
                }
            }).Run();
        }
    }
}
