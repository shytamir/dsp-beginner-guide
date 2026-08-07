using System;
using System.Collections.Generic;
using System.Globalization;

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

        private sealed class RiskItemSpec
        {
            public int ItemId;
            public string Name;
            public double ExactTargetPerMinute;
        }

        private sealed class OrderedRisk
        {
            public ProductionRiskResult Result;
            public int PhaseOrder;
        }

        private static readonly Phase[] Phases = new Phase[] {
            new Phase { Id = "blue", Title = "Sustain the Blue Cube science loop", GateTechId = 1002, NextTechId = 1111, NextResearch = "Energy Matrix" },
            new Phase { Id = "red", Title = "Sustain Red Cubes without refinery deadlock", GateTechId = 1111, NextTechId = 2902, NextResearch = "Drive Engine Lv2" },
            new Phase { Id = "ils", Title = "Complete the first interplanetary logistics expedition", GateTechId = 2902, NextTechId = 1124, NextResearch = "Structure Matrix" },
            new Phase { Id = "yellow", Title = "Run three continuous Yellow Cube labs", GateTechId = 1124, NextTechId = 1312, NextResearch = "Information Matrix" },
            new Phase { Id = "purple", Title = "Run three continuous Purple Cube labs", GateTechId = 1312, NextTechId = 1705, NextResearch = "Gravity Matrix" },
            new Phase { Id = "green", Title = "Run two continuous Green Cube labs", GateTechId = 1705, NextTechId = 1505, NextResearch = "Planetary Ionosphere Utilization" },
            new Phase { Id = "dyson", Title = "Build the Photon swarm", GateTechId = 1505, NextTechId = 1506, NextResearch = "Dirac Inversion Mechanism" },
            new Phase { Id = "photon", Title = "Run the critical-photon receiver array", GateTechId = 1506, NextTechId = 1507, NextResearch = "Universe Matrix" },
            new Phase { Id = "white", Title = "Complete the main progression route", GateTechId = 1507, NextTechId = 1508, NextResearch = "Mission Completed" }
        };

        private static readonly Dictionary<string, RiskItemSpec[]> RiskItems =
            new Dictionary<string, RiskItemSpec[]>(StringComparer.OrdinalIgnoreCase) {
                { "blue", new RiskItemSpec[] {
                    RiskItem(6001, "Blue Cubes", 20.0)
                } },
                { "red", new RiskItemSpec[] {
                    RiskItem(6002, "Red Cubes", 20.0)
                } },
                { "ils", new RiskItemSpec[] {
                    RiskItem(1106, "Titanium Ingots", 0.0),
                    RiskItem(1105, "High-Purity Silicon", 0.0)
                } },
                { "yellow", new RiskItemSpec[] {
                    RiskItem(1112, "Diamonds", 0.0),
                    RiskItem(1118, "Titanium Crystals", 0.0),
                    RiskItem(6003, "Yellow Cubes", 0.0)
                } },
                { "purple", new RiskItemSpec[] {
                    RiskItem(1303, "Processors", 0.0),
                    RiskItem(1402, "Particle Broadband", 0.0),
                    RiskItem(6004, "Purple Cubes", 0.0)
                } },
                { "green", new RiskItemSpec[] {
                    RiskItem(1305, "Quantum Chips", 0.0),
                    RiskItem(1209, "Graviton Lenses", 0.0),
                    RiskItem(6005, "Green Cubes", 0.0)
                } },
                { "dyson", new RiskItemSpec[] {
                    RiskItem(1501, "Solar Sails", 0.0)
                } },
                { "photon", new RiskItemSpec[] {
                    RiskItem(1208, "Critical Photons", 0.0),
                    RiskItem(1122, "Antimatter", 0.0)
                } },
                { "white", new RiskItemSpec[] {
                    RiskItem(6001, "Blue Cubes", 0.0),
                    RiskItem(6002, "Red Cubes", 0.0),
                    RiskItem(6003, "Yellow Cubes", 0.0),
                    RiskItem(6004, "Purple Cubes", 0.0),
                    RiskItem(6005, "Green Cubes", 0.0),
                    RiskItem(1122, "Antimatter", 0.0),
                    RiskItem(6006, "White Cubes", 40.0)
                } }
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

            ProductionRiskResult selectedRisk;
            Dictionary<string, object> productionRisk =
                AnalyzeProductionRisk(state, phase.Id, out selectedRisk);
            if (selectedRisk != null && selectedRisk.Actionable)
                findings.Add(RiskFinding(selectedRisk));

            if (phase.Id == "red")
                AddRefineryCongestionFinding(findings, state);
            if (phase.Id == "photon")
                AddPhotonPowerFinding(findings, state);
            KeepStrongestFinding(findings);

            var phaseResult = new Dictionary<string, object> {
                { "id", phase.Id },
                { "title", phase.Title },
                { "gateTechId", phase.GateTechId },
                { "nextTechId", phase.NextTechId },
                { "nextResearch", phase.NextResearch },
                { "basis", "Player-selected phase; runtime evidence evaluates this phase but cannot change it." }
            };

            return new Dictionary<string, object> {
                { "analysisVersion", "3.1" },
                { "phaseSelectionAuthority", "player" },
                { "phase", phaseResult },
                { "progression", progression.Export() },
                { "normalizedState", state.Export() },
                { "productionRisk", productionRisk },
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

        private static RiskItemSpec RiskItem(
            int itemId, string name, double exactTargetPerMinute)
        {
            return new RiskItemSpec {
                ItemId = itemId,
                Name = name,
                ExactTargetPerMinute = exactTargetPerMinute
            };
        }

        private static Dictionary<string, object> AnalyzeProductionRisk(
            ObservedGameState state,
            string phaseId,
            out ProductionRiskResult selected)
        {
            selected = null;
            int evaluated = 0;
            var orderedRisks = new List<OrderedRisk>();
            RiskItemSpec[] specs;
            if (!RiskItems.TryGetValue(phaseId ?? "", out specs))
                specs = new RiskItemSpec[0];

            for (int specIndex = 0; specIndex < specs.Length; specIndex++)
            {
                RiskItemSpec spec = specs[specIndex];
                ProductionRiskResult itemRisk = null;
                ObservedItemBufferEvidence buffer;
                bool hasLocalScopes = state.ItemBuffers.TryGetValue(
                    spec.ItemId, out buffer) && buffer.Scopes.Count > 0;
                if (hasLocalScopes)
                {
                    foreach (ObservedBufferScopeEvidence scope in buffer.Scopes)
                    {
                        ProductionRiskResult result =
                            ProductionRiskAnalyzer.Evaluate(
                                PlanetRiskInput(state, spec, scope));
                        evaluated++;
                        itemRisk = WorseRisk(itemRisk, result);
                    }
                    if (spec.ExactTargetPerMinute > 0.0)
                    {
                        ProductionRiskInput targetInput =
                            ClusterRiskInput(state, spec);
                        targetInput.BackpressureStatus =
                            buffer.BackpressureStatus;
                        ProductionRiskResult targetResult =
                            ProductionRiskAnalyzer.Evaluate(targetInput);
                        evaluated++;
                        itemRisk = WorseRisk(itemRisk, targetResult);
                    }
                }
                else
                {
                    ProductionRiskResult result =
                        ProductionRiskAnalyzer.Evaluate(
                            ClusterRiskInput(state, spec));
                    evaluated++;
                    itemRisk = WorseRisk(itemRisk, result);
                }
                selected = WorseRisk(selected, itemRisk);
                if (itemRisk != null && itemRisk.Actionable)
                    orderedRisks.Add(new OrderedRisk {
                        Result = itemRisk,
                        PhaseOrder = specIndex
                    });
            }

            orderedRisks.Sort(CompareOrderedRisks);
            if (orderedRisks.Count > 0)
                selected = orderedRisks[0].Result;
            var actionable = new List<object>();
            foreach (OrderedRisk risk in orderedRisks)
                actionable.Add(risk.Result.Export());

            return new Dictionary<string, object> {
                { "contractVersion", "1.1" },
                { "basis", "Deterministic selected-phase evaluation from scope-matched native rates and conservative accessible-buffer evidence." },
                { "evaluatedItemScopes", evaluated },
                { "actionable", actionable },
                { "selected", selected != null ? (object)selected.Export() : null }
            };
        }

        private static int CompareOrderedRisks(
            OrderedRisk left,
            OrderedRisk right)
        {
            int severity = RiskRank(right.Result.State).CompareTo(
                RiskRank(left.Result.State));
            if (severity != 0) return severity;
            if (left.Result.DepletionMinutesAvailable !=
                right.Result.DepletionMinutesAvailable)
                return left.Result.DepletionMinutesAvailable ? -1 : 1;
            if (left.Result.DepletionMinutesAvailable)
            {
                int depletion = left.Result.DepletionMinutes.CompareTo(
                    right.Result.DepletionMinutes);
                if (depletion != 0) return depletion;
            }
            return left.PhaseOrder.CompareTo(right.PhaseOrder);
        }

        private static ProductionRiskInput ClusterRiskInput(
            ObservedGameState state,
            RiskItemSpec spec)
        {
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(spec.ItemId, out flow);
            return new ProductionRiskInput {
                ItemId = spec.ItemId,
                Name = spec.Name,
                Scope = "entire-star-cluster",
                OneMinuteAvailable = flow != null && flow.OneMinuteAvailable,
                ProducedPerMinute = flow != null
                    ? flow.ProducedPerMinute : 0.0,
                ConsumedPerMinute = flow != null
                    ? flow.ConsumedPerMinute : 0.0,
                TenMinuteAvailable = flow != null && flow.TenMinuteAvailable,
                TenMinuteReady = flow != null && flow.TenMinuteReady,
                TenMinuteProducedPerMinute = flow != null
                    ? flow.TenMinuteProducedPerMinute : 0.0,
                TenMinuteConsumedPerMinute = flow != null
                    ? flow.TenMinuteConsumedPerMinute : 0.0,
                BackpressureStatus = "unknown",
                ExactTargetPerMinute = spec.ExactTargetPerMinute
            };
        }

        private static ProductionRiskInput PlanetRiskInput(
            ObservedGameState state,
            RiskItemSpec spec,
            ObservedBufferScopeEvidence scope)
        {
            bool found = false;
            bool oneMinuteAvailable = false;
            bool tenMinuteAvailable = false;
            bool tenMinuteReady = true;
            double produced = 0.0;
            double consumed = 0.0;
            double tenMinuteProduced = 0.0;
            double tenMinuteConsumed = 0.0;
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
            {
                if (flow.ItemId != spec.ItemId ||
                    flow.PlanetId != scope.PlanetId)
                    continue;
                found = true;
                oneMinuteAvailable |= flow.OneMinuteAvailable;
                tenMinuteAvailable |= flow.TenMinuteAvailable;
                tenMinuteReady &= flow.TenMinuteReady;
                produced += flow.ProducedPerMinute;
                consumed += flow.ConsumedPerMinute;
                tenMinuteProduced += flow.TenMinuteProducedPerMinute;
                tenMinuteConsumed += flow.TenMinuteConsumedPerMinute;
            }
            return new ProductionRiskInput {
                ItemId = spec.ItemId,
                Name = spec.Name,
                Scope = "planet-local",
                PlanetId = scope.PlanetId,
                PlanetName = scope.PlanetName,
                OneMinuteAvailable = found && oneMinuteAvailable,
                ProducedPerMinute = produced,
                ConsumedPerMinute = consumed,
                TenMinuteAvailable = found && tenMinuteAvailable,
                TenMinuteReady = found && tenMinuteReady,
                TenMinuteProducedPerMinute = tenMinuteProduced,
                TenMinuteConsumedPerMinute = tenMinuteConsumed,
                RunwayAvailable = scope.RunwayAvailable,
                RunwayMinutes = scope.RunwayMinutes,
                AccessibleCount = scope.AccessibleCount,
                BackpressureStatus = scope.BackpressureStatus,
                // Exact guide targets are cluster contracts. They are not
                // applied to a single local buffer scope.
                ExactTargetPerMinute = 0.0
            };
        }

        private static ProductionRiskResult WorseRisk(
            ProductionRiskResult current,
            ProductionRiskResult candidate)
        {
            if (current == null) return candidate;
            int currentRank = RiskRank(current.State);
            int candidateRank = RiskRank(candidate.State);
            if (candidateRank > currentRank) return candidate;
            if (candidateRank < currentRank) return current;
            if (candidate.DepletionMinutesAvailable !=
                current.DepletionMinutesAvailable)
                return candidate.DepletionMinutesAvailable
                    ? candidate : current;
            if (candidate.DepletionMinutesAvailable &&
                candidate.DepletionMinutes != current.DepletionMinutes)
                return candidate.DepletionMinutes < current.DepletionMinutes
                    ? candidate : current;
            return candidate.Score > current.Score ? candidate : current;
        }

        private static int RiskRank(string state)
        {
            if (state == "starved") return 6;
            if (state == "draining") return 5;
            if (state == "warming") return 4;
            if (state == "unknown") return 3;
            if (state == "backpressured") return 2;
            return 1;
        }

        private static Dictionary<string, object> RiskFinding(
            ProductionRiskResult result)
        {
            string claim;
            if (result.State == "starved")
                claim = result.Name + " production is starved.";
            else if (result.DemandDeficit)
                claim = result.Name +
                    " are draining faster than they are replenished.";
            else
                claim = result.Name + " production is below the phase target.";

            string evidence = "Found " + Rate(result.ProducedPerMinute) +
                "/min produced";
            if (result.ConsumedPerMinute > 0.0)
                evidence += " and " + Rate(result.ConsumedPerMinute) +
                    "/min consumed";
            if (result.ExactTargetPerMinute > 0.0)
                evidence += " against a " +
                    Rate(result.ExactTargetPerMinute) + "/min target";
            if (result.RunwayAvailable)
                evidence += "; accessible runway is " +
                    Rate(result.RunwayMinutes) + " min";
            evidence += "; the ten-minute production baseline is " +
                Rate(result.BaselinePerMinute) + "/min. " +
                (result.State == "starved"
                    ? "Restore the upstream supply or stopped production line."
                    : "Increase production before the deficit consumes its buffer.");

            return new Dictionary<string, object> {
                { "id", "production-risk-" + result.ItemId },
                { "status", result.State == "starved" ? "blocked" : "watch" },
                { "claim", claim },
                { "evidence", evidence },
                { "confidence", "high" },
                { "priority", result.State == "starved" ? 20 : 30 },
                { "risk", result.Export() }
            };
        }

        private static string Rate(double value)
        {
            return Math.Round(value, 1).ToString(
                "0.0", CultureInfo.InvariantCulture);
        }

        private static void KeepStrongestFinding(List<object> findings)
        {
            if (findings.Count <= 1) return;
            object strongest = findings[0];
            int strongestPriority = FindingPriority(strongest);
            for (int i = 1; i < findings.Count; i++)
            {
                int priority = FindingPriority(findings[i]);
                if (priority >= strongestPriority) continue;
                strongest = findings[i];
                strongestPriority = priority;
            }
            findings.Clear();
            findings.Add(strongest);
        }

        private static int FindingPriority(object value)
        {
            var finding = value as Dictionary<string, object>;
            object priority;
            return finding != null && finding.TryGetValue("priority", out priority)
                ? Convert.ToInt32(priority, CultureInfo.InvariantCulture) : 100;
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
            string status = requested > available || available <= 0
                ? "watch" : "ready";
            findings.Add(Finding(
                "photon-receiver-power",
                status,
                "Photon receivers request " + FormatPower(requested) +
                    " from " + FormatPower(available) + " of Dyson generation.",
                "The receiver array is currently supplied with " + FormatPower(supplied) + ".",
                "high",
                status == "watch" ? 10 : 80));
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
                "high",
                10));
        }

        private static Dictionary<string, object> Finding(
            string id,
            string status,
            string claim,
            string evidence,
            string confidence,
            int priority)
        {
            return new Dictionary<string, object> {
                { "id", id },
                { "status", status },
                { "claim", claim },
                { "evidence", evidence },
                { "confidence", confidence },
                { "priority", priority }
            };
        }
    }
}
