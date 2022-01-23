using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace OpenNetcode.Server.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup), OrderLast = true)]
    public class TickServerSendSystem : SystemBase
    {
        private IServerNetworkSystem _server;

        public TickServerSendSystem(IServerNetworkSystem server)
        {
            _server = server;
        }

        protected override void OnUpdate()
        {
            _server.SendUpdate();
        }
    }
}