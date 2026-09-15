using System;
using System.Collections.Generic;
using System.Globalization;

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
                { "contractVersion", "3.8" },
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
            new GateDefinition { Id = "dyson", Title = "Build the Photon swarm" },
            new GateDefinition { Id = "photon", Title = "Run the Critical Photon receiver array" },
            new GateDefinition { Id = "white", Title = "Complete the main progression route" }
        };

        public static GuideProgressionEvaluation EvaluatePhase(
            string selectedPhaseId,
            ObservedGameState state, int ilsStage = 1)
        {
            string selected = ManualPhaseNavigator.NormalizePhase(
                selectedPhaseId);
            GateDefinition definition = FindGate(selected);
            GuideGateResult gate = EvaluateCurrentGate(
                definition ?? Gates[0], state, ilsStage);

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

        private static GuideGateResult EvaluateCurrentGate(GateDefinition definition, ObservedGameState state, int ilsStage)
        {
            var result = new GuideGateResult {
                Id = definition.Id,
                Title = definition.Title,
                Basis = "Current practical conditions evaluated from normalized live evidence."
            };

            if (definition.Id == "blue") EvaluateBlue(result, state);
            else if (definition.Id == "red") EvaluateRed(result, state);
            else if (definition.Id == "ils") EvaluateIls(result, state, ilsStage);
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
                    "red-loop", "Two labs sustain 20 Red Cubes (Energy Matrices) per minute",
                    "unknown", true, "Production statistics are still warming up.",
                    "unknown", "Let the factory run long enough to measure the Red science loop."));
                return;
            }
            double red = ItemRate(state, 6002);
            int labs = ConfiguredRecipeMachines(state, 18);
            bool ready = labs >= 2 && red >= 20;
            gate.Conditions.Add(Condition(
                "red-loop", "Two labs sustain 20 Red Cubes (Energy Matrices) per minute",
                ready ? "ready" : "blocked", true,
                "Found " + labs + " configured lab(s), " + Math.Round(red, 1) +
                    " Red Cubes/min.",
                "derived", ready ? null : "Configure and supply two Red Cube labs at 20/min."));
        }

        private static void EvaluateYellow(GuideGateResult gate, ObservedGameState state)
        {
            AddConfiguredLabFlow(gate, state, "yellow-labs",
                "Three Yellow Cube (Structure Matrix) labs run continuously",
                27, 6003, 3,
                "Configure and supply three Yellow Cube labs.");
        }

        private static void EvaluateIls(GuideGateResult gate, ObservedGameState state, int stage)
        {
            if (stage == 3) EvaluateIlsRush(gate, state);
            else if (stage == 2) EvaluateIlsExpedition(gate, state);
            else EvaluateIlsPreparation(gate, state);
            if (stage != 3) gate.Conditions.Insert(0, IlsResearchPolicy.Evaluate(state, stage));
            if (stage != 2 && stage != 3) gate.Conditions.Add(IlsResearchPolicy.Survey(state));
        }

        private static void EvaluateIlsPreparation(GuideGateResult gate, ObservedGameState state)
        {
            var missing = new List<string>();
            if (state.PlayerInventoryAvailable)
            {
                AddMissingPlayerItem(state, missing, 2301, "Mining Machine");
                AddMissingPlayerItem(state, missing, 2302, "Arc Smelter");
                AddMissingPlayerItem(state, missing, 2101, "Storage Mk.I");
                AddMissingPlayerItem(state, missing, 2001, "Conveyor Belt");
                AddMissingPlayerItem(state, missing, 2011, "Sorter");
                AddMissingPlayerItem(state, missing, 2201, "Tesla Tower");
                if (PlayerOwned(state, 2203) <= 0 && PlayerOwned(state, 2204) <= 0)
                    missing.Add("independent power");
            }
            bool available = state.PlayerInventoryAvailable;
            bool ready = available && missing.Count == 0;
            gate.Conditions.Add(Condition(
                "ils-preparation", "Departure equipment",
                ready ? "ready" : (available ? "blocked" : "unknown"), true,
                !available ? "Icarus inventory is unavailable." : ready
                    ? "Outpost equipment observed. Check fuel and building space before departure."
                    : "Still needed: " + String.Join(", ", missing.ToArray()) + ".",
                "observed", available && !ready ? "Load the listed outpost equipment." : null));
        }

        private static void EvaluateIlsExpedition(GuideGateResult gate, ObservedGameState state)
        {
            int planetId = FindExpeditionPlanet(state);
            double titaniumRate, siliconRate;
            bool titaniumKnown = TryPlanetProduction(state, planetId, 1106, out titaniumRate);
            bool siliconKnown = TryPlanetProduction(state, planetId, 1105, out siliconRate);
            bool productionKnown = titaniumKnown && siliconKnown;
            bool productionReady = productionKnown && titaniumRate > 0 && siliconRate > 0;
            gate.Conditions.Add(Condition(
                "ils-expedition-production", "Outpost smelting",
                productionReady ? "ready" : (productionKnown ? "blocked" : "unknown"), true,
                productionKnown ? PlanetName(state, planetId) + ": " + Math.Round(titaniumRate, 1) +
                    "/min Titanium Ingots; " + Math.Round(siliconRate, 1) + "/min High-Purity Silicon."
                    : "Outpost production is unavailable.",
                "observed", productionKnown && !productionReady ? "Smelt both Titanium Ingots and High-Purity Silicon at the outpost." : null));

            bool homeKnown = state.PlayerLocationAvailable && state.StarterPlanetId > 0;
            bool atHome = homeKnown && state.PlayerPlanetId == state.StarterPlanetId;
            bool cargoKnown = homeKnown && state.PlayerInventoryAvailable && (!atHome || state.AvailablePlanetInventories.Contains(state.StarterPlanetId));
            long titanium = PlayerOwned(state, 1106) + (atHome ? PlanetOwned(state, state.StarterPlanetId, 1106) : 0);
            long silicon = PlayerOwned(state, 1105) + (atHome ? PlanetOwned(state, state.StarterPlanetId, 1105) : 0);
            bool cargoReady = cargoKnown && titanium >= 860 && silicon >= 520;
            string cargoDetail = cargoKnown ? titanium + "/860 Titanium Ingots; " + silicon +
                "/520 High-Purity Silicon " + (atHome ? "aboard or stored at home." : "aboard Icarus.")
                : "Cargo inventory is unavailable.";
            if (!atHome && state.AvailablePlanetInventories.Contains(planetId))
                cargoDetail += " Outpost stock: " + PlanetOwned(state, planetId, 1106) + " Titanium; " + PlanetOwned(state, planetId, 1105) + " Silicon.";
            var load = new List<string>();
            if (titanium < 860) load.Add((860 - titanium) + " Titanium Ingots");
            if (silicon < 520) load.Add((520 - silicon) + " High-Purity Silicon");
            gate.Conditions.Add(Condition(
                "ils-expedition-cargo", "Return cargo",
                cargoReady ? "ready" : (cargoKnown ? "blocked" : "unknown"), true,
                cargoDetail, "observed", cargoKnown && !cargoReady
                    ? (atHome ? "Gather " : "Load ") + String.Join(" and ", load.ToArray()) + (atHome ? " at home." : " into Icarus.") : null));
            bool homeReady = atHome && cargoReady;
            gate.Conditions.Add(Condition(
                "ils-expedition-home", "Cargo at home",
                homeReady ? "ready" : (homeKnown && cargoKnown ? "blocked" : "unknown"), true,
                homeReady ? "Cargo available at home. Continue with III Automation."
                    : !homeKnown ? "Home location is unavailable." : atHome ? "Home reached; the full cargo is not available." : "Return to the home planet with the cargo.",
                "observed", homeKnown && !atHome && cargoReady ? "Bring the cargo home." : null));
        }

        private static void EvaluateIlsRush(GuideGateResult gate, ObservedGameState state)
        {
            GuideGateCondition research = IlsResearchPolicy.Evaluate(state, 3);
            gate.Conditions.Add(research);
            bool researchReady = research.Status == "ready";

            IlsTransportEvidence transport = IlsTransportEvidence.Build(state);
            gate.Conditions.Add(Condition(
                "ils-rush-hardware", "Two ILS towers and five Vessels",
                transport.HardwareReady ? "ready" : transport.Available ? "blocked" : "unknown", true,
                "Observed: " + (transport.InventoryTowers + transport.DeployedTowers) + "/2 towers; " +
                    (transport.InventoryVessels + transport.AssignedVessels) + "/5 Vessels at home or assigned to the selected endpoints.",
                "observed", researchReady && transport.Available && !transport.HardwareReady
                    ? "Finish the package: two ILS towers and five Logistics Vessels." : null));
            gate.Conditions.Add(Condition(
                "ils-rush-deployment", "Configure the home and outpost stations",
                transport.DeploymentReady ? "ready" : transport.Available ? "blocked" : "unknown", true,
                transport.DeploymentReady ? "Home demands both finished materials with five assigned Vessels; the outpost supplies both."
                    : transport.Available ? "Both endpoint policies and the home fleet are needed." : "Station evidence is unavailable.",
                "observed", researchReady && transport.HardwareReady && transport.Available && !transport.DeploymentReady
                    ? "Set home to Remote Demand, outpost to Remote Supply, and assign five Vessels at home." : null));

            IlsReceiptEvidence receipt = state.IlsReceipt;
            gate.Conditions.Add(Condition(
                "ils-home-delivery", "Finished materials reach home",
                receipt.Status == "confirmed" ? "ready" : receipt.Status == "unavailable" ? "unknown" : "watch", true,
                receipt.Status == "confirmed" ? "Home imports observed for both finished materials."
                    : receipt.Status == "source-empty" ? "The outpost needs finished material stock or production."
                    : receipt.Status == "unavailable" ? "Delivery evidence is unavailable."
                    : "Awaiting delivery evidence at home.",
                "planet-level corroboration", researchReady && transport.DeploymentReady && receipt.Status == "source-empty"
                    ? "Supply Titanium Ingots and High-Purity Silicon at the outpost." : null));
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
            AddVisibleCubeInputs(gate, state, "green-inputs",
                "Quantum Chips and Graviton Lenses are visible in storage",
                1305, "Quantum Chips", 1209, "Graviton Lenses",
                "Buffer both Green Cube inputs in visible storage.");
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
            AddDysonBridge(gate, state);
        }

        private static void AddReceiverCondition(GuideGateResult gate, ObservedGameState state, string id, bool allowAction)
        {
            bool receiverEvidence = state.Dyson.ReceiverTelemetryAvailable;
            bool receiversReady = receiverEvidence &&
                state.Dyson.ConfiguredPhotonReceiverCount >= 4 &&
                state.Dyson.LensedPhotonReceiverCount >= 4 &&
                state.Dyson.SustainedPhotonReceiverCount >= 4;
            gate.Conditions.Add(Condition(
                id, "Four lensed Ray Receivers remain continuously supplied",
                receiversReady ? "ready" : (receiverEvidence ? "blocked" : "unknown"), true,
                receiverEvidence
                    ? state.Dyson.SustainedPhotonReceiverCount + "/4 sustained; " +
                        state.Dyson.LensedPhotonReceiverCount + "/4 currently lensed."
                    : "Receiver continuity telemetry is not ready.",
                receiverEvidence ? "observed" : "unknown",
                receiversReady || !allowAction || !receiverEvidence ? null : "Keep four Photon Generation receivers lensed and continuously supplied."));
        }

        internal static long StationaryStock(ObservedGameState state, int itemId, out bool available)
        {
            long total = 0;
            available = state.PlanetItemCounts.Count > 0;
            foreach (var planet in state.PlanetItemCounts)
            {
                bool known = state.AvailablePlanetInventories.Contains(planet.Key);
                available &= known;
                long count;
                if (known && planet.Value.TryGetValue(itemId, out count)) total += Math.Max(0L, count);
            }
            return total;
        }

        private static bool AddBridgeResearch(GuideGateResult gate, ObservedGameState state)
        {
            int[] ids = { 1504, 1505, 1506 };
            string[] names = { "Ray Receiver", "Planetary Ionosphere Utilization", "Dirac Inversion Mechanism" };
            for (int i = 0; i < ids.Length; i++)
            {
                if (state.UnlockedTechIds.Contains(ids[i])) continue;
                bool queued = state.QueuedTechIds.Contains(ids[i]);
                bool known = state.AvailableTechIds.Contains(ids[i]) && state.ResearchQueueAvailable;
                gate.Conditions.Add(Condition("dyson-bridge-research", "Receiver research", queued ? "watch" : known ? "blocked" : "unknown", true,
                    queued ? names[i] + " queued." : known ? names[i] + " is not yet researched." : "Research evidence is unavailable.",
                    "observed", known && !queued ? "Research " + names[i] + "." : null));
                return false;
            }
            gate.Conditions.Add(Condition("dyson-bridge-research", "Receiver research", "ready", true, "Receiver and conversion research complete.", "observed", null));
            return true;
        }

        private static void AddDysonBridge(GuideGateResult gate, ObservedGameState state)
        {
            bool researchReady = AddBridgeResearch(gate, state);
            AddReceiverCondition(gate, state, "dyson-receivers", researchReady);
            ObservedItemFlow photons, antimatter;
            bool ratesKnown = state.ItemFlows.TryGetValue(1208, out photons) && photons.OneMinuteAvailable;
            ratesKnown &= state.ItemFlows.TryGetValue(1122, out antimatter) && antimatter.OneMinuteAvailable;
            bool stockKnown;
            long stock = StationaryStock(state, 1122, out stockKnown);
            bool known = state.RecipeTelemetryAvailable && ratesKnown && (stockKnown || stock > 0);
            bool ready = known && ConfiguredRecipeMachines(state, 74) > 0 && photons.ProducedPerMinute > 0 &&
                photons.ConsumedPerMinute > 0 && antimatter.ProducedPerMinute > 0 && stock > 0;
            gate.Conditions.Add(Condition("dyson-conversion", "Convert Critical Photons to Antimatter", ready ? "ready" : known ? "blocked" : "unknown", true,
                ready ? "Cluster production and consumption observed; " + stock + " Antimatter in stationary storage."
                    : known ? "Needs Photon Materialization, Photon production and consumption, and stored Antimatter."
                    : "Conversion or stationary inventory evidence is unavailable.",
                "cluster-level observation", !ready && known && researchReady ? "Run Photon Materialization in a Collider and store its Antimatter." : null));
            AddManualCondition(gate, "dyson-handoff", "Check the Hydrogen outlet and science delivery",
                "Player check: confirm returned Hydrogen has an outlet and Antimatter reaches the science district automatically.",
                ready ? "Check the Hydrogen outlet and automatic Antimatter delivery." : null);
        }

        private static void EvaluatePhoton(GuideGateResult gate, ObservedGameState state)
        {
            AddReceiverCondition(gate, state, "photon-receivers", true);
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
            bool whiteResearched = state.UnlockedTechIds.Contains(1507);
            gate.Conditions.Add(Condition(
                "tech-1507", "White Cubes researched",
                whiteResearched ? "ready" : "blocked", true,
                null, "observed", null));
            bool missionComplete = state.UnlockedTechIds.Contains(1508);
            int labs = ConfiguredRecipeMachines(state, 75);
            double whiteRate = ItemRate(state, 6006);
            long whiteStored = Owned(state, 6006);
            bool whiteReady = missionComplete ||
                (state.ProductionWindowReady && labs >= 10 && whiteRate >= 40);
            gate.Conditions.Add(Condition(
                "white-production", "Ten labs sustain 40 White Cubes/min",
                whiteReady ? "ready" : (state.ProductionWindowReady ? "blocked" : "unknown"), true,
                labs + "/10 labs configured; " +
                    whiteStored.ToString("N0", CultureInfo.InvariantCulture) +
                    " White Cubes stored",
                state.ProductionWindowReady ? "observed" : "unknown",
                null));

            ObservedTechProgress progress;
            bool progressAvailable = state.TechProgress.TryGetValue(
                1508, out progress) && progress.HashUploaded > 0;
            string missionEvidence = missionComplete
                ? "Mission Completed complete"
                : (progressAvailable
                    ? "Mission Completed " +
                        Math.Min(99, progress.Percent) + "% done"
                    : (state.QueuedTechIds.Contains(1508)
                        ? "Mission Completed queued"
                        : "Mission Completed not queued"));
            gate.Conditions.Add(Condition(
                "mission-completed", "Mission Completed",
                missionComplete ? "ready" :
                    (state.QueuedTechIds.Contains(1508) ? "watch" : "blocked"),
                true,
                missionEvidence,
                "observed",
                missionComplete ? null : "Complete Mission Completed research."));
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
            ObservedItemFlow flow;
            bool terminalEvidenceMissing = (itemId == 6003 || itemId == 6004) &&
                (!state.RecipeTelemetryAvailable || !state.ItemFlows.TryGetValue(itemId, out flow) || !flow.OneMinuteAvailable);
            if (terminalEvidenceMissing)
            {
                gate.Conditions.Add(Condition(id, label, "unknown", true,
                    "Lab configuration or production evidence is unavailable.", "unknown", null));
                return;
            }
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

        private static void AddVisibleCubeInputs(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int firstItemId,
            string firstItemName,
            int secondItemId,
            string secondItemName,
            string action)
        {
            long firstOwned = Owned(state, firstItemId);
            long secondOwned = Owned(state, secondItemId);
            bool ready = firstOwned > 0 && secondOwned > 0;
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                "Owned: " + firstOwned + " " + firstItemName + " and " +
                    secondOwned + " " + secondItemName + ".",
                "observed", ready ? null : action));
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

        internal static int FindExpeditionPlanet(ObservedGameState state)
        {
            if (state.StarterPlanetId <= 0) return 0;
            if (state.PlayerLocationAvailable && state.PlayerPlanetId > 0 && state.PlayerPlanetId != state.StarterPlanetId)
                return state.PlayerPlanetId;
            var planetIds = new SortedSet<int>();
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                if (flow.PlanetId > 0 && flow.OneMinuteAvailable && flow.ProducedPerMinute > 0 &&
                    (flow.ItemId == 1105 || flow.ItemId == 1106)) planetIds.Add(flow.PlanetId);
            foreach (int planetId in state.AvailablePlanetInventories)
                if (PlanetOwned(state, planetId, 1105) > 0 || PlanetOwned(state, planetId, 1106) > 0) planetIds.Add(planetId);
            foreach (int planetId in planetIds)
                if (planetId != state.StarterPlanetId) return planetId;
            return 0;
        }

        private static bool TryPlanetProduction(ObservedGameState state, int planetId, int itemId, out double rate)
        {
            rate = 0;
            bool found = false;
            if (planetId <= 0) return false;
            foreach (ObservedFactoryItemFlow flow in state.FactoryItemFlows)
                if (flow.PlanetId == planetId && flow.ItemId == itemId)
                {
                    if (!flow.OneMinuteAvailable) return false;
                    found = true;
                    rate += flow.ProducedPerMinute;
                }
            return found;
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
