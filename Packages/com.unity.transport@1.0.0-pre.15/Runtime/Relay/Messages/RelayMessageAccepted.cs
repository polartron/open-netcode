using System.Runtime.InteropServices;

namespace Unity.Networking.Transport.Relay
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RelayMessageAccepted
    {
        public const int Length = RelayMessageHeader.Length + RelayAllocationId.k_Length * 2; // Header + FromAllocationId + ToAllocationId

        public RelayMessageHeader Header;

        public RelayAllocationId FromAllocationId;
        public RelayAllocationId ToAllocationId;

        internal static RelayMessageAccepted Create(RelayAllocationId fromAllocationId, RelayAllocationId toAllocationId)
        {
            return new RelayMessageAccepted
            {
                Header = RelayMessageHeader.Create(RelayMessageType.Accepted),
                FromAllocationId = fromAllocationId,
                ToAllocationId = toAllocationId
            };
        }
    }
}
