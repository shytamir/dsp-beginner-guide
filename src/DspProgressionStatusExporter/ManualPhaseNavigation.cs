using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class ManualPhaseSelection
    {
        public string PhaseId;
        public string SeedSource;
        public string PersistenceState;
        public string IdentityVersion;
        public int IlsStage;
        public string IlsStageOrigin;

        public string Serialize()
        {
            return "nav3;phase=" + ManualPhaseNavigator.NormalizePhase(PhaseId) +
                ";seed=" + (String.IsNullOrEmpty(SeedSource)
                    ? "stored"
                    : SeedSource.Replace(";", "").Replace("=", "")) +
                (IlsStage >= 1 && IlsStage <= 3
                    ? ";ils=" + IlsStage + ";ilsOrigin=" +
                        (IlsStageOrigin ?? "stored").Replace(";", "").Replace("=", "")
                    : "");
        }

        public Dictionary<string, object> Export(string saveKey)
        {
            return new Dictionary<string, object> {
                { "contractVersion", "1.7" },
                { "authority", "player" },
                { "saveKey", saveKey },
                { "identityVersion", IdentityVersion },
                { "persistenceState", PersistenceState },
                { "selectedPhase", ManualPhaseNavigator.NormalizePhase(PhaseId) },
                { "selectionOrigin", SeedSource },
                { "ilsStage", IlsStage },
                { "ilsStageOrigin", IlsStageOrigin },
                { "automaticTransitionsEnabled", false }
            };
        }

        public static ManualPhaseSelection Parse(string serialized)
        {
            if (String.IsNullOrEmpty(serialized)) return null;
            string phase = null;
            string seed = null;
            int stage = 0;
            string stageOrigin = null;
            string[] parts = serialized.Split(';');
            if (parts.Length == 0 ||
                (!String.Equals(parts[0], "nav1", StringComparison.Ordinal) &&
                 !String.Equals(parts[0], "nav2", StringComparison.Ordinal) &&
                 !String.Equals(parts[0], "nav3", StringComparison.Ordinal)))
                return null;
            for (int i = 1; i < parts.Length; i++)
            {
                int equals = parts[i].IndexOf('=');
                if (equals <= 0) continue;
                string key = parts[i].Substring(0, equals);
                string value = parts[i].Substring(equals + 1);
                if (key == "phase") phase = value;
                else if (key == "seed") seed = value;
                else if (key == "ils" && parts[0] == "nav3") Int32.TryParse(value, out stage);
                else if (key == "ilsOrigin" && parts[0] == "nav3") stageOrigin = value;
            }
            phase = ManualPhaseNavigator.MigrateLegacyPhase(phase);
            if (!ManualPhaseNavigator.IsValidPhase(phase)) return null;
            return new ManualPhaseSelection {
                PhaseId = ManualPhaseNavigator.NormalizePhase(phase),
                SeedSource = String.IsNullOrEmpty(seed) ? "stored" : seed,
                IlsStage = stage >= 1 && stage <= 3 ? stage : 0,
                IlsStageOrigin = stage >= 1 && stage <= 3 ? stageOrigin ?? "stored" : null
            };
        }
    }

    internal sealed class PhaseSaveIdentity
    {
        public string SaveKey;
        public string Version;
        public bool Stable;

        public static PhaseSaveIdentity Build(
            string creationTime,
            string galaxySeed,
            string starCount,
            string sandboxMode,
            string fallbackGameName)
        {
            bool hasCreationTime =
                !String.IsNullOrWhiteSpace(creationTime) &&
                !String.Equals(creationTime, "0", StringComparison.Ordinal);
            string version = hasCreationTime
                ? "creation-time-v2"
                : "game-name-fallback-v2";
            string identity = String.Join(
                "|",
                new string[] {
                    version,
                    hasCreationTime ? creationTime : fallbackGameName ?? "",
                    galaxySeed ?? "",
                    starCount ?? "",
                    sandboxMode ?? ""
                });
            return new PhaseSaveIdentity {
                SaveKey = "save2-" + Hash(identity),
                Version = version,
                Stable = hasCreationTime
            };
        }

        public static string BuildLegacyKey(
            string gameName,
            string saveName,
            string galaxySeed,
            string starCount,
            string sandboxMode)
        {
            string identity = String.Join(
                "|",
                new string[] {
                    gameName ?? "",
                    saveName ?? "",
                    galaxySeed ?? "",
                    starCount ?? "",
                    sandboxMode ?? ""
                });
            return "save-" + Hash(identity);
        }

        private static string Hash(string identity)
        {
            uint hash = 2166136261;
            for (int i = 0; i < identity.Length; i++)
            {
                hash ^= identity[i];
                hash *= 16777619;
            }
            return hash.ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    internal static class ManualPhaseNavigator
    {
        public static bool EnsureIlsStage(ManualPhaseSelection selection, ObservedGameState state)
        {
            if (selection.PhaseId != "ils" || selection.IlsStage >= 1 && selection.IlsStage <= 3 || state == null)
                return false;
            selection.IlsStage = 1;
            selection.IlsStageOrigin = "departure-default";
            if (state.UnlockedTechIds.Contains(1605))
            {
                selection.IlsStage = 3;
                selection.IlsStageOrigin = "ils-researched";
            }
            else if (state.StarterPlanetId > 0)
            {
                bool away = state.PlayerPlanetId > 0 && state.PlayerPlanetId != state.StarterPlanetId;
                bool production = false;
                foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                    if (flow.PlanetId > 0 && flow.PlanetId != state.StarterPlanetId &&
                        (flow.ItemId == 1105 || flow.ItemId == 1106) &&
                        flow.OneMinuteAvailable && flow.ProducedPerMinute > 0)
                        production = true;
                if (away || production)
                {
                    selection.IlsStage = 2;
                    selection.IlsStageOrigin = away ? "non-birth-location" : "remote-finished-production";
                }
            }
            return true;
        }

        public static bool ApplyCommand(ManualPhaseSelection selection, string command)
        {
            string current = NormalizePhase(selection.PhaseId);
            string target = command == "previous" ? Previous(current) :
                command == "next" ? Next(current) : current;
            int stage = command == "ils-1" ? 1 : command == "ils-2" ? 2 : command == "ils-3" ? 3 : 0;
            if (current == "ils" && stage > 0 && stage != selection.IlsStage)
            {
                selection.IlsStage = stage;
                selection.IlsStageOrigin = "manual-control";
            }
            else if (target != current)
            {
                selection.PhaseId = target;
                selection.SeedSource = "manual-control";
            }
            else return false;
            selection.PersistenceState = "updated-by-player";
            return true;
        }

        private static readonly string[] Phases = new string[] {
            "blue", "red", "ils", "yellow",
            "purple", "green", "dyson", "photon", "white"
        };

        private static readonly HashSet<string> ValidPhases =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "blue", "red", "ils", "yellow",
                "purple", "green", "dyson", "photon", "white"
            };

        public static ManualPhaseSelection Seed(
            HashSet<int> unlockedTechIds)
        {
            string phase = "blue";
            if (unlockedTechIds != null)
            {
                if (unlockedTechIds.Contains(1508) ||
                    unlockedTechIds.Contains(1507)) phase = "white";
                else if (unlockedTechIds.Contains(1705)) phase = "green";
                else if (unlockedTechIds.Contains(1312)) phase = "purple";
                else if (unlockedTechIds.Contains(1124)) phase = "yellow";
                else if (unlockedTechIds.Contains(1111)) phase = "red";
                else if (unlockedTechIds.Contains(1002)) phase = "blue";
            }
            return new ManualPhaseSelection {
                PhaseId = phase,
                SeedSource = "latest-researched-cube"
            };
        }

        public static bool IsValidPhase(string phaseId)
        {
            return !String.IsNullOrEmpty(phaseId) &&
                ValidPhases.Contains(phaseId);
        }

        public static string NormalizePhase(string phaseId)
        {
            phaseId = MigrateLegacyPhase(phaseId);
            return IsValidPhase(phaseId)
                ? phaseId.ToLowerInvariant()
                : "blue";
        }

        public static string MigrateLegacyPhase(string phaseId)
        {
            if (String.Equals(
                phaseId, "bootstrap", StringComparison.OrdinalIgnoreCase))
                return "blue";
            if (String.Equals(phaseId, "flight", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(phaseId, "titanium", StringComparison.OrdinalIgnoreCase))
                return "ils";
            if (String.Equals(phaseId, "sphere", StringComparison.OrdinalIgnoreCase))
                return "dyson";
            if (String.Equals(phaseId, "warp", StringComparison.OrdinalIgnoreCase))
                return "green";
            if (String.Equals(phaseId, "logistics", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(phaseId, "complete", StringComparison.OrdinalIgnoreCase))
                return "white";
            return phaseId;
        }

        public static string Previous(string phaseId)
        {
            string phase = NormalizePhase(phaseId);
            int index = Array.IndexOf(Phases, phase);
            return index > 0 ? Phases[index - 1] : phase;
        }

        public static string Next(string phaseId)
        {
            string phase = NormalizePhase(phaseId);
            int index = Array.IndexOf(Phases, phase);
            return index >= 0 && index + 1 < Phases.Length
                ? Phases[index + 1]
                : phase;
        }
    }
}
