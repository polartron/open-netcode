using ExampleGame.Server;
using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Rendering;
using UnityEngine;

namespace ExampleGame.Client.Debugging
{
    public class DebugOverlay : MonoBehaviour
    {
        private bool _toggleView;

        void Start()
        {
            ServerBootstrap.World.GetExistingSystem<HybridRendererSystem>().Enabled = false;
            ClientBootstrap.World.GetExistingSystem<HybridRendererSystem>().Enabled = true;
        }
        
        void OnGUI()
        {
#if CLIENT || UNITY_EDITOR
            var clientTickSystem = ClientBootstrap.World.GetExistingSystem<TickSystem>();

            {
                GUI.color = Color.green;
                GUI.Label(new Rect(30, 10, 500, 20), "Client Tick = " + clientTickSystem.Tick);
                float x = clientTickSystem.Tick % (TimeConfig.TicksPerSecond * 10);
                GUI.Label(new Rect(x, 10, 500, 20), "|");

                double rtt = clientTickSystem.RttHalf;
                GUI.Label(new Rect(30, 50, 500, 20), "Round Trip Time = " + rtt);
            }
#endif

#if SERVER || UNITY_EDITOR
            var serverTickSystem = ExampleGame.Server.ServerBootstrap.World.GetExistingSystem<TickSystem>();

            {
                GUI.color = Color.red;
                GUI.Label(new Rect(30, 30, 500, 20), "Server Tick = " + serverTickSystem.Tick);
                float x = serverTickSystem.Tick % (TimeConfig.TicksPerSecond * 10);
                GUI.Label(new Rect(x, 30, 500, 20), "|");
            }
#endif

#if UNITY_EDITOR

            if (!_toggleView)
            {
                GUI.color = Color.red;
                if (GUI.Button(new Rect(30, 70, 100, 20), "Server View"))
                {
                    _toggleView = !_toggleView;

                    ServerBootstrap.World.GetExistingSystem<HybridRendererSystem>().Enabled = true;
                    ClientBootstrap.World.GetExistingSystem<HybridRendererSystem>().Enabled = false;
                }
            }
            else
            {
                GUI.color = Color.green;
                if (GUI.Button(new Rect(30, 70, 100, 20), "Client View"))
                {
                    _toggleView = !_toggleView;
                    ServerBootstrap.World.GetExistingSystem<HybridRendererSystem>().Enabled = false;
                    ClientBootstrap.World.GetExistingSystem<HybridRendererSystem>().Enabled = true;
                }
            }
            
            
#endif
        }
    }
}