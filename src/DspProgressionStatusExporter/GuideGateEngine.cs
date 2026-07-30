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
                { "contractVersion", "2.3" },
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
            new GateDefinition { Id = "blue", Title = "Keep Blue matrices running" },
            new GateDefinition { Id = "red", Title = "Build a stable oil economy" },
            new GateDefinition { Id = "flight", Title = "Prepare and establish planetary escape" },
            new GateDefinition { Id = "titanium", Title = "Establish useful off-world Titanium" },
            new GateDefinition { Id = "yellow", Title = "Complete the finite ILS research batch" },
            new GateDefinition { Id = "ils", Title = "End routine manual interplanetary hauling" },
            new GateDefinition { Id = "purple", Title = "Build the first truly wide production tier" },
            new GateDefinition { Id = "green", Title = "Build the Green tier and prepare Dyson industry" },
            new GateDefinition { Id = "dyson", Title = "Establish sufficient Dyson swarm generation" },
            new GateDefinition { Id = "photon", Title = "Run the critical-photon receiver array" },
            new GateDefinition { Id = "white", Title = "Sustain Universe Matrix production" }
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
            AddFlow(gate, state, "iron-ingot", "Iron smelting is automated", 1101, 30, true);
            AddFlow(gate, state, "copper-ingot", "Copper smelting is automated", 1104, 20, true);
            AddAvailable(gate, state, "magnetic-coil", "Magnetic Coils are readily available", 1202, 30, true);
            AddAvailable(gate, state, "circuit-board", "Circuit Boards are readily available", 1301, 30, true);
            AddAvailable(gate, state, "belts", "Belts are readily available", 2001, 100, true);
            AddAvailable(gate, state, "sorters", "Sorters are readily available", 2011, 50, true);
            AddPower(gate, state, true);
        }

        private static void EvaluateBlue(GuideGateResult gate, ObservedGameState state)
        {
            AddFlow(gate, state, "blue-rate", "Blue matrices meet the guide minimum continuously", 6001, 20, true);
            AddPower(gate, state, true);
        }

        private static void EvaluateRed(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 1111, "Energy Matrix is researched", true);
            AddFlow(gate, state, "red-rate", "Red matrices meet the guide minimum", 6002, 10, true);
            AddTankSafety(gate, state, 1114, "Refined Oil has safe remaining capacity", false);
            AddTankSafety(gate, state, 1120, "Hydrogen has safe remaining capacity", false);
        }

        private static void EvaluateFlight(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 2902, "Drive Engine Lv2 is researched", true);
            AddTech(gate, state, 1413, "Titanium Smelting is researched", true);
            AddTechOrQueue(gate, state, new int[] { 1604, 1703, 1302, 1114 },
                "PLS, Particle Trap, Processor, and Reinforced Thruster are complete or queued", true);
            gate.Conditions.Add(Condition(
                "flight-margin", "Fuel and destination power margin are comfortable",
                "unknown", false,
                "The current normalized model does not yet prove the player's intended travel margin.",
                "unknown", "Verify fuel, buildings, and destination power in the guide checklist."));
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
                remoteSilicon > 0 ? "ready" : "unknown", false,
                remoteSilicon > 0 ? "Observed remote Silicon Ore production." :
                    "A future plan cannot be proven from runtime state.",
                remoteSilicon > 0 ? "observed" : "unknown",
                "Plan a direct Silicon source before Processor demand grows."));
        }

        private static void EvaluateYellow(GuideGateResult gate, ObservedGameState state)
        {
            AddFlow(gate, state, "yellow-rate", "Yellow matrices meet the guide minimum continuously", 6003, 7.5, true);
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
            AddTech(gate, state, 1605, "Interstellar Logistics System is complete", true);
            bool stationReady = HasStellarStation(state) || Owned(state, 2104) >= 2;
            gate.Conditions.Add(Condition(
                "ils-hardware", "The first ILS station pair is built or available",
                stationReady ? "ready" : "unknown", false,
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
                    "No traffic or matching supplied/demanded Silicon route was proven.",
                siliconRoute ? "derived" : "observed",
                siliconRoute ? null : "Activate a sustainable Silicon route."));

            AddPower(gate, state, true);

            bool localDistribution = false;
            foreach (ObservedStationSlot slot in state.StationSlots)
                if (!slot.IsStellar) { localDistribution = true; break; }
            gate.Conditions.Add(Condition(
                "local-distribution", "Local PLS distribution is used where it saves effort",
                localDistribution ? "ready" : "unknown", false,
                localDistribution ? "At least one non-stellar logistics station is observed." :
                    "This is optional and usefulness cannot be inferred from absence alone.",
                localDistribution ? "observed" : "unknown",
                "Use PLS selectively when local belts are genuinely burdensome."));

            bool processor = state.UnlockedTechIds.Contains(1302);
            bool particle = state.UnlockedTechIds.Contains(1133);
            bool processorQueued = state.QueuedTechIds.Contains(1302);
            bool particleQueued = state.QueuedTechIds.Contains(1133);
            bool purpleQueued = state.QueuedTechIds.Contains(1312);
            bool aligned = (processor || processorQueued) && (particle || particleQueued) && purpleQueued;
            string action = !(processor || processorQueued) ? "Queue Processor." :
                (!(particle || particleQueued) ? "Queue Particle Control." : "Queue Information Matrix.");
            gate.Conditions.Add(Condition(
                "purple-direction", "The research queue is explicitly pointed toward Purple",
                aligned ? "ready" : "blocked", true,
                "Processor ready/queued: " + (processor || processorQueued) +
                    "; Particle Control ready/queued: " + (particle || particleQueued) +
                    "; Information Matrix queued: " + purpleQueued + ".",
                "derived", action));
        }

        private static void EvaluatePurple(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 1312, "Information Matrix is researched", true);
            AddPositiveFlow(gate, state, "purple-operating",
                "Purple production is established", 6004, true);
            AddFlow(gate, state, "processors", "Processors support the Purple target", 1303, 24, false);
            AddFlow(gate, state, "carbon-nanotubes", "Carbon Nanotubes support Particle Broadband", 1124, 24, false);
            AddFlow(gate, state, "particle-broadband", "Particle Broadband supports the Purple target", 1402, 12, false);
            AddFlow(gate, state, "purple-rate", "Purple matrices meet the guide minimum", 6004, 12, false);
            AddFlow(gate, state, "graphene-support", "The standard Graphene support route reaches 40/min", 1123, 40, false);
            AddOlderMatrixSupport(gate, state);
            AddTech(gate, state, 1704, "Gravitational Wave Refraction branch is complete", true);
            AddTech(gate, state, 1303, "Quantum Chip branch is complete", true);
            AddPower(gate, state, false);
        }

        private static void EvaluateGreen(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 1705, "Gravity Matrix is researched", true);
            AddPositiveFlow(gate, state, "green-operating",
                "Green production is established", 6005, true);
            AddFlow(gate, state, "deuterium-supply", "Deuterium supply supports the starter Green block", 1121, 50, false);
            AddFlow(gate, state, "quantum-chip-rate", "Quantum Chips support the Green target", 1305, 5, false);
            AddFlow(gate, state, "strange-matter-rate", "Strange Matter supports the Green target", 1127, 5, false);
            AddFlow(gate, state, "graviton-lens-rate", "Graviton Lenses support the Green target", 1209, 5, false);
            AddFlow(gate, state, "green-rate", "Green matrices meet the guide starter target", 6005, 10, false);
            AddTech(gate, state, 1505, "Planetary Ionosphere Utilization is researched", true);
            AddPower(gate, state, false);
        }

        private static void EvaluateDyson(GuideGateResult gate, ObservedGameState state)
        {
            AddTech(gate, state, 1505, "Planetary Ionosphere Utilization is researched", true);
            if (!state.Dyson.Available)
            {
                gate.Conditions.Add(Condition(
                    "dyson-generation", "Dyson generation reaches the guide target",
                    "unknown", true, "Dyson telemetry is unavailable.", "unknown",
                    "Establish and observe the Dyson swarm."));
            }
            else
            {
                double gigawatts = state.Dyson.GenerationWatts / 1000000000.0;
                bool ready = gigawatts >= 1.655;
                gate.Conditions.Add(Condition(
                    "dyson-generation", "Dyson generation reaches the guide target",
                    ready ? "ready" : "blocked", true,
                    "Observed generation " + Math.Round(gigawatts, 3) +
                        " GW; guide target 1.655 GW.",
                    "observed",
                    ready ? null : "Increase effective Dyson generation toward 1.655 GW."));
            }
            AddFlow(gate, state, "solar-sail-rate",
                "Solar Sail production supports the reference swarm build", 1501, 511, false);
            gate.Conditions.Add(Condition(
                "ejector-duty", "Ejector deployment and current firing opportunity are visible",
                state.Dyson.EjectorCount > 0 ? "ready" : "unknown", false,
                "Ejectors deployed: " + state.Dyson.EjectorCount +
                    "; currently on target: " + state.Dyson.EjectorsOnTarget +
                    "; net swarm population: " +
                    Math.Round(state.Dyson.NetSwarmSailsPerMinute, 1) + "/min.",
                state.Dyson.Available ? "observed" : "unknown", null));
        }

        private static GuideGateResult EvaluateSphere(ObservedGameState state)
        {
            var gate = new GuideGateResult {
                Id = "sphere",
                Title = "Build permanent structure and shell cells",
                Basis = "Player-selected optional route. Objectives establish the construction chain; reference rates never control navigation."
            };

            double rocketProduction = ItemRate(state, 1503);
            double rocketConsumption = ItemConsumption(state, 1503);
            long rocketStock = Owned(state, 1503);
            bool rocketHistory =
                state.Dyson.ConstructedStructurePoints > 0 ||
                state.Dyson.ConstructedNodes > 0;
            bool rocketsAvailable =
                rocketProduction > 0.0 ||
                rocketConsumption > 0.0 ||
                rocketStock > 0 ||
                state.Dyson.RocketsInFlight > 0 ||
                rocketHistory;
            gate.Conditions.Add(Condition(
                "sphere-rockets-available",
                "Small Carrier Rockets are available to the construction project",
                rocketsAvailable ? "ready" : "blocked",
                true,
                rocketHistory &&
                    rocketProduction <= 0.0 &&
                    rocketStock <= 0
                    ? "Permanent structure proves rockets have already reached the project."
                    : "Found " + rocketStock + " rockets; production " +
                        Math.Round(rocketProduction, 1) +
                        "/min. 5/min is a reference pace.",
                "observed",
                rocketsAvailable ? null : "Produce Small Carrier Rockets."));

            bool siloEstablished =
                state.Dyson.SiloCount > 0 &&
                (state.Dyson.SilosWithTarget > 0 ||
                 state.Dyson.SilosFiringNow > 0 ||
                 rocketConsumption > 0.0 ||
                 state.Dyson.RocketsInFlight > 0 ||
                 rocketHistory);
            string siloAction = null;
            if (!siloEstablished)
            {
                if (state.Dyson.SiloCount <= 0)
                    siloAction =
                        "Build and supply a Vertical Launching Silo.";
                else if (state.Dyson.SilosWithTarget <= 0 &&
                    !rocketHistory)
                    siloAction =
                        "Assign the silo to a planned sphere node.";
                else if (state.Dyson.SilosSupplied <= 0 &&
                    rocketConsumption <= 0.0)
                    siloAction =
                        "Supply the silo with Small Carrier Rockets.";
                else
                    siloAction =
                        "Let the silo begin building the planned sphere.";
            }
            gate.Conditions.Add(Condition(
                "sphere-silo-established",
                "A Vertical Launching Silo is building the planned sphere",
                siloEstablished ? "ready" : "blocked",
                true,
                "Found " + state.Dyson.SiloCount +
                    " silo(s); " + state.Dyson.SilosSupplied +
                    " supplied; " + state.Dyson.SilosWithTarget +
                    " with a target.",
                "observed",
                siloAction));

            bool shellDesignated =
                state.Dyson.DesignatedShellCount > 0 ||
                state.Dyson.TotalCellPoints > 0;
            bool shellReady =
                state.Dyson.ConstructedCellPoints > 0;
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

            double sailProduction = ItemRate(state, 1501);
            long sailStock = Owned(state, 1501);
            bool sailsAvailable =
                sailProduction > 0.0 ||
                sailStock > 0 ||
                state.Dyson.SwarmSailCount > 0 ||
                state.Dyson.EjectorsSupplied > 0;
            bool cellConstructionEstablished =
                state.Dyson.ConstructedCellPoints > 0 ||
                state.Dyson.ConstructedCellPointsPerMinute > 0.0;
            // Existing permanent cell points prove that sails reached this
            // project successfully. Do not regress an established objective
            // merely because a mature or temporarily paused project has no
            // sails moving during the current observation window.
            bool cellsEstablished = cellConstructionEstablished;
            string cellAction = null;
            if (!cellsEstablished)
            {
                if (!shellReady)
                    cellAction =
                        "Finish a shell boundary before launching sails for cells.";
                else if (!sailsAvailable)
                    cellAction =
                        "Produce and launch Solar Sails for shell cells.";
                else if (state.Dyson.EjectorCount <= 0)
                    cellAction =
                        "Build and supply an EM-Rail Ejector for shell cells.";
                else
                    cellAction =
                        "Check that launched Solar Sails are becoming permanent shell cells.";
            }
            gate.Conditions.Add(Condition(
                "sphere-cell-construction",
                "Solar Sails are available and shell-cell construction is established",
                cellsEstablished ? "ready" : "blocked",
                true,
                "Found " + state.Dyson.ConstructedCellPoints +
                    " permanent cell points; current growth " +
                    Math.Round(
                        state.Dyson.ConstructedCellPointsPerMinute,
                        1) +
                    "/min. 15 sails/min is a reference pace.",
                state.Dyson.ConstructionRateAvailable
                    ? "observed"
                    : (state.Dyson.ConstructedCellPoints > 0
                        ? "observed"
                        : "unknown"),
                cellAction));

            gate.Status =
                rocketsAvailable &&
                siloEstablished &&
                shellReady &&
                cellsEstablished
                    ? "established"
                    : "in-progress";
            return gate;
        }

        private static GuideGateResult EvaluateWarp(ObservedGameState state)
        {
            int expensiveMachines = ConfiguredRecipeMachines(state, 78);
            int cheapMachines = ConfiguredRecipeMachines(state, 79);
            double warperRate = ItemRate(state, 1210);
            long warperStock = Owned(state, 1210);
            var warp = new GuideGateResult {
                Id = "warp",
                Title = "Optional pre-Green interstellar scouting",
                Basis = "Player-selected optional detour; it never blocks the main guide sequence."
            };
            bool personalWarp = state.UnlockedTechIds.Contains(2904);
            bool personalWarpQueued = state.QueuedTechIds.Contains(2904);
            warp.Conditions.Add(Condition(
                "personal-warp", "Personal warp research is available",
                personalWarp ? "ready" :
                    (personalWarpQueued ? "watch" : "unknown"),
                false,
                "Drive Engine Lv4 complete: " + personalWarp +
                    "; queued: " + personalWarpQueued + ".",
                "observed", null));
            warp.Conditions.Add(Condition(
                "expensive-warper-route",
                "The expensive pre-Green Warper route is deliberately configured",
                expensiveMachines > 0 ? "ready" :
                    (cheapMachines > 0 || warperRate > 0 || warperStock > 0
                        ? "watch"
                        : "unknown"),
                false,
                "Recipe 78 machines: " + expensiveMachines +
                    "; observed Space Warper output: " +
                    Math.Round(warperRate, 1) + "/min; owned: " +
                    warperStock + ".",
                state.RecipeTelemetryAvailable ? "observed" : "unknown",
                null));
            warp.Conditions.Add(Condition(
                "named-target",
                "A named rare-resource target solves a known factory problem",
                "unknown", false,
                "Player intent and the value of an undiscovered target cannot be proven from runtime state.",
                "unknown", null));
            warp.Status = personalWarp && (expensiveMachines > 0 ||
                cheapMachines > 0 || warperRate > 0 || warperStock > 0)
                ? "active-observed"
                : "available";
            return warp;
        }

        private static void EvaluatePhoton(GuideGateResult gate, ObservedGameState state)
        {
            bool receiverResearch =
                state.UnlockedTechIds.Contains(1505) &&
                state.UnlockedTechIds.Contains(1506);
            gate.Conditions.Add(Condition(
                "photon-research",
                "Photon Generation and Graviton Lens receiver research are complete",
                receiverResearch ? "ready" : "blocked",
                true,
                receiverResearch
                    ? "Dirac Inversion Mechanism and Planetary Ionosphere Utilization are complete."
                    : "Required research is still incomplete.",
                "observed",
                receiverResearch
                    ? null
                    : "Research Dirac Inversion Mechanism and Planetary Ionosphere Utilization."));

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
                "The Photon Generation receiver array remains continuously supplied",
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
                "Critical Photon production approaches 48/min",
                1208, 48, true);
            AddFlow(gate, state, "antimatter",
                "Antimatter production approaches 48/min",
                1122, 48, true);
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
            AddPositiveFlow(gate, state, "white-operating",
                "Universe Matrix production is operating", 6006, true);
            AddFlow(gate, state, "white-comfort",
                "Universe Matrix production reaches the comfortable guide rate", 6006, 40, false);
            gate.Conditions.Add(Condition(
                "mission-completed",
                "Mission Completed is researched",
                "blocked",
                true,
                "Mission Completed is not yet researched.",
                "observed",
                "Research Mission Completed."));
        }

        private static void AddOlderMatrixSupport(GuideGateResult gate, ObservedGameState state)
        {
            AddSupportFlow(gate, state, "older-blue", "Blue matrix support has not regressed", 6001, 20);
            AddSupportFlow(gate, state, "older-red", "Red matrix support has not regressed", 6002, 10);
            AddSupportFlow(gate, state, "older-yellow", "Yellow matrix support has not regressed", 6003, 7.5);
        }

        private static void AddSupportFlow(
            GuideGateResult gate,
            ObservedGameState state,
            string id,
            string label,
            int itemId,
            double minimum)
        {
            if (!state.ProductionWindowReady)
            {
                gate.Conditions.Add(Condition(id, label, "unknown", false,
                    "Production observation window is not ready.", "unknown", null));
                return;
            }
            ObservedItemFlow flow;
            state.ItemFlows.TryGetValue(itemId, out flow);
            double rate = flow != null ? flow.ProducedPerMinute : 0.0;
            double active = flow != null ? flow.ProductionActiveFraction : 0.0;
            string status = rate >= minimum ? "ready" :
                (rate > 0 ? "watch" : "unknown");
            gate.Conditions.Add(Condition(
                id, label, status, false,
                "Observed " + Math.Round(rate, 1) + "/min; active in " +
                    Math.Round(active * 100.0, 0) + "% of intervals; reference minimum " +
                    minimum + "/min.",
                rate > 0 ? "observed" : "unknown", null));
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
            double active = flow != null ? flow.ProductionActiveFraction : 0.0;
            bool ready = rate >= minimum;
            string status = ready ? "ready" : "blocked";
            string itemName = flow != null && !String.IsNullOrEmpty(flow.Name)
                ? flow.Name : ItemNameForAction(itemId, label);
            gate.Conditions.Add(Condition(
                id, label, status, required,
                "Observed " + Math.Round(rate, 1) + "/min; active in " +
                    Math.Round(active * 100.0, 0) +
                    "% of sampled intervals (cadence context only); minimum " +
                    minimum + "/min.",
                "observed", ready ? null :
                    "Build or stabilize " + itemName + " at or above " + minimum + "/min."));
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
                    "Verify the refinery output cannot deadlock."));
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
