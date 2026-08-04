namespace DspProgressionStatusExporter
{
    public static class BuildFeatures
    {
#if DSP_GUIDE_SNAPSHOT_CONTROL
        public const string Variant = "diagnostic";
        public const bool SnapshotControlEnabled = true;
#else
        public const string Variant = "public";
        public const bool SnapshotControlEnabled = false;
#endif
    }
}
