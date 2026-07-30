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
                { "contractVersion", "2.4" },
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
            new GateDefinition { Id = "bootstrap", Title = "Stop handcrafting the factory" },
            new GateDefinition { Id = "blue", Title = "Build the first continuous matrix line" },
            new GateDefinition { Id = "red", Title = "Solve oil and prepare for flight" },
            new GateDefinition { Id = "flight", Title = "Reach another planet safely" },
            new GateDefinition { Id = "titanium", Title = "Establish a useful off-world Titanium source" },
            new GateDefinition { Id = "yellow", Title = "Make the finite ILS research batch" },
            new GateDefinition { Id = "ils", Title = "End manual interplanetary hauling" },
            new GateDefinition { Id = "purple", Title = "Build the first truly wide production tier" },
            new GateDefinition { Id = "green", Title = "Make warpers routine and prepare Dyson industry" },
            new GateDefinition { Id = "dyson", Title = "Build the minimum useful Dyson swarm" },
            new GateDefinition { Id = "photon", Title = "Run the critical-photon receiver array" },
            new GateDefinition { Id = "white", Title = "Sustain Universe Matrix production" },
            new GateDefinition { Id = "logistics", Title = "Automate the infrastructure that moves everything" }
        };

        public static GuideProgressionEvaluation EvaluatePhase(
            string selectedPhaseId,
            ObservedGameState state)
        {
            string selected = ManualPhaseNavigator.NormalizePhase(
                selectedPhaseId);
            GuideGateResult gate;
            if (selected == "sphere")
                gate = EvaluateSphere(state);
            else if (selected == "warp")
                gate = EvaluateWarp(state);
            else
            {
                GateDefinition definition = FindGate(selected);
                gate = EvaluateCurrentGate(
                    definition ?? Gates[0], state);
            }

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
            else if (definition.Id == "flight") EvaluateFlight(result, state);
            else if (definition.Id == "titanium") EvaluateTitanium(result, state);
            else if (definition.Id == "yellow") EvaluateYellow(result, state);
            else if (definition.Id == "ils") EvaluateIls(result, state);
            else if (definition.Id == "purple") EvaluatePurple(result, state);
            else if (definition.Id == "green") EvaluateGreen(result, state);
            else if (definition.Id == "dyson") EvaluateDyson(result, state);
            else if (definition.Id == "photon") EvaluatePhoton(result, state);
            else if (definition.Id == "white") EvaluateWhite(result, state);
            else if (definition.Id == "logistics") EvaluateLogistics(result, state);

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
            AddCombinedFlow(gate, state, "ore-arrival",
                "Iron and Copper arrive continuously",
                new int[] { 1101, 1104 }, new double[] { 30, 20 },
                "Keep Iron and Copper smelting supplied.");
            AddCombinedFlow(gate, state, "basic-smelting",
                "Basic smelting runs without manual feeding",
                new int[] { 1101, 1104 }, new double[] { 30, 20 },
                "Connect miners, smelters, and output storage continuously.");
            AddCombinedAvailability(gate, state, "early-components",
                "Magnetic Coils and Circuit Boards are automated",
                new int[] { 1202, 1301 }, new long[] { 30, 30 },
                "Automate Magnetic Coils and Circuit Boards.");
            AddCombinedAvailability(gate, state, "basic-logistics",
                "Belts and Sorters are readily available",
                new int[] { 2001, 2011 }, new long[] { 100, 50 },
                "Automate and stock Belts and Sorters.");
            AddManualCondition(gate, "routine-machinery",
                "Ordinary machinery no longer requires repeated handcrafting",
                "Check that routine machines come from automated production.",
                "Automate the ordinary machines you keep rebuilding.");
            AddPower(gate, state, true);
        }

        private static void EvaluateBlue(GuideGateResult gate, ObservedGameState state)
        {
            AddPositiveFlow(gate, state, "blue-continuous",
                "Blue Cubes (Electromagnetic Matrices) are continuous", 6001, true);
            AddManualCondition(gate, "blue-labs-fed",
                "Research labs run without hand feeding",
                "Direct lab feeding cannot be confirmed from production totals.",
                "Connect Blue Cube production to the research labs.");
            AddFlow(gate, state, "blue-rate",
                "Blue Cube (Electromagnetic Matrix) output reaches 20/min",
                6001, 20, true);
            AddPower(gate, state, true);
            AddCombinedAvailability(gate, state, "blue-factory-supply",
                "Basic factory components no longer consume every Iron and Copper batch",
                new int[] { 1202, 1301, 2001, 2011 },
                new long[] { 30, 30, 100, 50 },
                "Expand the basic component supply.");
        }

        private static void EvaluateRed(GuideGateResult gate, ObservedGameState state)
        {
            AddPositiveFlow(gate, state, "red-continuous",
                "Red Cubes (Energy Matrices) are continuous", 6002, true);
            AddFlow(gate, state, "red-rate",
                "Red Cube (Energy Matrix) output reaches 10/min", 6002, 10, true);
            AddTankSafety(gate, state, 1114,
                "Refined Oil cannot permanently jam the refinery", true);
            AddTankSafety(gate, state, 1120,
                "Hydrogen cannot permanently jam the refinery", true);
            AddStableSupply(gate, state, "red-graphite",
                "Energetic Graphite reaches the Red Cube labs without hand feeding",
                1109, "Connect Energetic Graphite production to the Red Cube labs.");
            AddCombinedAvailability(gate, state, "red-staging",
                "Steel and Foundation are staged for expansion",
                new int[] { 1103, 1131 }, new long[] { 60, 30 },
                "Stock Steel and Foundation for the next build.");
            AddCubeCapability(gate, state, "red-blue-support",
                "Blue Cube (Electromagnetic Matrix) production keeps pace",
                6001, 20);
        }

        private static void EvaluateFlight(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 2902, "Drive Engine Lv2 is researched", true);
            AddTech(gate, state, 2202, "Mecha Core Lv2 is researched", true);
            AddTech(gate, state, 1413, "Titanium Smelting is researched", true);
            AddTechReadyOrQueued(gate, state, 1604,
                "Planetary Logistics System is researched or immediately researchable");
            AddTechOrQueue(gate, state, new int[] { 1703, 1302, 1114 },
                "Magnetic Particle Trap, Processor, and Reinforced Thruster are researched or queued", true);
            AddManualCondition(gate, "flight-margin",
                "Fuel and Mecha core energy are comfortable for the trip",
                "Travel margin depends on the player's route and loadout.",
                "Prepare enough fuel and core energy for the round trip.");
            AddManualCondition(gate, "destination-plan",
                "The Titanium destination and its power plan are understood",
                "Destination choice and intended construction cannot be inferred.",
                "Choose the Titanium planet and prepare its power plan.");
        }

        private static void EvaluateTitanium(GuideGateResult gate, ObservedGameState state)
        {
            double remoteOre = RemoteProduction(state, 1004);
            double remoteIngot = RemoteProduction(state, 1106);
            gate.Conditions.Add(Condition(
                "remote-titanium", "Remote Titanium extraction is operating",
                remoteOre > 0 ? "ready" : "blocked", true,
                "Observed remote Titanium Ore production: " + Math.Round(remoteOre, 1) + "/min.",
                "observed", "Establish Titanium Ore extraction on the source planet."));
            gate.Conditions.Add(Condition(
                "source-smelting", "Titanium is smelted at source",
                remoteIngot >= 60 ? "ready" : (remoteIngot > 0 ? "watch" : "blocked"), true,
                "Observed remote Titanium Ingot production: " + Math.Round(remoteIngot, 1) + "/min; guide minimum 60/min.",
                "observed", "Smelt Titanium on the source planet at 60/min or more."));
            long ingots;
            state.OwnedItemCounts.TryGetValue(1106, out ingots);
            gate.Conditions.Add(Condition(
                "first-haul", "The first serious Titanium batch is available",
                ingots >= 810 ? "ready" : (ingots >= 770 ? "watch" : "blocked"), true,
                "Owned Titanium Ingots: " + ingots + "; practical target 810–860.",
                "observed", "Accumulate the ILS-rush Titanium batch."));
            double remoteSilicon = RemoteProduction(state, 1003);
            gate.Conditions.Add(Condition(
                "direct-silicon", "Direct Silicon mining is established or planned",
                remoteSilicon > 0 ? "ready" : "unknown", true,
                remoteSilicon > 0 ? "Observed remote Silicon Ore production." :
                    "A future plan cannot be proven from runtime state.",
                remoteSilicon > 0 ? "observed" : "unknown",
                "Plan a direct Silicon source before Processor demand grows."));
            AddManualCondition(gate, "outpost-plan",
                "The outpost power plan and any needed defense plan are ready",
                "Future power and defense intent cannot be inferred.",
                "Prepare the outpost power plan and any defense the site needs.");
        }

        private static void EvaluateYellow(GuideGateResult gate, ObservedGameState state)
        {
            AddPositiveFlow(gate, state, "yellow-continuous",
                "Yellow Cubes (Structure Matrices) are continuous", 6003, true);
            AddFlow(gate, state, "yellow-rate",
                "Yellow Cube (Structure Matrix) output reaches 7.5/min",
                6003, 7.5, true);
            long yellow;
            state.OwnedItemCounts.TryGetValue(6003, out yellow);
            bool spentProof = state.UnlockedTechIds.Contains(1414) || state.UnlockedTechIds.Contains(1605);
            gate.Conditions.Add(Condition(
                "yellow-batch", "The finite 200-Yellow research batch is available or demonstrably spent",
                yellow >= 200 || spentProof ? "ready" : "blocked", true,
                spentProof ? "A Yellow-dependent ILS-path technology is complete." :
                    "Owned Structure Matrices: " + yellow + "/200.",
                spentProof ? "derived" : "observed",
                "Produce and reserve the 200 Yellow matrices for Titanium Alloy and ILS."));
            AddTech(gate, state, 1414, "High-Strength Titanium Alloy is complete", true);
            AddTechReadyOrQueued(gate, state, 1605,
                "Interstellar Logistics System is complete or finishing");
            bool stationReady = HasStellarStation(state) || Owned(state, 2104) >= 2;
            gate.Conditions.Add(Condition(
                "ils-hardware", "Most non-Yellow ILS hardware is ready",
                stationReady ? "ready" : "unknown", true,
                stationReady ? "Observed stellar station infrastructure or two owned ILS buildings." :
                    "Preparation of unbuilt hardware cannot be proven from absence alone.",
                "observed", "Prepare or build the first two ILS stations."));
        }

        private static void EvaluateIls(GuideGateResult gate, ObservedGameState state)
        {
            AddAutomatedRoute(gate, state, 1106, "automated-titanium", "Titanium arrives automatically", true);

            bool siliconRoute = HasSustainableRoute(state, 1105) || HasSustainableRoute(state, 1003);
            gate.Conditions.Add(Condition(
                "automated-silicon", "Silicon arrives automatically or has an equivalent sustainable route",
                siliconRoute ? "ready" : "blocked", true,
                siliconRoute ? "Traffic and/or matching interstellar station policies prove an active Silicon route." :
                    "An active Silicon route wasn't found.",
                siliconRoute ? "derived" : "observed",
                siliconRoute ? null : "Activate a sustainable Silicon route."));

            bool automated = HasSustainableRoute(state, 1106) &&
                (HasSustainableRoute(state, 1105) ||
                 HasSustainableRoute(state, 1003));
            gate.Conditions.Add(Condition(
                "manual-hauling-ended",
                "Manual interplanetary hauling is no longer routine",
                automated ? "ready" : "blocked", true,
                automated
                    ? "Automated Titanium and Silicon routes were found."
                    : "The main interplanetary material routes are not both active.",
                "derived",
                automated ? null : "Automate the routine interplanetary material routes."));
            AddPower(gate, state, true);

            bool localDistribution = false;
            foreach (ObservedStationSlot slot in state.StationSlots)
                if (!slot.IsStellar) { localDistribution = true; break; }
            gate.Conditions.Add(Condition(
                "local-distribution", "Local PLS distribution is used where it saves effort",
                localDistribution ? "ready" : "unknown", true,
                localDistribution ? "At least one non-stellar logistics station is observed." :
                    "A useful local PLS route wasn't found.",
                localDistribution ? "observed" : "unknown",
                "Use PLS selectively when local belts are genuinely burdensome."));

            bool purpleQueued = state.QueuedTechIds.Contains(1312);
            gate.Conditions.Add(Condition(
                "purple-direction",
                "The main research queue points to Information Matrix",
                purpleQueued ? "ready" : "blocked", true,
                purpleQueued
                    ? "Information Matrix is queued."
                    : "Information Matrix is not queued.",
                "observed",
                purpleQueued ? null : "Queue Information Matrix."));
        }

        private static void EvaluatePurple(GuideGateResult gate, ObservedGameState state)
        {
            AddPositiveFlow(gate, state, "purple-operating",
                "Purple Cubes (Information Matrices) are continuous", 6004, true);
            AddFlow(gate, state, "purple-rate",
                "Purple Cube (Information Matrix) output reaches 12/min",
                6004, 12, true);
            AddStableSupply(gate, state, "processors",
                "Processor production keeps pace", 1303,
                "Expand Processor production if Purple Cube production is starved.");
            AddStableSupply(gate, state, "particle-broadband",
                "Particle Broadband production is stable", 1402,
                "Expand Particle Broadband production if Purple Cube production is starved.");
            AddCombinedStableSupply(gate, state, "graphene-carbon-nanotubes",
                "Graphene and Carbon Nanotube production are stable",
                new int[] { 1123, 1124 },
                "Expand the Graphene or Carbon Nanotube supply that is starving Particle Broadband.");
            AddTechReadyOrQueued(gate, state, 1704,
                "Branch A is complete or actively progressing");
            AddTechReadyOrQueued(gate, state, 1303,
                "Branch B is complete or actively progressing");
            AddOlderMatrixChecklist(gate, state);
        }

        private static void EvaluateGreen(GuideGateResult gate, ObservedGameState state)
        {
            AddPositiveFlow(gate, state, "green-operating",
                "Green Cubes (Gravity Matrices) are continuous", 6005, true);
            AddFlow(gate, state, "green-rate",
                "Green Cube (Gravity Matrix) output reaches 10/min",
                6005, 10, true);
            AddStableSupply(gate, state, "quantum-chip-supply",
                "Quantum Chip production is stable", 1305,
                "Stabilize Quantum Chip production.");
            AddStableSupply(gate, state, "strange-matter-supply",
                "Strange Matter production is stable", 1127,
                "Stabilize Strange Matter production.");
            AddCombinedStableSupply(gate, state, "hydrogen-deuterium-route",
                "Hydrogen and Deuterium have deliberate supply routes",
                new int[] { 1120, 1121 },
                "Establish deliberate Hydrogen and Deuterium supply routes.");
            bool cheapWarpers =
                ConfiguredRecipeMachines(state, 79) > 0 ||
                ItemRate(state, 1210) > 0 ||
                Owned(state, 1210) > 0;
            gate.Conditions.Add(Condition(
                "cheap-warpers",
                "Cheap 8:1 Space Warpers are available",
                cheapWarpers ? "ready" : "blocked", true,
                cheapWarpers
                    ? "Space Warper production, stock, or the Green recipe was found."
                    : "The Green Space Warper route wasn't found.",
                state.RecipeTelemetryAvailable ? "observed" : "derived",
                cheapWarpers ? null : "Configure the 8:1 Space Warper recipe."));
            AddManualCondition(gate, "green-scaling",
                "Green Cube (Gravity Matrix) production is scaling toward endgame pace",
                "The desired endgame pace is a player-selected target.",
                "Keep scaling Green Cubes toward the chosen endgame pace.");
            bool dysonPreparation =
                state.Dyson.EjectorCount > 0 ||
                state.Dyson.SiloCount > 0 ||
                state.Dyson.ReceiverCount > 0 ||
                ItemRate(state, 1501) > 0 ||
                ItemRate(state, 1503) > 0;
            gate.Conditions.Add(Condition(
                "dyson-preparation",
                "Dyson and photon preparation is underway",
                dysonPreparation ? "ready" : "unknown", true,
                dysonPreparation
                    ? "Dyson production or deployed Dyson infrastructure was found."
                    : "Dyson preparation wasn't found in the current runtime evidence.",
                dysonPreparation ? "observed" : "unknown",
                dysonPreparation ? null : "Begin the chosen Dyson route and photon preparation."));
        }

        private static void EvaluateDyson(GuideGateResult gate, ObservedGameState state)
        {
            AddFlow(gate, state, "solar-sail-replacement",
                "Solar Sail production sustains the recalculated replacement rate",
                1501, 511, true);
            double sailLaunchRate = ItemConsumption(state, 1501);
            bool launchReady = state.ProductionWindowReady &&
                sailLaunchRate >= 511;
            gate.Conditions.Add(Condition(
                "solar-sail-launches",
                "Measured long-run Solar Sail launches sustain the replacement rate",
                launchReady ? "ready" : "blocked", true,
                state.ProductionWindowReady
                    ? "Found " + Math.Round(sailLaunchRate, 1) +
                        " Solar Sails consumed per minute; desired 511/min."
                    : "The production statistics window is not ready.",
                state.ProductionWindowReady ? "observed" : "unknown",
                launchReady ? null : "Sustain the recalculated Solar Sail launch rate."));
            AddManualCondition(gate, "ray-efficiency",
                "Ray Transmission Efficiency is known and matches the generation target",
                "The selected efficiency level and recalculated target are a player choice.",
                "Check the current Ray Transmission Efficiency against the guide table.");
            if (!state.Dyson.Available)
            {
                gate.Conditions.Add(Condition(
                    "dyson-generation", "Live Dyson swarm generation meets the target",
                    "unknown", true, "Dyson telemetry is unavailable.", "unknown",
                    "Establish and observe the Dyson swarm."));
            }
            else
            {
                double gigawatts = state.Dyson.GenerationWatts / 1000000000.0;
                bool ready = gigawatts >= 1.655;
                gate.Conditions.Add(Condition(
                    "dyson-generation", "Live Dyson swarm generation meets the target",
                    ready ? "ready" : "blocked", true,
                    "Observed generation " + Math.Round(gigawatts, 3) +
                        " GW; guide target 1.655 GW.",
                    "observed",
                    ready ? null : "Increase effective Dyson generation toward 1.655 GW."));
            }
            AddTech(gate, state, 1504, "Ray Receiver research is complete", true);
        }

        private static GuideGateResult EvaluateSphere(ObservedGameState state)
        {
            var gate = new GuideGateResult {
                Id = "sphere",
                Title = "Build permanent structure and shell cells",
                Basis = "Player-selected optional route evaluated against the published SPHERE readiness checklist."
            };

            double rocketProduction = ItemRate(state, 1503);
            double rocketConsumption = ItemConsumption(state, 1503);
            bool rocketStable = state.ProductionWindowReady &&
                rocketProduction >= 5;
            gate.Conditions.Add(Condition(
                "sphere-rockets",
                "Small Carrier Rocket production is stable at 5/min",
                rocketStable ? "ready" : "blocked", true,
                state.ProductionWindowReady
                    ? "Found " + Math.Round(rocketProduction, 1) +
                        " Small Carrier Rockets produced per minute; desired 5/min."
                    : "The production statistics window is not ready.",
                state.ProductionWindowReady ? "observed" : "unknown",
                rocketStable ? null : "Stabilize Small Carrier Rocket production at 5/min."));

            bool siloReady =
                state.Dyson.SiloCount > 0 &&
                state.Dyson.SilosWithTarget > 0 &&
                state.ProductionWindowReady &&
                rocketConsumption >= 5;
            gate.Conditions.Add(Condition(
                "sphere-silo",
                "One Vertical Launching Silo sustains 5 launches/min",
                siloReady ? "ready" : "blocked", true,
                "Found " + state.Dyson.SiloCount + " silo(s), " +
                    state.Dyson.SilosWithTarget + " with a target, and " +
                    Math.Round(rocketConsumption, 1) +
                    " rockets consumed per minute.",
                state.Dyson.Available ? "observed" : "unknown",
                siloReady ? null : "Supply and target a silo to sustain 5 launches/min."));

            bool shellDesignated =
                state.Dyson.DesignatedShellCount > 0 ||
                state.Dyson.TotalCellPoints > 0;
            bool shellReady = shellDesignated &&
                state.Dyson.ConstructedNodes > 0 &&
                state.Dyson.ConstructedStructurePoints > 0;
            gate.Conditions.Add(Condition(
                "sphere-cell-boundary",
                "Completed nodes and frames enclose a designated shell area",
                shellReady ? "ready" : "blocked",
                true,
                shellDesignated
                    ? "Found " + state.Dyson.DesignatedShellCount +
                        " designated shell area(s); native cell capacity " +
                        state.Dyson.TotalCellPoints + "."
                    : "A designated shell area wasn't found.",
                state.Dyson.ConstructionAggregateAvailable
                    ? "observed"
                    : (state.Dyson.TotalCellPoints > 0
                        ? "derived"
                        : "unknown"),
                shellReady
                    ? null
                    : (shellDesignated
                        ? "Finish the nodes and frames around a designated shell area."
                        : "Designate an enclosed shell area in the Dyson Sphere Editor.")));

            double sailLaunchRate = ItemConsumption(state, 1501);
            bool sailsReady = state.ProductionWindowReady &&
                sailLaunchRate >= 15;
            gate.Conditions.Add(Condition(
                "sphere-sail-launches",
                "At least 15 Solar Sails/min are launched for cell absorption",
                sailsReady ? "ready" : "blocked", true,
                state.ProductionWindowReady
                    ? "Found " + Math.Round(sailLaunchRate, 1) +
                        " Solar Sails consumed per minute; desired 15/min."
                    : "The production statistics window is not ready.",
                state.ProductionWindowReady ? "observed" : "unknown",
                sailsReady ? null : "Sustain at least 15 Solar Sail launches/min."));

            AddManualCondition(gate, "sphere-efficiency",
                "Ray Transmission Efficiency is known and matches the generation target",
                "The selected efficiency level and recalculated target are a player choice.",
                "Check the current Ray Transmission Efficiency against the guide table.");
            double gigawatts = state.Dyson.PermanentGenerationWatts /
                1000000000.0;
            bool generationReady = state.Dyson.Available &&
                gigawatts >= 1.655;
            gate.Conditions.Add(Condition(
                "sphere-generation",
                "Live permanent-sphere generation meets the target",
                generationReady ? "ready" : "blocked", true,
                state.Dyson.Available
                    ? "Found " + Math.Round(gigawatts, 3) +
                        " GW permanent generation; desired 1.655 GW."
                    : "Dyson construction telemetry is unavailable.",
                state.Dyson.Available ? "observed" : "unknown",
                generationReady ? null : "Expand permanent generation toward the recalculated target."));

            bool complete = true;
            foreach (GuideGateCondition condition in gate.Conditions)
                if (condition.Required && condition.Status != "ready")
                    complete = false;
            gate.Status = complete ? "complete" : "in-progress";
            return gate;
        }

        private static GuideGateResult EvaluateWarp(ObservedGameState state)
        {
            var warp = new GuideGateResult {
                Id = "warp",
                Title = "Take the interstellar shortcuts you want",
                Basis = "Player-selected optional reference route. WARP has no completion gate."
            };
            warp.Status = "reference";
            return warp;
        }

        private static void EvaluatePhoton(GuideGateResult gate, ObservedGameState state)
        {
            int configured =
                state.Dyson.ConfiguredPhotonReceiverCount;
            bool four = configured >= 4;
            gate.Conditions.Add(Condition(
                "photon-receiver-array",
                "Four Ray Receivers are configured for Photon Generation",
                four ? "ready" : "blocked",
                true,
                "Found " + configured +
                    " in Photon Generation mode; " +
                    state.Dyson.ReceiverCount + " deployed.",
                state.Dyson.ReceiverTelemetryAvailable
                    ? "observed"
                    : "unknown",
                four
                    ? null
                    : "Configure four Ray Receivers for Photon Generation."));

            bool continuity =
                configured >= 4 &&
                state.Dyson.SustainedPhotonReceiverCount == configured;
            string continuityEvidence;
            if (!state.Dyson.ReceiverTelemetryAvailable)
                continuityEvidence =
                    "The rolling receiver window is not available yet.";
            else if (continuity)
                continuityEvidence =
                    "All " + configured +
                    " Photon Generation receivers stayed lensed, fully exposed, full-strength, and continuously receiving for " +
                    Math.Round(
                        state.Dyson.ReceiverContinuityWindowSeconds,
                        0) + " seconds.";
            else
                continuityEvidence =
                    state.Dyson.SustainedPhotonReceiverCount + "/" +
                    configured +
                    " configured receivers sustained the full rolling window; currently lensed " +
                    state.Dyson.LensedPhotonReceiverCount + "/" +
                    configured + ", fully warmed " +
                    state.Dyson.ContinuousPhotonReceiverCount + "/" +
                    configured + ".";
            gate.Conditions.Add(Condition(
                "photon-receiver-continuity",
                "Four Ray Receivers remain continuously lensed",
                continuity ? "ready" : "watch",
                true,
                continuityEvidence,
                state.Dyson.ReceiverTelemetryAvailable
                    ? "observed"
                    : "unknown",
                continuity
                    ? null
                    : "Keep every Photon Generation receiver lensed and continuously receiving for at least 60 seconds."));

            AddFlow(gate, state, "critical-photons",
                "Critical Photon production reaches 48/min",
                1208, 48, true);
            AddFlow(gate, state, "antimatter",
                "One collider produces 48 Antimatter/min",
                1122, 48, true);
            AddTankSafety(gate, state, 1120,
                "Returned Hydrogen cannot block the collider", true);
            AddAllCubeCapability(gate, state,
                "All five Cube lines can each reach 40/min", 40);
        }

        private static void EvaluateWhite(GuideGateResult gate, ObservedGameState state)
        {
            if (state.UnlockedTechIds.Contains(1508))
            {
                gate.Conditions.Add(Condition(
                    "mission-completed",
                    "Mission Accomplished!",
                    "ready",
                    true,
                    "Mission Completed is researched.",
                    "observed",
                    null));
                return;
            }
            AddTech(gate, state, 1507, "Universe Matrix is researched", true);
            AddAllCubeCapability(gate, state,
                "All five Cube colors sustain the chosen White Cube rate", 40);
            AddFlow(gate, state, "white-antimatter",
                "Antimatter sustains the chosen White Cube rate", 1122, 40, true);
            AddPositiveFlow(gate, state, "white-operating",
                "White Cube (Universe Matrix) production is continuous", 6006, true);
            gate.Conditions.Add(Condition(
                "mission-completed",
                "Mission Completed is consuming or has consumed the final 4,000 White Cubes",
                "blocked",
                true,
                "Mission Completed is not yet researched.",
                "observed",
                "Research Mission Completed."));
        }

        private static void EvaluateLogistics(
            GuideGateResult gate,
            ObservedGameState state)
        {
            AddCombinedAvailability(gate, state, "distributor-bots",
                "Logistics Distributors and Logistics Bots refill automatically",
                new int[] { 2107, 5003 }, new long[] { 1, 5 },
                "Automate Logistics Distributors and Logistics Bots.");
            AddCombinedAvailability(gate, state, "pls-drones",
                "Planetary Logistics Stations and Logistics Drones refill automatically",
                new int[] { 2103, 5001 }, new long[] { 1, 5 },
                "Automate Planetary Logistics Stations and Logistics Drones.");
            AddCombinedAvailability(gate, state, "ils-vessels",
                "Interstellar Logistics Stations and Logistics Vessels refill automatically",
                new int[] { 2104, 5002 }, new long[] { 1, 2 },
                "Automate Interstellar Logistics Stations and Logistics Vessels.");
            AddManualCondition(gate, "personal-resupply",
                "Personal construction inventory is resupplied without visiting storage",
                "Personal logistics intent cannot be confirmed from aggregate production.",
                "Configure automatic personal construction resupply.");
            AddManualCondition(gate, "route-literacy",
                "Provider and receiver routes can be traced across Local and Remote settings",
                "Route understanding is a player check.",
                "Trace one provider and receiver route across Local and Remote settings.");
        }

        private static void AddOlderMatrixChecklist(
            GuideGateResult gate,
            ObservedGameState state)
        {
            bool ready = CubeCanSupport(state, 6001, 20) &&
                CubeCanSupport(state, 6002, 10) &&
                CubeCanSupport(state, 6003, 7.5);
            gate.Conditions.Add(Condition(
                "older-cubes",
                "Older Blue, Red, and Yellow Cube rates have been rechecked",
                ready ? "ready" : "blocked", true,
                ready
                    ? "Past Cube production or reserves can support current demand."
                    : "One or more past Cube lines are below demand without a useful reserve.",
                "derived",
                ready ? null : "Recheck the Blue, Red, and Yellow Cube lines."));
        }

        private static void AddAllCubeCapability(
            GuideGateResult gate,
            ObservedGameState state,
            string label,
            double desired)
        {
            bool ready = true;
            for (int itemId = 6001; itemId <= 6005; itemId++)
                if (!CubeCanSupport(state, itemId, desired))
                    ready = false;
            gate.Conditions.Add(Condition(
                "all-cube-capability", label,
                ready ? "ready" : "blocked", true,
                ready
                    ? "Each matrix line has current production or a useful reserve at the selected pace."
                    : "At least one matrix line cannot currently support the selected pace.",
                "derived",
                ready ? null : "Restore the weakest matrix line."));
        }

        private static bool CubeCanSupport(
            ObservedGameState state,
            int itemId,
            double desired)
        {
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(itemId, out flow);
            if (flow == null) return Owned(state, itemId) >= desired * 10;
            if (flow.ProducedPerMinute >= desired) return true;
            if (flow.ConsumedPerMinute <= 0.1 &&
                Owned(state, itemId) >= desired * 10)
                return true;
            return flow.ProducedPerMinute >= flow.ConsumedPerMinute &&
                Owned(state, itemId) >= desired * 5;
        }

        private static void AddCubeCapability(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int itemId,
            double desired)
        {
            bool ready = CubeCanSupport(state, itemId, desired);
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                ready
                    ? "Current production or reserve can support demand."
                    : "Production is below demand without a useful reserve.",
                "derived",
                ready ? null : "Restore this matrix line."));
        }

        private static void AddStableSupply(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int itemId,
            string action)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(
                    id, label, "unknown", true,
                    "The production statistics window is not ready.",
                    "unknown", "Let the factory run long enough to check this supply."));
                return;
            }
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(itemId, out flow);
            double produced = flow != null ? flow.ProducedPerMinute : 0;
            double consumed = flow != null ? flow.ConsumedPerMinute : 0;
            bool ready = produced > 0 ||
                Owned(state, itemId) > Math.Max(100, consumed * 5);
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                ready
                    ? "Found active production or a useful reserve."
                    : "Active production or a useful reserve wasn't found.",
                "derived", ready ? null : action));
        }

        private static void AddCombinedStableSupply(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int[] itemIds,
            string action)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(
                    id, label, "unknown", true,
                    "The production statistics window is not ready.",
                    "unknown", "Let the factory run long enough to check this supply."));
                return;
            }
            bool ready = true;
            foreach (int itemId in itemIds)
            {
                ObservedItemFlow flow;
                state.ItemFlows.TryGetValue(itemId, out flow);
                double produced = flow != null ? flow.ProducedPerMinute : 0;
                double consumed = flow != null ? flow.ConsumedPerMinute : 0;
                if (produced <= 0 &&
                    Owned(state, itemId) <= Math.Max(100, consumed * 5))
                    ready = false;
            }
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", true,
                ready
                    ? "Found active production or useful reserves for both supplies."
                    : "Active production or a useful reserve wasn't found for both supplies.",
                "derived", ready ? null : action));
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

        private static void AddPositiveFlow(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int itemId,
            bool required)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(id, label, "unknown", required,
                    "Production observation window is not ready.", "unknown",
                    "Let the factory run long enough to establish production."));
                return;
            }
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(itemId, out flow);
            double rate = flow != null ? flow.ProducedPerMinute : 0.0;
            bool ready = rate > 0.0 || Owned(state, itemId) > 0;
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", required,
                "Observed production " + Math.Round(rate, 1) +
                    "/min; owned " + Owned(state, itemId) + ".",
                "observed",
                ready ? null : "Establish " + ItemNameForAction(itemId, label) + "."));
        }

        private static void AddAvailable(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int itemId,
            long comfortableStock,
            bool required)
        {
            long stock = Owned(state, itemId);
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(itemId, out flow);
            bool ready = stock >= comfortableStock || (flow != null && flow.ProducedPerMinute > 0);
            gate.Conditions.Add(Condition(
                id, label, ready ? "ready" : "blocked", required,
                "Owned: " + stock + "; observed production: " +
                    Math.Round(flow != null ? flow.ProducedPerMinute : 0.0, 1) + "/min.",
                "observed", ready ? null : "Automate or stock this basic component."));
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

        private static void AddTechReadyOrQueued(
            GuideGateResult gate,
            ObservedGameState state,
            int techId,
            string label)
        {
            bool complete = state.UnlockedTechIds.Contains(techId);
            bool queued = state.QueuedTechIds.Contains(techId);
            bool ready = complete || queued;
            gate.Conditions.Add(Condition(
                "tech-or-queue-" + techId,
                label,
                ready ? "ready" : "blocked",
                true,
                complete
                    ? TechName(state, techId) + " is complete."
                    : (queued
                        ? TechName(state, techId) + " is queued."
                        : TechName(state, techId) + " is neither complete nor queued."),
                "observed",
                ready ? null : "Research or queue " + TechName(state, techId) + "."));
        }

        private static void AddTechOrQueue(
            GuideGateResult gate,
            ObservedGameState state,
            int[] techIds,
            string label,
            bool required)
        {
            var missing = new List<string>();
            foreach (int id in techIds)
                if (!state.UnlockedTechIds.Contains(id) && !state.QueuedTechIds.Contains(id))
                    missing.Add(TechName(state, id));
            gate.Conditions.Add(Condition(
                "research-lineup", label, missing.Count == 0 ? "ready" : "blocked", required,
                missing.Count == 0 ? "All listed technologies are complete or queued." :
                    "Missing from completion and queue: " + String.Join(", ", missing.ToArray()) + ".",
                "derived", missing.Count == 0 ? null : "Queue the missing pre-ILS research."));
        }

        private static void AddTankSafety(
            GuideGateResult gate,
            ObservedGameState state,
            int itemId,
            string label,
            bool required)
        {
            ObservedCapacity capacity;
            if (!state.TankStorage.TryGetValue(itemId, out capacity) || capacity.Capacity <= 0)
            {
                gate.Conditions.Add(Condition(
                    "tank-" + itemId, label, "unknown", required,
                    "No tank capacity for this item is observed.", "unknown",
                    "Make sure this output cannot bottleneck production."));
                return;
            }
            double fill = capacity.Count * 1.0 / capacity.Capacity;
            string status = fill >= 0.98 ? "blocked" : (fill >= 0.80 ? "watch" : "ready");
            gate.Conditions.Add(Condition(
                "tank-" + itemId, label, status, required,
                "Observed tank fill: " + Math.Round(fill * 100.0, 1) + "%.",
                "observed", status == "blocked" ? "Create consumption or capacity before the refinery stops." : null));
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
