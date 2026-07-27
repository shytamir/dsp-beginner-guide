using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class ManualPhaseSelection
    {
        public string PhaseId;
        public string LateRoute;
        public string SeedSource;

        public string Serialize()
        {
            return "nav1;phase=" + ManualPhaseNavigator.NormalizePhase(PhaseId) +
                ";route=" + ManualPhaseNavigator.NormalizeLateRoute(LateRoute) +
                ";seed=" + (String.IsNullOrEmpty(SeedSource)
                    ? "stored"
                    : SeedSource.Replace(";", "").Replace("=", ""));
        }

        public Dictionary<string, object> Export(string saveKey)
        {
            return new Dictionary<string, object> {
                { "contractVersion", "1.2" },
                { "authority", "player" },
                { "saveKey", saveKey },
                { "selectedPhase", ManualPhaseNavigator.NormalizePhase(PhaseId) },
                { "selectedLateRoute", ManualPhaseNavigator.NormalizeLateRoute(LateRoute) },
                { "selectionOrigin", SeedSource },
                { "automaticTransitionsEnabled", false }
            };
        }

        public static ManualPhaseSelection Parse(string serialized)
        {
            if (String.IsNullOrEmpty(serialized)) return null;
            string phase = null;
            string route = null;
            string seed = null;
            string[] parts = serialized.Split(';');
            if (parts.Length == 0 ||
                !String.Equals(parts[0], "nav1", StringComparison.Ordinal))
                return null;
            for (int i = 1; i < parts.Length; i++)
            {
                int equals = parts[i].IndexOf('=');
                if (equals <= 0) continue;
                string key = parts[i].Substring(0, equals);
                string value = parts[i].Substring(equals + 1);
                if (key == "phase") phase = value;
                else if (key == "route") route = value;
                else if (key == "seed") seed = value;
            }
            if (String.Equals(
                phase, "complete", StringComparison.OrdinalIgnoreCase))
                phase = "white";
            if (!ManualPhaseNavigator.IsValidPhase(phase)) return null;
            return new ManualPhaseSelection {
                PhaseId = ManualPhaseNavigator.NormalizePhase(phase),
                LateRoute = ManualPhaseNavigator.NormalizeLateRoute(route),
                SeedSource = String.IsNullOrEmpty(seed) ? "stored" : seed
            };
        }
    }

    internal static class ManualPhaseNavigator
    {
        private static readonly string[] BeforeLateRoute = new string[] {
            "bootstrap", "blue", "red", "flight", "titanium",
            "yellow", "ils", "purple", "green"
        };

        private static readonly HashSet<string> ValidPhases =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "bootstrap", "blue", "red", "flight", "titanium",
                "yellow", "ils", "purple", "warp", "green", "dyson",
                "sphere", "photon", "white"
            };

        public static ManualPhaseSelection Seed(
            HashSet<int> unlockedTechIds)
        {
            string phase = "bootstrap";
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
                LateRoute = "",
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
            return IsValidPhase(phaseId)
                ? phaseId.ToLowerInvariant()
                : "bootstrap";
        }

        public static string NormalizeLateRoute(string route)
        {
            if (String.Equals(route, "sphere", StringComparison.OrdinalIgnoreCase))
                return "sphere";
            if (String.Equals(route, "dyson", StringComparison.OrdinalIgnoreCase))
                return "dyson";
            return "";
        }

        public static string Previous(string phaseId, string lateRoute)
        {
            string phase = NormalizePhase(phaseId);
            if (phase == "warp") return "purple";
            if (phase == "dyson" || phase == "sphere") return "green";
            if (phase == "photon")
            {
                string route = NormalizeLateRoute(lateRoute);
                return String.IsNullOrEmpty(route) ? "dyson" : route;
            }
            if (phase == "white") return "photon";
            int index = Array.IndexOf(BeforeLateRoute, phase);
            return index > 0 ? BeforeLateRoute[index - 1] : phase;
        }

        public static string Next(string phaseId, string lateRoute)
        {
            string phase = NormalizePhase(phaseId);
            if (phase == "warp") return "green";
            if (phase == "dyson" || phase == "sphere") return "photon";
            if (phase == "photon") return "white";
            if (phase == "white") return phase;
            if (phase == "green")
            {
                string route = NormalizeLateRoute(lateRoute);
                return String.IsNullOrEmpty(route) ? phase : route;
            }
            int index = Array.IndexOf(BeforeLateRoute, phase);
            return index >= 0 && index + 1 < BeforeLateRoute.Length
                ? BeforeLateRoute[index + 1]
                : phase;
        }
    }
}
