using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Samples the game's own cumulative production-stat counters. Inventory
    /// movement is deliberately not used as a production proxy.
    /// </summary>
    internal sealed class ProductionTelemetry
    {
        private const int MaximumSamples = 25;
        private const double MinimumWindowSeconds = 4.0;

        private sealed class CounterPair
        {
            public long Produced;
            public long Consumed;
        }

        private sealed class SamplePoint
        {
            public DateTime AtUtc;
            public long GameTick;
            public Dictionary<int, CounterPair> Galaxy = new Dictionary<int, CounterPair>();
            public Dictionary<int, Dictionary<int, CounterPair>> Factories =
                new Dictionary<int, Dictionary<int, CounterPair>>();
            public Dictionary<int, int> FactoryPlanetIds = new Dictionary<int, int>();
            public Dictionary<int, string> FactoryPlanetNames = new Dictionary<int, string>();
        }

        private readonly Queue<SamplePoint> samples = new Queue<SamplePoint>();
        private object sampledGameData;
        private string lastFailure;

        public void Clear()
        {
            samples.Clear();
            sampledGameData = null;
            lastFailure = null;
        }

        public void Sample(object gameData, long gameTick)
        {
            try
            {
                if (!Object.ReferenceEquals(sampledGameData, gameData))
                {
                    samples.Clear();
                    sampledGameData = gameData;
                }

                object gameStatistics = Plugin.GetMember(gameData, "statistics");
                object production = Plugin.GetMember(gameStatistics, "production");
                object factoryStatPool = Plugin.GetMember(production, "factoryStatPool");
                if (factoryStatPool == null)
                {
                    lastFailure = "GameData.statistics.production.factoryStatPool was unavailable.";
                    return;
                }

                var point = new SamplePoint();
                point.AtUtc = DateTime.UtcNow;
                point.GameTick = gameTick;
                if (samples.Count > 0)
                {
                    SamplePoint previous = null;
                    foreach (SamplePoint existing in samples) previous = existing;
                    if (point.GameTick < previous.GameTick) samples.Clear();
                }

                var gameFactories = new List<object>();
                foreach (object factory in Plugin.Enumerate(Plugin.GetMember(gameData, "factories")))
                    gameFactories.Add(factory);

                int factoryIndex = 0;
                foreach (object factoryStat in Plugin.Enumerate(factoryStatPool))
                {
                    if (factoryStat != null)
                    {
                        Dictionary<int, CounterPair> counters = ReadFactoryCounters(factoryStat);
                        if (counters.Count > 0)
                        {
                            point.Factories[factoryIndex] = counters;
                            Merge(point.Galaxy, counters);
                        }
                    }
                    if (factoryIndex < gameFactories.Count && gameFactories[factoryIndex] != null)
                    {
                        object planet = Plugin.GetMember(gameFactories[factoryIndex], "planet");
                        point.FactoryPlanetIds[factoryIndex] =
                            Plugin.ToInt(Plugin.GetMember(planet, "id", "planetId"));
                        object name = Plugin.GetMember(planet, "displayName", "name");
                        if (name != null) point.FactoryPlanetNames[factoryIndex] = name.ToString();
                    }
                    factoryIndex++;
                }

                samples.Enqueue(point);
                while (samples.Count > MaximumSamples) samples.Dequeue();
                lastFailure = null;
            }
            catch (Exception ex)
            {
                lastFailure = ex.GetType().Name + ": " + ex.Message;
            }
        }

        public Dictionary<string, object> Export()
        {
            var result = new Dictionary<string, object>();
            result["available"] = samples.Count > 0;
            result["source"] = "GameData.statistics.production.factoryStatPool[*].productPool[*].total[6 production, 13 consumption]";
            result["semantics"] = "Rates are deltas of the game's cumulative production and consumption counters; inventory transfers do not affect them.";
            result["sampleCount"] = samples.Count;
            if (!String.IsNullOrEmpty(lastFailure)) result["lastFailure"] = lastFailure;
            if (samples.Count == 0) return result;

            SamplePoint first = null;
            SamplePoint last = null;
            foreach (SamplePoint sample in samples)
            {
                if (first == null) first = sample;
                last = sample;
            }

            long gameTicks = last.GameTick - first.GameTick;
            double seconds = gameTicks / 60.0;
            result["sampledAtUtc"] = last.AtUtc.ToString("o", CultureInfo.InvariantCulture);
            result["windowGameTicks"] = gameTicks;
            result["windowGameSeconds"] = Math.Round(seconds, 3);
            result["wallClockSpanSeconds"] = Math.Round((last.AtUtc - first.AtUtc).TotalSeconds, 3);
            result["windowReady"] = seconds >= MinimumWindowSeconds;
            var galaxyWindows = new List<Dictionary<int, CounterPair>>();
            foreach (SamplePoint sample in samples) galaxyWindows.Add(sample.Galaxy);
            result["galaxy"] = ExportRates(first.Galaxy, last.Galaxy, seconds, galaxyWindows);

            var factories = new List<object>();
            var factoryIds = new SortedSet<int>();
            foreach (int id in first.Factories.Keys) factoryIds.Add(id);
            foreach (int id in last.Factories.Keys) factoryIds.Add(id);
            foreach (int id in factoryIds)
            {
                var row = new Dictionary<string, object>();
                row["factoryIndex"] = id;
                int planetId;
                string planetName;
                if (last.FactoryPlanetIds.TryGetValue(id, out planetId)) row["planetId"] = planetId;
                if (last.FactoryPlanetNames.TryGetValue(id, out planetName)) row["planetName"] = planetName;
                Dictionary<int, CounterPair> a;
                Dictionary<int, CounterPair> b;
                if (!first.Factories.TryGetValue(id, out a)) a = new Dictionary<int, CounterPair>();
                if (!last.Factories.TryGetValue(id, out b)) b = new Dictionary<int, CounterPair>();
                var factoryWindows = new List<Dictionary<int, CounterPair>>();
                foreach (SamplePoint sample in samples)
                {
                    Dictionary<int, CounterPair> window;
                    if (!sample.Factories.TryGetValue(id, out window))
                        window = new Dictionary<int, CounterPair>();
                    factoryWindows.Add(window);
                }
                row["items"] = ExportRates(a, b, seconds, factoryWindows);
                factories.Add(row);
            }
            result["factories"] = factories;
            return result;
        }

        private static Dictionary<int, CounterPair> ReadFactoryCounters(object factoryStat)
        {
            var result = new Dictionary<int, CounterPair>();
            foreach (object stat in Plugin.Enumerate(Plugin.GetMember(factoryStat, "productPool")))
            {
                if (stat == null) continue;
                int itemId = Plugin.ToInt(Plugin.GetMember(stat, "itemId"));
                if (itemId <= 0) continue;

                long produced;
                long consumed;
                if (!TryReadCounterPair(Plugin.GetMember(stat, "total"), out produced, out consumed))
                    continue;

                result[itemId] = new CounterPair { Produced = produced, Consumed = consumed };
            }
            return result;
        }

        private static bool TryReadCounterPair(object value, out long produced, out long consumed)
        {
            produced = 0L;
            consumed = 0L;
            if (value == null || value is string) return false;

            int index = 0;
            bool foundProduction = false;
            bool foundConsumption = false;
            foreach (object x in Plugin.Enumerate(value))
            {
                if (index == 6)
                {
                    produced = Plugin.ToLong(x);
                    foundProduction = true;
                }
                else if (index == 13)
                {
                    consumed = Plugin.ToLong(x);
                    foundConsumption = true;
                    break;
                }
                index++;
            }
            return foundProduction && foundConsumption;
        }

        private static void Merge(Dictionary<int, CounterPair> target, Dictionary<int, CounterPair> source)
        {
            foreach (var kv in source)
            {
                CounterPair pair;
                if (!target.TryGetValue(kv.Key, out pair))
                {
                    pair = new CounterPair();
                    target[kv.Key] = pair;
                }
                pair.Produced += kv.Value.Produced;
                pair.Consumed += kv.Value.Consumed;
            }
        }

        private static List<object> ExportRates(
            Dictionary<int, CounterPair> first,
            Dictionary<int, CounterPair> last,
            double seconds,
            List<Dictionary<int, CounterPair>> windows)
        {
            var rows = new List<object>();
            var ids = new SortedSet<int>();
            foreach (int id in first.Keys) ids.Add(id);
            foreach (int id in last.Keys) ids.Add(id);

            foreach (int id in ids)
            {
                CounterPair a;
                CounterPair b;
                if (!first.TryGetValue(id, out a)) a = new CounterPair();
                if (!last.TryGetValue(id, out b)) b = new CounterPair();

                long producedDelta = b.Produced - a.Produced;
                long consumedDelta = b.Consumed - a.Consumed;
                bool reset = producedDelta < 0 || consumedDelta < 0;

                var row = new Dictionary<string, object>();
                row["itemId"] = id;
                row["name"] = Plugin.ItemName(id);
                row["producedTotal"] = b.Produced;
                row["consumedTotal"] = b.Consumed;
                row["counterReset"] = reset;
                if (!reset && seconds >= MinimumWindowSeconds)
                {
                    row["producedPerMinute"] = Math.Round(producedDelta * 60.0 / seconds, 3);
                    row["consumedPerMinute"] = Math.Round(consumedDelta * 60.0 / seconds, 3);
                    row["netPerMinute"] = Math.Round((producedDelta - consumedDelta) * 60.0 / seconds, 3);
                    AddContinuity(row, id, windows);
                }
                rows.Add(row);
            }
            return rows;
        }

        private static void AddContinuity(
            Dictionary<string, object> row,
            int itemId,
            List<Dictionary<int, CounterPair>> windows)
        {
            int intervals = 0;
            int activeProduction = 0;
            int activeConsumption = 0;
            long previousProduced = 0L;
            long previousConsumed = 0L;
            bool havePrevious = false;

            foreach (Dictionary<int, CounterPair> window in windows)
            {
                CounterPair current;
                if (!window.TryGetValue(itemId, out current)) current = new CounterPair();
                if (havePrevious)
                {
                    long producedDelta = current.Produced - previousProduced;
                    long consumedDelta = current.Consumed - previousConsumed;
                    if (producedDelta >= 0 && consumedDelta >= 0)
                    {
                        intervals++;
                        if (producedDelta > 0) activeProduction++;
                        if (consumedDelta > 0) activeConsumption++;
                    }
                }
                previousProduced = current.Produced;
                previousConsumed = current.Consumed;
                havePrevious = true;
            }

            row["observedIntervals"] = intervals;
            if (intervals > 0)
            {
                double productionFraction = activeProduction * 1.0 / intervals;
                double consumptionFraction = activeConsumption * 1.0 / intervals;
                row["productionActiveIntervals"] = activeProduction;
                row["productionActiveFraction"] = Math.Round(productionFraction, 3);
                row["consumptionActiveIntervals"] = activeConsumption;
                row["consumptionActiveFraction"] = Math.Round(consumptionFraction, 3);
                row["productionContinuity"] = productionFraction >= 0.90 ? "continuous-observed" :
                    (productionFraction >= 0.50 ? "intermittent-observed" : "mostly-idle-observed");
            }
        }
    }
}
