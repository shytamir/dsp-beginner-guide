using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Pure normalized-state consumer. It contains no runtime reflection or UI.
    /// </summary>
    internal static class GuideAnalyzer
    {
        private sealed class Phase
        {
            public string Id;
            public string Title;
            public int GateTechId;
            public int NextTechId;
            public string NextResearch;
        }

        private static readonly Phase[] Phases = new Phase[] {
            new Phase { Id = "blue", Title = "Sustain the Blue Cube science loop", GateTechId = 1002, NextTechId = 1111, NextResearch = "Energy Matrix" },
            new Phase { Id = "red", Title = "Sustain Red Cubes without refinery deadlock", GateTechId = 1111, NextTechId = 2902, NextResearch = "Drive Engine Lv2" },
            new Phase { Id = "ils", Title = "Complete the first interplanetary logistics expedition", GateTechId = 2902, NextTechId = 1124, NextResearch = "Structure Matrix" },
            new Phase { Id = "yellow", Title = "Run three continuous Yellow Cube labs", GateTechId = 1124, NextTechId = 1312, NextResearch = "Information Matrix" },
            new Phase { Id = "purple", Title = "Run three continuous Purple Cube labs", GateTechId = 1312, NextTechId = 1705, NextResearch = "Gravity Matrix" },
            new Phase { Id = "green", Title = "Run two continuous Green Cube labs", GateTechId = 1705, NextTechId = 1505, NextResearch = "Planetary Ionosphere Utilization" },
            new Phase { Id = "dyson", Title = "Build and sustain the Dyson swarm", GateTechId = 1505, NextTechId = 1506, NextResearch = "Dirac Inversion Mechanism" },
            new Phase { Id = "photon", Title = "Run the critical-photon receiver array", GateTechId = 1506, NextTechId = 1507, NextResearch = "Universe Matrix" },
            new Phase { Id = "white", Title = "Complete the main progression route", GateTechId = 1507, NextTechId = 1508, NextResearch = "Mission Completed" }
        };

        public static Dictionary<string, object> AnalyzeSelected(
            ObservedGameState state,
            string selectedPhaseId)
        {
            selectedPhaseId = ManualPhaseNavigator.NormalizePhase(selectedPhaseId);
            GuideProgressionEvaluation progression =
                GuideGateEngine.EvaluatePhase(selectedPhaseId, state);
            Phase phase = FindPhase(selectedPhaseId) ?? FindPhase("blue");
            var findings = new List<object>();

            if (phase.Id == "red")
                AddRefineryCongestionFinding(findings, state);
            if (phase.Id == "purple")
                AddDrainingInputFinding(findings, state,
                    "purple-support-drain", "Purple",
                    new int[] { 1303, 1124, 1402 },
                    new string[] { "Processors", "Carbon Nanotubes", "Particle Broadband" });
            if (phase.Id == "green")
                AddDrainingInputFinding(findings, state,
                    "green-support-drain", "Green",
                    new int[] { 1305, 1209 },
                    new string[] { "Quantum Chips", "Graviton Lenses" });
            if (phase.Id == "photon")
                AddPhotonPowerFinding(findings, state);
            if (phase.Id == "white")
                AddDrainingInputFinding(findings, state,
                    "white-feeder-drain", "White",
                    new int[] { 6001, 6002, 6003, 6004, 6005, 1122 },
                    new string[] {
                        "Blue Cubes", "Red Cubes", "Yellow Cubes",
                        "Purple Cubes", "Green Cubes", "Antimatter"
                    });

            var phaseResult = new Dictionary<string, object> {
                { "id", phase.Id },
                { "title", phase.Title },
                { "gateTechId", phase.GateTechId },
                { "nextTechId", phase.NextTechId },
                { "nextResearch", phase.NextResearch },
                { "basis", "Player-selected phase; runtime evidence evaluates this phase but cannot change it." }
            };

            return new Dictionary<string, object> {
                { "analysisVersion", "2.8" },
                { "phaseSelectionAuthority", "player" },
                { "phase", phaseResult },
                { "progression", progression.Export() },
                { "normalizedState", state.Export() },
                { "findings", findings },
                { "limitations", new List<object> {
                    "A technology unlock proves availability, not that the corresponding factory objective is complete.",
                    "Production claims require a valid native Statistics Panel observation window.",
                    "Only positively observed runtime evidence is reported as ready; player checks remain explicit."
                } }
            };
        }

        private static Phase FindPhase(string id)
        {
            foreach (Phase phase in Phases)
                if (String.Equals(phase.Id, id, StringComparison.OrdinalIgnoreCase))
                    return phase;
            return null;
        }

        private static void AddPhotonPowerFinding(
            List<object> findings,
            ObservedGameState state)
        {
            if (!state.Dyson.Available ||
                state.Dyson.ConfiguredPhotonReceiverCount <= 0) return;
            double requested = state.Dyson.ReceiverArrayRequestedDysonPowerWatts;
            double supplied = state.Dyson.ReceiverArraySuppliedPowerWatts;
            double available = state.Dyson.GenerationWatts;
            findings.Add(Finding(
                "photon-receiver-power",
                requested > available || available <= 0 ? "watch" : "ready",
                "Photon receivers request " + FormatPower(requested) +
                    " from " + FormatPower(available) + " of Dyson generation.",
                "The receiver array is currently supplied with " + FormatPower(supplied) + ".",
                "high"));
        }

        private static void AddDrainingInputFinding(
            List<object> findings,
            ObservedGameState state,
            string id,
            string phaseName,
            int[] itemIds,
            string[] names)
        {
            if (!state.ProductionWindowReady) return;

            int weakestIndex = -1;
            double weakestRatio = Double.MaxValue;
            ObservedItemFlow weakestFlow = null;
            for (int i = 0; i < itemIds.Length; i++)
            {
                ObservedItemFlow flow;
                if (!state.ItemFlows.TryGetValue(itemIds[i], out flow) ||
                    flow == null || flow.ConsumedPerMinute <= 0 ||
                    flow.ProducedPerMinute >= flow.ConsumedPerMinute)
                    continue;
                double ratio = flow.ProducedPerMinute / flow.ConsumedPerMinute;
                if (ratio >= weakestRatio) continue;
                weakestIndex = i;
                weakestRatio = ratio;
                weakestFlow = flow;
            }
            if (weakestIndex < 0) return;

            findings.Add(Finding(
                id,
                weakestFlow.ProducedPerMinute <= 0 ? "blocked" : "watch",
                names[weakestIndex] + " are draining faster than they are replenished.",
                "Found " + Math.Round(weakestFlow.ProducedPerMinute, 1) +
                    "/min produced and " +
                    Math.Round(weakestFlow.ConsumedPerMinute, 1) +
                    "/min consumed; reinforce this " + phaseName + " input.",
                "high"));
        }

        private static string FormatPower(double watts)
        {
            if (watts >= 1000000000.0)
                return Math.Round(watts / 1000000000.0, 3) + " GW";
            if (watts >= 1000000.0)
                return Math.Round(watts / 1000000.0, 1) + " MW";
            if (watts >= 1000.0)
                return Math.Round(watts / 1000.0, 1) + " kW";
            return Math.Round(watts, 0) + " W";
        }

        private static void AddRefineryCongestionFinding(
            List<object> findings,
            ObservedGameState state)
        {
            if (!state.ProductionWindowReady) return;

            ObservedItemFlow flow;
            ObservedCapacity storage;
            if (!state.ItemFlows.TryGetValue(1114, out flow) || flow == null ||
                !state.TankStorage.TryGetValue(1114, out storage) ||
                storage.Capacity <= 0)
                return;

            double net = flow.ProducedPerMinute - flow.ConsumedPerMinute;
            double fill = storage.Count * 1.0 / storage.Capacity;
            double minutesRemaining = net > 0
                ? (storage.Capacity - storage.Count) / net
                : Double.PositiveInfinity;
            if (fill < 0.80 && minutesRemaining > 20.0) return;

            string status = fill >= 0.98 || minutesRemaining <= 2.0
                ? "blocked" : "watch";
            string remaining = Double.IsInfinity(minutesRemaining)
                ? "not filling at the current net rate"
                : "about " + Math.Max(0.0, Math.Round(minutesRemaining, 1)) +
                    " production minutes remain";
            findings.Add(Finding(
                "refined-oil-congestion",
                status,
                "Refined Oil tanks are " + Math.Round(fill * 100.0, 1) +
                    "% full; " + remaining + ".",
                "Use Refined Oil or expand storage to avoid refinery bottlenecks.",
                "high"));
        }

        private static Dictionary<string, object> Finding(
            string id,
            string status,
            string claim,
            string evidence,
            string confidence)
        {
            return new Dictionary<string, object> {
                { "id", id },
                { "status", status },
                { "claim", claim },
                { "evidence", evidence },
                { "confidence", confidence }
            };
        }
    }
}
