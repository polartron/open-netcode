using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using UnityEngine;

namespace ExampleGame.Client.Debugging
{
    public class DisplayTicks : MonoBehaviour
    {
        void OnGUI()
        {
#if CLIENT || UNITY_EDITOR
            var clientTickSystem = ClientBootstrap.World.GetExistingSystem<TickSystem>();
            
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(30, 10, 500, 20), "Client Tick = " + clientTickSystem.Tick);
                float x = clientTickSystem.Tick % (TimeConfig.TicksPerSecond * 10);
                GUI.Label(new Rect(x, 10, 500, 20), "|");
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
        }
    }
}
