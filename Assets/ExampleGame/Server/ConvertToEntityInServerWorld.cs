using ExampleGame.Server;
using Unity.Entities;
using UnityEngine;

namespace ExampleGame.Shared
{
    public class ConvertToEntityInServerWorld : ConvertToEntity
    {
        // Start is called before the first frame update
        void Awake()
        {
            if (ServerBootstrap.World != null)
            {
                var system = ServerBootstrap.World.GetOrCreateSystem<ConvertToEntitySystem>();
                system.AddToBeConverted(ServerBootstrap.World, this);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"{nameof(ConvertToEntity)} failed because there is no {nameof(ServerBootstrap.World)}", this);
            }
        }
    }
}
