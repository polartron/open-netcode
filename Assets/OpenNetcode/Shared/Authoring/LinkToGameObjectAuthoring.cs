using System.Collections.Generic;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared.Authoring
{
    public class LinkToGameObjectAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        internal static int Index = 0;
        internal static Dictionary<GameObject, int> Prefabs = new Dictionary<GameObject, int>();
        internal static Dictionary<int, GameObject> PrefabsFromIndex = new Dictionary<int, GameObject>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            Prefabs.Clear();
            PrefabsFromIndex.Clear();
            Index = 0;
        }
        
        public GameObject Prefab;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!Prefabs.ContainsKey(Prefab))
            {
                PrefabsFromIndex[Index] = Prefab;
                Prefabs.Add(Prefab, Index++);
            }

            int index = Prefabs[Prefab];

            dstManager.AddComponent<LinkToGameObject>(entity);
            dstManager.SetComponentData(entity, new LinkToGameObject()
            {
                Type = index
            });
        }
    }
}
