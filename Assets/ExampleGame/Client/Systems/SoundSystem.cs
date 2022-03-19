using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using ExampleGame.Shared.Components;
using Mono.Cecil;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [DisableAutoCreation]
    public partial class SoundSystem : SystemBase
    {
        private Dictionary<Entity, AudioSource> _activeSounds = new Dictionary<Entity, AudioSource>();
        private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
        
        public AudioClip LoadSound(string sound)
        {
            if (!_audioClips.ContainsKey(sound))
            {
                AudioClip clip = Resources.Load<AudioClip>(sound);
                
                if (clip == null)
                {
                    Debug.LogError($"Could not load audio clip from resources: {sound}");
                    return null;
                }

                _audioClips[sound] = clip;
            }

            return _audioClips[sound];
        }
        
        public void PlaySound(Entity entity, string sound, float volume = 1f, float pitch = 1f)
        {
            if (_activeSounds.ContainsKey(entity))
                return;
            
            AudioClip clip = LoadSound(sound);
            
            if (clip == null)
                return;
            
            var gameObject = new GameObject(sound);
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
            
            var position = EntityManager.GetComponentData<Translation>(entity);
            gameObject.transform.position = position.Value;

            _activeSounds[entity] = audioSource;
        }
        
        protected override void OnUpdate()
        {
            List<Entity> remove = new List<Entity>();
            
            foreach (var active in _activeSounds)
            {
                if (active.Value == null)
                {
                    remove.Add(active.Key);
                    continue;
                }
                
                if (!EntityManager.Exists(active.Key))
                {
                    remove.Add(active.Key);
                    continue;
                }

                if (active.Value != null && !active.Value.isPlaying)
                {
                    remove.Add(active.Key);
                    continue;
                }

                var position = EntityManager.GetComponentData<Translation>(active.Key);
                active.Value.transform.position = position.Value;
            }

            foreach (var r in remove)
            {
                var audioSource = _activeSounds[r];
                if (audioSource != null)
                {
                    GameObject.Destroy(audioSource.gameObject);
                }
                _activeSounds.Remove(r);
            }
        }
    }
}
