using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Server.Components;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using UnityEngine;

namespace ExampleGame.Server.Authoring
{
    public class PlayerServer : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<EntityPosition>(entity);
            dstManager.AddComponent<EntityVelocity>(entity);
            dstManager.AddComponent<MovementInput>(entity);
            dstManager.AddComponent<MovementConfig>(entity);
            dstManager.AddComponent<SimulatedEntity>(entity);
            dstManager.AddComponent<SpatialHash>(entity);
            dstManager.AddComponent<ServerNetworkedEntity>(entity);
            
            dstManager.SetComponentData(entity, new MovementConfig()
            {
                Acceleration = 16,
                Friction = 2,
                MaxSpeed = 4,
                StoppingSpeed = 2
            });
            
            dstManager.SetComponentData(entity, new EntityPosition()
            {
                Value = GameUnits.FromUnityVector3(transform.position)
            });
        }
    }
}
