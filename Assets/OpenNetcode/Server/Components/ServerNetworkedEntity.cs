using Unity.Entities;

namespace OpenNetcode.Server.Components
{
    [GenerateAuthoringComponent]
    public struct ServerNetworkedEntity : IComponentData
    {
        public int OwnerNetworkId;
    }
}
