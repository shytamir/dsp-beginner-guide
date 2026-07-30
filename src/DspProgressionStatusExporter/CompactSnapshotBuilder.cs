using System;
using System.Collections;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal static class CompactSnapshotBuilder
    {
        private const int ReceiverDetailLimit = 8;

        private static readonly int[] CubeIds = {
            6001, 6002, 6003, 6004, 6005, 6006
        };

        private static readonly Dictionary<string, int[]> PhaseItems =
            new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase) {
                { "bootstrap", new int[] {
                    1101, 1104, 1202, 1301, 2001, 2011
                } },
                { "blue", new int[] {
                    6001, 1202, 1301, 2001, 2011
                } },
                { "red", new int[] {
                    6002, 6001, 1114, 1120, 1109, 1103, 1131
                } },
                { "flight", new int[0] },
                { "titanium", new int[] { 1004, 1106, 1003 } },
                { "yellow", new int[] { 6003, 1114, 1120, 1116, 1117, 1118 } },
                { "ils", new int[] { 1106, 1105, 1003 } },
                { "purple", new int[] { 6004, 1303, 1123, 1124, 1402 } },
                { "warp", new int[] { 1210 } },
                { "green", new int[] {
                    6005, 1120, 1121, 1305, 1127, 1210
                } },
                { "dyson", new int[] { 1501 } },
                { "sphere", new int[] { 1501, 1503 } },
                { "photon", new int[] { 1208, 1122, 1120, 1209 } },
                { "white", new int[] { 6006, 1208, 1122 } },
                { "logistics", new int[] {
                    2107, 5003, 2103, 5001, 2104, 5002
                } }
            };

        public static Dictionary<string, object> Build(
            string schemaVersion,
            string exporterVersion,
            Dictionary<string, object> provenance,
            Dictionary<string, object> game,
            Dictionary<string, object> location,
            Dictionary<string, object> research,
            ObservedGameState state,
            Dictionary<string, object> selection,
            Dictionary<string, object> analysis,
            Dictionary<string, object> samplingPerformance,
            bool includeCollectorDiagnostics)
        {
            string phaseId = Text(Value(
                Dictionary(analysis, "phase"), "id"));
            var snapshot = new Dictionary<string, object> {
                { "schemaVersion", schemaVersion },
                { "exporterVersion", exporterVersion },
                { "exportedAtUtc", DateTime.UtcNow.ToString("o") },
                { "provenance", provenance },
                { "game", game },
                { "location", location },
                { "guideSelection", selection },
                { "guide", GuideSummary(analysis) },
                { "summary", new Dictionary<string, object> {
                    { "research", ResearchSummary(research, state) },
                    { "cubes", CubeSummary(state) }
                } },
                { "evidence", Evidence(
                    phaseId, state, samplingPerformance,
                    includeCollectorDiagnostics) },
                { "omissions", new Dictionary<string, object> {
                    { "policy", "Only conclusions and evidence used by implemented guide functions are exported." },
                    { "omitted", new List<object> {
                        "raw factory and entity state",
                        "all-technology rows",
                        "player and mecha detail",
                        "broad inventory detail",
                        "all station slots",
                        "all-item and per-factory telemetry",
                        "broad reflection diagnostics"
                    } },
                    { "detailsTruncated",
                        state.Dyson.Receivers.Count > ReceiverDetailLimit }
                } }
            };
            Validate(snapshot);
            return snapshot;
        }

        private static Dictionary<string, object> GuideSummary(
            Dictionary<string, object> analysis)
        {
            return new Dictionary<string, object> {
                { "analysisVersion", Value(analysis, "analysisVersion") },
                { "selectionAuthority", Value(analysis, "phaseSelectionAuthority") },
                { "phase", Value(analysis, "phase") },
                { "objectives", Value(analysis, "progression") },
                { "currentStatus", Value(analysis, "findings") },
                { "limitations", Value(analysis, "limitations") }
            };
        }

        private static Dictionary<string, object> ResearchSummary(
            Dictionary<string, object> research,
            ObservedGameState state)
        {
            int total = 0;
            foreach (object ignored in Enumerate(Value(research, "technologies")))
                total++;
            int queued = 0;
            foreach (object ignored in Enumerate(Value(research, "techQueue")))
                queued++;
            return new Dictionary<string, object> {
                { "available", Bool(Value(research, "available")) },
                { "totalTechnologies", total },
                { "unlockedTechnologies", state.UnlockedTechIds.Count },
                { "queuedTechnologies", queued },
                { "currentTechId", Value(research, "currentTech") },
                { "missionAccomplished", Value(research, "missionAccomplished") }
            };
        }

        private static List<object> CubeSummary(ObservedGameState state)
        {
            var rows = new List<object>();
            foreach (int id in CubeIds)
                rows.Add(ItemEvidence(state, id, true));
            return rows;
        }

        private static Dictionary<string, object> Evidence(
            string phaseId,
            ObservedGameState state,
            Dictionary<string, object> samplingPerformance,
            bool includeCollectorDiagnostics)
        {
            var result = new Dictionary<string, object> {
                { "collectors", CollectorHealth(
                    state, samplingPerformance,
                    includeCollectorDiagnostics) },
                { "phaseItems", PhaseItemEvidence(phaseId, state) },
                { "logistics", LogisticsEvidence(phaseId, state) },
                { "power", PowerEvidence(state) }
            };
            if (IsDysonPhase(phaseId))
                result["dyson"] = DysonEvidence(phaseId, state);
            return result;
        }

        private static Dictionary<string, object> CollectorHealth(
            ObservedGameState state,
            Dictionary<string, object> samplingPerformance,
            bool includeCollectorDiagnostics)
        {
            var production = new Dictionary<string, object> {
                { "available", state.ProductionWindowReady },
                { "source", state.ProductionSource },
                { "scope", state.ProductionScope },
                { "period", state.ProductionPeriod },
                { "windowGameSeconds", state.ProductionWindowSeconds },
                { "sampleCount", state.ProductionSampleCount },
                { "watchedItemCount", state.ProductionWatchedItemCount },
                { "itemCoverage", state.ProductionItemCoverage },
                { "factoryCount", state.ProductionFactoryCount }
            };
            if (!String.IsNullOrEmpty(state.ProductionFailure))
                production["failure"] = state.ProductionFailure;
            var result = new Dictionary<string, object> {
                { "cadenceSeconds", 5.0 },
                { "production", production },
                { "traffic", new Dictionary<string, object> {
                    { "available", state.TrafficWindowReady },
                    { "windowGameSeconds", state.TrafficWindowSeconds }
                } },
                { "power", new Dictionary<string, object> {
                    { "available", state.PowerPlanets.Count > 0 },
                    { "windowGameSeconds", state.PowerWindowSeconds },
                    { "planetCount", state.PowerPlanets.Count }
                } },
                { "recipes", new Dictionary<string, object> {
                    { "available", state.RecipeTelemetryAvailable }
                } },
                { "receiverContinuity", new Dictionary<string, object> {
                    { "available", state.Dyson.ReceiverTelemetryAvailable },
                    { "windowSeconds", state.Dyson.ReceiverContinuityWindowSeconds }
                } }
            };
            if (includeCollectorDiagnostics)
                result["performance"] = samplingPerformance;
            return result;
        }

        private static List<object> PhaseItemEvidence(
            string phaseId,
            ObservedGameState state)
        {
            int[] ids;
            if (!PhaseItems.TryGetValue(phaseId ?? "", out ids))
                ids = new int[0];
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(ItemEvidence(state, id, false));
            return rows;
        }

        private static Dictionary<string, object> ItemEvidence(
            ObservedGameState state,
            int itemId,
            bool includeLifetime)
        {
            ObservedItemFlow flow;
            ObservedLifetimeItemTotals totals;
            long owned;
            state.ItemFlows.TryGetValue(itemId, out flow);
            state.LifetimeItemTotals.TryGetValue(itemId, out totals);
            state.OwnedItemCounts.TryGetValue(itemId, out owned);
            string name = flow != null ? flow.Name :
                totals != null ? totals.Name : Plugin.ItemName(itemId);
            var row = new Dictionary<string, object> {
                { "itemId", itemId },
                { "name", name },
                { "owned", owned },
                { "nativeWindowAvailable", state.ProductionWindowReady && flow != null },
                { "producedPerMinute", flow != null ? (object)flow.ProducedPerMinute : null },
                { "consumedPerMinute", flow != null ? (object)flow.ConsumedPerMinute : null },
                { "netPerMinute", flow != null ? (object)flow.NetPerMinute : null },
                { "observedIntervals", flow != null ? (object)flow.ObservedIntervals : null },
                { "productionActiveFraction", flow != null ? (object)flow.ProductionActiveFraction : null },
                { "productionContinuity", flow != null ? flow.ProductionContinuity : null }
            };
            if (includeLifetime)
            {
                row["lifetimeProduced"] =
                    totals != null ? (object)totals.Produced : null;
                row["lifetimeConsumed"] =
                    totals != null ? (object)totals.Consumed : null;
            }
            return row;
        }

        private static Dictionary<string, object> LogisticsEvidence(
            string phaseId,
            ObservedGameState state)
        {
            int[] ids;
            if (!PhaseItems.TryGetValue(phaseId ?? "", out ids))
                ids = new int[0];
            var wanted = new HashSet<int>(ids);
            var aggregates =
                new Dictionary<int, Dictionary<string, object>>();
            foreach (ObservedTrafficFlow flow in state.TrafficFlows)
            {
                if (!wanted.Contains(flow.ItemId)) continue;
                Dictionary<string, object> row;
                if (!aggregates.TryGetValue(flow.ItemId, out row))
                {
                    row = new Dictionary<string, object> {
                        { "itemId", flow.ItemId },
                        { "name", flow.Name },
                        { "inputPerMinute", 0.0 },
                        { "outputPerMinute", 0.0 },
                        { "internalPerMinute", 0.0 }
                    };
                    aggregates[flow.ItemId] = row;
                }
                row["inputPerMinute"] =
                    Plugin.ToDouble(row["inputPerMinute"]) + flow.InputPerMinute;
                row["outputPerMinute"] =
                    Plugin.ToDouble(row["outputPerMinute"]) + flow.OutputPerMinute;
                row["internalPerMinute"] =
                    Plugin.ToDouble(row["internalPerMinute"]) + flow.InternalPerMinute;
            }
            var rows = new List<object>();
            var sorted = new List<int>(aggregates.Keys);
            sorted.Sort();
            foreach (int id in sorted) rows.Add(aggregates[id]);
            return new Dictionary<string, object> {
                { "available", state.TrafficWindowReady },
                { "windowGameSeconds", state.TrafficWindowSeconds },
                { "selectedPhaseItems", rows }
            };
        }

        private static Dictionary<string, object> PowerEvidence(
            ObservedGameState state)
        {
            ObservedPowerState worst = null;
            foreach (ObservedPowerState power in state.PowerPlanets)
                if (worst == null ||
                    power.MinimumSatisfaction < worst.MinimumSatisfaction)
                    worst = power;
            return new Dictionary<string, object> {
                { "available", worst != null },
                { "windowGameSeconds", state.PowerWindowSeconds },
                { "worstPlanet", worst == null ? null :
                    new Dictionary<string, object> {
                        { "planetId", worst.PlanetId },
                        { "planetName", worst.PlanetName },
                        { "observations", worst.Observations },
                        { "minimumSatisfaction", worst.MinimumSatisfaction },
                        { "undersuppliedFraction", worst.UndersuppliedFraction },
                        { "maximumDemandToCapacity", worst.MaximumDemandToCapacity }
                    } }
            };
        }

        private static Dictionary<string, object> DysonEvidence(
            string phaseId,
            ObservedGameState state)
        {
            ObservedDysonState d = state.Dyson;
            var result = new Dictionary<string, object> {
                { "available", d.Available },
                { "aggregateSource", d.AggregateSource },
                { "aggregateSystemCount", d.AggregateSystemCount },
                { "constructionAggregateAvailable", d.ConstructionAggregateAvailable },
                { "aggregateNodesRead", d.AggregateNodesRead },
                { "aggregateNodesMissing", d.AggregateNodesMissing },
                { "generationWatts", d.GenerationWatts },
                { "permanentGenerationWatts", d.PermanentGenerationWatts },
                { "swarmGenerationWatts", d.SwarmGenerationWatts },
                { "requestedWatts", d.RequestedWatts }
            };
            if (phaseId == "dyson")
            {
                result["swarm"] = new Dictionary<string, object> {
                    { "sailCount", d.SwarmSailCount },
                    { "netSailPopulationPerMinute", d.NetSwarmSailsPerMinute },
                    { "ejectorCount", d.EjectorCount },
                    { "ejectorsOnTarget", d.EjectorsOnTarget },
                    { "ejectorsSupplied", d.EjectorsSupplied },
                    { "ejectorsFiringNow", d.EjectorsFiringNow }
                };
            }
            if (phaseId == "sphere")
            {
                result["construction"] = new Dictionary<string, object> {
                    { "siloCount", d.SiloCount },
                    { "silosSupplied", d.SilosSupplied },
                    { "silosWithTarget", d.SilosWithTarget },
                    { "silosFiringNow", d.SilosFiringNow },
                    { "constructedNodes", d.ConstructedNodes },
                    { "totalNodes", d.TotalNodes },
                    { "constructedStructurePoints", d.ConstructedStructurePoints },
                    { "totalStructurePoints", d.TotalStructurePoints },
                    { "constructedCellPoints", d.ConstructedCellPoints },
                    { "totalCellPoints", d.TotalCellPoints },
                    { "rocketsInFlight", d.RocketsInFlight },
                    { "constructionRateAvailable", d.ConstructionRateAvailable },
                    { "constructedStructurePointsPerMinute", d.ConstructedStructurePointsPerMinute },
                    { "constructedCellPointsPerMinute", d.ConstructedCellPointsPerMinute },
                    { "sphereRouteObserved", d.SphereRouteObserved }
                };
            }
            if (phaseId == "photon" || phaseId == "white")
                result["receivers"] = ReceiverEvidence(d);
            return result;
        }

        private static Dictionary<string, object> ReceiverEvidence(
            ObservedDysonState d)
        {
            var detail = new List<object>();
            foreach (ObservedReceiverState receiver in d.Receivers)
            {
                if (detail.Count >= ReceiverDetailLimit) break;
                detail.Add(new Dictionary<string, object> {
                    { "planetId", receiver.PlanetId },
                    { "planetName", receiver.PlanetName },
                    { "entityId", receiver.EntityId },
                    { "sampleCount", receiver.SampleCount },
                    { "windowSeconds", receiver.WindowSeconds },
                    { "windowReady", receiver.WindowReady },
                    { "configuredForPhotonGeneration", receiver.ConfiguredForPhotonGeneration },
                    { "lensedNow", receiver.LensedNow },
                    { "continuousReceivingNow", receiver.ContinuousReceivingNow },
                    { "minimumWarmup", receiver.MinimumWarmup },
                    { "minimumStrength", receiver.MinimumStrength },
                    { "sustainedHealthy", receiver.SustainedHealthy },
                    { "requestedDysonPowerWatts", receiver.RequestedDysonPowerWatts },
                    { "suppliedPowerWatts", receiver.SuppliedPowerWatts },
                    { "criticalPhotonOutputPerMinute", receiver.CriticalPhotonOutputPerMinute }
                });
            }
            return new Dictionary<string, object> {
                { "deployed", d.ReceiverCount },
                { "configuredForPhotonGeneration", d.ConfiguredPhotonReceiverCount },
                { "lensed", d.LensedPhotonReceiverCount },
                { "fullStrength", d.FullStrengthPhotonReceiverCount },
                { "continuous", d.ContinuousPhotonReceiverCount },
                { "sustainedHealthy", d.SustainedPhotonReceiverCount },
                { "continuityWindowSeconds", d.ReceiverContinuityWindowSeconds },
                { "requestedDysonPowerWatts", d.ReceiverArrayRequestedDysonPowerWatts },
                { "suppliedPowerWatts", d.ReceiverArraySuppliedPowerWatts },
                { "criticalPhotonOutputPerMinute", d.ReceiverArrayCriticalPhotonOutputPerMinute },
                { "details", detail },
                { "detailsTruncated", d.Receivers.Count > ReceiverDetailLimit },
                { "omittedDetailCount", Math.Max(
                    0, d.Receivers.Count - ReceiverDetailLimit) }
            };
        }

        private static bool IsDysonPhase(string phaseId)
        {
            return phaseId == "green" || phaseId == "dyson" ||
                phaseId == "sphere" || phaseId == "photon" ||
                phaseId == "white";
        }

        private static void Validate(Dictionary<string, object> snapshot)
        {
            string[] required = {
                "schemaVersion", "exporterVersion", "provenance", "game",
                "guideSelection", "guide", "summary", "evidence", "omissions"
            };
            foreach (string key in required)
                if (!snapshot.ContainsKey(key))
                    throw new InvalidOperationException(
                        "Compact snapshot is missing " + key + ".");
            string[] forbidden = {
                "factories", "player", "research", "productionTelemetry",
                "trafficTelemetry", "powerTelemetry", "recipeTelemetry",
                "observedState", "guideAnalysis", "guidePanel",
                "diagnostics"
            };
            foreach (string key in forbidden)
                if (snapshot.ContainsKey(key))
                    throw new InvalidOperationException(
                        "Compact snapshot contains legacy section " + key + ".");
        }

        private static Dictionary<string, object> Dictionary(
            Dictionary<string, object> source,
            string key)
        {
            return Value(source, key) as Dictionary<string, object> ??
                new Dictionary<string, object>();
        }

        private static object Value(
            Dictionary<string, object> source,
            string key)
        {
            object value;
            return source != null && source.TryGetValue(key, out value)
                ? value : null;
        }

        private static IEnumerable Enumerate(object value)
        {
            return value as IEnumerable ?? new object[0];
        }

        private static string Text(object value)
        {
            return value == null ? "" : value.ToString();
        }

        private static bool Bool(object value)
        {
            if (value is bool) return (bool)value;
            bool parsed;
            return value != null &&
                Boolean.TryParse(value.ToString(), out parsed) && parsed;
        }
    }
}
