using System.Collections;
using System.Collections.Generic;
using Client;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Server;
using UnityEngine;

public class DisplayTicks : MonoBehaviour
{
    private TickSystem _client;
    private TickSystem _server;
    
    // Start is called before the first frame update
    void Start()
    {
        _server = ServerBootstrap.World.GetExistingSystem<TickSystem>();
    }

    void OnGUI()
    {
        var serverTickSystem = ServerBootstrap.World.GetExistingSystem<TickSystem>();
        var clientTickSystem = ClientBootstrap.World.GetExistingSystem<TickSystem>();

        if (serverTickSystem == null || clientTickSystem == null)
            return;
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(30, 10, 500, 20), "Client Tick = " + clientTickSystem.Tick);
            float x = clientTickSystem.Tick % (TimeConfig.TicksPerSecond * 10);
            GUI.Label(new Rect(x, 10, 500, 20), "|");
        }
        
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(30, 30, 500, 20), "Server Tick = " + serverTickSystem.Tick);
            float x = serverTickSystem.Tick % (TimeConfig.TicksPerSecond * 10);
            GUI.Label(new Rect(x, 30, 500, 20), "|");
        }
    }
}
