using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class GuideGateCondition
    {
        public string Id;
        public string Label;
        public string Status;
        public bool Required;
        public string Evidence;
        public string EvidenceKind;
        public string Action;

            public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object> {
                { "id", Id }, { "label", Label }, { "status", Status },
                { "required", Required }, { "evidence", Evidence },
                { "evidenceKind", EvidenceKind }, { "action", Action }
            };
        }
    }

    internal sealed class GuideGateResult
    {
        public string Id;
        public string Title;
        public string Status;
        public string Basis;
        public readonly List<GuideGateCondition> Conditions = new List<GuideGateCondition>();

        public Dictionary<string, object> Export()
        {
            var conditions = new List<object>();
            foreach (GuideGateCondition condition in Conditions) conditions.Add(condition.Export());
            return new Dictionary<string, object> {
                { "id", Id }, { "title", Title }, { "status", Status },
                { "basis", Basis }, { "conditions", conditions }
            };
        }
    }

    internal sealed class GuideProgressionEvaluation
    {
        public string SelectedPhase;
        public readonly List<GuideGateResult> Gates = new List<GuideGateResult>();

        public Dictionary<string, object> Export()
        {
            var gates = new List<object>();
            foreach (GuideGateResult gate in Gates) gates.Add(gate.Export());
            return new Dictionary<string, object> {
                { "contractVersion", "2.5" },
                { "selectionAuthority", "player" },
                { "selectedPhase", SelectedPhase },
                { "gateEvaluations", gates }
            };
        }
    }

    internal static class GuideGateEngine
    {
        private sealed class GateDefinition
        {
            public string Id;
            public string Title;
        }

        private static readonly GateDefinition[] Gates = new GateDefinition[] {
            new GateDefinition { Id = "bootstrap", Title = "Automate the starter factory" },
            new GateDefinition { Id = "blue", Title = "Sustain the Blue Cube science loop" },
            new GateDefinition { Id = "red", Title = "Sustain Red Cubes without refinery deadlock" },
            new GateDefinition { Id = "ils", Title = "Complete the first interplanetary logistics expedition" },
            new GateDefinition { Id = "yellow", Title = "Run three continuous Yellow Cube labs" },
            new GateDefinition { Id = "purple", Title = "Run three continuous Purple Cube labs" },
            new GateDefinition { Id = "green", Title = "Run two continuous Green Cube labs" },
            new GateDefinition { Id = "dyson", Title = "Establish reliable Antimatter production" },
            new GateDefinition { Id = "photon", Title = "Bank Antimatter for White science" },
            new GateDefinition { Id = "white", Title = "Complete the main progression route" }
        };

        public static GuideProgressionEvaluation EvaluatePhase(
            string selectedPhaseId,
            ObservedGameState state)
        {
            string selected = ManualPhaseNavigator.NormalizePhase(
                selectedPhaseId);
            GateDefinition definition = FindGate(selected);
            GuideGateResult gate = EvaluateCurrentGate(
                definition ?? Gates[0], state);

            var result = new GuideProgressionEvaluation {
                SelectedPhase = selected
            };
            result.Gates.Add(gate);
            return result;
        }

        private static GateDefinition FindGate(string id)
        {
            foreach (GateDefinition gate in Gates)
                if (String.Equals(
                    gate.Id, id, StringComparison.OrdinalIgnoreCase))
                    return gate;
            return null;
        }

        private static GuideGateResult EvaluateCurrentGate(GateDefinition definition, ObservedGameState state)
        {
            var result = new GuideGateResult {
                Id = definition.Id,
                Title = definition.Title,
                Basis = "Current practical conditions evaluated from normalized live evidence."
            };

            if (definition.Id == "bootstrap") EvaluateBootstrap(result, state);
            else if (definition.Id == "blue") EvaluateBlue(result, state);
            else if (definition.Id == "red") EvaluateRed(result, state);
            else if (definition.Id == "ils") EvaluateIls(result, state);
            else if (definition.Id == "yellow") EvaluateYellow(result, state);
            else if (definition.Id == "purple") EvaluatePurple(result, state);
            else if (definition.Id == "green") EvaluateGreen(result, state);
            else if (definition.Id == "dyson") EvaluateDyson(result, state);
            else if (definition.Id == "photon") EvaluatePhoton(result, state);
            else if (definition.Id == "white") EvaluateWhite(result, state);

            bool blocked = false;
            bool watch = false;
            bool unknown = false;
            foreach (GuideGateCondition condition in result.Conditions)
            {
                if (!condition.Required) continue;
                if (condition.Status == "blocked") blocked = true;
                else if (condition.Status == "watch") watch = true;
                else if (condition.Status == "unknown") unknown = true;
            }
            result.Status = blocked || watch ? "in-progress" :
                (unknown ? "evidence-incomplete" : "complete");
            return result;
        }

        private static void EvaluateBootstrap(GuideGateResult gate, ObservedGameState state)
        {
            AddCombinedFlow(gate, state, "starter-inputs",
                "Iron, Copper, Magnetic Coils, and Circuit Boards arrive continuously",
                new int[] { 1101, 1104, 1202, 1301 },
                new double[] { 0.1, 0.1, 0.1, 0.1 },
                "Connect the four starter inputs to continuous production.");
            AddCombinedAvailability(gate, state, "starter-mall",
                "Routine buildings replenish automatically",
                new int[] { 2001, 2011, 2301, 2302, 2303, 2101, 2106, 2203, 2201 },
                new long[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                "Automate the routine buildings used to extend the factory.");
            AddPower(gate, state, true);
        }

        private static void EvaluateBlue(GuideGateResult gate, ObservedGameState state)
        {
            AddFlow(gate, state, "blue-continuous",
                "Blue Cubes (Electromagnetic Matrices) run continuously at 20/min",
                6001, 20, true);
            AddManualCondition(gate, "blue-labs-fed",
                "Research labs run without hand feeding",
                "Confirm that the Blue science loop reaches research automatically.",
                "Connect Blue Cube production to the research labs.");
            AddPower(gate, state, true);
        }

        private static void EvaluateRed(GuideGateResult gate, ObservedGameState state)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(
                    "red-loop", "Two labs sustain 20 Red Cubes (Energy Matrices) per minute without refinery deadlock",
                    "unknown", true, "Production statistics are still warming up.",
                    "unknown", "Let the factory run long enough to measure the Red science loop."));
                return;
            }
            double red = ItemRate(state, 6002);
            double oilOut = ItemConsumption(state, 1114);
            double hydrogenOut = ItemConsumption(state, 1120);
            int labs = ConfiguredRecipeMachines(state, 18);
            bool ready = labs >= 2 && red >= 20 && oilOut > 0 && hydrogenOut > 0;
            gate.Conditions.Add(Condition(
                "red-loop", "Two labs sustain 20 Red Cubes (Energy Matrices) per minute while both refinery outputs keep moving",
                ready ? "ready" : "blocked", true,
                "Found " + labs + " configured lab(s), " + Math.Round(red, 1) +
                    " Red Cubes/min, " + Math.Round(oilOut, 1) +
                    " Refined Oil/min leaving production, and " +
                    Math.Round(hydrogenOut, 1) +
                    " Hydrogen/min leaving production.",
                "derived", ready ? null : "Sustain two Red labs and keep both refinery outputs moving."));
        }

        private static void EvaluateYellow(GuideGateResult gate, ObservedGameState state)
        {
            AddConfiguredLabFlow(gate, state, "yellow-labs",
                "Three Yellow Cube (Structure Matrix) labs run continuously",
                27, 6003, 3,
                "Configure and supply three Yellow Cube labs.");
        }

        private static void EvaluateIls(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 2902, "Drive Engine Lv2 is researched", true);
            AddTech(gate, state, 1413, "Titanium Smelting is researched", true);
            AddManualCondition(gate, "expedition-loadout",
                "The round-trip fuel, power, buildings, and defense loadout is ready",
                "Confirm the expedition loadout before launch.",
                "Prepare the complete interplanetary expedition loadout.");

            double remoteTitanium = RemoteProduction(state, 1106);
            double remoteSilicon = RemoteProduction(state, 1105) + RemoteProduction(state, 1003);
            bool remoteLines = remoteTitanium > 0 && remoteSilicon > 0;
            gate.Conditions.Add(Condition(
                "remote-lines", "Remote Titanium and Silicon production is operating",
                remoteLines ? "ready" : "blocked", true,
                "Found remote Titanium at " + Math.Round(remoteTitanium, 1) +
                    "/min and Silicon at " + Math.Round(remoteSilicon, 1) + "/min.",
                "observed", remoteLines ? null : "Establish remote Titanium and Silicon production."));

            long titanium = Owned(state, 1106);
            long silicon = Owned(state, 1105) + Owned(state, 1003);
            bool cargoReady = titanium >= 860 && silicon >= 520;
            gate.Conditions.Add(Condition(
                "first-cargo", "The first return carries 860 Titanium Ingots and 520 Silicon",
                cargoReady ? "ready" : "blocked", true,
                "Owned cargo: " + titanium + " Titanium Ingots and " + silicon + " Silicon.",
                "observed", cargoReady ? null : "Return with the full 860 Titanium and 520 Silicon cargo."));

            bool yellowSpent = state.UnlockedTechIds.Contains(1414) &&
                state.UnlockedTechIds.Contains(1605);
            long yellow = Owned(state, 6003);
            gate.Conditions.Add(Condition(
                "yellow-purchase", "The 200 Yellow Cube research purchase is funded or complete",
                yellow >= 200 || yellowSpent ? "ready" : "blocked", true,
                yellowSpent ? "Titanium Alloy and Interstellar Logistics are complete." :
                    "Owned Yellow Cubes: " + yellow + "/200.",
                yellowSpent ? "derived" : "observed",
                yellow >= 200 || yellowSpent ? null : "Reserve 200 Yellow Cubes for the ILS purchase."));

            int stations = CountStellarStations(state) + (int)Owned(state, 2104);
            int vessels = CountLogisticsVessels(state) + (int)Owned(state, 5002);
            bool fleetReady = stations >= 2 && vessels >= 5;
            gate.Conditions.Add(Condition(
                "ils-fleet", "Two ILS stations and five Logistics Vessels are available",
                fleetReady ? "ready" : "blocked", true,
                "Found " + stations + " station(s) and " + vessels + " vessel(s) deployed or owned.",
                "observed", fleetReady ? null : "Prepare two ILS stations and five Logistics Vessels."));

            AddManualCondition(gate, "source-outpost",
                "The source outpost is powered, buffered, placed safely, and defended as needed",
                "Confirm the source outpost checklist in game.",
                "Finish the source outpost checklist before depending on it.");
            AddAutomatedRoute(gate, state, 1106, "automated-titanium",
                "Titanium arrives home without Icarus", true);
            bool siliconRoute = HasSustainableRoute(state, 1105) ||
                HasSustainableRoute(state, 1003);
            gate.Conditions.Add(Condition(
                "automated-silicon", "Silicon arrives home without Icarus",
                siliconRoute ? "ready" : "blocked", true,
                siliconRoute ? "An active interstellar Silicon route was found." :
                    "An active interstellar Silicon route wasn't found.",
                siliconRoute ? "derived" : "observed",
                siliconRoute ? null : "Activate the interstellar Silicon route."));
        }

        private static void EvaluatePurple(GuideGateResult gate, ObservedGameState state)
        {
            AddConfiguredLabFlow(gate, state, "purple-labs",
                "Three Purple Cube (Information Matrix) labs run continuously",
                55, 6004, 3,
                "Configure and supply three Purple Cube labs.");
        }

        private static void EvaluateGreen(GuideGateResult gate, ObservedGameState state)
        {
            AddConfiguredLabFlow(gate, state, "green-labs",
                "Two Green Cube (Gravity Matrix) labs run continuously",
                102, 6005, 2,
                "Configure and supply two Green Cube labs.");
            bool visibleInputs = Owned(state, 1305) > 0 && Owned(state, 1209) > 0;
            gate.Conditions.Add(Condition(
                "green-inputs", "Quantum Chips and Graviton Lenses are visible in storage",
                visibleInputs ? "ready" : "blocked", true,
                "Owned: " + Owned(state, 1305) + " Quantum Chips and " +
                    Owned(state, 1209) + " Graviton Lenses.",
                "observed", visibleInputs ? null : "Buffer both Green Cube inputs in visible storage."));
        }

        private static void EvaluateDyson(GuideGateResult gate, ObservedGameState state)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(
                    "antimatter-production", "Critical Photons become reliable Antimatter production",
                    "unknown", true, "Production statistics are still warming up.",
                    "unknown", "Let the factory run long enough to measure Photon and Antimatter production."));
            }
            else
            {
                double photons = ItemRate(state, 1208);
                double antimatter = ItemRate(state, 1122);
                bool ready = photons > 0 && antimatter > 0;
                gate.Conditions.Add(Condition(
                    "antimatter-production", "Critical Photons become reliable Antimatter production",
                    ready ? "ready" : "blocked", true,
                    "Found " + Math.Round(photons, 1) + " Critical Photons/min and " +
                        Math.Round(antimatter, 1) + " Antimatter/min.",
                    "observed", ready ? null : "Establish continuous Critical Photon and Antimatter production."));
            }
            bool delivered = ItemConsumption(state, 1122) > 0 ||
                HasSustainableRoute(state, 1122);
            gate.Conditions.Add(Condition(
                "antimatter-delivery", "Antimatter reaches the science build without manual carrying",
                delivered ? "ready" : "blocked", true,
                delivered ? "Antimatter consumption or an active logistics route was found." :
                    "Automatic Antimatter delivery wasn't found.",
                delivered ? "derived" : "observed",
                delivered ? null : "Connect Antimatter production to the science build."));
        }

        private static void EvaluatePhoton(GuideGateResult gate, ObservedGameState state)
        {
            long antimatter = Owned(state, 1122);
            gate.Conditions.Add(Condition(
                "antimatter-stock", "At least 2,000 Antimatter is stored",
                antimatter >= 2000 ? "ready" : "blocked", true,
                "Owned Antimatter: " + antimatter + "/2,000.",
                "observed", antimatter >= 2000 ? null : "Accumulate 2,000 Antimatter."));
            AddManualCondition(gate, "antimatter-pace",
                "The current Antimatter rate is sufficient for the planned White science run",
                "Confirm that the observed rate is sufficient for your intended finish.",
                "Keep expanding Photon and Antimatter production until the pace is acceptable.");
        }

        private static void EvaluateWhite(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 1507, "Universe Matrix is researched", true);
            bool missionComplete = state.UnlockedTechIds.Contains(1508);
            bool sixInputs = missionComplete || SixWhiteInputsReachScience(state);
            gate.Conditions.Add(Condition(
                "white-inputs", "All six White science inputs reach the labs continuously",
                sixInputs ? "ready" : "blocked", true,
                sixInputs ? "All five Matrix inputs and Antimatter are reaching active White production." :
                    "One or more White science inputs are not reaching active production.",
                "derived", sixInputs ? null : "Connect all five Matrix colors and Antimatter to White science."));
            int labs = ConfiguredRecipeMachines(state, 75);
            double whiteRate = ItemRate(state, 6006);
            bool whiteReady = missionComplete ||
                (state.ProductionWindowReady && labs >= 10 && whiteRate >= 40);
            gate.Conditions.Add(Condition(
                "white-production", "Ten labs sustain 40 White Cubes (Universe Matrices) per minute",
                whiteReady ? "ready" : (state.ProductionWindowReady ? "blocked" : "unknown"), true,
                "Found " + labs + " configured lab(s) and " + Math.Round(whiteRate, 1) +
                    " White Cubes/min.",
                state.ProductionWindowReady ? "observed" : "unknown",
                whiteReady ? null : "Build and supply ten White Cube labs at 40/min."));
            gate.Conditions.Add(Condition(
                "mission-completed",
                "Mission Completed consumes the final 4,000 White Cubes",
                missionComplete ? "ready" :
                    (state.QueuedTechIds.Contains(1508) ? "watch" : "blocked"),
                true,
                missionComplete ? "Mission Completed is researched." :
                    (state.QueuedTechIds.Contains(1508) ? "Mission Completed is in the research queue." :
                        "Mission Completed has not been queued."),
                "observed",
                missionComplete ? null : "Research Mission Completed with 4,000 White Cubes."));
        }

        private static void AddConfiguredLabFlow(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int recipeId,
            int itemId,
            int desiredLabs,
            string action)
        {
            int labs = ConfiguredRecipeMachines(state, recipeId);
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(
                    id, label, "unknown", true,
                    "Found " + labs + " configured lab(s); production statistics are still warming up.",
                    "unknown", action));
                return;
            }
            double rate = ItemRate(state, itemId);
            bool ready = labs >= desiredLabs && rate > 0;
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                "Found " + labs + " configured lab(s) producing " +
                    Math.Round(rate, 1) + "/min.",
                "observed", ready ? null : action));
        }

        private static int CountStellarStations(ObservedGameState state)
        {
            int count = 0;
            foreach (ObservedStationState station in state.Stations)
                if (station.IsStellar) count++;
            return count;
        }

        private static int CountLogisticsVessels(ObservedGameState state)
        {
            int count = 0;
            foreach (ObservedStationState station in state.Stations)
                if (station.IsStellar)
                    count += station.IdleShipCount + station.WorkShipCount;
            return count;
        }

        private static bool SixWhiteInputsReachScience(
            ObservedGameState state)
        {
            if (!state.ProductionWindowReady || ItemRate(state, 6006) <= 0)
                return false;
            for (int itemId = 6001; itemId <= 6005; itemId++)
                if (ItemConsumption(state, itemId) <= 0 && Owned(state, itemId) <= 0)
                    return false;
            return ItemConsumption(state, 1122) > 0 || Owned(state, 1122) > 0;
        }

        private static void AddFlow(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int itemId,
            double minimum,
            bool required)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(id, label, "unknown", required,
                    "Production observation window is not ready.", "unknown",
                    "Let the factory run long enough to establish a rate."));
                return;
            }
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(itemId, out flow);
            double rate = flow != null ? flow.ProducedPerMinute : 0.0;
            bool ready = rate >= minimum;
            string status = ready ? "ready" : "blocked";
            string itemName = flow != null && !String.IsNullOrEmpty(flow.Name)
                ? flow.Name : ItemNameForAction(itemId, label);
            gate.Conditions.Add(Condition(
                id, label, status, required,
                "Found " + Math.Round(rate, 1) +
                    "/min; desired " + minimum + "/min.",
                "observed", ready ? null :
                    "Build or stabilize " + itemName + " at or above " + minimum + "/min."));
        }

        private static void AddCombinedFlow(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int[] itemIds,
            double[] desiredRates,
            string action)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(
                    id, label, "unknown", true,
                    "The production statistics window is not ready.",
                    "unknown", "Let the factory run long enough to check this objective."));
                return;
            }
            bool ready = true;
            var rates = new List<string>();
            for (int i = 0; i < itemIds.Length; i++)
            {
                double rate = ItemRate(state, itemIds[i]);
                if (rate < desiredRates[i]) ready = false;
                rates.Add(Math.Round(rate, 1) + "/" +
                    desiredRates[i] + "/min");
            }
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                "Found rates: " + String.Join(", ", rates.ToArray()) + ".",
                "observed", ready ? null : action));
        }

        private static void AddCombinedAvailability(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int[] itemIds,
            long[] usefulStocks,
            string action)
        {
            bool ready = true;
            var evidence = new List<string>();
            for (int i = 0; i < itemIds.Length; i++)
            {
                long stock = Owned(state, itemIds[i]);
                double rate = ItemRate(state, itemIds[i]);
                if (stock < usefulStocks[i] && rate <= 0) ready = false;
                evidence.Add(stock + " owned, " +
                    Math.Round(rate, 1) + "/min");
            }
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                "Found " + String.Join("; ", evidence.ToArray()) + ".",
                "derived", ready ? null : action));
        }

        private static void AddManualCondition(
            GuideGateResult gate,
            string id,
            string label,
            string evidence,
            string action)
        {
            gate.Conditions.Add(Condition(
                id, label, "unknown", true,
                evidence, "player-check", action));
        }

        private static void AddTech(
            GuideGateResult gate,
            ObservedGameState state,
            int techId,
            string label,
            bool required)
        {
            bool ready = state.UnlockedTechIds.Contains(techId);
            gate.Conditions.Add(Condition(
                "tech-" + techId, label, ready ? "ready" : "blocked", required,
                "Technology " + techId + (ready ? " is complete." : " is incomplete."),
                "observed", ready ? null : "Research " + TechName(state, techId) + "."));
        }

        private static void AddPower(GuideGateResult gate, ObservedGameState state, bool required)
        {
            if (state.PowerPlanets.Count == 0)
            {
                gate.Conditions.Add(Condition(
                    "power", "Power remains supplied with useful headroom", "unknown", required,
                    "No rolling power observations are available.", "unknown",
                    "Let the factory run long enough to observe power."));
                return;
            }
            double minimum = 1.0;
            double maximumDemand = 0.0;
            foreach (ObservedPowerState power in state.PowerPlanets)
            {
                if (power.Observations <= 0) continue;
                if (power.MinimumSatisfaction < minimum) minimum = power.MinimumSatisfaction;
                if (power.MaximumDemandToCapacity > maximumDemand) maximumDemand = power.MaximumDemandToCapacity;
            }
            string status = minimum < 0.99 ? "blocked" : (maximumDemand > 0.85 ? "watch" : "ready");
            gate.Conditions.Add(Condition(
                "power", "Power remains supplied with useful headroom", status, required,
                "Minimum satisfaction " + Math.Round(minimum * 100.0, 1) +
                    "%; peak demand/capacity " + Math.Round(maximumDemand * 100.0, 1) + "%.",
                "observed", status == "ready" ? null : "Increase reliable generation or reduce charging pressure."));
        }

        private static void AddAutomatedRoute(
            GuideGateResult gate,
            ObservedGameState state,
            int itemId,
            string id,
            string label,
            bool required)
        {
            bool pairedTraffic = HasImportAndExport(state, itemId);
            bool ready = pairedTraffic || HasSustainableRoute(state, itemId);
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", required,
                pairedTraffic
                    ? "Matching import and export traffic was observed during the rolling window."
                    : (ready
                        ? "A one-sided traffic observation is corroborated by matching remote Supply/Demand station policy and source production or stock."
                        : "No traffic or corroborated supplied/demanded route was proven."),
                pairedTraffic ? "observed" : (ready ? "derived" : "observed"),
                ready ? null : "Activate the automated interplanetary route."));
        }

        private static bool HasImportAndExport(ObservedGameState state, int itemId)
        {
            bool input = false;
            bool output = false;
            foreach (ObservedTrafficFlow flow in state.TrafficFlows)
            {
                if (flow.ItemId != itemId) continue;
                if (flow.InputPerMinute > 0) input = true;
                if (flow.OutputPerMinute > 0) output = true;
            }
            return input && output;
        }

        private static bool HasSustainableRoute(ObservedGameState state, int itemId)
        {
            if (HasImportAndExport(state, itemId)) return true;
            bool trafficInput = false;
            bool trafficOutput = false;
            foreach (ObservedTrafficFlow flow in state.TrafficFlows)
            {
                if (flow.ItemId != itemId) continue;
                if (flow.InputPerMinute > 0.0) trafficInput = true;
                if (flow.OutputPerMinute > 0.0) trafficOutput = true;
            }

            var supplyPlanets = new HashSet<int>();
            var demandPlanets = new HashSet<int>();
            foreach (ObservedStationSlot slot in state.StationSlots)
            {
                if (!slot.IsStellar || slot.ItemId != itemId) continue;
                string remote = slot.RemoteLogic ?? "";
                if (remote.IndexOf("Supply", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (slot.Count > 0 || RemotePlanetProduction(state, slot.PlanetId, itemId) > 0.0))
                    supplyPlanets.Add(slot.PlanetId);
                if (remote.IndexOf("Demand", StringComparison.OrdinalIgnoreCase) >= 0)
                    demandPlanets.Add(slot.PlanetId);
            }
            bool matchedPolicies = false;
            foreach (int supply in supplyPlanets)
                foreach (int demand in demandPlanets)
                    if (supply != demand) matchedPolicies = true;

            return matchedPolicies && (trafficInput || trafficOutput);
        }

        private static double RemotePlanetProduction(
            ObservedGameState state,
            int planetId,
            int itemId)
        {
            double total = 0.0;
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                if (flow.PlanetId == planetId && flow.ItemId == itemId)
                    total += flow.ProducedPerMinute;
            return total;
        }

        private static bool HasStellarStation(ObservedGameState state)
        {
            foreach (ObservedStationSlot slot in state.StationSlots)
                if (slot.IsStellar) return true;
            return false;
        }

        private static int DistinctFactoryPlanets(ObservedGameState state)
        {
            var ids = new HashSet<int>();
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                if (flow.PlanetId > 0) ids.Add(flow.PlanetId);
            return ids.Count;
        }

        private static double RemoteProduction(ObservedGameState state, int itemId)
        {
            double total = 0.0;
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                if (flow.FactoryIndex > 0 && flow.ItemId == itemId)
                    total += flow.ProducedPerMinute;
            return total;
        }

        private static long Owned(ObservedGameState state, int itemId)
        {
            long count;
            return state.OwnedItemCounts.TryGetValue(itemId, out count) ? count : 0L;
        }

        private static int ConfiguredRecipeMachines(ObservedGameState state, int recipeId)
        {
            int total = 0;
            foreach (ObservedRecipeConfiguration recipe in state.RecipeConfigurations)
                if (recipe.RecipeId == recipeId)
                    total += recipe.ConfiguredMachineCount;
            return total;
        }

        private static double ItemRate(ObservedGameState state, int itemId)
        {
            ObservedItemFlow flow;
            return state.ItemFlows.TryGetValue(itemId, out flow)
                ? flow.ProducedPerMinute : 0.0;
        }

        private static double ItemConsumption(ObservedGameState state, int itemId)
        {
            ObservedItemFlow flow;
            return state.ItemFlows.TryGetValue(itemId, out flow)
                ? flow.ConsumedPerMinute : 0.0;
        }

        private static string TechName(ObservedGameState state, int techId)
        {
            string name;
            return state.TechNames.TryGetValue(techId, out name) ? name : ("Technology " + techId);
        }

        private static string ItemNameForAction(int itemId, string fallback)
        {
            if (itemId == 6001) return "Electromagnetic Matrix production";
            if (itemId == 6002) return "Energy Matrix production";
            if (itemId == 6003) return "Structure Matrix production";
            if (itemId == 6004) return "Information Matrix production";
            if (itemId == 6005) return "Gravity Matrix production";
            if (itemId == 1101) return "Iron Ingot production";
            if (itemId == 1104) return "Copper Ingot production";
            if (itemId == 1121) return "Deuterium supply";
            if (itemId == 1123) return "Graphene production";
            if (itemId == 1124) return "Carbon Nanotube production";
            if (itemId == 1127) return "Strange Matter production";
            if (itemId == 1209) return "Graviton Lens production";
            if (itemId == 1303) return "Processor production";
            if (itemId == 1305) return "Quantum Chip production";
            if (itemId == 1402) return "Particle Broadband production";
            return fallback;
        }

        private static GuideGateCondition Condition(
            string id,
            string label,
            string status,
            bool required,
            string evidence,
            string evidenceKind,
            string action)
        {
            return new GuideGateCondition {
                Id = id, Label = label, Status = status, Required = required,
                Evidence = evidence, EvidenceKind = evidenceKind, Action = action
            };
        }

    }
}
