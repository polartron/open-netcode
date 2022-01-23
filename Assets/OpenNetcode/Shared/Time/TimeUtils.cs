using System;

namespace Shared.Time
{
    public class TimeUtils
    {
        public static long CurrentTimeInMs()
        {
            return (long) (UnityEngine.Time.time * 1000f);
        }
    }
}