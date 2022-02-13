using System;
using System.Collections;
using System.Collections.Generic;
using Shared.Coordinates;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public static class TypeExtensions
{
    // Write
    
    public static void Write(this GameUnits value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in GameUnits baseline)
    {
        (value - baseline).Write(ref writer, compressionModel);
    }

    public static void Write(this bool value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, bool baseline)
    {
        writer.WriteRawBits(Convert.ToUInt32(value), 1);
    }
    
    public static void Write(this int value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, int baseline)
    {
        writer.WritePackedIntDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this uint value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, uint baseline)
    {
        writer.WritePackedUIntDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this long value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, long baseline)
    {
        writer.WritePackedLongDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this ulong value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, ulong baseline)
    {
        writer.WritePackedULongDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this short value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, short baseline)
    {
        writer.WritePackedIntDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this ushort value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, ushort baseline)
    {
        writer.WritePackedUIntDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this float value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, float baseline)
    {
        writer.WritePackedFloatDelta(value, baseline, compressionModel);
    }
    
    public static void Write(this FixedString32Bytes value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in FixedString32Bytes baseline)
    {
        writer.WritePackedFixedString32Delta(value, baseline, compressionModel);
    }
    
    public static void Write(this FixedString64Bytes value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in FixedString64Bytes baseline)
    {
        writer.WritePackedFixedString64Delta(value, baseline, compressionModel);
    }
    
    public static void Write(this FixedString128Bytes value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in FixedString128Bytes baseline)
    {
        writer.WritePackedFixedString128Delta(value, baseline, compressionModel);
    }
    
    public static void Write(this FixedString512Bytes value, ref DataStreamWriter writer, in NetworkCompressionModel compressionModel, in FixedString512Bytes baseline)
    {
        writer.WritePackedFixedString512Delta(value, baseline, compressionModel);
    }

    // Read
    
    public static void Read(this ref GameUnits value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in GameUnits baseline)
    {
        value.Read(ref reader, compressionModel);
        value += baseline;
    }

    public static void Read(this ref bool value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, bool baseline)
    {
        value = Convert.ToBoolean(reader.ReadRawBits(1));
    }

    public static void Read(this ref int value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, int baseline)
    {
        value = reader.ReadPackedIntDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref uint value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, uint baseline)
    {
        value = reader.ReadPackedUIntDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref long value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, long baseline)
    {
        value = reader.ReadPackedLongDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref ulong value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, ulong baseline)
    {
        value = reader.ReadPackedULongDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref short value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, short baseline)
    {
        value = (short) reader.ReadPackedIntDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref ushort value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, ushort baseline)
    {
        value = (ushort) reader.ReadPackedUIntDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref float value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, float baseline)
    {
        value = reader.ReadPackedFloatDelta(baseline, compressionModel);
    }
    
    public static void Read(this ref FixedString32Bytes value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in FixedString32Bytes baseline)
    {
        value = reader.ReadPackedFixedString32Delta(baseline, compressionModel);
    }
    
    public static void Read(this ref FixedString64Bytes value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in FixedString64Bytes baseline)
    {
        value = reader.ReadPackedFixedString64Delta(baseline, compressionModel);
    }
    
    public static void Read(this ref FixedString128Bytes value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in FixedString128Bytes baseline)
    {
        value = reader.ReadPackedFixedString128Delta(baseline, compressionModel);
    }
    
    public static void Read(this ref FixedString512Bytes value, ref DataStreamReader reader, in NetworkCompressionModel compressionModel, in FixedString512Bytes baseline)
    {
        value = reader.ReadPackedFixedString512Delta(baseline, compressionModel);
    }
}
