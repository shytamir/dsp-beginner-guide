using System;
using System.Collections;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class ObservedItemFlow
    {
        public int ItemId;
        public string Name;
        public double ProducedPerMinute;
        public double ConsumedPerMinute;
        public double NetPerMinute;
        public int ObservedIntervals;
        public double ProductionActiveFraction;
        public string ProductionContinuity;
        public bool OneMinuteAvailable;
        public string OneMinuteStatus;
        public bool TenMinuteAvailable;
        public bool TenMinuteReady;
        public string TenMinuteStatus;
        public double TenMinuteObservedGameSeconds;
        public double TenMinuteProducedPerMinute;
        public double TenMinuteConsumedPerMinute;
        public double TenMinuteNetPerMinute;
    }

    internal sealed class ObservedLifetimeItemTotals
    {
        public int ItemId;
        public string Name;
        public long Produced;
        public long Consumed;
    }

    internal sealed class ObservedTrafficFlow
    {
        public int FactoryIndex;
        public int PlanetId;
        public string PlanetName;
        public int ItemId;
        public string Name;
        public double InputPerMinute;
        public double OutputPerMinute;
        public double InternalPerMinute;
    }

    internal sealed class ObservedFactoryItemFlow
    {
        public int FactoryIndex;
        public int PlanetId;
        public string PlanetName;
        public int ItemId;
        public string Name;
        public double ProducedPerMinute;
        public double ConsumedPerMinute;
        public double ProductionActiveFraction;
        public string ProductionContinuity;
    }

    internal sealed class ObservedPowerState
    {
        public int FactoryIndex;
        public int PlanetId;
        public string PlanetName;
        public int Observations;
        public double AverageSatisfaction;
        public double MinimumSatisfaction;
        public double UndersuppliedFraction;
        public double MaximumDemandToCapacity;
    }

    internal sealed class ObservedStationSlot
    {
        public int PlanetId;
        public string PlanetName;
        public int StationId;
        public bool IsStellar;
        public int ItemId;
        public string Name;
        public long Count;
        public long Maximum;
        public string LocalLogic;
        public string RemoteLogic;
    }

    internal sealed class ObservedStationState
    {
        public int PlanetId;
        public string PlanetName;
        public int StationId;
        public bool IsStellar;
        public int IdleShipCount;
        public int WorkShipCount;
    }

    internal sealed class ObservedCapacity
    {
        public long Count;
        public long Capacity;
    }

    internal sealed class ObservedBufferSourceEvidence
    {
        public int PlanetId;
        public string PlanetName;
        public int StationId;
        public string SourceType;
        public int ItemId;
        public string Name;
        public long Count;
        public long Capacity;
        public string LocalLogic;
        public string RemoteLogic;
        public string ExclusionReason;
    }

    internal sealed class ObservedBufferScopeEvidence
    {
        public int PlanetId;
        public string PlanetName;
        public int ItemId;
        public string Name;
        public long AccessibleCount;
        public long AccessibleCapacity;
        public bool DemandEvidenceAvailable;
        public double DemandPerMinute;
        public bool RunwayAvailable;
        public double RunwayMinutes;
        public string BackpressureStatus;
        public readonly List<ObservedBufferSourceEvidence> Contributors =
            new List<ObservedBufferSourceEvidence>();
    }

    internal sealed class ObservedItemBufferEvidence
    {
        public int ItemId;
        public string Name;
        public long AccessibleCount;
        public long AccessibleCapacity;
        public string BackpressureStatus;
        public readonly List<ObservedBufferScopeEvidence> Scopes =
            new List<ObservedBufferScopeEvidence>();
        public readonly List<ObservedBufferSourceEvidence> ExcludedSources =
            new List<ObservedBufferSourceEvidence>();
    }

    internal sealed class ObservedRecipeConfiguration
    {
        public int FactoryIndex;
        public int PlanetId;
        public string PlanetName;
        public int RecipeId;
        public string Name;
        public int ConfiguredMachineCount;
    }

    internal sealed class ObservedDysonState
    {
        public bool Available;
        public double GenerationWatts;
        public double PermanentGenerationWatts;
        public double SwarmGenerationWatts;
        public double RequestedWatts;
        public double SolarSailLifeRaw;
        public long SwarmSailCount;
        public double NetSwarmSailsPerMinute;
        public int EjectorCount;
        public int EjectorsOnTarget;
        public int EjectorsSupplied;
        public int EjectorsFiringNow;
        public int SiloCount;
        public int SilosSupplied;
        public int SilosWithTarget;
        public int SilosFiringNow;
        public int ReceiverCount;
        public int LensedReceiverCount;
        public int FullStrengthReceiverCount;
        public double MinimumReceiverWarmup;
        public bool ReceiverTelemetryAvailable;
        public double ReceiverContinuityWindowSeconds;
        public int ConfiguredPhotonReceiverCount;
        public int LensedPhotonReceiverCount;
        public int FullStrengthPhotonReceiverCount;
        public int ContinuousPhotonReceiverCount;
        public int SustainedPhotonReceiverCount;
        public double ReceiverArrayRequestedDysonPowerWatts;
        public double ReceiverArraySuppliedPowerWatts;
        public double ReceiverArrayCriticalPhotonOutputPerMinute;
        public readonly List<ObservedReceiverState> Receivers =
            new List<ObservedReceiverState>();
        public long ConstructedNodes;
        public long TotalNodes;
        public long ConstructedStructurePoints;
        public long TotalStructurePoints;
        public long ConstructedCellPoints;
        public long TotalCellPoints;
        public long DesignatedShellCount;
        public long RocketsInFlight;
        public bool ConstructionRateAvailable;
        public double ConstructedStructurePointsPerMinute;
        public double ConstructedCellPointsPerMinute;
        public double PermanentGenerationWattsChangePerMinute;
        public string AggregateSource;
        public int AggregateSystemCount;
        public int AggregateNodesRead;
        public int AggregateNodesMissing;
        public bool ConstructionAggregateAvailable;
        public bool SphereRouteObserved;
    }

    internal sealed class ObservedReceiverState
    {
        public int PlanetId;
        public string PlanetName;
        public int EntityId;
        public int SampleCount;
        public double WindowSeconds;
        public bool WindowReady;
        public bool ConfiguredForPhotonGeneration;
        public bool LensedNow;
        public bool LensSustained;
        public bool ContinuousReceivingNow;
        public double WarmupNow;
        public double MinimumWarmup;
        public bool FullStrengthNow;
        public double StrengthNow;
        public double MinimumStrength;
        public bool SustainedHealthy;
        public double RequestedDysonPowerWatts;
        public double SuppliedPowerWatts;
        public double CriticalPhotonOutputPerMinute;
    }

    /// <summary>
    /// Stable analysis input. Runtime collectors may remain reflection-driven,
    /// but guide rules consume this model rather than the forensic JSON shape.
    /// </summary>
    internal sealed class ObservedGameState
    {
        public readonly HashSet<int> UnlockedTechIds = new HashSet<int>();
        public readonly HashSet<int> QueuedTechIds = new HashSet<int>();
        public readonly Dictionary<int, string> TechNames = new Dictionary<int, string>();
        public readonly Dictionary<int, long> OwnedItemCounts = new Dictionary<int, long>();
        public readonly Dictionary<int, long> PlayerItemCounts = new Dictionary<int, long>();
        public readonly Dictionary<int, Dictionary<int, long>> PlanetItemCounts =
            new Dictionary<int, Dictionary<int, long>>();
        public readonly Dictionary<int, string> PlanetNames = new Dictionary<int, string>();
        public readonly Dictionary<int, long> FactoryBuildingCounts = new Dictionary<int, long>();
        public readonly Dictionary<int, ObservedItemFlow> ItemFlows = new Dictionary<int, ObservedItemFlow>();
        public readonly Dictionary<int, ObservedLifetimeItemTotals> LifetimeItemTotals =
            new Dictionary<int, ObservedLifetimeItemTotals>();
        public readonly List<ObservedFactoryItemFlow> FactoryItemFlows = new List<ObservedFactoryItemFlow>();
        public readonly List<ObservedTrafficFlow> TrafficFlows = new List<ObservedTrafficFlow>();
        public readonly List<ObservedPowerState> PowerPlanets = new List<ObservedPowerState>();
        public readonly List<ObservedStationSlot> StationSlots = new List<ObservedStationSlot>();
        public readonly List<ObservedStationState> Stations = new List<ObservedStationState>();
        public readonly Dictionary<int, ObservedCapacity> TankStorage = new Dictionary<int, ObservedCapacity>();
        public readonly Dictionary<int, ObservedItemBufferEvidence> ItemBuffers =
            new Dictionary<int, ObservedItemBufferEvidence>();
        public readonly List<ObservedRecipeConfiguration> RecipeConfigurations =
            new List<ObservedRecipeConfiguration>();
        public readonly ObservedDysonState Dyson = new ObservedDysonState();

        public bool ProductionWindowReady;
        public double ProductionWindowSeconds;
        public string ProductionSource;
        public string ProductionScope;
        public string ProductionPeriod;
        public string ProductionFailure;
        public int ProductionSampleCount;
        public int ProductionWatchedItemCount;
        public int ProductionItemCoverage;
        public int ProductionFactoryCount;
        public bool ProductionTenMinuteWindowAvailable;
        public bool ProductionTenMinuteWindowReady;
        public string ProductionTenMinuteWindowStatus;
        public double ProductionTenMinuteWindowSeconds;
        public string ProductionTenMinuteReadinessSource;
        public int ProductionTenMinuteAvailableItemCount;
        public int ProductionTenMinuteReadyItemCount;
        public bool TrafficWindowReady;
        public double TrafficWindowSeconds;
        public double PowerWindowSeconds;
        public bool RecipeTelemetryAvailable;
        public int PlayerPlanetId;

        public static ObservedGameState Build(
            Dictionary<string, object> legacySnapshot,
            Dictionary<string, object> production,
            Dictionary<string, object> traffic,
            Dictionary<string, object> power,
            Dictionary<string, object> recipes)
        {
            var state = new ObservedGameState();
            state.ReadLocation(GetDictionary(legacySnapshot, "location"));
            state.ReadResearch(GetDictionary(legacySnapshot, "research"));
            state.ReadOwnedItems(GetDictionary(legacySnapshot, "ownedInventorySummary"));
            state.ReadBuildingCounts(GetDictionary(legacySnapshot, "progressionSummary"));
            state.ReadTanks(GetValue(legacySnapshot, "factories"));
            state.ReadStations(GetValue(legacySnapshot, "factories"));
            state.ReadProduction(production);
            state.BuildBufferEvidence();
            state.ReadTraffic(traffic);
            state.ReadPower(power);
            state.ReadRecipes(recipes);
            state.ReadDyson(GetDictionary(legacySnapshot, "dyson"));
            return state;
        }

        public Dictionary<string, object> Export()
        {
            var result = new Dictionary<string, object>();
            result["modelVersion"] = "1.9";
            result["evidencePolicy"] = new Dictionary<string, object> {
                { "observed", "Direct runtime value or native game aggregate." },
                { "derived", "Deterministic calculation from observed values." },
                { "inferred", "Guide-aware interpretation that remains explicitly qualified." },
                { "unknown", "Required evidence was unavailable or the observation window was insufficient." }
            };
            result["unlockedTechIds"] = SortedIds(UnlockedTechIds);
            result["queuedTechIds"] = SortedIds(QueuedTechIds);
            result["ownedItemCounts"] = ExportCounts();
            result["playerPlanetId"] = PlayerPlanetId;
            result["playerItemCounts"] = ExportCounts(PlayerItemCounts);
            result["planetItemCounts"] = ExportPlanetItemCounts();
            var production = new Dictionary<string, object> {
                { "available", ProductionWindowReady },
                { "windowGameSeconds", ProductionWindowSeconds },
                { "source", ProductionSource },
                { "scope", ProductionScope },
                { "period", ProductionPeriod },
                { "sampleCount", ProductionSampleCount },
                { "watchedItemCount", ProductionWatchedItemCount },
                { "itemCoverage", ProductionItemCoverage },
                { "factoryCount", ProductionFactoryCount },
                { "oneMinuteWindow", new Dictionary<string, object> {
                    { "available", ProductionWindowReady },
                    { "ready", ProductionWindowReady },
                    { "status", ProductionWindowReady
                        ? "ready" : "unavailable" },
                    { "windowGameSeconds", ProductionWindowSeconds }
                } },
                { "tenMinuteWindow", new Dictionary<string, object> {
                    { "available", ProductionTenMinuteWindowAvailable },
                    { "ready", ProductionTenMinuteWindowReady },
                    { "status", ProductionTenMinuteWindowStatus },
                    { "windowGameSeconds", ProductionTenMinuteWindowSeconds },
                    { "readinessSource", ProductionTenMinuteReadinessSource },
                    { "availableItemCount", ProductionTenMinuteAvailableItemCount },
                    { "readyItemCount", ProductionTenMinuteReadyItemCount }
                } },
                { "items", ExportItemFlows() },
                { "factoryItems", ExportFactoryItemFlows() }
            };
            if (!String.IsNullOrEmpty(ProductionFailure))
                production["failure"] = ProductionFailure;
            result["production"] = production;
            result["traffic"] = new Dictionary<string, object> {
                { "available", TrafficWindowReady },
                { "windowGameSeconds", TrafficWindowSeconds },
                { "source", "DSP cumulative factory traffic statistics" },
                { "items", ExportTrafficFlows() }
            };
            result["power"] = new Dictionary<string, object> {
                { "available", PowerPlanets.Count > 0 },
                { "windowGameSeconds", PowerWindowSeconds },
                { "source", "Rolling live power-network observations" },
                { "planets", ExportPowerStates() }
            };
            result["recipes"] = new Dictionary<string, object> {
                { "available", RecipeTelemetryAvailable },
                { "source", "Configured runtime production-machine recipes" },
                { "configurations", ExportRecipeConfigurations() }
            };
            result["dyson"] = ExportDyson();
            result["factoryBuildingCounts"] = ExportBuildingCounts();
            result["tankStorage"] = ExportCapacities();
            result["stationSlots"] = ExportStationSlots();
            result["stations"] = ExportStations();
            result["itemBuffers"] = ExportItemBuffers();
            return result;
        }

        private void ReadResearch(Dictionary<string, object> research)
        {
            foreach (object rowObject in Enumerate(GetValue(research, "technologies")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                object name = GetValue(row, "name");
                if (id > 0 && name != null) TechNames[id] = name.ToString();
                if (id > 0 && ToBool(GetValue(row, "unlocked"))) UnlockedTechIds.Add(id);
            }
            foreach (object value in Enumerate(GetValue(research, "techQueue")))
            {
                int id = Plugin.ToInt(value);
                if (id > 0) QueuedTechIds.Add(id);
            }
        }

        private void ReadLocation(Dictionary<string, object> location)
        {
            PlayerPlanetId = Plugin.ToInt(GetValue(location, "playerPlanetId"));
        }

        private void ReadOwnedItems(Dictionary<string, object> summary)
        {
            foreach (object rowObject in Enumerate(GetValue(summary, "allOwnedItems")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                if (id > 0) OwnedItemCounts[id] = Plugin.ToLong(GetValue(row, "count"));
            }
            ReadCountRows(GetValue(summary, "playerInventoryItems"), PlayerItemCounts);
            foreach (object planetObject in Enumerate(GetValue(summary, "factoryPlanetItems")))
            {
                var planetRow = planetObject as Dictionary<string, object>;
                if (planetRow == null) continue;
                Dictionary<string, object> planet = GetDictionary(planetRow, "planet");
                int planetId = Plugin.ToInt(GetValue(planet, "id"));
                if (planetId <= 0) continue;
                string planetName = ToText(GetValue(planet, "name"));
                if (!String.IsNullOrEmpty(planetName)) PlanetNames[planetId] = planetName;
                var counts = new Dictionary<int, long>();
                ReadCountRows(GetValue(planetRow, "contents"), counts);
                PlanetItemCounts[planetId] = counts;
            }
        }

        private static void ReadCountRows(object rows, Dictionary<int, long> destination)
        {
            foreach (object rowObject in Enumerate(rows))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                if (id <= 0) id = Plugin.ToInt(GetValue(row, "itemId"));
                if (id > 0) destination[id] = Plugin.ToLong(GetValue(row, "count"));
            }
        }

        private void ReadBuildingCounts(Dictionary<string, object> progression)
        {
            foreach (object rowObject in Enumerate(GetValue(progression, "allFactoryBuildingCounts")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "id"));
                if (id > 0) FactoryBuildingCounts[id] = Plugin.ToLong(GetValue(row, "count"));
            }
        }

        private void ReadTanks(object factories)
        {
            foreach (object factoryObject in Enumerate(factories))
            {
                var factory = factoryObject as Dictionary<string, object>;
                if (factory == null) continue;
                Dictionary<string, object> storage = GetDictionary(factory, "ownedStorage");
                foreach (object tankObject in Enumerate(GetValue(storage, "tanks")))
                {
                    var tank = tankObject as Dictionary<string, object>;
                    if (tank == null) continue;
                    int itemId = Plugin.ToInt(GetValue(tank, "itemId"));
                    if (itemId <= 0) continue;
                    ObservedCapacity aggregate;
                    if (!TankStorage.TryGetValue(itemId, out aggregate))
                    {
                        aggregate = new ObservedCapacity();
                        TankStorage[itemId] = aggregate;
                    }
                    aggregate.Count += Plugin.ToLong(GetValue(tank, "count"));
                    aggregate.Capacity += Plugin.ToLong(GetValue(tank, "capacity"));
                }
            }
        }

        private void ReadStations(object factories)
        {
            foreach (object factoryObject in Enumerate(factories))
            {
                var factory = factoryObject as Dictionary<string, object>;
                if (factory == null) continue;
                Dictionary<string, object> planet = GetDictionary(factory, "planet");
                int planetId = Plugin.ToInt(GetValue(planet, "id"));
                string planetName = ToText(GetValue(planet, "name"));
                Dictionary<string, object> logistics = GetDictionary(factory, "logistics");
                foreach (object stationObject in Enumerate(GetValue(logistics, "stations")))
                {
                    var station = stationObject as Dictionary<string, object>;
                    if (station == null) continue;
                    int stationId = Plugin.ToInt(GetValue(station, "id"));
                    bool isStellar = ToBool(GetValue(station, "isStellar"));
                    Dictionary<string, object> fleet =
                        GetDictionary(station, "fleet");
                    Stations.Add(new ObservedStationState {
                        PlanetId = planetId,
                        PlanetName = planetName,
                        StationId = stationId,
                        IsStellar = isStellar,
                        IdleShipCount = Plugin.ToInt(
                            GetValue(fleet, "idleShipCount")),
                        WorkShipCount = Plugin.ToInt(
                            GetValue(fleet, "workShipCount"))
                    });
                    foreach (object slotObject in Enumerate(GetValue(station, "storage")))
                    {
                        var slot = slotObject as Dictionary<string, object>;
                        if (slot == null) continue;
                        StationSlots.Add(new ObservedStationSlot {
                            PlanetId = planetId,
                            PlanetName = planetName,
                            StationId = stationId,
                            IsStellar = isStellar,
                            ItemId = Plugin.ToInt(GetValue(slot, "itemId")),
                            Name = ToText(GetValue(slot, "name")),
                            Count = Plugin.ToLong(GetValue(slot, "count")),
                            Maximum = Plugin.ToLong(GetValue(slot, "max")),
                            LocalLogic = ToText(GetValue(slot, "localLogic")),
                            RemoteLogic = ToText(GetValue(slot, "remoteLogic"))
                        });
                    }
                }
            }
        }

        private void ReadProduction(Dictionary<string, object> production)
        {
            ProductionWindowReady = ToBool(GetValue(production, "windowReady"));
            ProductionWindowSeconds = Plugin.ToDouble(GetValue(production, "windowGameSeconds"));
            ProductionSource = ToText(GetValue(production, "source"));
            ProductionScope = ToText(GetValue(production, "scope"));
            ProductionPeriod = ToText(GetValue(production, "period"));
            ProductionFailure = ToText(GetValue(production, "lastFailure"));
            ProductionSampleCount =
                Plugin.ToInt(GetValue(production, "sampleCount"));
            ProductionWatchedItemCount =
                Plugin.ToInt(GetValue(production, "watchedItemCount"));
            ProductionItemCoverage =
                Plugin.ToInt(GetValue(production, "galaxyItemCoverage"));
            ProductionFactoryCount =
                Plugin.ToInt(GetValue(production, "factoryCount"));
            Dictionary<string, object> tenMinuteWindow =
                GetDictionary(production, "tenMinuteWindow");
            ProductionTenMinuteWindowAvailable =
                ToBool(GetValue(tenMinuteWindow, "available"));
            ProductionTenMinuteWindowReady =
                ToBool(GetValue(tenMinuteWindow, "ready"));
            ProductionTenMinuteWindowStatus =
                ToText(GetValue(tenMinuteWindow, "status"));
            ProductionTenMinuteWindowSeconds =
                Plugin.ToDouble(GetValue(
                    tenMinuteWindow, "windowGameSeconds"));
            ProductionTenMinuteReadinessSource =
                ToText(GetValue(tenMinuteWindow, "readinessSource"));
            ProductionTenMinuteAvailableItemCount =
                Plugin.ToInt(GetValue(
                    tenMinuteWindow, "availableItemCount"));
            ProductionTenMinuteReadyItemCount =
                Plugin.ToInt(GetValue(tenMinuteWindow, "readyItemCount"));
            foreach (object rowObject in Enumerate(GetValue(production, "galaxy")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                int id = Plugin.ToInt(GetValue(row, "itemId"));
                if (id <= 0) continue;
                LifetimeItemTotals[id] = new ObservedLifetimeItemTotals {
                    ItemId = id,
                    Name = ToText(GetValue(row, "name")),
                    Produced = Plugin.ToLong(GetValue(row, "producedTotal")),
                    Consumed = Plugin.ToLong(GetValue(row, "consumedTotal"))
                };
                Dictionary<string, object> oneMinute =
                    GetDictionary(row, "oneMinuteWindow");
                Dictionary<string, object> tenMinute =
                    GetDictionary(row, "tenMinuteWindow");
                ItemFlows[id] = new ObservedItemFlow {
                    ItemId = id,
                    Name = ToText(GetValue(row, "name")),
                    ProducedPerMinute = Plugin.ToDouble(GetValue(row, "producedPerMinute")),
                    ConsumedPerMinute = Plugin.ToDouble(GetValue(row, "consumedPerMinute")),
                    NetPerMinute = Plugin.ToDouble(GetValue(row, "netPerMinute")),
                    ObservedIntervals = Plugin.ToInt(GetValue(row, "observedIntervals")),
                    ProductionActiveFraction = Plugin.ToDouble(GetValue(row, "productionActiveFraction")),
                    ProductionContinuity = ToText(GetValue(row, "productionContinuity")),
                    OneMinuteAvailable = ToBool(
                        GetValue(oneMinute, "available")),
                    OneMinuteStatus = ToText(
                        GetValue(oneMinute, "status")),
                    TenMinuteAvailable = ToBool(
                        GetValue(tenMinute, "available")),
                    TenMinuteReady = ToBool(
                        GetValue(tenMinute, "ready")),
                    TenMinuteStatus = ToText(
                        GetValue(tenMinute, "status")),
                    TenMinuteObservedGameSeconds = Plugin.ToDouble(
                        GetValue(tenMinute, "observedGameSeconds")),
                    TenMinuteProducedPerMinute = Plugin.ToDouble(
                        GetValue(tenMinute, "producedPerMinute")),
                    TenMinuteConsumedPerMinute = Plugin.ToDouble(
                        GetValue(tenMinute, "consumedPerMinute")),
                    TenMinuteNetPerMinute = Plugin.ToDouble(
                        GetValue(tenMinute, "netPerMinute"))
                };
            }

            foreach (object factoryObject in Enumerate(GetValue(production, "factories")))
            {
                var factory = factoryObject as Dictionary<string, object>;
                if (factory == null) continue;
                int factoryIndex = Plugin.ToInt(GetValue(factory, "factoryIndex"));
                int planetId = Plugin.ToInt(GetValue(factory, "planetId"));
                string planetName = ToText(GetValue(factory, "planetName"));
                foreach (object rowObject in Enumerate(GetValue(factory, "items")))
                {
                    var row = rowObject as Dictionary<string, object>;
                    if (row == null) continue;
                    FactoryItemFlows.Add(new ObservedFactoryItemFlow {
                        FactoryIndex = factoryIndex,
                        PlanetId = planetId,
                        PlanetName = planetName,
                        ItemId = Plugin.ToInt(GetValue(row, "itemId")),
                        Name = ToText(GetValue(row, "name")),
                        ProducedPerMinute = Plugin.ToDouble(GetValue(row, "producedPerMinute")),
                        ConsumedPerMinute = Plugin.ToDouble(GetValue(row, "consumedPerMinute")),
                        ProductionActiveFraction = Plugin.ToDouble(GetValue(row, "productionActiveFraction")),
                        ProductionContinuity = ToText(GetValue(row, "productionContinuity"))
                    });
                }
            }
        }

        private void BuildBufferEvidence()
        {
            foreach (ObservedItemFlow flow in ItemFlows.Values)
                GetOrCreateItemBuffer(flow.ItemId, flow.Name);

            var demandByScope = new Dictionary<string, double>();
            foreach (ObservedFactoryItemFlow flow in FactoryItemFlows)
            {
                if (flow.ItemId <= 0 || flow.PlanetId <= 0) continue;
                string key = BufferScopeKey(flow.ItemId, flow.PlanetId);
                double demand;
                demandByScope.TryGetValue(key, out demand);
                demandByScope[key] = demand + flow.ConsumedPerMinute;
            }

            var scopes = new Dictionary<string, ObservedBufferScopeEvidence>();
            foreach (ObservedStationSlot slot in StationSlots)
            {
                if (slot.ItemId <= 0) continue;
                ObservedItemBufferEvidence item = GetOrCreateItemBuffer(
                    slot.ItemId, slot.Name);
                var source = new ObservedBufferSourceEvidence {
                    PlanetId = slot.PlanetId,
                    PlanetName = slot.PlanetName,
                    StationId = slot.StationId,
                    SourceType = slot.IsStellar
                        ? "interstellar-logistics-slot"
                        : "planetary-logistics-slot",
                    ItemId = slot.ItemId,
                    Name = slot.Name,
                    Count = slot.Count,
                    Capacity = slot.Maximum,
                    LocalLogic = slot.LocalLogic,
                    RemoteLogic = slot.RemoteLogic
                };

                if (slot.Maximum > 0 && IsSupplyLogic(slot.LocalLogic))
                {
                    string key = BufferScopeKey(slot.ItemId, slot.PlanetId);
                    ObservedBufferScopeEvidence scope;
                    if (!scopes.TryGetValue(key, out scope))
                    {
                        scope = new ObservedBufferScopeEvidence {
                            PlanetId = slot.PlanetId,
                            PlanetName = slot.PlanetName,
                            ItemId = slot.ItemId,
                            Name = slot.Name,
                            BackpressureStatus = "unknown"
                        };
                        scopes[key] = scope;
                        item.Scopes.Add(scope);
                    }
                    scope.AccessibleCount += slot.Count;
                    scope.AccessibleCapacity += slot.Maximum;
                    scope.Contributors.Add(source);
                    item.AccessibleCount += slot.Count;
                    item.AccessibleCapacity += slot.Maximum;
                }
                else
                {
                    source.ExclusionReason = slot.Maximum <= 0
                        ? "capacity-unavailable"
                        : IsSupplyLogic(slot.RemoteLogic)
                            ? "remote-only"
                            : "not-local-supply";
                    item.ExcludedSources.Add(source);
                }
            }

            foreach (KeyValuePair<int, ObservedCapacity> pair in TankStorage)
            {
                ObservedItemBufferEvidence item = GetOrCreateItemBuffer(
                    pair.Key, Plugin.ItemName(pair.Key));
                item.ExcludedSources.Add(new ObservedBufferSourceEvidence {
                    SourceType = "tank-storage-aggregate",
                    ItemId = pair.Key,
                    Name = item.Name,
                    Count = pair.Value.Count,
                    Capacity = pair.Value.Capacity,
                    ExclusionReason = "accessibility-not-proven"
                });
            }

            foreach (ObservedItemBufferEvidence item in ItemBuffers.Values)
            {
                bool hasNotProven = false;
                bool hasProven = false;
                foreach (ObservedBufferScopeEvidence scope in item.Scopes)
                {
                    double demand;
                    scope.DemandEvidenceAvailable = demandByScope.TryGetValue(
                        BufferScopeKey(scope.ItemId, scope.PlanetId), out demand);
                    scope.DemandPerMinute = demand;
                    scope.RunwayAvailable =
                        scope.DemandEvidenceAvailable && demand > 0.0;
                    if (scope.RunwayAvailable)
                        scope.RunwayMinutes = scope.AccessibleCount / demand;

                    bool full = scope.Contributors.Count > 0;
                    foreach (ObservedBufferSourceEvidence source in scope.Contributors)
                    {
                        if (source.Capacity <= 0 || source.Count < source.Capacity)
                        {
                            full = false;
                            break;
                        }
                    }
                    scope.BackpressureStatus = full ? "proven" : "not-proven";
                    hasProven |= full;
                    hasNotProven |= !full;
                }
                item.BackpressureStatus = hasNotProven
                    ? "not-proven"
                    : hasProven ? "proven" : "unknown";
            }
        }

        private ObservedItemBufferEvidence GetOrCreateItemBuffer(
            int itemId, string name)
        {
            ObservedItemBufferEvidence item;
            if (!ItemBuffers.TryGetValue(itemId, out item))
            {
                item = new ObservedItemBufferEvidence {
                    ItemId = itemId,
                    Name = String.IsNullOrEmpty(name)
                        ? Plugin.ItemName(itemId) : name,
                    BackpressureStatus = "unknown"
                };
                ItemBuffers[itemId] = item;
            }
            else if (String.IsNullOrEmpty(item.Name) && !String.IsNullOrEmpty(name))
            {
                item.Name = name;
            }
            return item;
        }

        private static string BufferScopeKey(int itemId, int planetId)
        {
            return itemId.ToString() + ":" + planetId.ToString();
        }

        private static bool IsSupplyLogic(string value)
        {
            return String.Equals(value, "Supply", StringComparison.OrdinalIgnoreCase);
        }

        private void ReadTraffic(Dictionary<string, object> traffic)
        {
            TrafficWindowReady = ToBool(GetValue(traffic, "windowReady"));
            TrafficWindowSeconds = Plugin.ToDouble(GetValue(traffic, "windowGameSeconds"));
            foreach (object factoryObject in Enumerate(GetValue(traffic, "factories")))
            {
                var factory = factoryObject as Dictionary<string, object>;
                if (factory == null) continue;
                int factoryIndex = Plugin.ToInt(GetValue(factory, "factoryIndex"));
                int planetId = Plugin.ToInt(GetValue(factory, "planetId"));
                string planetName = ToText(GetValue(factory, "planetName"));
                foreach (object rowObject in Enumerate(GetValue(factory, "items")))
                {
                    var row = rowObject as Dictionary<string, object>;
                    if (row == null || ToBool(GetValue(row, "counterReset"))) continue;
                    TrafficFlows.Add(new ObservedTrafficFlow {
                        FactoryIndex = factoryIndex,
                        PlanetId = planetId,
                        PlanetName = planetName,
                        ItemId = Plugin.ToInt(GetValue(row, "itemId")),
                        Name = ToText(GetValue(row, "name")),
                        InputPerMinute = Plugin.ToDouble(GetValue(row, "inputPerMinute")),
                        OutputPerMinute = Plugin.ToDouble(GetValue(row, "outputPerMinute")),
                        InternalPerMinute = Plugin.ToDouble(GetValue(row, "internalPerMinute"))
                    });
                }
            }
        }

        private void ReadPower(Dictionary<string, object> power)
        {
            PowerWindowSeconds = Plugin.ToDouble(GetValue(power, "windowGameSeconds"));
            foreach (object rowObject in Enumerate(GetValue(power, "planets")))
            {
                var row = rowObject as Dictionary<string, object>;
                if (row == null) continue;
                PowerPlanets.Add(new ObservedPowerState {
                    FactoryIndex = Plugin.ToInt(GetValue(row, "factoryIndex")),
                    PlanetId = Plugin.ToInt(GetValue(row, "planetId")),
                    PlanetName = ToText(GetValue(row, "planetName")),
                    Observations = Plugin.ToInt(GetValue(row, "observations")),
                    AverageSatisfaction = Plugin.ToDouble(GetValue(row, "averageSatisfaction")),
                    MinimumSatisfaction = Plugin.ToDouble(GetValue(row, "minimumSatisfaction")),
                    UndersuppliedFraction = Plugin.ToDouble(GetValue(row, "undersuppliedFraction")),
                    MaximumDemandToCapacity = Plugin.ToDouble(GetValue(row, "maximumDemandToCapacity"))
                });
            }
        }

        private void ReadRecipes(Dictionary<string, object> recipes)
        {
            RecipeTelemetryAvailable = ToBool(GetValue(recipes, "available"));
            foreach (object factoryObject in Enumerate(GetValue(recipes, "factories")))
            {
                var factory = factoryObject as Dictionary<string, object>;
                if (factory == null) continue;
                int factoryIndex = Plugin.ToInt(GetValue(factory, "factoryIndex"));
                int planetId = Plugin.ToInt(GetValue(factory, "planetId"));
                string planetName = ToText(GetValue(factory, "planetName"));
                foreach (object recipeObject in Enumerate(GetValue(factory, "recipes")))
                {
                    var recipe = recipeObject as Dictionary<string, object>;
                    if (recipe == null) continue;
                    int recipeId = Plugin.ToInt(GetValue(recipe, "recipeId"));
                    int count = Plugin.ToInt(GetValue(recipe, "configuredMachineCount"));
                    if (recipeId <= 0 || count <= 0) continue;
                    RecipeConfigurations.Add(new ObservedRecipeConfiguration {
                        FactoryIndex = factoryIndex,
                        PlanetId = planetId,
                        PlanetName = planetName,
                        RecipeId = recipeId,
                        Name = ToText(GetValue(recipe, "name")),
                        ConfiguredMachineCount = count
                    });
                }
            }
        }

        private void ReadDyson(Dictionary<string, object> dyson)
        {
            Dyson.SolarSailLifeRaw = Plugin.ToDouble(
                GetValue(GetDictionary(dyson, "researchModifiers"), "solarSailLife"));

            foreach (object systemObject in Enumerate(GetValue(dyson, "systems")))
            {
                var system = systemObject as Dictionary<string, object>;
                if (system == null) continue;
                Dyson.AggregateSystemCount++;
                Dictionary<string, object> metrics = GetDictionary(system, "metrics");
                Dyson.Available |= ToBool(GetValue(metrics, "available"));
                if (String.IsNullOrEmpty(Dyson.AggregateSource))
                    Dyson.AggregateSource =
                        ToText(GetValue(metrics, "source"));
                Dyson.ConstructionAggregateAvailable |=
                    ToBool(GetValue(
                        metrics, "constructionAggregateAvailable"));
                Dyson.AggregateNodesRead +=
                    Plugin.ToInt(GetValue(metrics, "aggregateNodesRead"));
                Dyson.AggregateNodesMissing +=
                    Plugin.ToInt(GetValue(metrics, "aggregateNodesMissing"));
                Dyson.GenerationWatts +=
                    Plugin.ToDouble(GetValue(metrics, "energyGenCurrentTick")) * 60.0;
                Dyson.PermanentGenerationWatts +=
                    Plugin.ToDouble(
                        GetValue(metrics, "energyGenCurrentTick_Layers")) *
                    60.0;
                Dyson.SwarmGenerationWatts +=
                    Plugin.ToDouble(
                        GetValue(metrics, "energyGenCurrentTick_Swarm")) *
                    60.0;
                Dyson.RequestedWatts +=
                    Plugin.ToDouble(GetValue(metrics, "energyReqCurrentTick")) * 60.0;
                Dyson.ConstructedNodes += Plugin.ToLong(GetValue(metrics, "totalConstructedNodeCount"));
                Dyson.TotalNodes += Plugin.ToLong(GetValue(metrics, "totalNodeCount"));
                Dyson.ConstructedStructurePoints +=
                    Plugin.ToLong(GetValue(metrics, "totalConstructedStructurePoint"));
                Dyson.TotalStructurePoints += Plugin.ToLong(GetValue(metrics, "totalStructurePoint"));
                Dyson.ConstructedCellPoints +=
                    Plugin.ToLong(GetValue(metrics, "totalConstructedCellPoint"));
                Dyson.TotalCellPoints += Plugin.ToLong(GetValue(metrics, "totalCellPoint"));
                Dyson.RocketsInFlight += Plugin.ToLong(GetValue(metrics, "rocketCount"));
                Dyson.DesignatedShellCount += Plugin.ToLong(
                    GetValue(metrics, "designatedShellCount"));

                Dictionary<string, object> constructionRate =
                    GetDictionary(system, "observedConstructionRate");
                if (constructionRate.Count > 0)
                {
                    Dyson.ConstructionRateAvailable = true;
                    Dyson.ConstructedStructurePointsPerMinute +=
                        Plugin.ToDouble(GetValue(
                            constructionRate,
                            "constructedStructurePointsPerMinute"));
                    Dyson.ConstructedCellPointsPerMinute +=
                        Plugin.ToDouble(GetValue(
                            constructionRate,
                            "constructedCellPointsPerMinute"));
                    Dyson.PermanentGenerationWattsChangePerMinute +=
                        Plugin.ToDouble(GetValue(
                            constructionRate,
                            "permanentGenerationWattsChangePerMinute"));
                }

                Dictionary<string, object> swarm = GetDictionary(system, "swarm");
                Dictionary<string, object> swarmMetrics = GetDictionary(swarm, "metrics");
                Dyson.SwarmSailCount += Plugin.ToLong(GetValue(swarmMetrics, "sailCount"));
                Dictionary<string, object> population =
                    GetDictionary(swarm, "observedPopulationRate");
                Dyson.NetSwarmSailsPerMinute +=
                    Plugin.ToDouble(GetValue(population, "netSailPopulationPerMinute"));
            }

            foreach (object planetObject in Enumerate(GetValue(dyson, "planets")))
            {
                var planet = planetObject as Dictionary<string, object>;
                if (planet == null) continue;
                Dictionary<string, object> ejectors = GetDictionary(planet, "ejectors");
                Dyson.EjectorCount += Plugin.ToInt(GetValue(ejectors, "deployedCount"));
                Dyson.EjectorsSupplied += Plugin.ToInt(
                    GetValue(ejectors, "suppliedCount"));
                Dyson.EjectorsFiringNow += Plugin.ToInt(
                    GetValue(ejectors, "firingNowCount"));
                if (Plugin.ToInt(GetValue(ejectors, "suppliedCount")) == 0 &&
                    Plugin.ToDouble(GetValue(
                        GetDictionary(ejectors, "counterSums"),
                        "bulletCount")) > 0.0)
                    Dyson.EjectorsSupplied++;
                foreach (object distributionObject in Enumerate(
                    GetValue(ejectors, "stateDistributions")))
                {
                    var distribution = distributionObject as Dictionary<string, object>;
                    if (distribution == null ||
                        !String.Equals(ToText(GetValue(distribution, "member")),
                            "targetState", StringComparison.OrdinalIgnoreCase)) continue;
                    foreach (object valueObject in Enumerate(GetValue(distribution, "values")))
                    {
                        var value = valueObject as Dictionary<string, object>;
                        if (value != null &&
                            String.Equals(ToText(GetValue(value, "value")), "OK",
                                StringComparison.OrdinalIgnoreCase))
                            Dyson.EjectorsOnTarget += Plugin.ToInt(GetValue(value, "count"));
                    }
                }

                Dictionary<string, object> silos = GetDictionary(planet, "silos");
                Dyson.SiloCount += Plugin.ToInt(GetValue(silos, "deployedCount"));
                Dyson.SilosSupplied += Plugin.ToInt(
                    GetValue(silos, "suppliedCount"));
                Dyson.SilosWithTarget += Plugin.ToInt(
                    GetValue(silos, "targetAssignedCount"));
                Dyson.SilosFiringNow += Plugin.ToInt(
                    GetValue(silos, "firingNowCount"));
                if (Plugin.ToInt(GetValue(silos, "suppliedCount")) == 0 &&
                    Plugin.ToDouble(GetValue(
                        GetDictionary(silos, "counterSums"),
                        "bulletCount")) > 0.0)
                    Dyson.SilosSupplied++;

                Dictionary<string, object> receivers = GetDictionary(planet, "receivers");
                Dyson.ReceiverCount += Plugin.ToInt(GetValue(receivers, "deployedCount"));
                foreach (object deviceObject in Enumerate(GetValue(receivers, "devices")))
                {
                    var device = deviceObject as Dictionary<string, object>;
                    if (device == null) continue;
                    Dictionary<string, object> metrics = GetDictionary(device, "metrics");
                    if (Plugin.ToInt(GetValue(metrics, "catalystId")) == 1209)
                        Dyson.LensedReceiverCount++;
                    if (Plugin.ToDouble(GetValue(metrics, "currentStrength")) >= 0.999)
                        Dyson.FullStrengthReceiverCount++;
                    double warmup = Plugin.ToDouble(GetValue(metrics, "warmup"));
                    if (warmup > 0.0 &&
                        (Dyson.MinimumReceiverWarmup <= 0.0 ||
                         warmup < Dyson.MinimumReceiverWarmup))
                        Dyson.MinimumReceiverWarmup = warmup;
                }
            }

            Dictionary<string, object> continuity =
                GetDictionary(dyson, "receiverContinuity");
            if (continuity.Count > 0)
            {
                Dyson.ReceiverTelemetryAvailable =
                    ToBool(GetValue(continuity, "available"));
                Dyson.ReceiverCount =
                    Plugin.ToInt(GetValue(continuity, "deployedCount"));
                Dyson.ReceiverContinuityWindowSeconds =
                    Plugin.ToDouble(
                        GetValue(continuity, "maximumWindowSeconds"));
                Dyson.ConfiguredPhotonReceiverCount =
                    Plugin.ToInt(
                        GetValue(continuity, "configuredPhotonCount"));
                Dyson.LensedPhotonReceiverCount =
                    Plugin.ToInt(
                        GetValue(continuity, "lensedPhotonCount"));
                Dyson.LensedReceiverCount =
                    Dyson.LensedPhotonReceiverCount;
                Dyson.FullStrengthPhotonReceiverCount =
                    Plugin.ToInt(
                        GetValue(continuity, "fullStrengthPhotonCount"));
                Dyson.FullStrengthReceiverCount =
                    Dyson.FullStrengthPhotonReceiverCount;
                Dyson.ContinuousPhotonReceiverCount =
                    Plugin.ToInt(
                        GetValue(
                            continuity,
                            "continuousReceivingPhotonCount"));
                Dyson.SustainedPhotonReceiverCount =
                    Plugin.ToInt(
                        GetValue(continuity, "sustainedPhotonCount"));
                Dyson.ReceiverArrayRequestedDysonPowerWatts =
                    Plugin.ToDouble(
                        GetValue(
                            continuity,
                            "arrayRequestedDysonPowerWatts"));
                Dyson.ReceiverArraySuppliedPowerWatts =
                    Plugin.ToDouble(
                        GetValue(
                            continuity,
                            "arraySuppliedPowerWatts"));
                Dyson.ReceiverArrayCriticalPhotonOutputPerMinute =
                    Plugin.ToDouble(
                        GetValue(
                            continuity,
                            "arrayCriticalPhotonOutputPerMinute"));
                foreach (object receiverObject in Enumerate(
                    GetValue(continuity, "devices")))
                {
                    var receiver =
                        receiverObject as Dictionary<string, object>;
                    if (receiver == null) continue;
                    Dyson.Receivers.Add(new ObservedReceiverState {
                        PlanetId = Plugin.ToInt(
                            GetValue(receiver, "planetId")),
                        PlanetName = ToText(
                            GetValue(receiver, "planetName")),
                        EntityId = Plugin.ToInt(
                            GetValue(receiver, "entityId")),
                        SampleCount = Plugin.ToInt(
                            GetValue(receiver, "sampleCount")),
                        WindowSeconds = Plugin.ToDouble(
                            GetValue(receiver, "windowSeconds")),
                        WindowReady = ToBool(
                            GetValue(receiver, "windowReady")),
                        ConfiguredForPhotonGeneration = ToBool(
                            GetValue(
                                receiver,
                                "configuredForPhotonGeneration")),
                        LensedNow = ToBool(
                            GetValue(receiver, "lensedNow")),
                        LensSustained = ToBool(
                            GetValue(receiver, "lensSustained")),
                        ContinuousReceivingNow = ToBool(
                            GetValue(
                                receiver,
                                "continuousReceivingNow")),
                        WarmupNow = Plugin.ToDouble(
                            GetValue(receiver, "warmupNow")),
                        MinimumWarmup = Plugin.ToDouble(
                            GetValue(receiver, "minimumWarmup")),
                        FullStrengthNow = ToBool(
                            GetValue(receiver, "fullStrengthNow")),
                        StrengthNow = Plugin.ToDouble(
                            GetValue(receiver, "strengthNow")),
                        MinimumStrength = Plugin.ToDouble(
                            GetValue(receiver, "minimumStrength")),
                        SustainedHealthy = ToBool(
                            GetValue(receiver, "sustainedHealthy")),
                        RequestedDysonPowerWatts = Plugin.ToDouble(
                            GetValue(
                                receiver,
                                "requestedDysonPowerWatts")),
                        SuppliedPowerWatts = Plugin.ToDouble(
                            GetValue(receiver, "suppliedPowerWatts")),
                        CriticalPhotonOutputPerMinute =
                            Plugin.ToDouble(
                                GetValue(
                                    receiver,
                                    "criticalPhotonOutputPerMinute"))
                    });
                }
            }

            if (Dyson.PermanentGenerationWatts <= 0.0 &&
                Dyson.GenerationWatts > 0.0)
                Dyson.PermanentGenerationWatts =
                    Math.Max(
                        0.0,
                        Dyson.GenerationWatts -
                            Dyson.SwarmGenerationWatts);
            ObservedItemFlow rockets;
            double rocketLaunchRate = ItemFlows.TryGetValue(1503, out rockets)
                ? rockets.ConsumedPerMinute : 0.0;
            bool substantialStructure =
                Dyson.ConstructedNodes >= 2 ||
                Dyson.ConstructedStructurePoints >= 60 ||
                Dyson.ConstructedCellPoints > 0;
            Dyson.SphereRouteObserved =
                substantialStructure && Dyson.SiloCount > 0 &&
                (rocketLaunchRate > 0.0 || Dyson.RocketsInFlight > 0);
        }

        private List<object> ExportItemFlows()
        {
            var ids = new List<int>(ItemFlows.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
            {
                ObservedItemFlow x = ItemFlows[id];
                rows.Add(new Dictionary<string, object> {
                    { "itemId", x.ItemId }, { "name", x.Name },
                    { "producedPerMinute", x.ProducedPerMinute },
                    { "consumedPerMinute", x.ConsumedPerMinute },
                    { "netPerMinute", x.NetPerMinute },
                    { "observedIntervals", x.ObservedIntervals },
                    { "productionActiveFraction", x.ProductionActiveFraction },
                    { "productionContinuity", x.ProductionContinuity },
                    { "oneMinuteWindow", new Dictionary<string, object> {
                        { "available", x.OneMinuteAvailable },
                        { "status", x.OneMinuteStatus },
                        { "producedPerMinute", x.OneMinuteAvailable
                            ? (object)x.ProducedPerMinute : null },
                        { "consumedPerMinute", x.OneMinuteAvailable
                            ? (object)x.ConsumedPerMinute : null },
                        { "netPerMinute", x.OneMinuteAvailable
                            ? (object)x.NetPerMinute : null }
                    } },
                    { "tenMinuteWindow", new Dictionary<string, object> {
                        { "available", x.TenMinuteAvailable },
                        { "ready", x.TenMinuteReady },
                        { "status", x.TenMinuteStatus },
                        { "observedGameSeconds", x.TenMinuteObservedGameSeconds },
                        { "producedPerMinute", x.TenMinuteAvailable
                            ? (object)x.TenMinuteProducedPerMinute : null },
                        { "consumedPerMinute", x.TenMinuteAvailable
                            ? (object)x.TenMinuteConsumedPerMinute : null },
                        { "netPerMinute", x.TenMinuteAvailable
                            ? (object)x.TenMinuteNetPerMinute : null }
                    } }
                });
            }
            return rows;
        }

        private List<object> ExportTrafficFlows()
        {
            var rows = new List<object>();
            foreach (ObservedTrafficFlow x in TrafficFlows)
                rows.Add(new Dictionary<string, object> {
                    { "factoryIndex", x.FactoryIndex }, { "planetId", x.PlanetId },
                    { "planetName", x.PlanetName }, { "itemId", x.ItemId }, { "name", x.Name },
                    { "inputPerMinute", x.InputPerMinute }, { "outputPerMinute", x.OutputPerMinute },
                    { "internalPerMinute", x.InternalPerMinute }
                });
            return rows;
        }

        private List<object> ExportFactoryItemFlows()
        {
            var rows = new List<object>();
            foreach (ObservedFactoryItemFlow x in FactoryItemFlows)
                rows.Add(new Dictionary<string, object> {
                    { "factoryIndex", x.FactoryIndex }, { "planetId", x.PlanetId },
                    { "planetName", x.PlanetName }, { "itemId", x.ItemId },
                    { "name", x.Name }, { "producedPerMinute", x.ProducedPerMinute },
                    { "consumedPerMinute", x.ConsumedPerMinute },
                    { "productionActiveFraction", x.ProductionActiveFraction },
                    { "productionContinuity", x.ProductionContinuity }
                });
            return rows;
        }

        private List<object> ExportPowerStates()
        {
            var rows = new List<object>();
            foreach (ObservedPowerState x in PowerPlanets)
                rows.Add(new Dictionary<string, object> {
                    { "factoryIndex", x.FactoryIndex }, { "planetId", x.PlanetId },
                    { "planetName", x.PlanetName }, { "observations", x.Observations },
                    { "averageSatisfaction", x.AverageSatisfaction },
                    { "minimumSatisfaction", x.MinimumSatisfaction },
                    { "undersuppliedFraction", x.UndersuppliedFraction },
                    { "maximumDemandToCapacity", x.MaximumDemandToCapacity }
                });
            return rows;
        }

        private List<object> ExportRecipeConfigurations()
        {
            var rows = new List<object>();
            foreach (ObservedRecipeConfiguration x in RecipeConfigurations)
                rows.Add(new Dictionary<string, object> {
                    { "factoryIndex", x.FactoryIndex }, { "planetId", x.PlanetId },
                    { "planetName", x.PlanetName }, { "recipeId", x.RecipeId },
                    { "name", x.Name }, { "configuredMachineCount", x.ConfiguredMachineCount }
                });
            return rows;
        }

        private List<object> ExportCounts()
        {
            return ExportCounts(OwnedItemCounts);
        }

        private static List<object> ExportCounts(Dictionary<int, long> counts)
        {
            var ids = new List<int>(counts.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(new Dictionary<string, object> { { "itemId", id }, { "count", counts[id] } });
            return rows;
        }

        private List<object> ExportPlanetItemCounts()
        {
            var planetIds = new List<int>(PlanetItemCounts.Keys);
            planetIds.Sort();
            var rows = new List<object>();
            foreach (int planetId in planetIds)
                rows.Add(new Dictionary<string, object> {
                    { "planetId", planetId },
                    { "planetName", PlanetNames.ContainsKey(planetId) ? PlanetNames[planetId] : "" },
                    { "items", ExportCounts(PlanetItemCounts[planetId]) }
                });
            return rows;
        }

        private List<object> ExportBuildingCounts()
        {
            var ids = new List<int>(FactoryBuildingCounts.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(new Dictionary<string, object> {
                    { "itemId", id }, { "count", FactoryBuildingCounts[id] }
                });
            return rows;
        }

        private Dictionary<string, object> ExportDyson()
        {
            return new Dictionary<string, object> {
                { "available", Dyson.Available },
                { "generationWatts", Dyson.GenerationWatts },
                { "permanentGenerationWatts", Dyson.PermanentGenerationWatts },
                { "swarmGenerationWatts", Dyson.SwarmGenerationWatts },
                { "requestedWatts", Dyson.RequestedWatts },
                { "solarSailLifeRaw", Dyson.SolarSailLifeRaw },
                { "swarmSailCount", Dyson.SwarmSailCount },
                { "netSwarmSailsPerMinute", Dyson.NetSwarmSailsPerMinute },
                { "ejectorCount", Dyson.EjectorCount },
                { "ejectorsOnTarget", Dyson.EjectorsOnTarget },
                { "ejectorsSupplied", Dyson.EjectorsSupplied },
                { "ejectorsFiringNow", Dyson.EjectorsFiringNow },
                { "siloCount", Dyson.SiloCount },
                { "silosSupplied", Dyson.SilosSupplied },
                { "silosWithTarget", Dyson.SilosWithTarget },
                { "silosFiringNow", Dyson.SilosFiringNow },
                { "receiverCount", Dyson.ReceiverCount },
                { "lensedReceiverCount", Dyson.LensedReceiverCount },
                { "fullStrengthReceiverCount", Dyson.FullStrengthReceiverCount },
                { "minimumReceiverWarmup", Dyson.MinimumReceiverWarmup },
                { "receiverTelemetryAvailable", Dyson.ReceiverTelemetryAvailable },
                { "receiverContinuityWindowSeconds", Dyson.ReceiverContinuityWindowSeconds },
                { "configuredPhotonReceiverCount", Dyson.ConfiguredPhotonReceiverCount },
                { "lensedPhotonReceiverCount", Dyson.LensedPhotonReceiverCount },
                { "fullStrengthPhotonReceiverCount", Dyson.FullStrengthPhotonReceiverCount },
                { "continuousPhotonReceiverCount", Dyson.ContinuousPhotonReceiverCount },
                { "sustainedPhotonReceiverCount", Dyson.SustainedPhotonReceiverCount },
                { "receiverArrayRequestedDysonPowerWatts", Dyson.ReceiverArrayRequestedDysonPowerWatts },
                { "receiverArraySuppliedPowerWatts", Dyson.ReceiverArraySuppliedPowerWatts },
                { "receiverArrayCriticalPhotonOutputPerMinute", Dyson.ReceiverArrayCriticalPhotonOutputPerMinute },
                { "receivers", ExportReceivers() },
                { "constructedNodes", Dyson.ConstructedNodes },
                { "totalNodes", Dyson.TotalNodes },
                { "constructedStructurePoints", Dyson.ConstructedStructurePoints },
                { "totalStructurePoints", Dyson.TotalStructurePoints },
                { "constructedCellPoints", Dyson.ConstructedCellPoints },
                { "totalCellPoints", Dyson.TotalCellPoints },
                { "designatedShellCount", Dyson.DesignatedShellCount },
                { "rocketsInFlight", Dyson.RocketsInFlight },
                { "constructionRateAvailable", Dyson.ConstructionRateAvailable },
                { "constructedStructurePointsPerMinute", Dyson.ConstructedStructurePointsPerMinute },
                { "constructedCellPointsPerMinute", Dyson.ConstructedCellPointsPerMinute },
                { "permanentGenerationWattsChangePerMinute", Dyson.PermanentGenerationWattsChangePerMinute },
                { "aggregateSource", Dyson.AggregateSource },
                { "aggregateSystemCount", Dyson.AggregateSystemCount },
                { "constructionAggregateAvailable", Dyson.ConstructionAggregateAvailable },
                { "aggregateNodesRead", Dyson.AggregateNodesRead },
                { "aggregateNodesMissing", Dyson.AggregateNodesMissing },
                { "sphereRouteObserved", Dyson.SphereRouteObserved }
            };
        }

        private List<object> ExportReceivers()
        {
            var rows = new List<object>();
            foreach (ObservedReceiverState receiver in Dyson.Receivers)
                rows.Add(new Dictionary<string, object> {
                    { "planetId", receiver.PlanetId },
                    { "planetName", receiver.PlanetName },
                    { "entityId", receiver.EntityId },
                    { "sampleCount", receiver.SampleCount },
                    { "windowSeconds", receiver.WindowSeconds },
                    { "windowReady", receiver.WindowReady },
                    { "configuredForPhotonGeneration", receiver.ConfiguredForPhotonGeneration },
                    { "lensedNow", receiver.LensedNow },
                    { "lensSustained", receiver.LensSustained },
                    { "continuousReceivingNow", receiver.ContinuousReceivingNow },
                    { "warmupNow", receiver.WarmupNow },
                    { "minimumWarmup", receiver.MinimumWarmup },
                    { "fullStrengthNow", receiver.FullStrengthNow },
                    { "strengthNow", receiver.StrengthNow },
                    { "minimumStrength", receiver.MinimumStrength },
                    { "sustainedHealthy", receiver.SustainedHealthy },
                    { "requestedDysonPowerWatts", receiver.RequestedDysonPowerWatts },
                    { "suppliedPowerWatts", receiver.SuppliedPowerWatts },
                    { "criticalPhotonOutputPerMinute", receiver.CriticalPhotonOutputPerMinute }
                });
            return rows;
        }

        private List<object> ExportCapacities()
        {
            var ids = new List<int>(TankStorage.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(new Dictionary<string, object> {
                    { "itemId", id }, { "count", TankStorage[id].Count },
                    { "capacity", TankStorage[id].Capacity }
                });
            return rows;
        }

        private List<object> ExportStationSlots()
        {
            var rows = new List<object>();
            foreach (ObservedStationSlot x in StationSlots)
                rows.Add(new Dictionary<string, object> {
                    { "planetId", x.PlanetId }, { "planetName", x.PlanetName },
                    { "stationId", x.StationId }, { "isStellar", x.IsStellar },
                    { "itemId", x.ItemId }, { "name", x.Name },
                    { "count", x.Count }, { "max", x.Maximum },
                    { "localLogic", x.LocalLogic }, { "remoteLogic", x.RemoteLogic }
                });
            return rows;
        }

        private List<object> ExportStations()
        {
            var rows = new List<object>();
            foreach (ObservedStationState x in Stations)
                rows.Add(new Dictionary<string, object> {
                    { "planetId", x.PlanetId }, { "planetName", x.PlanetName },
                    { "stationId", x.StationId }, { "isStellar", x.IsStellar },
                    { "idleShipCount", x.IdleShipCount },
                    { "workShipCount", x.WorkShipCount }
                });
            return rows;
        }

        private List<object> ExportItemBuffers()
        {
            var ids = new List<int>(ItemBuffers.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int id in ids)
            {
                ObservedItemBufferEvidence item = ItemBuffers[id];
                var scopes = new List<object>();
                foreach (ObservedBufferScopeEvidence scope in item.Scopes)
                {
                    var contributors = new List<object>();
                    foreach (ObservedBufferSourceEvidence source in scope.Contributors)
                        contributors.Add(ExportBufferSource(source));
                    scopes.Add(new Dictionary<string, object> {
                        { "planetId", scope.PlanetId },
                        { "planetName", scope.PlanetName },
                        { "accessibleCount", scope.AccessibleCount },
                        { "accessibleCapacity", scope.AccessibleCapacity },
                        { "demandEvidenceAvailable", scope.DemandEvidenceAvailable },
                        { "demandPerMinute", scope.DemandEvidenceAvailable
                            ? (object)scope.DemandPerMinute : null },
                        { "runwayAvailable", scope.RunwayAvailable },
                        { "runwayMinutes", scope.RunwayAvailable
                            ? (object)scope.RunwayMinutes : null },
                        { "backpressureStatus", scope.BackpressureStatus },
                        { "contributors", contributors }
                    });
                }
                var excluded = new List<object>();
                foreach (ObservedBufferSourceEvidence source in item.ExcludedSources)
                    excluded.Add(ExportBufferSource(source));
                rows.Add(new Dictionary<string, object> {
                    { "itemId", item.ItemId },
                    { "name", item.Name },
                    { "accessibleCount", item.AccessibleCount },
                    { "accessibleCapacity", item.AccessibleCapacity },
                    { "backpressureStatus", item.BackpressureStatus },
                    { "scopes", scopes },
                    { "excludedSources", excluded }
                });
            }
            return rows;
        }

        private static Dictionary<string, object> ExportBufferSource(
            ObservedBufferSourceEvidence source)
        {
            var result = new Dictionary<string, object> {
                { "sourceType", source.SourceType },
                { "planetId", source.PlanetId },
                { "planetName", source.PlanetName },
                { "stationId", source.StationId },
                { "itemId", source.ItemId },
                { "name", source.Name },
                { "count", source.Count },
                { "capacity", source.Capacity },
                { "localLogic", source.LocalLogic },
                { "remoteLogic", source.RemoteLogic }
            };
            if (!String.IsNullOrEmpty(source.ExclusionReason))
                result["exclusionReason"] = source.ExclusionReason;
            return result;
        }

        private static List<object> SortedIds(HashSet<int> values)
        {
            var ids = new List<int>(values);
            ids.Sort();
            var result = new List<object>();
            foreach (int id in ids) result.Add(id);
            return result;
        }

        private static Dictionary<string, object> GetDictionary(Dictionary<string, object> source, string key)
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

        private static string ToText(object value)
        {
            return value != null ? value.ToString() : null;
        }
    }
}
