using System;

namespace Shared.Time
{
    public class TimeUtils
    {
        public static double CurrentTimeInMs()
        {
            return (UnityEngine.Time.time * 1000f);
        }
    }
}