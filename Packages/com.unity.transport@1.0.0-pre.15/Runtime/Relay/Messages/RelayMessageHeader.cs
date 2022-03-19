using System.Runtime.InteropServices;

namespace Unity.Networking.Transport.Relay
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RelayMessageHeader
    {
        public const int Length = 4;

        public ushort Signature;
        public byte Version;
        public RelayMessageType Type;

        public bool IsValid()
        {
            return Signature == 0x72DA && Version == 0;
        }

        internal static RelayMessageHeader Create(RelayMessageType type)
        {
            return new RelayMessageHeader
            {
                Signature = 0x72DA,
                Version = 0,
                Type = type,
            };
        }
    }

    internal enum RelayMessageType : byte
    {
        Bind = 0,
        BindReceived = 1,
        Ping = 2,
        ConnectRequest = 3,
        Accepted = 6,
        Disconnect = 9,
        Relay = 10,
        Error = 12,
    }
}
