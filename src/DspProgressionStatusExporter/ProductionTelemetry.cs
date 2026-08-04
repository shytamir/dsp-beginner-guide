using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Reads the same one-minute and ten-minute aggregates used by DSP's
    /// Statistics Panel.
    /// Inventory movement is deliberately not used as a production proxy.
    /// </summary>
    internal sealed class ProductionTelemetry
    {
        private const int MaximumSamples = 25;
        private const int ProductionPeriodIndex = 1;
        private const int ConsumptionPeriodIndex = 8;
        private const int TenMinuteProductionPeriodIndex = 2;
        private const int TenMinuteConsumptionPeriodIndex = 9;
        private const int LifetimeProductionIndex = 6;
        private const int LifetimeConsumptionIndex = 13;
        private const double NativePeriodSeconds = 60.0;
        private const double TenMinutePeriodSeconds = 600.0;
        private const double TenMinutePeriodMinutes = 10.0;

        // This is the union currently consumed by guide analysis, compact
        // snapshots, and the few planet-scoped route checks.
        private static readonly int[] WatchedItemIds = {
            1003, 1004,
            1101, 1103, 1104, 1105, 1106, 1109,
            1114, 1116, 1117, 1118,
            1120, 1121, 1122, 1123, 1124, 1127, 1131,
            1202, 1208, 1209, 1210,
            1301, 1303, 1305,
            1402,
            1501, 1503,
            2001, 2011, 2103, 2104, 2107,
            5001, 5002, 5003,
            6001, 6002, 6003, 6004, 6005, 6006
        };

        private static readonly HashSet<int> FactoryScopedItemIds =
            new HashSet<int> { 1003, 1004, 1105, 1106 };
        private static readonly HashSet<int> LifetimeItemIds =
            new HashSet<int> { 6001, 6002, 6003, 6004, 6005, 6006 };

        private sealed class AggregatePair
        {
            public long Produced;
            public long Consumed;
            public bool OneMinuteAvailable;
            public long TenMinuteProduced;
            public long TenMinuteConsumed;
            public bool TenMinuteAvailable;
            public bool TenMinuteReady;
            public double TenMinuteObservedGameSeconds;
            public long LifetimeProduced;
            public long LifetimeConsumed;
            public bool LifetimeAvailable;
        }

        private sealed class SamplePoint
        {
            public DateTime AtUtc;
            public long GameTick;
            public Dictionary<int, AggregatePair> Galaxy =
                new Dictionary<int, AggregatePair>();
            public Dictionary<int, Dictionary<int, AggregatePair>> Factories =
                new Dictionary<int, Dictionary<int, AggregatePair>>();
            public Dictionary<int, int> FactoryPlanetIds =
                new Dictionary<int, int>();
            public Dictionary<int, string> FactoryPlanetNames =
                new Dictionary<int, string>();
        }

        private readonly Queue<SamplePoint> samples =
            new Queue<SamplePoint>();
        private readonly Dictionary<int, long> tenMinuteFirstObservedTicks =
            new Dictionary<int, long>();
        private object sampledGameData;
        private string lastFailure;
        private int lastFactoryCount;

        public void Clear()
        {
            samples.Clear();
            tenMinuteFirstObservedTicks.Clear();
            sampledGameData = null;
            lastFailure = null;
            lastFactoryCount = 0;
        }

        public void Sample(object gameData, long gameTick)
        {
            try
            {
                if (!Object.ReferenceEquals(sampledGameData, gameData))
                {
                    samples.Clear();
                    tenMinuteFirstObservedTicks.Clear();
                    sampledGameData = gameData;
                }

                object gameStatistics = Plugin.GetMember(gameData, "statistics");
                object production = Plugin.GetMember(gameStatistics, "production");
                object factoryStatPool =
                    Plugin.GetMember(production, "factoryStatPool");
                if (factoryStatPool == null)
                {
                    lastFailure =
                        "GameData.statistics.production.factoryStatPool was unavailable.";
                    return;
                }

                var point = new SamplePoint {
                    AtUtc = DateTime.UtcNow,
                    GameTick = gameTick
                };
                SamplePoint previous = LastSample();
                if (previous != null && point.GameTick < previous.GameTick)
                {
                    samples.Clear();
                    tenMinuteFirstObservedTicks.Clear();
                }

                var gameFactories = new List<object>();
                foreach (object factory in Plugin.Enumerate(
                    Plugin.GetMember(gameData, "factories")))
                    gameFactories.Add(factory);

                int factoryIndex = 0;
                int activeFactoryCount = 0;
                foreach (object factoryStat in Plugin.Enumerate(factoryStatPool))
                {
                    if (factoryStat != null)
                    {
                        activeFactoryCount++;
                        Dictionary<int, AggregatePair> counters =
                            ReadFactoryAggregates(factoryStat);
                        if (counters.Count > 0)
                        {
                            Merge(point.Galaxy, counters);
                            Dictionary<int, AggregatePair> scoped =
                                FilterFactoryScoped(counters);
                            if (scoped.Count > 0)
                                point.Factories[factoryIndex] = scoped;
                        }
                    }
                    if (factoryIndex < gameFactories.Count &&
                        gameFactories[factoryIndex] != null)
                    {
                        object planet = Plugin.GetMember(
                            gameFactories[factoryIndex], "planet");
                        point.FactoryPlanetIds[factoryIndex] =
                            Plugin.ToInt(Plugin.GetMember(
                                planet, "id", "planetId"));
                        object name = Plugin.GetMember(
                            planet, "displayName", "name");
                        if (name != null)
                            point.FactoryPlanetNames[factoryIndex] =
                                name.ToString();
                    }
                    factoryIndex++;
                }
                lastFactoryCount = activeFactoryCount;

                UpdateTenMinuteReadiness(point);

                samples.Enqueue(point);
                while (samples.Count > MaximumSamples) samples.Dequeue();
                lastFailure = point.Galaxy.Count > 0
                    ? null
                    : "No watched ProductStat native aggregate rows were available.";
            }
            catch (Exception ex)
            {
                lastFailure = ex.GetType().Name + ": " + ex.Message;
            }
        }

        public Dictionary<string, object> Export()
        {
            SamplePoint last = LastSample();
            bool aggregateAvailable =
                CountOneMinuteItems(last) > 0;
            int tenMinuteAvailableItems = CountTenMinuteItems(last, false);
            int tenMinuteReadyItems = CountTenMinuteItems(last, true);
            bool tenMinuteAvailable = tenMinuteAvailableItems > 0;
            bool tenMinuteReady = tenMinuteAvailable &&
                tenMinuteReadyItems == tenMinuteAvailableItems;
            var result = new Dictionary<string, object> {
                { "available", aggregateAvailable },
                { "source", "GameData.statistics.production.factoryStatPool[*].productIndices[itemId] -> productPool[index].total" },
                { "scope", "entire-star-cluster" },
                { "period", "one-minute" },
                { "productionPeriodIndex", ProductionPeriodIndex },
                { "consumptionPeriodIndex", ConsumptionPeriodIndex },
                { "semantics", "Rates are DSP's pre-aggregated Statistics Panel values; ten-minute totals are divided by ten to normalize them to items per minute, and inventory transfers do not affect either window." },
                { "sampleCount", samples.Count },
                { "watchedItemCount", WatchedItemIds.Length },
                { "factoryCount", lastFactoryCount },
                { "windowGameSeconds", NativePeriodSeconds },
                { "windowReady", aggregateAvailable },
                { "oneMinuteWindow", WindowMetadata(
                    aggregateAvailable,
                    aggregateAvailable,
                    aggregateAvailable ? "ready" : "unavailable",
                    "one-minute",
                    NativePeriodSeconds,
                    ProductionPeriodIndex,
                    ConsumptionPeriodIndex,
                    "native-aggregate") },
                { "tenMinuteWindow", WindowMetadata(
                    tenMinuteAvailable,
                    tenMinuteReady,
                    tenMinuteAvailable
                        ? (tenMinuteReady ? "ready" : "not-ready")
                        : "unavailable",
                    "ten-minute",
                    TenMinutePeriodSeconds,
                    TenMinuteProductionPeriodIndex,
                    TenMinuteConsumptionPeriodIndex,
                    "per-item-mod-observation-age") }
            };
            Dictionary<string, object> tenMinuteWindow =
                (Dictionary<string, object>)result["tenMinuteWindow"];
            tenMinuteWindow["availableItemCount"] = tenMinuteAvailableItems;
            tenMinuteWindow["readyItemCount"] = tenMinuteReadyItems;
            if (!String.IsNullOrEmpty(lastFailure))
                result["lastFailure"] = lastFailure;
            if (last == null) return result;

            SamplePoint first = FirstSample();
            result["sampledAtUtc"] =
                last.AtUtc.ToString("o", CultureInfo.InvariantCulture);
            result["observationSpanGameSeconds"] =
                Math.Round((last.GameTick - first.GameTick) / 60.0, 3);
            result["galaxyItemCoverage"] = last.Galaxy.Count;

            var galaxyWindows =
                new List<Dictionary<int, AggregatePair>>();
            foreach (SamplePoint sample in samples)
                galaxyWindows.Add(sample.Galaxy);
            result["galaxy"] =
                ExportAggregates(last.Galaxy, galaxyWindows, true);

            var factories = new List<object>();
            var factoryIds = new SortedSet<int>();
            foreach (SamplePoint sample in samples)
                foreach (int id in sample.Factories.Keys)
                    factoryIds.Add(id);
            foreach (int id in factoryIds)
            {
                var row = new Dictionary<string, object> {
                    { "factoryIndex", id },
                    { "scope", "planet-factory" }
                };
                int planetId;
                string planetName;
                if (last.FactoryPlanetIds.TryGetValue(id, out planetId))
                    row["planetId"] = planetId;
                if (last.FactoryPlanetNames.TryGetValue(id, out planetName))
                    row["planetName"] = planetName;

                Dictionary<int, AggregatePair> current;
                if (!last.Factories.TryGetValue(id, out current))
                    current = new Dictionary<int, AggregatePair>();
                var windows =
                    new List<Dictionary<int, AggregatePair>>();
                foreach (SamplePoint sample in samples)
                {
                    Dictionary<int, AggregatePair> window;
                    if (!sample.Factories.TryGetValue(id, out window))
                        window = new Dictionary<int, AggregatePair>();
                    windows.Add(window);
                }
                row["items"] = ExportAggregates(current, windows, false);
                factories.Add(row);
            }
            result["factories"] = factories;
            return result;
        }

        private static Dictionary<int, AggregatePair>
            ReadFactoryAggregates(object factoryStat)
        {
            var result = new Dictionary<int, AggregatePair>();
            object productPool = Plugin.GetMember(factoryStat, "productPool");
            object productIndices =
                Plugin.GetMember(factoryStat, "productIndices");
            foreach (int itemId in WatchedItemIds)
            {
                int poolIndex =
                    Plugin.ToInt(ElementAt(productIndices, itemId));
                if (poolIndex <= 0) continue;
                object stat = ElementAt(productPool, poolIndex);
                if (stat == null ||
                    Plugin.ToInt(Plugin.GetMember(stat, "itemId")) != itemId)
                    continue;
                object totals = Plugin.GetMember(stat, "total");
                long produced = 0L;
                long consumed = 0L;
                long tenMinuteProduced = 0L;
                long tenMinuteConsumed = 0L;
                bool oneMinuteAvailable =
                    TryReadTotal(totals, ProductionPeriodIndex, out produced) &&
                    TryReadTotal(totals, ConsumptionPeriodIndex, out consumed);
                bool tenMinuteAvailable =
                    TryReadTotal(
                        totals, TenMinuteProductionPeriodIndex,
                        out tenMinuteProduced) &&
                    TryReadTotal(
                        totals, TenMinuteConsumptionPeriodIndex,
                        out tenMinuteConsumed);
                if (!oneMinuteAvailable && !tenMinuteAvailable)
                    continue;

                var pair = new AggregatePair {
                    Produced = produced,
                    Consumed = consumed,
                    OneMinuteAvailable = oneMinuteAvailable,
                    TenMinuteProduced = tenMinuteProduced,
                    TenMinuteConsumed = tenMinuteConsumed,
                    TenMinuteAvailable = tenMinuteAvailable
                };
                if (LifetimeItemIds.Contains(itemId))
                {
                    long lifetimeProduced = 0L;
                    long lifetimeConsumed = 0L;
                    pair.LifetimeAvailable =
                        TryReadTotal(
                            totals, LifetimeProductionIndex,
                            out lifetimeProduced) &&
                        TryReadTotal(
                            totals, LifetimeConsumptionIndex,
                            out lifetimeConsumed);
                    pair.LifetimeProduced = lifetimeProduced;
                    pair.LifetimeConsumed = lifetimeConsumed;
                }
                result[itemId] = pair;
            }
            return result;
        }

        private static object ElementAt(object collection, int index)
        {
            Array array = collection as Array;
            if (array != null)
                return index >= 0 && index < array.Length
                    ? array.GetValue(index) : null;
            IList list = collection as IList;
            return list != null && index >= 0 && index < list.Count
                ? list[index] : null;
        }

        private static bool TryReadTotal(
            object totals, int index, out long value)
        {
            value = 0L;
            object element = ElementAt(totals, index);
            if (element == null) return false;
            value = Plugin.ToLong(element);
            return true;
        }

        private static Dictionary<int, AggregatePair> FilterFactoryScoped(
            Dictionary<int, AggregatePair> source)
        {
            var result = new Dictionary<int, AggregatePair>();
            foreach (var kv in source)
                if (FactoryScopedItemIds.Contains(kv.Key))
                    result[kv.Key] = kv.Value;
            return result;
        }

        private static void Merge(
            Dictionary<int, AggregatePair> target,
            Dictionary<int, AggregatePair> source)
        {
            foreach (var kv in source)
            {
                AggregatePair pair;
                if (!target.TryGetValue(kv.Key, out pair))
                {
                    pair = new AggregatePair();
                    target[kv.Key] = pair;
                }
                pair.Produced += kv.Value.Produced;
                pair.Consumed += kv.Value.Consumed;
                pair.OneMinuteAvailable =
                    pair.OneMinuteAvailable || kv.Value.OneMinuteAvailable;
                pair.TenMinuteProduced += kv.Value.TenMinuteProduced;
                pair.TenMinuteConsumed += kv.Value.TenMinuteConsumed;
                pair.TenMinuteAvailable =
                    pair.TenMinuteAvailable || kv.Value.TenMinuteAvailable;
                pair.TenMinuteReady =
                    pair.TenMinuteReady || kv.Value.TenMinuteReady;
                pair.TenMinuteObservedGameSeconds = Math.Max(
                    pair.TenMinuteObservedGameSeconds,
                    kv.Value.TenMinuteObservedGameSeconds);
                if (kv.Value.LifetimeAvailable)
                {
                    pair.LifetimeAvailable = true;
                    pair.LifetimeProduced += kv.Value.LifetimeProduced;
                    pair.LifetimeConsumed += kv.Value.LifetimeConsumed;
                }
            }
        }

        private static List<object> ExportAggregates(
            Dictionary<int, AggregatePair> current,
            List<Dictionary<int, AggregatePair>> windows,
            bool includeLifetime)
        {
            var rows = new List<object>();
            var ids = new List<int>(current.Keys);
            ids.Sort();
            foreach (int id in ids)
            {
                AggregatePair pair = current[id];
                double tenMinuteProduced = NormalizePerMinute(
                    pair.TenMinuteProduced, TenMinutePeriodMinutes);
                double tenMinuteConsumed = NormalizePerMinute(
                    pair.TenMinuteConsumed, TenMinutePeriodMinutes);
                var row = new Dictionary<string, object> {
                    { "itemId", id },
                    { "name", Plugin.ItemName(id) },
                    { "producedPerMinute", (double)pair.Produced },
                    { "consumedPerMinute", (double)pair.Consumed },
                    { "netPerMinute", (double)(pair.Produced - pair.Consumed) },
                    { "oneMinuteWindow", WindowValues(
                        pair.OneMinuteAvailable,
                        pair.OneMinuteAvailable,
                        pair.OneMinuteAvailable ? "ready" : "unavailable",
                        NativePeriodSeconds,
                        pair.Produced,
                        pair.Consumed,
                        0.0) },
                    { "tenMinuteWindow", WindowValues(
                        pair.TenMinuteAvailable,
                        pair.TenMinuteReady,
                        pair.TenMinuteAvailable
                            ? (pair.TenMinuteReady ? "ready" : "not-ready")
                            : "unavailable",
                        TenMinutePeriodSeconds,
                        tenMinuteProduced,
                        tenMinuteConsumed,
                        pair.TenMinuteObservedGameSeconds) }
                };
                if (includeLifetime && pair.LifetimeAvailable)
                {
                    row["producedTotal"] = pair.LifetimeProduced;
                    row["consumedTotal"] = pair.LifetimeConsumed;
                }
                AddContinuity(row, id, windows);
                rows.Add(row);
            }
            return rows;
        }

        private static void AddContinuity(
            Dictionary<string, object> row,
            int itemId,
            List<Dictionary<int, AggregatePair>> windows)
        {
            int observed = 0;
            int productionActive = 0;
            int consumptionActive = 0;
            foreach (Dictionary<int, AggregatePair> window in windows)
            {
                AggregatePair pair;
                if (!window.TryGetValue(itemId, out pair)) continue;
                observed++;
                if (pair.Produced > 0) productionActive++;
                if (pair.Consumed > 0) consumptionActive++;
            }
            row["continuitySamples"] = observed;
            row["observedIntervals"] = observed;
            if (observed <= 0) return;

            double productionFraction =
                productionActive * 1.0 / observed;
            double consumptionFraction =
                consumptionActive * 1.0 / observed;
            row["productionActiveFraction"] =
                Math.Round(productionFraction, 3);
            row["consumptionActiveFraction"] =
                Math.Round(consumptionFraction, 3);
            row["productionContinuity"] =
                productionFraction >= 0.90
                    ? "continuous-native-window"
                    : (productionFraction >= 0.50
                        ? "intermittent-native-window"
                        : "mostly-idle-native-window");
        }

        private SamplePoint FirstSample()
        {
            foreach (SamplePoint sample in samples) return sample;
            return null;
        }

        private SamplePoint LastSample()
        {
            SamplePoint last = null;
            foreach (SamplePoint sample in samples) last = sample;
            return last;
        }

        private void UpdateTenMinuteReadiness(SamplePoint point)
        {
            foreach (var kv in point.Galaxy)
            {
                AggregatePair pair = kv.Value;
                if (!pair.TenMinuteAvailable) continue;
                long firstTick;
                if (!tenMinuteFirstObservedTicks.TryGetValue(
                    kv.Key, out firstTick))
                {
                    firstTick = point.GameTick;
                    tenMinuteFirstObservedTicks[kv.Key] = firstTick;
                }
                pair.TenMinuteObservedGameSeconds = Math.Max(
                    0.0, (point.GameTick - firstTick) / 60.0);
                pair.TenMinuteReady =
                    pair.TenMinuteObservedGameSeconds >=
                    TenMinutePeriodSeconds;
            }
            foreach (Dictionary<int, AggregatePair> factory in
                point.Factories.Values)
            {
                foreach (var kv in factory)
                {
                    AggregatePair pair = kv.Value;
                    long firstTick;
                    if (!pair.TenMinuteAvailable ||
                        !tenMinuteFirstObservedTicks.TryGetValue(
                            kv.Key, out firstTick))
                        continue;
                    pair.TenMinuteObservedGameSeconds = Math.Max(
                        0.0, (point.GameTick - firstTick) / 60.0);
                    pair.TenMinuteReady =
                        pair.TenMinuteObservedGameSeconds >=
                        TenMinutePeriodSeconds;
                }
            }
        }

        private static int CountOneMinuteItems(SamplePoint point)
        {
            if (point == null) return 0;
            int count = 0;
            foreach (AggregatePair pair in point.Galaxy.Values)
                if (pair.OneMinuteAvailable) count++;
            return count;
        }

        private static int CountTenMinuteItems(
            SamplePoint point, bool readyOnly)
        {
            if (point == null) return 0;
            int count = 0;
            foreach (AggregatePair pair in point.Galaxy.Values)
                if (pair.TenMinuteAvailable &&
                    (!readyOnly || pair.TenMinuteReady))
                    count++;
            return count;
        }

        private static double NormalizePerMinute(
            long windowTotal, double windowMinutes)
        {
            return windowMinutes > 0.0
                ? windowTotal / windowMinutes
                : 0.0;
        }

        private static Dictionary<string, object> WindowMetadata(
            bool available,
            bool ready,
            string status,
            string period,
            double windowGameSeconds,
            int productionIndex,
            int consumptionIndex,
            string readinessSource)
        {
            return new Dictionary<string, object> {
                { "available", available },
                { "ready", ready },
                { "status", status },
                { "period", period },
                { "windowGameSeconds", windowGameSeconds },
                { "productionPeriodIndex", productionIndex },
                { "consumptionPeriodIndex", consumptionIndex },
                { "readinessSource", readinessSource }
            };
        }

        private static Dictionary<string, object> WindowValues(
            bool available,
            bool ready,
            string status,
            double windowGameSeconds,
            double producedPerMinute,
            double consumedPerMinute,
            double observedGameSeconds)
        {
            var result = new Dictionary<string, object> {
                { "available", available },
                { "ready", ready },
                { "status", status },
                { "windowGameSeconds", windowGameSeconds },
                { "producedPerMinute", available
                    ? (object)producedPerMinute : null },
                { "consumedPerMinute", available
                    ? (object)consumedPerMinute : null },
                { "netPerMinute", available
                    ? (object)(producedPerMinute - consumedPerMinute) : null }
            };
            if (observedGameSeconds > 0.0 ||
                (available && windowGameSeconds > NativePeriodSeconds))
                result["observedGameSeconds"] =
                    Math.Round(observedGameSeconds, 3);
            return result;
        }
    }
}
