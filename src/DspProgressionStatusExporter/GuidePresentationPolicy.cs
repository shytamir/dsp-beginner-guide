using BepInEx.Configuration;

namespace DspProgressionStatusExporter
{
    internal sealed class GuidePresentationPolicy
    {
        public const bool DefaultExpertMode = false;
        public readonly bool ExpertMode;
        public bool GuidanceEnabled { get { return !ExpertMode; } }
        public bool SnapshotEnabled { get { return GuidanceEnabled && BuildFeatures.SnapshotControlEnabled; } }

        public GuidePresentationPolicy(bool expertMode = DefaultExpertMode)
        { ExpertMode = expertMode; }

        internal static bool BindExpertMode(ConfigFile config)
        {
            return config.Bind("General", "ExpertMode", DefaultExpertMode,
                "Show only the Cube-rate bar and DON'T PANIC button. Restart the game after changing this setting.").Value;
        }
    }
}
