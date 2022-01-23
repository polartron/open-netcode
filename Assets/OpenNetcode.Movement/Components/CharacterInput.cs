using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace OpenNetcode.Movement.Components
{
    [GenerateAuthoringComponent]
    public struct CharacterInput : INetworkedComponent
    {
        public float2 Move;
        public float Rotation;
        
        public void Write(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            writer.WritePackedUInt(Compress(Move, Rotation), compressionModel);
        }
        
        public void Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel)
        {
            Decompress(reader.ReadPackedUInt(compressionModel), out Move, out Rotation);
        }

        public int Hash()
        {
            return Move.GetHashCode() ^ Rotation.GetHashCode();
        }

        public static void Decompress(uint compressedInput, out float2 input, out float rotation01, int inputBits = 5, int rotationBits = 8)
        {
            uint y = (compressedInput >> 13) & (uint) (1 << inputBits) - 1;
            uint x = (compressedInput >> 8) & (uint) (1 << inputBits) - 1;
            uint r = compressedInput & (uint) (1 << rotationBits) - 1;
        
            uint rotationMax = (uint) Mathf.Pow(2, rotationBits) - 1;
            uint inputMax = (uint) Mathf.Pow(2, inputBits);
            uint maxHalf = inputMax / 2;
        
            rotation01 = (float) r / rotationMax;
        
            float xFloat = (float) (x + 1) / maxHalf - 1f;
            float yFloat = (float) (y + 1) / maxHalf - 1f;
            input = new Vector2(xFloat, yFloat);
        }
        
        public static uint Compress(float2 input, float rotation01, int inputBits = 5, int rotationBits = 8)
        {
            float x = Mathf.Clamp01((input.x + 1f) / 2f);
            float y = Mathf.Clamp01((input.y + 1f) / 2f);
        
            uint rotationMax = (uint) Mathf.Pow(2, rotationBits) - 1;
            uint inputMax = (uint) Mathf.Pow(2, inputBits) - 1;
        
            uint rInt = (uint) (Mathf.Clamp01(rotation01) * rotationMax);
            uint xInt = (uint) (x * inputMax);
            uint yInt = (uint) (y * inputMax);
        
            uint compressed = UpdateBits(0, rInt, 0, 8);
            compressed = UpdateBits(compressed, xInt, rotationBits, 13);
            compressed = UpdateBits(compressed, yInt, rotationBits + inputBits, 18);
        
            return compressed;
        }
        
        internal static uint UpdateBits(uint n, uint m, int i, int j)
        {
            uint allOnes = 0;
            uint left = allOnes << (j + 1);
            uint right = ((uint) (1 << i) - 1);
            uint mask = left | right;
            uint cleared = n & mask;
            uint shifted = m << i;
            return cleared | shifted;
        }


    }
}
