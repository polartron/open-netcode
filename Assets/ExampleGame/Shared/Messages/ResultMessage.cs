using System;
using OpenNetcode.Movement.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Networking.Transport;

namespace Shared.Messages
{
    public struct ResultMessage : IResultMessage<EntityPosition>
    {
        public int Tick { get; set; }
        public bool HasInput { get; set; }
        public int ProcessedTimeMs { get; set; }
        
        public EntityPosition Prediction => Position;
        public EntityPosition Position;
        public EntityVelocity Velocity;

        public void Apply(in EntityManager entityManager, in Entity entity)
        {
            entityManager.SetComponentData(entity, Position);
            entityManager.SetComponentData(entity, Velocity);
        }
        
        public void Write(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            Packets.WritePacketType(PacketType.Result, ref writer);
            writer.WritePackedUInt((uint) Tick, compressionModel);
            Position.Write(ref writer, compressionModel);
            Velocity.Write(ref writer, compressionModel);
        }

        public void Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel)
        {
            Packets.ReadPacketType(ref reader);
            HasInput = Convert.ToBoolean(reader.ReadRawBits(1));
            Tick = (int) reader.ReadPackedUInt(compressionModel);
            ProcessedTimeMs = (int) reader.ReadPackedUInt(compressionModel);
            Position.Read(ref reader, compressionModel);
            Velocity.Read(ref reader, compressionModel);
        }

        public int Hash()
        {
            return Position.Hash() ^ Velocity.Hash();
        }
    }
}
