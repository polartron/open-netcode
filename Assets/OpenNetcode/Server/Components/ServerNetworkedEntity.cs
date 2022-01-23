using Unity.Entities;

namespace OpenNetcode.Server.Components
{
    public struct ServerNetworkedEntity : IComponentData
    {
        public int OwnerNetworkId;
    }
}
