namespace OpenNetcode.Shared.Time
{
    public static class TimeConfig
    {
        public static readonly int TicksPerSecond = 32;
        public static readonly int SnapshotsPerSecond = 16;
        private static readonly int InputBufferLengthTicks = 3;
        private static readonly int MaxDilationFrames = 4;

        public static readonly long CommandBufferLengthMs = (long) (1000f * (1f / TicksPerSecond) * InputBufferLengthTicks);
        public static readonly long MaxDilationMs = (long) (1000f * (1f / TicksPerSecond) * MaxDilationFrames);
        public static readonly float FixedDeltaTime = 1f / TicksPerSecond;
        public static readonly int BaseSnapshotEvery = SnapshotsPerSecond;
    }
}