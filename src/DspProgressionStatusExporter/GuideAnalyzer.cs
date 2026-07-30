using System;
using System.Collections;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Pure snapshot consumer. It contains no game-object reflection and no UI.
    /// Findings therefore remain testable and can later feed any presentation.
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

        private sealed class RateTarget
        {
            public int ItemId;
            public string Name;
            public double Minimum;
            public double Comfortable;
        }

        private static readonly Phase[] Phases = new Phase[] {
            new Phase { Id = "bootstrap", Title = "Stop handcrafting the factory", GateTechId = 0, NextTechId = 1002, NextResearch = "Electromagnetism (blue matrix unlock)" },
            new Phase { Id = "blue", Title = "Build the first continuous matrix line", GateTechId = 1002, NextTechId = 1111, NextResearch = "Energy Matrix" },
            new Phase { Id = "red", Title = "Solve oil and prepare for flight", GateTechId = 1111, NextTechId = 2902, NextResearch = "Drive Engine Lv2" },
            new Phase { Id = "flight", Title = "Reach another planet safely", GateTechId = 2902, NextTechId = 1413, NextResearch = "Titanium Smelting" },
            new Phase { Id = "titanium", Title = "Establish a useful off-world Titanium source", GateTechId = 1413, NextTechId = 1124, NextResearch = "Structure Matrix" },
            new Phase { Id = "yellow", Title = "Make the finite ILS research batch", GateTechId = 1124, NextTechId = 1605, NextResearch = "Interstellar Logistics System" },
            new Phase { Id = "ils", Title = "End manual interplanetary hauling", GateTechId = 1605, NextTechId = 1312, NextResearch = "Information Matrix" },
            new Phase { Id = "purple", Title = "Build the first truly wide production tier", GateTechId = 1312, NextTechId = 1705, NextResearch = "Gravity Matrix" },
            new Phase { Id = "green", Title = "Make warpers routine and prepare Dyson industry", GateTechId = 1705, NextTechId = 1505, NextResearch = "Planetary Ionosphere Utilization" },
            new Phase { Id = "dyson", Title = "Build the minimum useful Dyson swarm", GateTechId = 1505, NextTechId = 1506, NextResearch = "Dirac Inversion Mechanism" },
            new Phase { Id = "photon", Title = "Run the critical-photon receiver array", GateTechId = 1506, NextTechId = 1507, NextResearch = "Universe Matrix" },
            new Phase { Id = "white", Title = "Sustain Universe Matrix production", GateTechId = 1507, NextTechId = 1508, NextResearch = "Mission Completed" },
            new Phase { Id = "logistics", Title = "Automate the infrastructure that moves everything", GateTechId = 1508, NextTechId = 0, NextResearch = "Choose a sandbox objective" }
        };

        private static readonly Dictionary<string, RateTarget> PhaseRateTargets =
            new Dictionary<string, RateTarget>(StringComparer.OrdinalIgnoreCase) {
                { "blue", new RateTarget { ItemId = 6001, Name = "Electromagnetic Matrix", Minimum = 20, Comfortable = 40 } },
                { "red", new RateTarget { ItemId = 6002, Name = "Energy Matrix", Minimum = 10, Comfortable = 20 } },
                { "yellow", new RateTarget { ItemId = 6003, Name = "Structure Matrix", Minimum = 7.5, Comfortable = 15 } },
                { "purple", new RateTarget { ItemId = 6004, Name = "Information Matrix", Minimum = 12, Comfortable = 24 } },
                { "green", new RateTarget { ItemId = 6005, Name = "Gravity Matrix", Minimum = 10, Comfortable = 20 } },
                { "white", new RateTarget { ItemId = 6006, Name = "Universe Matrix", Minimum = 20, Comfortable = 40 } }
            };

        public static Dictionary<string, object> AnalyzeSelected(
            ObservedGameState state,
            string selectedPhaseId)
        {
            selectedPhaseId =
                ManualPhaseNavigator.NormalizePhase(selectedPhaseId);
            var unlockedTechs = state.UnlockedTechIds;
            var ownedItems = state.OwnedItemCounts;
            var productionRates = new Dictionary<int, double>();
            var consumptionRates = new Dictionary<int, double>();
            foreach (var kv in state.ItemFlows)
            {
                productionRates[kv.Key] = kv.Value.ProducedPerMinute;
                consumptionRates[kv.Key] = kv.Value.ConsumedPerMinute;
            }
            var tankStorage = state.TankStorage;
            bool rateWindowReady = state.ProductionWindowReady;

            GuideProgressionEvaluation progression =
                GuideGateEngine.EvaluatePhase(selectedPhaseId, state);

            Phase phase = FindPhase(selectedPhaseId) ??
                FindPhase("bootstrap");
            var findings = new List<object>();

            bool completedWhite =
                phase.Id == "white" && unlockedTechs.Contains(1508);
            if (!completedWhite)
            {
                if (phase.Id == "sphere")
                {
                    // The optional Sphere route has its own causal status
                    // story. Generic late-game findings turn this panel into
                    // an unrelated dashboard, so only its single most useful
                    // construction conclusion is emitted here.
                    AddSphereStatusFinding(findings, state);
                }
                else if (phase.Id == "photon")
                {
                    // PHOTON has one causal status story. Generic late-game
                    // findings would repeat objectives and turn the panel into
                    // a dashboard of every supporting rate.
                    AddPhotonStatusFinding(findings, state);
                }
                else
                {
                    if (phase.GateTechId == 0 ||
                        unlockedTechs.Contains(phase.GateTechId))
                        AddPhaseRateFinding(
                            findings, phase, productionRates, rateWindowReady);
                    AddPhaseInputBottleneckFinding(findings, phase, state);
                    if (phase.Id == "green" && !unlockedTechs.Contains(1705))
                        AddPhaseInputBottleneckFinding(
                            findings, FindPhase("purple"), state);
                    AddOlderMatrixRegressionFinding(findings, phase, state);
                    AddTitaniumReadinessFinding(findings, phase, ownedItems);
                    AddTitaniumProcessingFinding(
                        findings, phase, productionRates, rateWindowReady);
                    AddRefineryCongestionFinding(
                        findings, phase, productionRates, consumptionRates,
                        tankStorage, rateWindowReady);
                    AddPowerFinding(findings, state);
                    AddOptionalWarpFinding(findings, phase, state);
                    AddGasGiantOpportunityFinding(findings, phase, state);
                    AddLateRouteFindings(findings, phase, state);
                    AddDysonFindings(findings, phase, state);
                }
            }

            var phaseResult = new Dictionary<string, object>();
            phaseResult["id"] = phase.Id;
            phaseResult["title"] = phase.Title;
            phaseResult["gateTechId"] = phase.GateTechId;
            phaseResult["nextTechId"] = phase.NextTechId;
            phaseResult["nextResearch"] = phase.NextResearch;
            phaseResult["basis"] =
                "Player-selected phase; runtime evidence evaluates this phase but cannot change it.";

            var result = new Dictionary<string, object>();
            result["analysisVersion"] = "2.4";
            result["phaseSelectionAuthority"] = "player";
            result["phase"] = phaseResult;
            result["progression"] = progression.Export();
            result["normalizedState"] = state.Export();
            result["findings"] = findings;
            result["limitations"] = new List<object> {
                "A technology unlock proves availability, not that the corresponding factory objective is complete.",
                "Production claims require a valid rolling statistics window.",
                "Five-second counter activity is cadence context, not proof that a machine was idle.",
                "Only positively observed runtime evidence is reported as ready; unobserved guide conditions remain unknown."
            };
            return result;
        }

        private static Phase FindPhase(string id)
        {
            if (String.Equals(id, "warp", StringComparison.OrdinalIgnoreCase))
                return new Phase {
                    Id = "warp",
                    Title = "Take the interstellar shortcuts you want",
                    GateTechId = 0,
                    NextTechId = 2904,
                    NextResearch = "Drive Engine Lv4"
                };
            if (String.Equals(id, "sphere", StringComparison.OrdinalIgnoreCase))
                return new Phase {
                    Id = "sphere",
                    Title = "Build permanent structure and shell cells",
                    GateTechId = 1505,
                    NextTechId = 1506,
                    NextResearch = "Dirac Inversion Mechanism"
                };
            foreach (Phase phase in Phases)
                if (String.Equals(phase.Id, id, StringComparison.OrdinalIgnoreCase)) return phase;
            return null;
        }

        private static void AddPhaseContinuityFinding(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            RateTarget target;
            if (!state.ProductionWindowReady ||
                !PhaseRateTargets.TryGetValue(phase.Id, out target))
                return;

            ObservedItemFlow flow;
            if (!state.ItemFlows.TryGetValue(target.ItemId, out flow) ||
                flow.ProducedPerMinute <= 0 || flow.ObservedIntervals < 6)
                return;
            if (flow.ProductionActiveFraction >= 0.90) return;

            findings.Add(Finding(
                "phase-matrix-continuity",
                "watch",
                target.Name + " production was active in " +
                    Math.Round(flow.ProductionActiveFraction * 100.0, 0) + "% of observed intervals.",
                "The average rate can hide pauses; continuity uses consecutive cumulative-counter samples.",
                "high"));
        }

        private static void AddPowerFinding(List<object> findings, ObservedGameState state)
        {
            foreach (ObservedPowerState power in state.PowerPlanets)
            {
                if (power.Observations < 2 || power.MinimumSatisfaction >= 0.99) continue;
                string status = power.MinimumSatisfaction < 0.90 ? "blocked" : "watch";
                findings.Add(Finding(
                    "power-satisfaction-" + power.PlanetId,
                    status,
                    (power.PlanetName ?? ("Planet " + power.PlanetId)) +
                        " power satisfaction fell to " +
                        Math.Round(power.MinimumSatisfaction * 100.0, 1) + "%.",
                    "Observed across " + power.Observations +
                        " rolling power samples; undersupplied fraction " +
                        Math.Round(power.UndersuppliedFraction * 100.0, 1) + "%.",
                    "high"));
            }
        }

        private sealed class InputTarget
        {
            public int ItemId;
            public string Name;
            public double Minimum;
        }

        private static void AddPhaseInputBottleneckFinding(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            if (!state.ProductionWindowReady) return;
            InputTarget[] targets = null;
            int unlockTech = 0;
            if (phase.Id == "purple")
            {
                unlockTech = 1312;
                targets = new InputTarget[] {
                    new InputTarget { ItemId = 1303, Name = "Processor", Minimum = 24 },
                    new InputTarget { ItemId = 1124, Name = "Carbon Nanotube", Minimum = 24 },
                    new InputTarget { ItemId = 1402, Name = "Particle Broadband", Minimum = 12 }
                };
            }
            else if (phase.Id == "green")
            {
                unlockTech = 1705;
                targets = new InputTarget[] {
                    new InputTarget { ItemId = 1121, Name = "Deuterium", Minimum = 50 },
                    new InputTarget { ItemId = 1305, Name = "Quantum Chip", Minimum = 5 },
                    new InputTarget { ItemId = 1127, Name = "Strange Matter", Minimum = 5 },
                    new InputTarget { ItemId = 1209, Name = "Graviton Lens", Minimum = 5 }
                };
            }
            if (targets == null || !state.UnlockedTechIds.Contains(unlockTech)) return;

            InputTarget weakest = null;
            ObservedItemFlow weakestFlow = null;
            double weakestRatio = Double.PositiveInfinity;
            foreach (InputTarget target in targets)
            {
                ObservedItemFlow flow;
                state.ItemFlows.TryGetValue(target.ItemId, out flow);
                double rate = flow != null ? flow.ProducedPerMinute : 0.0;
                double ratio = rate / target.Minimum;
                if (ratio < weakestRatio)
                {
                    weakestRatio = ratio;
                    weakest = target;
                    weakestFlow = flow;
                }
            }
            if (weakest == null || weakestRatio >= 1.0) return;
            double weakestRate = weakestFlow != null ? weakestFlow.ProducedPerMinute : 0.0;
            var finding = Finding(
                "phase-input-bottleneck",
                "blocked",
                weakest.Name + " is the weakest guide-relative intermediate at " +
                    Math.Round(weakestRate, 1) + "/min.",
                "Guide reference " + weakest.Minimum + "/min; this comparison ranks only the " +
                    phase.Title + " inputs explicitly sized by the guide.",
                "high");
            finding["evidenceKind"] = "inferred";
            findings.Add(finding);
        }

        private static void AddOlderMatrixRegressionFinding(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            if (!state.ProductionWindowReady ||
                (phase.Id != "purple" && phase.Id != "green")) return;
            int[] ids = new int[] { 6001, 6002, 6003 };
            string[] names = new string[] {
                "Electromagnetic Matrix", "Energy Matrix", "Structure Matrix"
            };
            double[] minimums = new double[] { 20.0, 10.0, 7.5 };
            for (int i = 0; i < ids.Length; i++)
            {
                ObservedItemFlow flow;
                if (!state.ItemFlows.TryGetValue(ids[i], out flow)) continue;
                if (flow.ConsumedPerMinute <= 0.0) continue;
                if (flow.ProducedPerMinute >= minimums[i]) continue;
                findings.Add(Finding(
                    "older-matrix-regression-" + ids[i],
                    "watch",
                    names[i] + " is being consumed but its support line is below the earlier guide minimum.",
                    "Observed " + Math.Round(flow.ProducedPerMinute, 1) + "/min produced and " +
                        Math.Round(flow.ConsumedPerMinute, 1) +
                        "/min consumed; reference " + minimums[i] + "/min.",
                    "high"));
            }
        }

        private static void AddPhaseRateFinding(
            List<object> findings,
            Phase phase,
            Dictionary<int, double> rates,
            bool windowReady)
        {
            RateTarget target;
            if (!PhaseRateTargets.TryGetValue(phase.Id, out target)) return;

            double rate;
            if (!windowReady)
            {
                findings.Add(Finding(
                    "phase-matrix-rate",
                    "unknown",
                    target.Name + " production rate is not established yet.",
                    "Wait for at least two production-stat samples spanning four seconds of simulation time.",
                    "high"));
                return;
            }
            if (!rates.TryGetValue(target.ItemId, out rate)) rate = 0.0;

            string status = rate < target.Minimum ? "blocked" :
                (rate < target.Comfortable ? "ready" : "comfortable");
            string claim = target.Name + ": " + Math.Round(rate, 1) + "/min";
            string evidence = "Guide minimum " + target.Minimum + "/min; comfortable " +
                target.Comfortable + "/min.";
            findings.Add(Finding("phase-matrix-rate", status, claim, evidence, "high"));
        }

        private static void AddTitaniumReadinessFinding(
            List<object> findings,
            Phase phase,
            Dictionary<int, long> ownedItems)
        {
            if (phase.Id != "titanium") return;
            long count;
            ownedItems.TryGetValue(1106, out count);
            string status = count >= 810 ? "ready" : (count >= 770 ? "watch" : "blocked");
            findings.Add(Finding(
                "first-titanium-budget",
                status,
                "Owned Titanium Ingots: " + count,
                "Guide practical first-haul target 810–860; hard floor about 770.",
                "medium"));
        }

        private static void AddTitaniumProcessingFinding(
            List<object> findings,
            Phase phase,
            Dictionary<int, double> productionRates,
            bool windowReady)
        {
            if (!windowReady ||
                (phase.Id != "titanium" && phase.Id != "yellow" && phase.Id != "ils"))
                return;

            double oreRate;
            double ingotRate;
            productionRates.TryGetValue(1004, out oreRate);
            productionRates.TryGetValue(1106, out ingotRate);
            if (oreRate < 1.0 || ingotRate >= 1.0) return;

            findings.Add(Finding(
                "titanium-processing",
                "watch",
                "Titanium Ore is being mined at " + Math.Round(oreRate, 1) +
                    "/min, while Titanium Ingot production is 0/min.",
                "This proves active extraction without observed smelting during the window. The guide prefers source-side smelting.",
                "high"));
        }

        private static void AddRefineryCongestionFinding(
            List<object> findings,
            Phase phase,
            Dictionary<int, double> productionRates,
            Dictionary<int, double> consumptionRates,
            Dictionary<int, ObservedCapacity> tankStorage,
            bool windowReady)
        {
            if (!windowReady ||
                (phase.Id != "red" && phase.Id != "flight" && phase.Id != "titanium" &&
                 phase.Id != "yellow" && phase.Id != "ils" &&
                 phase.Id != "purple" && phase.Id != "green"))
                return;

            double produced;
            double consumed;
            ObservedCapacity storage;
            productionRates.TryGetValue(1114, out produced);
            consumptionRates.TryGetValue(1114, out consumed);
            if (!tankStorage.TryGetValue(1114, out storage) || storage.Capacity <= 0) return;

            double net = produced - consumed;
            double fill = storage.Count * 1.0 / storage.Capacity;
            double minutesRemaining = net > 0 ? (storage.Capacity - storage.Count) / net : Double.PositiveInfinity;
            if (fill < 0.80 && minutesRemaining > 20.0) return;

            string status = fill >= 0.98 || minutesRemaining <= 2.0 ? "blocked" : "watch";
            string remaining = Double.IsInfinity(minutesRemaining)
                ? "not filling at the observed rates"
                : "about " + Math.Max(0.0, Math.Round(minutesRemaining, 1)) + " production minutes remain";
            findings.Add(Finding(
                "refined-oil-congestion",
                status,
                "Refined Oil tanks are " + Math.Round(fill * 100.0, 1) + "% full; " + remaining + ".",
                "Observed Refined Oil: " + Math.Round(produced, 1) + "/min produced, " +
                    Math.Round(consumed, 1) + "/min consumed. A full byproduct path can stop Hydrogen production.",
                "high"));
        }

        private static void AddOptionalWarpFinding(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            if (phase.Id != "purple") return;
            bool configured = ConfiguredRecipeMachines(state, 78) > 0 ||
                ConfiguredRecipeMachines(state, 79) > 0;
            ObservedItemFlow warpers;
            state.ItemFlows.TryGetValue(1210, out warpers);
            double produced = warpers != null ? warpers.ProducedPerMinute : 0.0;
            long owned;
            state.OwnedItemCounts.TryGetValue(1210, out owned);
            if (!configured && produced <= 0.0 && owned <= 0) return;
            findings.Add(Finding(
                "optional-warp-route",
                "opportunity",
                "The optional Warp route is active.",
                "Configured warper recipe: " + configured +
                    "; production " + Math.Round(produced, 1) +
                    "/min; owned " + owned +
                    ". Research availability alone does not elect this route.",
                "high"));
        }

        private static void AddGasGiantOpportunityFinding(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            if ((phase.Id != "green" && phase.Id != "dyson" &&
                 phase.Id != "photon" && phase.Id != "white" &&
                 phase.Id != "sphere") || !state.ProductionWindowReady) return;
            ObservedItemFlow hydrogen;
            ObservedItemFlow deuterium;
            state.ItemFlows.TryGetValue(1120, out hydrogen);
            state.ItemFlows.TryGetValue(1121, out deuterium);
            double hydrogenUse = hydrogen != null ? hydrogen.ConsumedPerMinute : 0.0;
            double deuteriumUse = deuterium != null ? deuterium.ConsumedPerMinute : 0.0;
            if (hydrogenUse < 100.0 && deuteriumUse < 50.0) return;
            bool unlocked = state.UnlockedTechIds.Contains(1606);
            findings.Add(Finding(
                "gas-giant-opportunity",
                "opportunity",
                unlocked
                    ? "Gas Giant Exploitation is available while gas demand is substantial."
                    : "Gas demand is now large enough to reconsider Gas Giant Exploitation.",
                "Use it when the starting system and actual payoff make it worthwhile.",
                "medium"));
        }

        private static void AddLateRouteFindings(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            bool late = phase.Id == "green" || phase.Id == "dyson" ||
                phase.Id == "photon" || phase.Id == "white" ||
                phase.Id == "sphere";
            if (!late) return;

            int advancedGraphene = ConfiguredRecipeMachines(state, 32);
            if (advancedGraphene > 0)
            {
                findings.Add(Finding(
                    "fire-ice-graphene-route",
                    "ready",
                    "The Fire Ice route is supplying the advanced Graphene chain.",
                    "Configured Graphene (advanced) machines: " + advancedGraphene +
                        ". A great fallback for the standard Sulfuric Acid route.",
                    "high"));
            }

            long fractionators;
            state.FactoryBuildingCounts.TryGetValue(2314, out fractionators);
            int colliderDeuterium = ConfiguredRecipeMachines(state, 40);
            if (fractionators > 0)
            {
                findings.Add(Finding(
                    "fractionator-deuterium-route",
                    "ready",
                    "The Deuterium economy uses a substantial Fractionator route.",
                    "Deployed Fractionators: " + fractionators +
                        "; configured collider Deuterium machines: " + colliderDeuterium +
                        ". Acceptable optional path.",
                    "high"));
            }

            long combatBuildings = BuildingCount(state, 3001) +
                BuildingCount(state, 3002) + BuildingCount(state, 3003) +
                BuildingCount(state, 3004) + BuildingCount(state, 3005) +
                BuildingCount(state, 3006) + BuildingCount(state, 3007) +
                BuildingCount(state, 3008) + BuildingCount(state, 3009);
            if (combatBuildings >= 20)
            {
                findings.Add(Finding(
                    "combat-investment",
                    "context",
                    "This save has made a substantial optional combat investment.",
                    "Found deployed combat buildings: " + combatBuildings +
                        ". Acceptable optional path.",
                    "high"));
            }
        }

        private static void AddSphereStatusFinding(
            List<object> findings,
            ObservedGameState state)
        {
            if (!state.Dyson.Available)
            {
                findings.Add(Finding(
                    "sphere-status-unavailable",
                    "watch",
                    "Sphere construction status isn't available in the current game state.",
                    "Open a loaded factory save before checking this route.",
                    "high"));
                return;
            }

            ObservedItemFlow rockets;
            ObservedItemFlow sails;
            state.ItemFlows.TryGetValue(1503, out rockets);
            state.ItemFlows.TryGetValue(1501, out sails);
            double rocketConsumption =
                rockets != null ? rockets.ConsumedPerMinute : 0.0;
            double sailProduction =
                sails != null ? sails.ProducedPerMinute : 0.0;
            long rocketStock;
            long sailStock;
            state.OwnedItemCounts.TryGetValue(1503, out rocketStock);
            state.OwnedItemCounts.TryGetValue(1501, out sailStock);

            bool constructionHistory =
                state.Dyson.ConstructedStructurePoints > 0 ||
                state.Dyson.ConstructedNodes > 0;
            bool shellDesignated =
                state.Dyson.DesignatedShellCount > 0 ||
                state.Dyson.TotalCellPoints > 0;
            bool shellReady =
                state.Dyson.ConstructedCellPoints > 0;
            bool cellHistory = state.Dyson.ConstructedCellPoints > 0;

            if (state.Dyson.SiloCount <= 0)
            {
                findings.Add(Finding(
                    "sphere-status-no-silo",
                    "watch",
                    constructionHistory
                        ? "Permanent structure exists, but no Vertical Launching Silo is deployed now."
                        : "Sphere construction has not started.",
                    "Build and supply a Vertical Launching Silo with Small Carrier Rockets.",
                    "high"));
                return;
            }

            if (!constructionHistory &&
                state.Dyson.SilosWithTarget <= 0 &&
                state.Dyson.SilosFiringNow <= 0)
            {
                findings.Add(Finding(
                    "sphere-status-no-target",
                    "watch",
                    "The Vertical Launching Silo is waiting for a sphere construction target.",
                    "Plan a node in the Dyson Sphere Editor and assign the silo to it.",
                    "high"));
                return;
            }

            if (!constructionHistory &&
                state.Dyson.SilosSupplied <= 0 &&
                rocketConsumption <= 0.0 &&
                state.Dyson.RocketsInFlight <= 0)
            {
                findings.Add(Finding(
                    "sphere-status-no-rockets",
                    "watch",
                    "The Vertical Launching Silo is waiting for Small Carrier Rockets.",
                    rocketStock > 0
                        ? "Rockets are in storage; route them to the silo."
                        : "Produce Small Carrier Rockets and supply the silo.",
                    "high"));
                return;
            }

            double weakestPower = 1.0;
            bool powerMeasured = false;
            foreach (ObservedPowerState power in state.PowerPlanets)
            {
                if (power.Observations < 2) continue;
                powerMeasured = true;
                weakestPower = Math.Min(
                    weakestPower, power.MinimumSatisfaction);
            }
            if (powerMeasured &&
                weakestPower < 0.99 &&
                (state.Dyson.SilosSupplied > 0 ||
                 state.Dyson.SilosWithTarget > 0))
            {
                findings.Add(Finding(
                    "sphere-status-power",
                    "watch",
                    "Sphere construction is being slowed by an undersupplied power grid.",
                    "Lowest power satisfaction " +
                        Math.Round(weakestPower * 100.0, 1) + "%.",
                    "high"));
                return;
            }

            if (!shellDesignated)
            {
                bool structureGrowing =
                    state.Dyson.ConstructedStructurePointsPerMinute > 0.0 ||
                    rocketConsumption > 0.0 ||
                    state.Dyson.RocketsInFlight > 0 ||
                    state.Dyson.SilosFiringNow > 0;
                findings.Add(Finding(
                    "sphere-status-structure",
                    structureGrowing ? "ready" : "watch",
                    structureGrowing
                        ? "Rockets are building permanent sphere structure."
                        : "Permanent sphere structure is established but currently paused.",
                    "Constructed structure points: " +
                        state.Dyson.ConstructedStructurePoints +
                        ". Designate an enclosed shell area when its boundary is planned.",
                    "high"));
                return;
            }

            if (!shellReady)
            {
                findings.Add(Finding(
                    "sphere-status-boundary",
                    state.Dyson.ConstructedStructurePointsPerMinute > 0.0
                        ? "ready"
                        : "watch",
                    "A shell area is designated; its node-and-frame boundary is still being built.",
                    state.Dyson.ConstructedStructurePointsPerMinute > 0.0
                        ? "Permanent structure is growing at " +
                            Math.Round(
                                state.Dyson.ConstructedStructurePointsPerMinute,
                                1) + " points/min."
                        : "Keep the silo supplied until the boundary is complete.",
                    "high"));
                return;
            }

            if (!cellHistory)
            {
                bool sailsAvailable =
                    sailProduction > 0.0 ||
                    sailStock > 0 ||
                    state.Dyson.SwarmSailCount > 0 ||
                    state.Dyson.EjectorsSupplied > 0;
                if (state.Dyson.EjectorCount <= 0)
                {
                    findings.Add(Finding(
                        "sphere-status-no-ejector",
                        "watch",
                        "The shell boundary is ready, but no EM-Rail Ejector is deployed.",
                        "Build and supply an ejector with Solar Sails.",
                        "high"));
                    return;
                }
                if (!sailsAvailable)
                {
                    findings.Add(Finding(
                        "sphere-status-no-sails",
                        "watch",
                        "The shell boundary is ready, but Solar Sails aren't available.",
                        "Produce Solar Sails and supply the EM-Rail Ejectors.",
                        "high"));
                    return;
                }
                findings.Add(Finding(
                    "sphere-status-cells-waiting",
                    "watch",
                    "Solar Sails have not begun permanent shell-cell construction.",
                    "Check that an ejector can reach its orbit and that the shell boundary is complete.",
                    "high"));
                return;
            }

            double permanentGw =
                state.Dyson.PermanentGenerationWatts / 1000000000.0;
            double swarmGw =
                state.Dyson.SwarmGenerationWatts / 1000000000.0;
            bool cellsGrowing =
                state.Dyson.ConstructedCellPointsPerMinute > 0.0 ||
                state.Dyson.PermanentGenerationWattsChangePerMinute > 0.0;
            findings.Add(Finding(
                "sphere-status-cells",
                "ready",
                cellsGrowing
                    ? "Solar Sails are filling permanent shell cells."
                    : "Permanent shell-cell construction is established but currently paused.",
                "Permanent generation " + Math.Round(permanentGw, 3) +
                    " GW; temporary swarm generation " +
                    Math.Round(swarmGw, 3) + " GW.",
                "high"));
        }

        private static void AddPhotonStatusFinding(
            List<object> findings,
            ObservedGameState state)
        {
            int deployed = state.Dyson.ReceiverCount;
            int configured =
                state.Dyson.ConfiguredPhotonReceiverCount;
            if (!state.Dyson.ReceiverTelemetryAvailable)
            {
                findings.Add(Finding(
                    "photon-status-window",
                    "watch",
                    "Receiver continuity is waiting for its rolling observation window.",
                    "Keep the game running while PHOTON measures each receiver.",
                    "high"));
                return;
            }

            if (deployed <= 0)
            {
                findings.Add(Finding(
                    "photon-status-no-receivers",
                    "watch",
                    "No Ray Receivers were found.",
                    "Deploy four Ray Receivers and set them to Photon Generation.",
                    "high"));
                return;
            }

            if (configured < 4)
            {
                int powerMode = Math.Max(0, deployed - configured);
                findings.Add(Finding(
                    "photon-status-configuration",
                    "watch",
                    configured + "/4 Ray Receivers are configured for Photon Generation.",
                    powerMode > 0
                        ? powerMode +
                            " deployed receiver" +
                            (powerMode == 1 ? " is" : "s are") +
                            " still in power-generation mode."
                        : "Deploy and configure " +
                            (4 - configured) +
                            " more receiver" +
                            (4 - configured == 1 ? "." : "s."),
                    "high"));
                return;
            }

            if (state.Dyson.LensedPhotonReceiverCount < configured)
            {
                findings.Add(Finding(
                    "photon-status-lenses",
                    "watch",
                    "One or more Photon Generation receivers are missing a Graviton Lens.",
                    "Lensed now: " +
                        state.Dyson.LensedPhotonReceiverCount + "/" +
                        configured + ".",
                    "high"));
                return;
            }

            int interruptedLenses = 0;
            double lowestWarmup = 1.0;
            double lowestStrength = 1.0;
            bool anyWindowReady = false;
            foreach (ObservedReceiverState receiver in
                state.Dyson.Receivers)
            {
                if (!receiver.ConfiguredForPhotonGeneration)
                    continue;
                anyWindowReady =
                    anyWindowReady || receiver.WindowReady;
                if (receiver.WindowReady &&
                    !receiver.LensSustained)
                    interruptedLenses++;
                lowestWarmup = Math.Min(
                    lowestWarmup, receiver.WarmupNow);
                lowestStrength = Math.Min(
                    lowestStrength, receiver.StrengthNow);
            }
            if (interruptedLenses > 0)
            {
                findings.Add(Finding(
                    "photon-status-lens-continuity",
                    "watch",
                    interruptedLenses == 1
                        ? "One receiver repeatedly lost its Graviton Lens."
                        : interruptedLenses +
                            " receivers repeatedly lost their Graviton Lenses.",
                    "Restore a continuous lens supply before relying on the array.",
                    "high"));
                return;
            }

            if (state.Dyson.SustainedPhotonReceiverCount < configured)
            {
                string claim;
                string evidence;
                if (state.Dyson.FullStrengthPhotonReceiverCount <
                    configured || lowestStrength < 0.999)
                {
                    claim =
                        "One or more receivers are losing Dyson exposure.";
                    evidence =
                        "Full-strength now: " +
                        state.Dyson.FullStrengthPhotonReceiverCount +
                        "/" + configured + ".";
                }
                else if (state.Dyson.ContinuousPhotonReceiverCount <
                    configured || lowestWarmup < 0.999)
                {
                    claim = "The receiver array is still warming up.";
                    evidence =
                        "Lowest Continuous Receiving: " +
                        Math.Round(lowestWarmup * 100.0, 1) + "%.";
                }
                else
                {
                    claim =
                        "The receiver array is healthy while continuity is being confirmed.";
                    evidence = anyWindowReady
                        ? state.Dyson.SustainedPhotonReceiverCount +
                            "/" + configured +
                            " receivers sustained the full window."
                        : "Keep the array stable for at least 60 seconds.";
                }
                findings.Add(Finding(
                    "photon-status-continuity",
                    "watch",
                    claim,
                    evidence,
                    "high"));
                return;
            }

            ObservedItemFlow photons;
            ObservedItemFlow antimatter;
            ObservedItemFlow hydrogen;
            state.ItemFlows.TryGetValue(1208, out photons);
            state.ItemFlows.TryGetValue(1122, out antimatter);
            state.ItemFlows.TryGetValue(1120, out hydrogen);

            double photonRate =
                photons != null ? photons.ProducedPerMinute : 0.0;
            double antimatterRate =
                antimatter != null ? antimatter.ProducedPerMinute : 0.0;
            if (!state.ProductionWindowReady)
            {
                findings.Add(Finding(
                    "photon-status-production-window",
                    "watch",
                    "The receiver array is stable while production rates are still being measured.",
                    "Critical Photon and Antimatter rates need a complete rolling window.",
                    "high"));
                return;
            }

            ObservedCapacity hydrogenTanks;
            if (antimatterRate > 0.0 &&
                hydrogen != null &&
                hydrogen.NetPerMinute > 0.0 &&
                state.TankStorage.TryGetValue(
                    1120, out hydrogenTanks) &&
                hydrogenTanks.Capacity > 0 &&
                hydrogenTanks.Count /
                    (double)hydrogenTanks.Capacity >= 0.90)
            {
                findings.Add(Finding(
                    "photon-status-hydrogen",
                    "watch",
                    "Returned Hydrogen may stop Antimatter production.",
                    "Use Hydrogen or expand storage before the tanks fill.",
                    "high"));
                return;
            }

            const double softPowerGoal = 1655000000.0;
            if (photonRate < 48.0 &&
                state.Dyson.GenerationWatts > 0.0 &&
                state.Dyson.GenerationWatts < softPowerGoal)
            {
                findings.Add(Finding(
                    "photon-status-power",
                    "watch",
                    "Supplied Dyson power is below the receiver array's soft goal.",
                    "Found " +
                        Math.Round(
                            state.Dyson.GenerationWatts /
                                1000000000.0,
                            3) +
                        " GW; 1.655 GW is a sizing reference, not a gate.",
                    "high"));
                return;
            }

            if (photonRate < 48.0)
            {
                findings.Add(Finding(
                    "photon-status-critical-photons",
                    "watch",
                    "Critical Photons are the current shortfall.",
                    "Found " + Math.Round(photonRate, 1) +
                        "/min; reference 48/min.",
                    "high"));
                return;
            }

            if (antimatterRate < 48.0)
            {
                findings.Add(Finding(
                    "photon-status-antimatter",
                    "watch",
                    "Antimatter conversion is the current shortfall.",
                    "Found " + Math.Round(antimatterRate, 1) +
                        "/min; reference 48/min.",
                    "high"));
                return;
            }

            string cubeAtRisk =
                FindOlderCubeAtRisk(state);
            if (!String.IsNullOrEmpty(cubeAtRisk))
            {
                findings.Add(Finding(
                    "photon-status-older-cube-risk",
                    "watch",
                    cubeAtRisk +
                        " Cube production is falling behind WHITE demand.",
                    "Expand that Cube line before its remaining reserve is exhausted.",
                    "high"));
                return;
            }

            findings.Add(Finding(
                "photon-status-ready",
                "ready",
                "The photon-to-Antimatter path is ready for WHITE.",
                "Critical Photons " +
                    Math.Round(photonRate, 1) +
                    "/min; Antimatter " +
                    Math.Round(antimatterRate, 1) + "/min.",
                "high"));
        }

        private static string FindOlderCubeAtRisk(
            ObservedGameState state)
        {
            ObservedItemFlow white;
            if (!state.ItemFlows.TryGetValue(6006, out white))
                return null;
            double whiteDemand = Math.Max(
                white.ProducedPerMinute,
                white.ConsumedPerMinute);
            if (whiteDemand <= 0.0) return null;

            int[] itemIds = { 6001, 6002, 6003, 6004, 6005 };
            string[] colors = {
                "Blue", "Red", "Yellow", "Purple", "Green"
            };
            for (int i = 0; i < itemIds.Length; i++)
            {
                ObservedItemFlow cube;
                if (!state.ItemFlows.TryGetValue(
                        itemIds[i], out cube))
                    continue;
                double shortage =
                    cube.ConsumedPerMinute -
                    cube.ProducedPerMinute;
                if (shortage <= 0.0 ||
                    cube.ProducedPerMinute >=
                        whiteDemand * 0.75)
                    continue;
                long reserve;
                state.OwnedItemCounts.TryGetValue(
                    itemIds[i], out reserve);
                double reserveMinutes =
                    reserve / Math.Max(1.0, shortage);
                if (reserveMinutes < 10.0)
                    return colors[i];
            }
            return null;
        }

        private static void AddDysonFindings(
            List<object> findings,
            Phase phase,
            ObservedGameState state)
        {
            bool relevant = phase.Id == "dyson" || phase.Id == "sphere" ||
                phase.Id == "photon" || phase.Id == "white";
            if (!relevant || !state.Dyson.Available) return;

            if (state.Dyson.SphereRouteObserved)
            {
                findings.Add(Finding(
                    "dyson-route-choice",
                    "ready",
                    "Active permanent-sphere construction identifies the optional Sphere route.",
                    "Silos, active rocket use, and non-rudimentary constructed sphere structure are all observed. The swarm-only Dyson route is no longer treated as the chosen route.",
                    "high"));
            }
            else
            {
                findings.Add(Finding(
                    "dyson-route-choice",
                    "ready",
                    "No permanent-sphere construction is observed; the swarm-based Dyson route remains selected.",
                    "Sphere route selection requires active rocket launches and more than rudimentary constructed sphere structure.",
                    "high"));
            }

            double generationGw = state.Dyson.GenerationWatts / 1000000000.0;
            if (phase.Id == "dyson" && generationGw < 1.655)
            {
                ObservedItemFlow sails;
                state.ItemFlows.TryGetValue(1501, out sails);
                double sailRate = sails != null ? sails.ProducedPerMinute : 0.0;
                findings.Add(Finding(
                    "dyson-generation-shortfall",
                    "blocked",
                    "Dyson generation is " + Math.Round(generationGw, 3) +
                        " GW against the 1.655 GW guide target.",
                    "Solar Sail production " + Math.Round(sailRate, 1) +
                        "/min against the 511/min reference; net swarm population " +
                        Math.Round(state.Dyson.NetSwarmSailsPerMinute, 1) +
                        "/min. Solar-sail lifetime runtime value: " +
                        Math.Round(state.Dyson.SolarSailLifeRaw, 1) +
                        ". Net population is not labeled launch rate.",
                    "high"));
            }
        }

        private static int ConfiguredRecipeMachines(ObservedGameState state, int recipeId)
        {
            int total = 0;
            foreach (ObservedRecipeConfiguration recipe in state.RecipeConfigurations)
                if (recipe.RecipeId == recipeId) total += recipe.ConfiguredMachineCount;
            return total;
        }

        private static long BuildingCount(ObservedGameState state, int itemId)
        {
            long count;
            return state.FactoryBuildingCounts.TryGetValue(itemId, out count) ? count : 0L;
        }

        private static Dictionary<string, object> Finding(
            string id,
            string status,
            string claim,
            string evidence,
            string confidence)
        {
            int priority = status == "blocked" ? 10 :
                (status == "watch" ? 20 :
                (status == "next" ? 30 :
                (status == "opportunity" ? 40 :
                (status == "ready" ? 50 :
                (status == "comfortable" ? 60 : 70)))));
            return new Dictionary<string, object> {
                { "id", id },
                { "status", status },
                { "priority", priority },
                { "claim", claim },
                { "evidence", evidence },
                { "confidence", confidence },
                { "evidenceKind", "derived" },
                { "source", "normalized runtime state" }
            };
        }

        private static HashSet<int> ReadUnlockedTechs(Dictionary<string, object> snapshot)
        {
            var result = new HashSet<int>();
            Dictionary<string, object> research = GetDictionary(snapshot, "research");
            foreach (object rowObject in Enumerate(GetValue(research, "technologies")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null || !ToBool(GetValue(row, "unlocked"))) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                if (id > 0) result.Add(id);
            }
            return result;
        }

        private static Dictionary<int, string> ReadTechNames(Dictionary<string, object> snapshot)
        {
            var result = new Dictionary<int, string>();
            Dictionary<string, object> research = GetDictionary(snapshot, "research");
            foreach (object rowObject in Enumerate(GetValue(research, "technologies")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                object name = GetValue(row, "name");
                if (id > 0 && name != null) result[id] = name.ToString();
            }
            return result;
        }

        private static HashSet<int> ReadQueuedTechs(Dictionary<string, object> snapshot)
        {
            var result = new HashSet<int>();
            Dictionary<string, object> research = GetDictionary(snapshot, "research");
            foreach (object value in Enumerate(GetValue(research, "techQueue")))
            {
                int id = Plugin.ToInt(value);
                if (id > 0) result.Add(id);
            }
            return result;
        }

        private static Dictionary<int, long> ReadNamedCounts(
            Dictionary<string, object> section,
            string member)
        {
            var result = new Dictionary<int, long>();
            foreach (object rowObject in Enumerate(GetValue(section, member)))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                if (id > 0) result[id] = Plugin.ToLong(GetValue(row, "count"));
            }
            return result;
        }

        private static Dictionary<int, double> ReadProductionRates(Dictionary<string, object> telemetry)
        {
            var result = new Dictionary<int, double>();
            foreach (object rowObject in Enumerate(GetValue(telemetry, "galaxy")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null || ToBool(GetValue(row, "counterReset"))) continue;
                object rate = GetValue(row, "producedPerMinute");
                if (rate == null) continue;
                int id = Plugin.ToInt(GetValue(row, "itemId"));
                if (id > 0) result[id] = Plugin.ToDouble(rate);
            }
            return result;
        }

        private static Dictionary<int, double> ReadConsumptionRates(Dictionary<string, object> telemetry)
        {
            var result = new Dictionary<int, double>();
            foreach (object rowObject in Enumerate(GetValue(telemetry, "galaxy")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null || ToBool(GetValue(row, "counterReset"))) continue;
                object rate = GetValue(row, "consumedPerMinute");
                if (rate == null) continue;
                int id = Plugin.ToInt(GetValue(row, "itemId"));
                if (id > 0) result[id] = Plugin.ToDouble(rate);
            }
            return result;
        }

        private static Dictionary<int, ObservedCapacity> ReadTankStorage(Dictionary<string, object> snapshot)
        {
            var result = new Dictionary<int, ObservedCapacity>();
            foreach (object factoryObject in Enumerate(GetValue(snapshot, "factories")))
            {
                var factory = factoryObject as Dictionary<string, object>;
                if (factory == null) continue;
                Dictionary<string, object> ownedStorage = GetDictionary(factory, "ownedStorage");
                foreach (object tankObject in Enumerate(GetValue(ownedStorage, "tanks")))
                {
                    var tank = tankObject as Dictionary<string, object>;
                    if (tank == null) continue;
                    int itemId = Plugin.ToInt(GetValue(tank, "itemId"));
                    if (itemId <= 0) continue;
                    ObservedCapacity aggregate;
                    if (!result.TryGetValue(itemId, out aggregate))
                    {
                        aggregate = new ObservedCapacity();
                        result[itemId] = aggregate;
                    }
                    aggregate.Count += Plugin.ToLong(GetValue(tank, "count"));
                    aggregate.Capacity += Plugin.ToLong(GetValue(tank, "capacity"));
                }
            }
            return result;
        }

        private static Dictionary<string, object> GetDictionary(
            Dictionary<string, object> source,
            string key)
        {
            object value = GetValue(source, key);
            return value as Dictionary<string, object> ?? new Dictionary<string, object>();
        }

        private static object GetValue(Dictionary<string, object> source, string key)
        {
            if (source == null) return null;
            object value;
            return source.TryGetValue(key, out value) ? value : null;
        }

        private static IEnumerable Enumerate(object value)
        {
            return value as IEnumerable ?? new object[0];
        }

        private static bool ToBool(object value)
        {
            if (value is bool) return (bool)value;
            bool result;
            return value != null && Boolean.TryParse(value.ToString(), out result) && result;
        }

        private static List<object> SortedKeys(HashSet<int> values)
        {
            var ids = new List<int>(values);
            ids.Sort();
            var result = new List<object>();
            foreach (int id in ids) result.Add(id);
            return result;
        }

        private static List<object> CountRows(Dictionary<int, long> values)
        {
            var ids = new List<int>(values.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(new Dictionary<string, object> { { "itemId", id }, { "count", values[id] } });
            return rows;
        }

        private static List<object> RateRows(Dictionary<int, double> values)
        {
            var ids = new List<int>(values.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(new Dictionary<string, object> { { "itemId", id }, { "producedPerMinute", values[id] } });
            return rows;
        }
    }
}
