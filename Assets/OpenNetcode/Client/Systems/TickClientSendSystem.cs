using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

namespace OpenNetcode.Client.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup), OrderLast = true)]
    public class TickClientSendSystem : SystemBase
    {
        private IClientNetworkSystem _clientNetworkSystem;

        public TickClientSendSystem(IClientNetworkSystem clientNetworkSystem)
        {
            _clientNetworkSystem = clientNetworkSystem;
        }

        protected override void OnUpdate()
        {
            _clientNetworkSystem.SendUpdate();
        }
    }
}