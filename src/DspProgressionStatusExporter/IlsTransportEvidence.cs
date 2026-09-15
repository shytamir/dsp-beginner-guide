using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class IlsTransportEvidence
    {
        public ObservedStationState Home;
        public ObservedStationState Source;
        public int HomePolicies;
        public int SourcePolicies;
        public long InventoryTowers;
        public long InventoryVessels;
        public int DeployedTowers;
        public int AssignedVessels;
        public bool Available;
        public bool HardwareReady { get { return InventoryTowers + DeployedTowers >= 2 && InventoryVessels + AssignedVessels >= 5; } }
        public bool DeploymentReady { get { return Home != null && Source != null && Home.EvidenceAvailable && Source.EvidenceAvailable && HomePolicies == 2 && SourcePolicies == 2 && Home.IdleShipCount + Home.WorkShipCount >= 5; } }

        public static IlsTransportEvidence Build(ObservedGameState state)
        {
            return state.IlsTransport ?? Resolve(state, null);
        }

        internal static IlsTransportEvidence Resolve(ObservedGameState state, IlsTransportEvidence preferred)
        {
            var result = new IlsTransportEvidence();
            int outpost = GuideGateEngine.FindExpeditionPlanet(state);
            foreach (ObservedStationState station in state.Stations)
            {
                if (!station.IsStellar || station.StationId <= 0 || station.PlanetId <= 0 || state.StarterPlanetId <= 0) continue;
                bool home = station.PlanetId == state.StarterPlanetId;
                int policies = CountPolicies(state, station, home ? "Demand" : "Supply");
                if (!home && policies == 0 && station.PlanetId != outpost) continue;
                ObservedStationState selected = home ? result.Home : result.Source;
                int score = home ? result.HomePolicies : result.SourcePolicies;
                ObservedStationState prior = preferred == null ? null : home ? preferred.Home : preferred.Source;
                bool prefer = Same(station, prior);
                if (selected == null || policies > score || policies == score &&
                    (prefer || !Same(selected, prior) && (station.PlanetId < selected.PlanetId || station.PlanetId == selected.PlanetId && station.StationId < selected.StationId)))
                {
                    if (home) { result.Home = station; result.HomePolicies = policies; }
                    else { result.Source = station; result.SourcePolicies = policies; }
                }
            }
            bool atHome = state.PlayerLocationAvailable && state.PlayerPlanetId == state.StarterPlanetId;
            bool homeStockKnown = state.StarterPlanetId > 0 && state.AvailablePlanetInventories.Contains(state.StarterPlanetId);
            if (homeStockKnown)
            {
                Dictionary<int, long> stock;
                if (state.PlanetItemCounts.TryGetValue(state.StarterPlanetId, out stock))
                {
                    result.InventoryTowers += Count(stock, 2104);
                    result.InventoryVessels += Count(stock, 5002);
                }
            }
            if (atHome && state.PlayerInventoryAvailable)
            {
                result.InventoryTowers += Count(state.PlayerItemCounts, 2104);
                result.InventoryVessels += Count(state.PlayerItemCounts, 5002);
            }
            foreach (ObservedStationState station in new[] { result.Home, result.Source })
            {
                if (station == null || !station.EvidenceAvailable) continue;
                result.DeployedTowers++;
                result.AssignedVessels += Math.Max(0, station.IdleShipCount) + Math.Max(0, station.WorkShipCount);
            }
            result.Available = homeStockKnown && state.PlayerLocationAvailable &&
                (!atHome || state.PlayerInventoryAvailable) && state.AvailableStationPlanets.Contains(state.StarterPlanetId) &&
                (outpost <= 0 || state.AvailableStationPlanets.Contains(outpost)) &&
                (result.Home == null || result.Home.EvidenceAvailable) &&
                (result.Source == null || result.Source.EvidenceAvailable);
            return result;
        }

        private static bool Same(ObservedStationState a, ObservedStationState b)
        { return a != null && b != null && a.PlanetId == b.PlanetId && a.StationId == b.StationId; }

        private static long Count(Dictionary<int, long> stock, int id)
        {
            long count;
            return stock.TryGetValue(id, out count) ? Math.Max(0, count) : 0;
        }

        internal static int CountPolicies(ObservedGameState state, ObservedStationState station, string policy)
        {
            bool titanium = false, silicon = false;
            foreach (ObservedStationSlot slot in state.StationSlots)
                if (slot.PlanetId == station.PlanetId && slot.StationId == station.StationId && slot.IsStellar &&
                    String.Equals(slot.RemoteLogic, policy, StringComparison.OrdinalIgnoreCase))
                {
                    if (slot.ItemId == 1106) titanium = true;
                    if (slot.ItemId == 1105) silicon = true;
                }
            return (titanium ? 1 : 0) + (silicon ? 1 : 0);
        }

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object> {
                { "available", Available }, { "home", Endpoint(Home, HomePolicies) },
                { "source", Endpoint(Source, SourcePolicies) },
                { "homeInventoryTowers", InventoryTowers }, { "homeInventoryVessels", InventoryVessels },
                { "deployedEndpointTowers", DeployedTowers }, { "assignedEndpointVessels", AssignedVessels },
                { "hardwareReady", HardwareReady }, { "deploymentReady", DeploymentReady },
                { "scope", "Home stationary stock, Icarus while at home, and the selected home/source endpoints; each unit counted once." }
            };
        }

        private static object Endpoint(ObservedStationState station, int policies)
        {
            if (station == null) return null;
            return new Dictionary<string, object> {
                { "planetId", station.PlanetId }, { "stationId", station.StationId },
                { "available", station.EvidenceAvailable }, { "matchingPolicies", policies },
                { "idleVessels", station.IdleShipCount }, { "workingVessels", station.WorkShipCount }
            };
        }
    }
}
