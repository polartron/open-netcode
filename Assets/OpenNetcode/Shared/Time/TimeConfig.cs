namespace OpenNetcode.Shared.Time
{
    public static class TimeConfig
    {
        // CommandBufferLengthMs = 1000 * (1 / TicksPerSecond) * 3;
#if TICKRATE_128
        public const int TicksPerSecond = 128;
        public const float FixedDeltaTime = 0.0078125f;
        public const long CommandBufferLengthMs = 23;
#elif TICKRATE_32
        public const int TicksPerSecond = 32;
        public const float FixedDeltaTime = 0.03125f;
        public const long CommandBufferLengthMs = 93;
#elif TICKRATE_16
        public const int TicksPerSecond = 16;
        public const float FixedDeltaTime = 0.0625f
        public const long CommandBufferLengthMs = 187;
#elif TICKRATE_8
        public const int TicksPerSecond = 8;
        public const float FixedDeltaTime = 0.125f
        public const long CommandBufferLengthMs = 375;
#else // 64
        public const int TicksPerSecond = 64;
        public const float FixedDeltaTime = 0.015625f;
        public const long CommandBufferLengthMs = 187;
#endif

#if SNAPSHOTRATE_128
        public const int SnapshotsPerSecond = 128;
#elif SNAPSHOTRATE_32
        public const int SnapshotsPerSecond = 32;
#elif SNAPSHOTRATE_16
        public const int SnapshotsPerSecond = 16;
#elif SNAPSHOTRATE_8
        public const int SnapshotsPerSecond = 8;
#elif SNAPSHOTRATE_4
        public const int SnapshotsPerSecond = 4
#elif SNAPSHOTRATE_2
        public const int SnapshotsPerSecond = 2
#else
        public const int SnapshotsPerSecond = 16;
#endif
    }
}