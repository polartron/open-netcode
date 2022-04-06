using ExampleGame.Server.Components;
using ExampleGame.Shared.Components;
using ExampleGame.Shared.Debugging;
using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExampleGame.Server.Systems
{

    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    public partial class TickDevServer : SystemBase
    {
        protected override void OnCreate()
        {
            var spawner = World.GetExistingSystem<ServerEntitySystem>();

            //for (int i = 0; i < 50; i++)
            //{
            //    spawner.SpawnPathingMonster(new Vector3(0, 0, 0) + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10)));
            //}
            //
            for (int i = 0; i < 1; i++)
            {
                spawner.SpawnMonster(new Vector3(0, 0, 0) + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10)));
            }
            
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;

            float range = 20;
            
            Vector3 center = new Vector3(0, 0, 0);
            
            Entities.ForEach((ref MovementInput input, ref Translation translation, ref DynamicBuffer<BumpEvent> bumpEvents, in Entity entity, in WanderingAiTag wanderingAiTag) =>
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

            var floatingOrigin = GetSingleton<FloatingOrigin>();
            
            Entities.ForEach((ref Translation translation, ref PathComponent pathComponent, in Entity entity, in WanderingAiTag wanderingAiTag) =>
            {
                if (tick > pathComponent.Stop)
                {
                    Vector3 target = new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
                    float distance = Vector3.Distance(target, translation.Value);
                    pathComponent.From = pathComponent.To;
                    pathComponent.To = floatingOrigin.GetGameUnits(target);
                    pathComponent.Start = tick;
                    pathComponent.Stop = tick + (int) (distance * 0.5f * TimeConfig.TicksPerSecond);
                }
            }).Run();
            
            DebugOverlay.AddTickElement("Server Tick", new TickElement()
            {
                Color = Color.red,
                Tick = GetSingleton<TickData>().Value
            });
        }
    }
}
