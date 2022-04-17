using System;
using System.Collections.Generic;
using ExampleGame.Shared.Movement.Components;
using OpenNetcode.Shared.Components;
using Shared.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using MeshCollider = Unity.Physics.MeshCollider;

namespace ExampleGame.Shared.Archetypes
{
    public class PhysicsProp : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Mesh Collider;
        public float Mass;

        private BlobAssetReference<Collider> _meshCollider;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            List<Vector3> verts = new List<Vector3>();
            Collider.GetVertices(verts);
            var triangles = Collider.GetTriangles(0);

            NativeArray<float3> tmpVerts = new NativeArray<float3>(verts.Count, Allocator.Temp);
            for (int i = 0; i < tmpVerts.Length; i++)
            {
                tmpVerts[i] = verts[i];
            }

            NativeArray<int3> tmpTriangles = new NativeArray<int3>(triangles.Length, Allocator.Temp);
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                tmpTriangles[i] = new int3(triangles[i * 3], triangles[i * 3 + 1], triangles[i * 3 + 2]);
            }
            
            _meshCollider = MeshCollider.Create(tmpVerts, tmpTriangles);
            
            dstManager.AddComponent<EntityPosition>(entity);
            dstManager.SetComponentData(entity, new EntityPosition()
            {
                Value = GameUnits.FromUnityVector3(transform.position)
            });
            
            dstManager.AddComponent<EntityVelocity>(entity);

            MakePhysical(
                entity : entity, 
                entityManager : dstManager, 
                position : transform.position, 
                orientation: transform.rotation, 
                collider: _meshCollider,
                mass: Mass, 
                isDynamic: true);
        }
    
        public unsafe void MakePhysical(Entity entity, EntityManager entityManager, float3 position, quaternion orientation, BlobAssetReference<Collider> collider,
             float mass, bool isDynamic)
        {
            ComponentType[] componentTypes = new ComponentType[isDynamic ? 7 : 4];

            componentTypes[0] = typeof(Translation);
            componentTypes[1] = typeof(Rotation);
            componentTypes[2] = typeof(LocalToWorld);
            componentTypes[3] = typeof(PhysicsCollider);

            if (isDynamic)
            {
                componentTypes[4] = typeof(PhysicsVelocity);
                componentTypes[5] = typeof(PhysicsMass);
                componentTypes[6] = typeof(PhysicsDamping);
            }

            for (int i = 0; i < (isDynamic ? 9 : 6); i++)
            {
                var type = componentTypes[i];
            
                if (!entityManager.HasComponent(entity, type))
                {
                    entityManager.AddComponent(entity, type);
                }
            }

            entityManager.SetComponentData(entity, new Translation { Value = position });
            entityManager.SetComponentData(entity, new Rotation { Value = orientation });

            entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });
            entityManager.AddSharedComponentData(entity, new PhysicsWorldIndex());

            if (isDynamic)
            {
                Collider* colliderPtr = (Collider*)collider.GetUnsafePtr();
                entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));
                // Calculate the angular velocity in local space from rotation and world angular velocity
                entityManager.SetComponentData(entity, new PhysicsVelocity()
                {
                    Linear = float3.zero,
                    Angular = float3.zero
                });
                
                entityManager.SetComponentData(entity, new PhysicsDamping()
                {
                    Linear = 0.01f,
                    Angular = 0.05f
                });
            }
        }
    }
}