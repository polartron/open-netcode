using OpenNetcode.Shared.Authoring;
using Unity.Transforms;
using UnityEngine;

namespace ExampleGame.Client.Behaviours
{
    public class ReadTranslationToTransform : MonoBehaviour
    {
        private LinkedGameObject _linkedGameObject;

        void OnDisable()
        {
            transform.position = new Vector3(9999,9999,9999);
        }
        
        // Start is called before the first frame update
        void Start()
        {
            _linkedGameObject = GetComponent<LinkedGameObject>();
        }

        // Update is called once per frame
        void Update()
        {
            var translation = _linkedGameObject.EntityManager.GetComponentData<Translation>(_linkedGameObject.Entity);
            transform.position = translation.Value;

            var rotation = _linkedGameObject.EntityManager.GetComponentData<Rotation>(_linkedGameObject.Entity);
            transform.rotation = rotation.Value;
        }
    }
}
