using System;
using OpenNetcode.Shared.Components;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace Shared.Coordinates
{
    [Serializable]
    public struct GameUnits : IEquatable<GameUnits>, INetworkedComponent
    {
        public int x;
        public int y;
        public int z;

        public GameUnits(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool Equals(GameUnits other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is GameUnits other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                return hashCode;
            }
        }

        public static GameUnits operator+(GameUnits a, GameUnits b)
        {
            return new GameUnits(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static GameUnits operator-(GameUnits a, GameUnits b)
        {
            return new GameUnits(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static GameUnits operator*(GameUnits a, GameUnits b)
        {
            return new GameUnits(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static GameUnits operator-(GameUnits a)
        {
            return new GameUnits(-a.x, -a.y, -a.z);
        }

        public static GameUnits operator*(GameUnits a, int b)
        {
            return new GameUnits(a.x * b, a.y * b, a.z * b);
        }

        public static GameUnits operator*(int a, GameUnits b)
        {
            return new GameUnits(a * b.x, a * b.y, a * b.z);
        }

        public static GameUnits operator*(GameUnits b, float a)
        {
            float x = b.x * a;
            float y = b.y * a;
            float z = b.z * a;

            return new GameUnits((int) x, (int) y, (int) z);
        }

        public static GameUnits operator/(GameUnits a, int b)
        {
            return new GameUnits(a.x / b, a.y / b, a.z / b);
        }

        public static bool operator==(GameUnits lhs, GameUnits rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        public static bool operator!=(GameUnits lhs, GameUnits rhs)
        {
            return !(lhs == rhs);
        }

        public float3 ToUnityVector3(int unitsPerMeter)
        {
            float factor = 1.0f / unitsPerMeter;

            return new float3(x * factor, y * factor, z * factor);
        }

        public float3 ToUnityVector3()
        {
            return ToUnityVector3(FloatingOrigin.UnitsPerMeter);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }

        public static GameUnits FromUnityVector3(Vector3 vector)
        {
            return FromUnityVector3(vector, FloatingOrigin.UnitsPerMeter);
        }

        public static GameUnits FromUnityVector3(Vector3 vector, int unitsPerMeter)
        {
            int x = (int) Mathf.RoundToInt(vector.x * unitsPerMeter);
            int y = (int) Mathf.RoundToInt(vector.y * unitsPerMeter);
            int z = (int) Mathf.RoundToInt(vector.z * unitsPerMeter);

            return new GameUnits(x, y, z);
        }

        public static double Distance(GameUnits a, GameUnits b)
        {
            float num1 = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;

            return Math.Sqrt(num1 * num1 + num2 * num2 + num3 * num3);
        }

        public static float Dot(GameUnits lhs, GameUnits rhs)
        {
            return (float) ((double) lhs.x * (double) rhs.x + (double) lhs.y * (double) rhs.y + (double) lhs.z * (double) rhs.z);
        }

        public static GameUnits Lerp(GameUnits a, GameUnits b, double t)
        {
            t = t > 1.0 ? 1 : t;

            //Will this create precision issues with large differences?

            int x = (int) (a.x + (b.x - a.x) * t);
            int y = (int) (a.y + (b.y - a.y) * t);
            int z = (int) (a.z + (b.z - a.z) * t);

            return new GameUnits(x, y, z);
        }

        static readonly GameUnits zeroCoordinate = new GameUnits(0, 0, 0);
        public static GameUnits zero { get { return zeroCoordinate; } }
        
        public void Write(ref DataStreamWriter writer, in NetworkCompressionModel compressionModel)
        {
            writer.WritePackedInt(x, compressionModel);
            writer.WritePackedInt(y, compressionModel);
            writer.WritePackedInt(z, compressionModel); 
        }

        public void Read(ref DataStreamReader reader, in NetworkCompressionModel compressionModel)
        {
            x = reader.ReadPackedInt(compressionModel);
            y = reader.ReadPackedInt(compressionModel);
            z = reader.ReadPackedInt(compressionModel);
        }

        public int Hash()
        {
            return GetHashCode();
        }
    }
}