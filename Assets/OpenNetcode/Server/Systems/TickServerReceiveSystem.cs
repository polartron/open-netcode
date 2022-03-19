using OpenNetcode.Shared.Systems;
using Unity.Entities;

namespace OpenNetcode.Server.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderFirst = true)]
    public partial class TickServerReceiveSystem : SystemBase
    {
        private IServerNetworkSystem _server;

        public TickServerReceiveSystem(IServerNetworkSystem server)
        {
            _server = server;
        }
        
        protected override void OnUpdate()
        {
            _server.ReceiveUpdate();
        }
    }
}