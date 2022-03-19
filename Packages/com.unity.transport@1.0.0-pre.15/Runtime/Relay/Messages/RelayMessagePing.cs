using System.Runtime.InteropServices;

namespace Unity.Networking.Transport.Relay
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RelayMessagePing
    {
        public const int Length = RelayMessageHeader.Length + RelayAllocationId.k_Length + 2; // Header + FromAllocationId + SequenceNumber

        public RelayMessageHeader Header;
        public RelayAllocationId FromAllocationId;
        public ushort SequenceNumber;

        internal static RelayMessagePing Create(RelayAllocationId fromAllocationId, ushort dataLength)
        {
            return new RelayMessagePing
            {
                Header = RelayMessageHeader.Create(RelayMessageType.Ping),
                FromAllocationId = fromAllocationId,
                SequenceNumber = 1
            };
        }
    }
}
