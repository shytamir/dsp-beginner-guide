using BepInEx.Configuration;

namespace DspProgressionStatusExporter
{
    internal sealed class GuidePresentationPolicy
    {
        public const bool DefaultExpertMode = false;
        public const string ExpertPhaseId = "white";
        public readonly bool ExpertMode;
        public bool GuidanceEnabled { get { return !ExpertMode; } }
        public bool SnapshotEnabled { get { return GuidanceEnabled && BuildFeatures.SnapshotControlEnabled; } }

        public GuidePresentationPolicy(bool expertMode = DefaultExpertMode)
        { ExpertMode = expertMode; }

        internal static bool BindExpertMode(ConfigFile config)
        {
            return config.Bind("General", "ExpertMode", DefaultExpertMode,
                "Show all six Cube counters and DON'T PANIC using the WHITE phase. Restart the game after changing this setting.").Value;
        }
    }
}
