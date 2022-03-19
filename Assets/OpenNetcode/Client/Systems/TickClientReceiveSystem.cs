using OpenNetcode.Shared.Systems;
using Unity.Entities;

namespace OpenNetcode.Client.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderFirst = true)]
    public partial class TickClientReceiveSystem : SystemBase
    {
        private IClientNetworkSystem _clientNetworkSystem;

        public TickClientReceiveSystem(IClientNetworkSystem clientNetworkSystem)
        {
            _clientNetworkSystem = clientNetworkSystem;
        }

        protected override void OnUpdate()
        {
            _clientNetworkSystem.ReceiveUpdate();
        }
    }
}