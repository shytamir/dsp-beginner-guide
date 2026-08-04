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
                { "contractVersion", "2.7" },
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

            if (definition.Id == "blue") EvaluateBlue(result, state);
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

        private static void EvaluateBlue(GuideGateResult gate, ObservedGameState state)
        {
            AddNamedFlowSet(gate, state, "starter-inputs",
                "Starter inputs arrive continuously",
                new int[] { 1101, 1104, 1202, 1301 },
                new string[] { "Iron Ingots", "Copper Ingots", "Magnetic Coils", "Circuit Boards" },
                "Connect the missing starter inputs to continuous production.");
            AddNamedAvailabilitySet(gate, state, "starter-mall",
                "Routine factory hardware replenishes automatically",
                new int[] { 2001, 2011, 2301, 2302, 2303, 2101, 2106, 2203, 2201 },
                new string[] {
                    "Conveyor Belts", "Sorters", "Mining Machines", "Arc Smelters",
                    "Assembling Machines", "Storage", "Storage Tanks", "Wind Turbines",
                    "Tesla Towers"
                },
                "Automate the missing routine factory hardware.");
            AddFlow(gate, state, "blue-continuous",
                "Blue Cubes (Electromagnetic Matrices) run continuously at 20/min",
                6001, 20, true);
            AddManualCondition(gate, "blue-labs-fed",
                "Research labs run without hand feeding",
                "Confirm that the Blue science loop reaches research automatically.",
                "Connect Blue Cube production to the research labs.");
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
            int expeditionPlanetId = FindExpeditionPlanet(state);
            double titaniumRate = PlanetProduction(state, expeditionPlanetId, 1004, 1106);
            double siliconRate = PlanetProduction(state, expeditionPlanetId, 1003, 1105);
            long titaniumCargo = PlanetOwned(state, expeditionPlanetId, 1106);
            long siliconCargo = PlanetOwned(state, expeditionPlanetId, 1105);
            bool expeditionActive = expeditionPlanetId > 0 &&
                (titaniumRate > 0 || siliconRate > 0 || titaniumCargo > 0 || siliconCargo > 0);
            bool cargoReady = titaniumCargo >= 860 && siliconCargo >= 520;
            bool rushStarted = cargoReady || TechStarted(state, 1414) ||
                TechStarted(state, 1604) || TechStarted(state, 2903) ||
                TechStarted(state, 1605) || CountStellarStations(state) > 0;

            if (!expeditionActive && !rushStarted)
            {
                EvaluateIlsPreparation(gate, state);
                return;
            }
            if (!rushStarted)
            {
                EvaluateIlsExpedition(gate, state, expeditionPlanetId,
                    titaniumRate, siliconRate, titaniumCargo, siliconCargo);
                return;
            }
            EvaluateIlsRush(gate, state);
        }

        private static void EvaluateIlsPreparation(GuideGateResult gate, ObservedGameState state)
        {
            var missing = new List<string>();
            if (!state.UnlockedTechIds.Contains(2902)) missing.Add("Drive Engine Lv2");
            if (!state.UnlockedTechIds.Contains(1413)) missing.Add("Titanium Smelting");
            AddMissingPlayerItem(state, missing, 2301, "Mining Machine");
            AddMissingPlayerItem(state, missing, 2302, "Arc Smelter");
            AddMissingPlayerItem(state, missing, 2101, "Storage Mk.I");
            AddMissingPlayerItem(state, missing, 2001, "Conveyor Belt");
            AddMissingPlayerItem(state, missing, 2011, "Sorter");
            AddMissingPlayerItem(state, missing, 2201, "Tesla Tower");
            if (PlayerOwned(state, 2203) <= 0 && PlayerOwned(state, 2204) <= 0)
                missing.Add("independent power");
            bool ready = missing.Count == 0;
            gate.Conditions.Add(Condition(
                "ils-preparation", "The interplanetary expedition is ready to launch",
                ready ? "ready" : "blocked", true,
                ready ? "Required flight technology and outpost essentials are in Icarus." :
                    "Still needed: " + String.Join(", ", missing.ToArray()) + ".",
                "observed", ready ? null : "Research the missing technology and load the listed outpost essentials."));
        }

        private static void EvaluateIlsExpedition(
            GuideGateResult gate,
            ObservedGameState state,
            int planetId,
            double titaniumRate,
            double siliconRate,
            long titaniumCargo,
            long siliconCargo)
        {
            bool productionReady = titaniumRate > 0 && siliconRate > 0;
            string planetName = PlanetName(state, planetId);
            gate.Conditions.Add(Condition(
                "ils-expedition-production", "Titanium and Silicon production is active on " + planetName,
                productionReady ? "ready" : "blocked", true,
                "Found Titanium at " + Math.Round(titaniumRate, 1) +
                    "/min and Silicon at " + Math.Round(siliconRate, 1) + "/min.",
                "observed", productionReady ? null : "Start both Titanium and Silicon production on the expedition planet."));
            bool cargoReady = titaniumCargo >= 860 && siliconCargo >= 520;
            gate.Conditions.Add(Condition(
                "ils-expedition-cargo", "The return cargo is buffered in local storage",
                cargoReady ? "ready" : "blocked", true,
                titaniumCargo + "/860 Titanium Ingots and " +
                    siliconCargo + "/520 High-Purity Silicon stored on " + planetName + ".",
                "observed", cargoReady ? null : "Buffer the full return cargo in local storage."));
        }

        private static void EvaluateIlsRush(GuideGateResult gate, ObservedGameState state)
        {
            int[] chain = { 1414, 1604, 2903, 1605 };
            int currentTech = 0;
            foreach (int techId in chain)
                if (!state.UnlockedTechIds.Contains(techId)) { currentTech = techId; break; }
            bool chainReady = currentTech == 0;
            gate.Conditions.Add(Condition(
                "ils-rush-tech", chainReady ? "The ILS research chain is complete" :
                    "Current research target: " + TechName(state, currentTech),
                chainReady ? "ready" : (state.QueuedTechIds.Contains(currentTech) ? "watch" : "blocked"),
                true,
                chainReady ? "Interstellar Logistics System is researched." :
                    (state.QueuedTechIds.Contains(currentTech) ? "This technology is queued." : "This technology is not queued."),
                "observed", chainReady ? null : "Research " + TechName(state, currentTech) + "."));

            int stations = CountStellarStations(state) + (int)Owned(state, 2104);
            int vessels = CountLogisticsVessels(state) + (int)Owned(state, 5002);
            bool fleetReady = stations >= 2 && vessels >= 5;
            var missing = MissingIlsReserve(state);
            bool reserveReady = fleetReady || missing.Count == 0;
            gate.Conditions.Add(Condition(
                "ils-rush-reserve", "The protected ILS build reserve is complete",
                reserveReady ? "ready" : "blocked", true,
                fleetReady ? "Found two ILS stations and five Logistics Vessels." :
                    (reserveReady ? "All protected components are stored together." :
                        "Still needed: " + String.Join(", ", missing.ToArray()) + "."),
                "observed", reserveReady ? null : "Store the missing components without spending the protected reserve."));

            bool titaniumRoute = HasSustainableRoute(state, 1106);
            bool siliconRoute = HasSustainableRoute(state, 1105) ||
                HasSustainableRoute(state, 1003);
            bool routesReady = titaniumRoute && siliconRoute;
            var missingRoutes = new List<string>();
            if (!titaniumRoute) missingRoutes.Add("Titanium");
            if (!siliconRoute) missingRoutes.Add("Silicon");
            gate.Conditions.Add(Condition(
                "ils-rush-routes", "Titanium and Silicon arrive home automatically",
                routesReady ? "ready" : "blocked", true,
                routesReady ? "Both activated ILS routes were found." :
                    "Missing activated route: " + String.Join(" and ", missingRoutes.ToArray()) + ".",
                routesReady ? "derived" : "observed",
                routesReady ? null : "Activate the missing ILS route."));
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
            double sailProduction = ItemRate(state, 1501);
            double sailLaunches = ItemConsumption(state, 1501);
            bool sailsReady = state.ProductionWindowReady && sailProduction > 0 && sailLaunches > 0;
            gate.Conditions.Add(Condition(
                "dyson-sails", "Solar Sails are being produced and launched",
                sailsReady ? "ready" : (state.ProductionWindowReady ? "blocked" : "unknown"), true,
                "Found " + Math.Round(sailProduction, 1) + "/min produced and " +
                    Math.Round(sailLaunches, 1) + "/min launched.",
                state.ProductionWindowReady ? "observed" : "unknown",
                sailsReady ? null : "Supply an active EM-Rail Ejector line with Solar Sails."));
            bool swarmReady = state.Dyson.SwarmSailCount > 0 && state.Dyson.SwarmGenerationWatts > 0;
            gate.Conditions.Add(Condition(
                "dyson-swarm", "The Dyson swarm is generating power",
                swarmReady ? "ready" : "blocked", true,
                "Found " + state.Dyson.SwarmSailCount + " active sails generating " +
                    FormatPower(state.Dyson.SwarmGenerationWatts) + ".",
                "observed", swarmReady ? null : "Keep sails in orbit and confirm the swarm is generating power."));
        }

        private static void EvaluatePhoton(GuideGateResult gate, ObservedGameState state)
        {
            double photons = ItemRate(state, 1208);
            double antimatterRate = ItemRate(state, 1122);
            bool productionReady = state.ProductionWindowReady && photons > 0 && antimatterRate > 0;
            gate.Conditions.Add(Condition(
                "photon-production", "Critical Photon and Antimatter production is running",
                productionReady ? "ready" : (state.ProductionWindowReady ? "blocked" : "unknown"), true,
                "Found " + Math.Round(photons, 1) + " Critical Photons/min and " +
                    Math.Round(antimatterRate, 1) + " Antimatter/min; 48/min is the receiver-array reference.",
                state.ProductionWindowReady ? "observed" : "unknown",
                productionReady ? null : "Establish continuous Critical Photon and Antimatter production."));
            long antimatter = Owned(state, 1122);
            gate.Conditions.Add(Condition(
                "antimatter-stock", "The Antimatter bank reaches the 2,000 midpoint",
                antimatter >= 2000 ? "ready" : "blocked", true,
                antimatter + "/2,000 stored" + (antimatter >= 2000 ? " - halfway to the final research cost." : "."),
                "observed", antimatter >= 2000 ? null : "Bank 2,000 Antimatter to reach the midway checkpoint."));
        }

        private static void EvaluateWhite(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 1507, "Universe Matrix is researched", true);
            bool missionComplete = state.UnlockedTechIds.Contains(1508);
            int labs = ConfiguredRecipeMachines(state, 75);
            double whiteRate = ItemRate(state, 6006);
            long whiteStored = Owned(state, 6006);
            bool whiteReady = missionComplete ||
                (state.ProductionWindowReady && labs >= 10 && whiteRate >= 40);
            gate.Conditions.Add(Condition(
                "white-production", "Ten labs sustain 40 White Cubes (Universe Matrices) per minute",
                whiteReady ? "ready" : (state.ProductionWindowReady ? "blocked" : "unknown"), true,
                "Found " + labs + " configured lab(s), " + Math.Round(whiteRate, 1) +
                    " White Cubes/min, and " + whiteStored + " White Cubes stored.",
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

        private static void AddNamedFlowSet(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int[] itemIds,
            string[] itemNames,
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
            var missing = new List<string>();
            for (int i = 0; i < itemIds.Length; i++)
            {
                double rate = ItemRate(state, itemIds[i]);
                if (rate <= 0) missing.Add(itemNames[i]);
            }
            bool ready = missing.Count == 0;
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                ready ? "All four starter inputs are producing." :
                    "Not producing: " + String.Join(", ", missing.ToArray()) + ".",
                "observed", ready ? null : action));
        }

        private static void AddNamedAvailabilitySet(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int[] itemIds,
            string[] itemNames,
            string action)
        {
            var missing = new List<string>();
            for (int i = 0; i < itemIds.Length; i++)
            {
                long stock = Owned(state, itemIds[i]);
                double rate = ItemRate(state, itemIds[i]);
                if (stock < 1 && rate <= 0) missing.Add(itemNames[i]);
            }
            bool ready = missing.Count == 0;
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                ready ? "Routine factory hardware is stocked or replenishing." :
                    "Missing: " + String.Join(", ", missing.ToArray()) + ".",
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

        private static int FindExpeditionPlanet(ObservedGameState state)
        {
            var planetIds = new HashSet<int>();
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                if (flow.PlanetId > 0) planetIds.Add(flow.PlanetId);
            foreach (int planetId in state.PlanetItemCounts.Keys)
                if (planetId > 0) planetIds.Add(planetId);

            int bestPlanetId = 0;
            double bestScore = 0.0;
            foreach (int planetId in planetIds)
            {
                double titaniumRate = PlanetProduction(state, planetId, 1004, 1106);
                double siliconRate = PlanetProduction(state, planetId, 1003, 1105);
                long titanium = PlanetOwned(state, planetId, 1106);
                long silicon = PlanetOwned(state, planetId, 1105);
                double score = (titaniumRate > 0 ? 1000.0 : 0.0) +
                    (siliconRate > 0 ? 1000.0 : 0.0) +
                    Math.Min(titanium, 860) / 860.0 +
                    Math.Min(silicon, 520) / 520.0;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPlanetId = planetId;
                }
            }
            return bestPlanetId;
        }

        private static double PlanetProduction(
            ObservedGameState state,
            int planetId,
            params int[] itemIds)
        {
            if (planetId <= 0) return 0.0;
            double total = 0.0;
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
            {
                if (flow.PlanetId != planetId) continue;
                foreach (int itemId in itemIds)
                    if (flow.ItemId == itemId) total += flow.ProducedPerMinute;
            }
            return total;
        }

        private static long PlanetOwned(ObservedGameState state, int planetId, int itemId)
        {
            Dictionary<int, long> counts;
            long count;
            return planetId > 0 && state.PlanetItemCounts.TryGetValue(planetId, out counts) &&
                counts.TryGetValue(itemId, out count) ? count : 0L;
        }

        private static long PlayerOwned(ObservedGameState state, int itemId)
        {
            long count;
            return state.PlayerItemCounts.TryGetValue(itemId, out count) ? count : 0L;
        }

        private static string PlanetName(ObservedGameState state, int planetId)
        {
            string name;
            return state.PlanetNames.TryGetValue(planetId, out name) &&
                !String.IsNullOrEmpty(name) ? name : "the expedition planet";
        }

        private static bool TechStarted(ObservedGameState state, int techId)
        {
            return state.UnlockedTechIds.Contains(techId) ||
                state.QueuedTechIds.Contains(techId);
        }

        private static void AddMissingPlayerItem(
            ObservedGameState state,
            List<string> missing,
            int itemId,
            string name)
        {
            if (PlayerOwned(state, itemId) <= 0) missing.Add(name);
        }

        private static List<string> MissingIlsReserve(ObservedGameState state)
        {
            int planetId = BestIlsReservePlanet(state);
            int[] itemIds = { 1103, 1106, 1303, 1206, 1107, 1203 };
            long[] targets = { 80, 80, 130, 80, 180, 50 };
            string[] names = {
                "Steel", "Titanium Ingots", "Processors",
                "Particle Containers", "Titanium Alloy", "Electromagnetic Turbines"
            };
            var missing = new List<string>();
            for (int i = 0; i < itemIds.Length; i++)
            {
                long found = PlanetOwned(state, planetId, itemIds[i]) +
                    PlayerOwned(state, itemIds[i]);
                if (found < targets[i])
                    missing.Add((targets[i] - found) + " " + names[i]);
            }
            long yellowTarget = state.UnlockedTechIds.Contains(1605) ? 0 :
                (state.UnlockedTechIds.Contains(1414) ? 120 : 200);
            long yellowFound = PlanetOwned(state, planetId, 6003) + PlayerOwned(state, 6003);
            if (yellowFound < yellowTarget)
                missing.Add((yellowTarget - yellowFound) + " Yellow Cubes");
            return missing;
        }

        private static int BestIlsReservePlanet(ObservedGameState state)
        {
            if (state.PlayerPlanetId > 0 && state.PlanetItemCounts.ContainsKey(state.PlayerPlanetId))
                return state.PlayerPlanetId;
            int bestPlanetId = 0;
            long bestScore = -1;
            foreach (int planetId in state.PlanetItemCounts.Keys)
            {
                long score = PlanetOwned(state, planetId, 1103) +
                    PlanetOwned(state, planetId, 1106) +
                    PlanetOwned(state, planetId, 1303) +
                    PlanetOwned(state, planetId, 1206) +
                    PlanetOwned(state, planetId, 1107) +
                    PlanetOwned(state, planetId, 1203) +
                    PlanetOwned(state, planetId, 6003);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPlanetId = planetId;
                }
            }
            return bestPlanetId;
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
