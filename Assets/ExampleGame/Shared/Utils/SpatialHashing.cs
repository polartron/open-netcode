using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Shared.Utils
{
    public struct SpatialHashing
    {
        public static readonly int HashSegments = 1000;
        public static readonly int AreaSize = 10;

        public static int HashKey(float3 position)
        {
            return (int) (math.floor(position.x / AreaSize) + (HashSegments * math.floor(position.z / AreaSize)));
        }

        public static int Sectors(float3 position, float factor)
        {
            int radius = (int) math.max(1, math.ceil(math.length(position) / factor));
            return radius * 2;
        }

        public static int SectorHashKey(float3 position, float factor)
        {
            int sectors = Sectors(position, factor);
            float angle = math.atan2(position.x, position.z) / math.PI;
            
            int sector = (int) math.floor(sectors * angle);

            return sectors + sector;
        }

        public static int SectorOffset(int sectors, int value, int offset)
        {
            int max = sectors * 2;
            int min = sectors + 1;

            if (value + offset > max)
            {
                return (value + offset) - sectors;
            }
            else if (value + offset < min)
            {
                return (value + offset) + sectors;
            }

            if (value + offset < 0)
                return 0;
            
            return value + offset;
        }
    }
}
