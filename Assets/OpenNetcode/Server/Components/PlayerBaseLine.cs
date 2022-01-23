using Unity.Entities;

namespace OpenNetcode.Server.Components
{
    public struct PlayerBaseLine : IComponentData
    {
        public int BaseLine;
        public int LastHash;
        public int Version;
        public int ExpectedVersion;
    }
}
