using Unity.Entities;

namespace OpenNetcode.Server.Components
{
    /// <summary>
    /// Contains information about the latest received input package from the client
    /// It is used for round trip time calculation
    /// </summary>
    public struct InputTimeData : IComponentData
    {
        public int LatestReceivedTick;
        public double ArrivedTime;
        public int ProcessedTick;
    }
}
