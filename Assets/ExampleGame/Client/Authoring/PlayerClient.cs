using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Time;
using Unity.Entities;
using UnityEngine;

namespace ExampleGame.Client.Authoring
{
    public class PlayerClient : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<EntityPosition>(entity);
            dstManager.AddComponent<EntityVelocity>(entity);
            dstManager.AddComponent<MovementInput>(entity);
            dstManager.AddComponent<MovementConfig>(entity);
            dstManager.AddComponent<CachedTranslation>(entity);
            
            dstManager.SetComponentData(entity, new MovementConfig()
            {
                Acceleration = 16,
                Friction = 2,
                MaxSpeed = 4,
                StoppingSpeed = 2
            });
        }
    }
}
