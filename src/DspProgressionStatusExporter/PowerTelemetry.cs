using System;
using System.Collections.Generic;
using System.Globalization;

namespace DspProgressionStatusExporter
{
    internal sealed class PowerTelemetry
    {
        private const int MaximumSamples = 25;

        private sealed class PlanetPower
        {
            public int PlanetId;
            public string PlanetName;
            public double Required;
            public double Served;
            public double Capacity;
        }

        private sealed class Sample
        {
            public DateTime AtUtc;
            public long GameTick;
            public Dictionary<int, PlanetPower> Planets = new Dictionary<int, PlanetPower>();
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

                var point = new Sample { AtUtc = DateTime.UtcNow, GameTick = gameTick };
                int factoryIndex = 0;
                foreach (object factory in Plugin.Enumerate(Plugin.GetMember(gameData, "factories")))
                {
                    if (factory != null)
                    {
                        object planet = Plugin.GetMember(factory, "planet");
                        int planetId = Plugin.ToInt(Plugin.GetMember(planet, "id", "planetId"));
                        object name = Plugin.GetMember(planet, "displayName", "name");
                        var power = new PlanetPower {
                            PlanetId = planetId,
                            PlanetName = name != null ? name.ToString() : null
                        };

                        object powerSystem = Plugin.GetMember(factory, "powerSystem");
                        foreach (object network in Plugin.Enumerate(Plugin.GetMember(powerSystem, "netPool")))
                        {
                            if (network == null || Plugin.ToInt(Plugin.GetMember(network, "id")) <= 0) continue;
                            power.Required += Plugin.ToDouble(Plugin.GetMember(network, "energyRequired"));
                            power.Served += Plugin.ToDouble(Plugin.GetMember(network, "energyServed"));
                            power.Capacity += Plugin.ToDouble(Plugin.GetMember(network, "energyCapacity"));
                        }
                        point.Planets[factoryIndex] = power;
                    }
                    factoryIndex++;
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
            result["source"] = "PlanetFactory.powerSystem.netPool live energyRequired/energyServed/energyCapacity";
            result["semantics"] = "Rolling power-service observations; ratios are derived from live network totals on each established factory planet.";
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
            result["sampledAtUtc"] = last.AtUtc.ToString("o", CultureInfo.InvariantCulture);
            result["windowGameTicks"] = last.GameTick - first.GameTick;
            result["windowGameSeconds"] = Math.Round((last.GameTick - first.GameTick) / 60.0, 3);

            var planetIds = new SortedSet<int>();
            foreach (Sample point in samples)
                foreach (int id in point.Planets.Keys) planetIds.Add(id);

            var rows = new List<object>();
            foreach (int id in planetIds)
            {
                int observations = 0;
                int undersupplied = 0;
                double satisfactionSum = 0.0;
                double minimumSatisfaction = 1.0;
                double maximumUtilization = 0.0;
                PlanetPower latest = null;

                foreach (Sample point in samples)
                {
                    PlanetPower power;
                    if (!point.Planets.TryGetValue(id, out power)) continue;
                    latest = power;
                    if (power.Required <= 0) continue;
                    double satisfaction = power.Served / power.Required;
                    double utilization = power.Capacity > 0 ? power.Required / power.Capacity : 0.0;
                    observations++;
                    satisfactionSum += satisfaction;
                    if (satisfaction < minimumSatisfaction) minimumSatisfaction = satisfaction;
                    if (utilization > maximumUtilization) maximumUtilization = utilization;
                    if (satisfaction < 0.99) undersupplied++;
                }

                if (latest == null) continue;
                var row = new Dictionary<string, object> {
                    { "factoryIndex", id },
                    { "planetId", latest.PlanetId },
                    { "planetName", latest.PlanetName },
                    { "observations", observations },
                    { "latestRequired", Math.Round(latest.Required, 3) },
                    { "latestServed", Math.Round(latest.Served, 3) },
                    { "latestCapacity", Math.Round(latest.Capacity, 3) },
                    { "latestDemandWatts", Math.Round(latest.Required * 60.0, 3) },
                    { "latestServedWatts", Math.Round(latest.Served * 60.0, 3) },
                    { "latestCapacityWatts", Math.Round(latest.Capacity * 60.0, 3) }
                };
                if (observations > 0)
                {
                    row["averageSatisfaction"] = Math.Round(satisfactionSum / observations, 6);
                    row["minimumSatisfaction"] = Math.Round(minimumSatisfaction, 6);
                    row["undersuppliedFraction"] = Math.Round(undersupplied * 1.0 / observations, 3);
                    row["maximumDemandToCapacity"] = Math.Round(maximumUtilization, 6);
                }
                rows.Add(row);
            }
            result["planets"] = rows;
            return result;
        }
    }
}
