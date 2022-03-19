using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;

#if !UNITY_EDITOR
using System.Reflection;
#endif

namespace Shared
{
    public interface IWorldBootstrap
    {
        public string Name { get; }
        public bool Initialize();
    }

    public class SharedBootstrap : ICustomBootstrap
    {
        public SharedBootstrap()
        {
            // Default ctor
        }
        
        public static World CreateWorld(string name)
        {
            World world = new World(name);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default));
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            ScriptBehaviourUpdateOrder.AppendWorldToPlayerLoop(world, ref loop);
            PlayerLoop.SetPlayerLoop(loop);
            
            return world;
        }
        
        public static void AddSystem<T>(in World world, in SystemBase system) where T : ComponentSystemGroup
        {
            world.AddSystem(system);
            world.GetExistingSystem<T>().AddSystemToUpdateList(system);
        }
        
        public bool Initialize(string defaultWorldName)
        {
#if !UNITY_DOTSRUNTIME
            TypeManager.Initialize();
            
            // Create Default World
            World defaultWorld = new World("Default World");
            
            IReadOnlyList<Type> list = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(defaultWorld, list);
            World.DefaultGameObjectInjectionWorld = defaultWorld;
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            ScriptBehaviourUpdateOrder.AppendWorldToPlayerLoop(defaultWorld, ref loop);
            PlayerLoop.SetPlayerLoop(loop);
            
            var bootstrapTypes = GetTypesDerivedFrom(typeof(IWorldBootstrap));

            foreach (var bootType in bootstrapTypes)
            {
                Debug.Log("Attempting to create bootstrap " + bootType.Name);
                IWorldBootstrap bootstrap = Activator.CreateInstance(bootType) as IWorldBootstrap;

                if (bootstrap != null && bootstrap.Initialize())
                {
                    Debug.Log("Created " + bootstrap.Name);
                }
#else
                throw new Exception("This method should have been replaced by code-gen.");
#endif
            }
            
            return true;
        }

        // TypeManager.GetTypesDerivedFrom is internal, this is a copy of that function
            private static IEnumerable<Type> GetTypesDerivedFrom(Type type)
            {
#if UNITY_EDITOR
                return UnityEditor.TypeCache.GetTypesDerivedFrom(type);
#else
                var types = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!TypeManager.IsAssemblyReferencingEntities(assembly))
                        continue;

                    try
                    {
                        var assemblyTypes = assembly.GetTypes();
                        foreach (var t in assemblyTypes)
                        {
                            if (type.IsAssignableFrom(t) && !t.IsInterface)
                                types.Add(t);
                        }
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        foreach (var t in e.Types)
                        {
                            if (t != null && type.IsAssignableFrom(t) && !t.IsInterface)
                                types.Add(t);
                        }

                        Debug.LogWarning(
                            $"DefaultWorldInitialization failed loading assembly: {(assembly.IsDynamic ? assembly.ToString() : assembly.Location)}");
                    }
                }

                return types;
#endif
            }
        }
    }