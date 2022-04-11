using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared.Authoring
{
    public class LinkedGameObject : MonoBehaviour
    {
        public int SpawnedIndex;
        public EntityManager EntityManager;
        public World World;
        public Entity Entity;
    }
}
