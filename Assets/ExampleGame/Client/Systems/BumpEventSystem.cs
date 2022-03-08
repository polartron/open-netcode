using ExampleGame.Shared.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared.Systems;
using Unity.Entities;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup), OrderFirst = true)]
    [DisableAutoCreation]
    public class BumpEventSystem : TickEventCallbackSystem<BumpEvent>
    {
        private SoundSystem _soundSystem;

        protected override void OnCreate()
        {
            _soundSystem = World.GetExistingSystem<SoundSystem>();
            base.OnCreate();
        }

        protected override void OnEvent(Entity entity, BumpEvent invokedEvent)
        {
            _soundSystem.PlaySound(entity, "Sounds/Bump", 0.1f, Random.Range(0.8f, 1.2f));
        }
    }
}
