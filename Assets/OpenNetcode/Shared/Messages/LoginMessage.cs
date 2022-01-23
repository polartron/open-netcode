using System;
using Unity.Collections;
using Unity.Networking.Transport;

namespace OpenNetcode.Shared.Messages
{
    public struct LoginMessage
    {
        public bool Guest;
        public FixedString512Bytes SessionTicket;
        
        public static bool Read(ref NativeHashMap<int, LoginMessage> logins, ref DataStreamReader reader, int internalId)
        {
            Packets.ReadPacketType(ref reader);
            uint guest = reader.ReadRawBits(1);
            if (guest == 1)
            {
                logins.TryAdd(internalId, new LoginMessage()
                {
                    Guest = true,
                    SessionTicket = ""
                });
                
                return !reader.HasFailedReads;
            }

            FixedString512Bytes sessionTicket = reader.ReadFixedString512();
            logins.TryAdd(internalId, new LoginMessage()
            {
                Guest = true,
                SessionTicket = sessionTicket,
            });
            
            return !reader.HasFailedReads;
        }
        
        public bool Write(ref DataStreamWriter writer)
        {
            Packets.WritePacketType(PacketType.Login, ref writer);
            writer.WriteRawBits(Convert.ToUInt32(Guest), 1);
            
            if (!Guest)
            {
                writer.WriteFixedString512(SessionTicket);
            }

            return !writer.HasFailedWrites;
        }
    }
}
