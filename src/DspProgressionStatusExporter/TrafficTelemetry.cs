using System;
using System.Collections.Generic;
using System.Globalization;

namespace DspProgressionStatusExporter
{
    internal sealed class TrafficTelemetry
    {
        private const int MaximumSamples = 25;
        private const double MinimumWindowSeconds = 4.0;

        private sealed class Counters
        {
            public long Input;
            public long Output;
            public long Internal;
        }

        private sealed class Sample
        {
            public DateTime AtUtc;
            public long GameTick;
            public Dictionary<int, Dictionary<int, Counters>> Factories =
                new Dictionary<int, Dictionary<int, Counters>>();
            public Dictionary<int, int> PlanetIds = new Dictionary<int, int>();
            public Dictionary<int, string> PlanetNames = new Dictionary<int, string>();
        }

        private readonly Queue<Sample> samples = new Queue<Sample>();
        private object sampledGameData;
        private string lastFailure;

        public void Clear()
        {
            samples.Clear();
            sampledGameData = null;
            lastFailure = null;
        }

        public void SampleNow(object gameData, long gameTick)
        {
            try
            {
                if (!Object.ReferenceEquals(sampledGameData, gameData))
                {
                    samples.Clear();
                    sampledGameData = gameData;
                }

                object statistics = Plugin.GetMember(gameData, "statistics");
                object traffic = Plugin.GetMember(statistics, "traffic");
                object pool = Plugin.GetMember(traffic, "factoryTrafficPool");
                if (pool == null)
                {
                    lastFailure = "GameData.statistics.traffic.factoryTrafficPool was unavailable.";
                    return;
                }

                var point = new Sample { AtUtc = DateTime.UtcNow, GameTick = gameTick };
                var factories = new List<object>();
                foreach (object factory in Plugin.Enumerate(Plugin.GetMember(gameData, "factories")))
                    factories.Add(factory);

                int index = 0;
                foreach (object stat in Plugin.Enumerate(pool))
                {
                    if (stat != null)
                    {
                        Dictionary<int, Counters> counters = ReadCounters(stat);
                        if (counters.Count > 0) point.Factories[index] = counters;
                    }
                    if (index < factories.Count && factories[index] != null)
                    {
                        object planet = Plugin.GetMember(factories[index], "planet");
                        point.PlanetIds[index] = Plugin.ToInt(Plugin.GetMember(planet, "id", "planetId"));
                        object name = Plugin.GetMember(planet, "displayName", "name");
                        if (name != null) point.PlanetNames[index] = name.ToString();
                    }
                    index++;
                }

                if (samples.Count > 0)
                {
                    Sample previous = null;
                    foreach (Sample existing in samples) previous = existing;
                    if (gameTick < previous.GameTick) samples.Clear();
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
            result["source"] = "GameData.statistics.traffic.factoryTrafficPool[*].trafficPool[*].total[6 input, 13 output, 20 internal]";
            result["semantics"] = "Planetary logistics traffic recorded by DSP; rates are cumulative-counter deltas over simulation time.";
            result["sampleCount"] = samples.Count;
            if (!String.IsNullOrEmpty(lastFailure)) result["lastFailure"] = lastFailure;
            if (samples.Count == 0) return result;

            Sample first = null;
            Sample last = null;
            foreach (Sample point in samples)
            {
                if (first == null) first = point;
                last = point;
            }
            long ticks = last.GameTick - first.GameTick;
            double seconds = ticks / 60.0;
            result["sampledAtUtc"] = last.AtUtc.ToString("o", CultureInfo.InvariantCulture);
            result["windowGameTicks"] = ticks;
            result["windowGameSeconds"] = Math.Round(seconds, 3);
            result["windowReady"] = seconds >= MinimumWindowSeconds;

            var rows = new List<object>();
            var factoryIds = new SortedSet<int>();
            foreach (int id in first.Factories.Keys) factoryIds.Add(id);
            foreach (int id in last.Factories.Keys) factoryIds.Add(id);
            foreach (int factoryId in factoryIds)
            {
                Dictionary<int, Counters> a;
                Dictionary<int, Counters> b;
                if (!first.Factories.TryGetValue(factoryId, out a)) a = new Dictionary<int, Counters>();
                if (!last.Factories.TryGetValue(factoryId, out b)) b = new Dictionary<int, Counters>();
                var row = new Dictionary<string, object>();
                row["factoryIndex"] = factoryId;
                int planetId;
                string planetName;
                if (last.PlanetIds.TryGetValue(factoryId, out planetId)) row["planetId"] = planetId;
                if (last.PlanetNames.TryGetValue(factoryId, out planetName)) row["planetName"] = planetName;
                row["items"] = ExportRates(a, b, seconds);
                rows.Add(row);
            }
            result["factories"] = rows;
            return result;
        }

        private static Dictionary<int, Counters> ReadCounters(object astroStat)
        {
            var result = new Dictionary<int, Counters>();
            foreach (object stat in Plugin.Enumerate(Plugin.GetMember(astroStat, "trafficPool")))
            {
                if (stat == null) continue;
                int itemId = Plugin.ToInt(Plugin.GetMember(stat, "itemId"));
                if (itemId <= 0) continue;
                long input;
                long output;
                long internalTraffic;
                if (!ReadTotals(Plugin.GetMember(stat, "total"), out input, out output, out internalTraffic))
                    continue;
                result[itemId] = new Counters { Input = input, Output = output, Internal = internalTraffic };
            }
            return result;
        }

        private static bool ReadTotals(object value, out long input, out long output, out long internalTraffic)
        {
            input = output = internalTraffic = 0L;
            bool a = false;
            bool b = false;
            bool c = false;
            int index = 0;
            foreach (object x in Plugin.Enumerate(value))
            {
                if (index == 6) { input = Plugin.ToLong(x); a = true; }
                else if (index == 13) { output = Plugin.ToLong(x); b = true; }
                else if (index == 20) { internalTraffic = Plugin.ToLong(x); c = true; break; }
                index++;
            }
            return a && b && c;
        }

        private static List<object> ExportRates(
            Dictionary<int, Counters> first,
            Dictionary<int, Counters> last,
            double seconds)
        {
            var rows = new List<object>();
            var ids = new SortedSet<int>();
            foreach (int id in first.Keys) ids.Add(id);
            foreach (int id in last.Keys) ids.Add(id);
            foreach (int id in ids)
            {
                Counters a;
                Counters b;
                if (!first.TryGetValue(id, out a)) a = new Counters();
                if (!last.TryGetValue(id, out b)) b = new Counters();
                long inputDelta = b.Input - a.Input;
                long outputDelta = b.Output - a.Output;
                long internalDelta = b.Internal - a.Internal;
                bool reset = inputDelta < 0 || outputDelta < 0 || internalDelta < 0;
                var row = new Dictionary<string, object> {
                    { "itemId", id },
                    { "name", Plugin.ItemName(id) },
                    { "counterReset", reset }
                };
                if (!reset && seconds >= MinimumWindowSeconds)
                {
                    row["inputPerMinute"] = Math.Round(inputDelta * 60.0 / seconds, 3);
                    row["outputPerMinute"] = Math.Round(outputDelta * 60.0 / seconds, 3);
                    row["internalPerMinute"] = Math.Round(internalDelta * 60.0 / seconds, 3);
                }
                rows.Add(row);
            }
            return rows;
        }
    }
}
