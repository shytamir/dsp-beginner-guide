using System;
using System.Collections;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Rolling per-device evidence for Ray Receivers. A healthy frame is not
    /// continuity: every currently configured Photon Generation receiver must
    /// retain its mode, lens, exposure, strength, and warmup across the window.
    /// </summary>
    internal sealed class ReceiverTelemetry
    {
        private const int CriticalPhotonItemId = 1208;
        private const int GravitonLensItemId = 1209;
        private const double MinimumWindowSeconds = 60.0;
        private const double RetainedWindowSeconds = 65.0;
        private const int MinimumSamples = 10;

        private sealed class ReceiverSample
        {
            public long GameTick;
            public int PlanetId;
            public string PlanetName;
            public int EntityId;
            public int ProductId;
            public int CatalystId;
            public int CatalystPoints;
            public double Warmup;
            public double Strength;
            public double RequestedPowerWatts;
            public double SuppliedPowerWatts;
            public double CriticalPhotonOutputPerMinute;
        }

        private readonly Dictionary<string, Queue<ReceiverSample>> histories =
            new Dictionary<string, Queue<ReceiverSample>>();
        private readonly HashSet<string> currentKeys =
            new HashSet<string>();
        private object sampledGameData;
        private string lastFailure;
        private int deployedCount;

        public void Clear()
        {
            histories.Clear();
            currentKeys.Clear();
            sampledGameData = null;
            lastFailure = null;
            deployedCount = 0;
        }

        public void Sample(
            object gameData,
            object gameHistory,
            long gameTick)
        {
            try
            {
                if (!Object.ReferenceEquals(sampledGameData, gameData))
                {
                    histories.Clear();
                    currentKeys.Clear();
                    sampledGameData = gameData;
                }

                currentKeys.Clear();
                deployedCount = 0;
                foreach (object factory in Plugin.Enumerate(
                    Plugin.GetMember(gameData, "factories")))
                {
                    if (factory == null) continue;
                    object planet = Plugin.GetMember(factory, "planet");
                    int planetId = Plugin.ToInt(
                        Plugin.GetMember(planet, "id", "planetId"));
                    string planetName = Convert.ToString(
                        Plugin.GetMember(planet, "displayName", "name"));
                    object dysonSphere =
                        Plugin.GetMember(factory, "dysonSphere");
                    object sunDirection = Plugin.GetMember(
                        planet, "runtimeLocalSunDirection");
                    double sunX = Plugin.ToDouble(
                        Plugin.GetMember(sunDirection, "x"));
                    double sunY = Plugin.ToDouble(
                        Plugin.GetMember(sunDirection, "y"));
                    double sunZ = Plugin.ToDouble(
                        Plugin.GetMember(sunDirection, "z"));
                    double sunDistance = Plugin.ToDouble(
                        Plugin.GetMember(planet, "sunDistance"));
                    double grossRadius = Plugin.ToDouble(
                        Plugin.GetMember(dysonSphere, "grossRadius"));
                    double radiusRatio =
                        sunDistance > 0.0
                            ? grossRadius /
                                (sunDistance * 40000.0)
                            : 0.0;
                    double transmission =
                        1.0 - Plugin.ToDouble(
                            Plugin.GetMember(
                                gameHistory,
                                "solarEnergyLossRate"));

                    object power = Plugin.GetMember(factory, "powerSystem");
                    foreach (object component in Plugin.Enumerate(
                        Plugin.GetMember(power, "genPool")))
                    {
                        if (component == null) continue;
                        int entityId = Plugin.ToInt(
                            Plugin.GetMember(component, "entityId"));
                        if (entityId <= 0 ||
                            !Plugin.ToBool(
                                Plugin.GetMember(component, "gamma")))
                            continue;
                        deployedCount++;

                        double capacityPerTick = Plugin.ToDouble(
                            Plugin.GetMember(
                                component, "capacityCurrentTick"));
                        long productHeat = Plugin.ToLong(
                            Plugin.GetMember(component, "productHeat"));
                        double requestedPerTick = 0.0;
                        if (dysonSphere != null &&
                            sunDirection != null &&
                            radiusRatio > 0.0 &&
                            transmission > 0.0)
                            requestedPerTick = Plugin.ToDouble(
                                Plugin.TryInvoke(
                                    component,
                                    "EnergyCap_Gamma_Req",
                                    sunX,
                                    sunY,
                                    sunZ,
                                    radiusRatio,
                                    transmission));
                        var sample = new ReceiverSample {
                            GameTick = gameTick,
                            PlanetId = planetId,
                            PlanetName = planetName,
                            EntityId = entityId,
                            ProductId = Plugin.ToInt(
                                Plugin.GetMember(component, "productId")),
                            CatalystId = Plugin.ToInt(
                                Plugin.GetMember(component, "catalystId")),
                            CatalystPoints = Plugin.ToInt(
                                Plugin.GetMember(component, "catalystPoint")),
                            Warmup = Plugin.ToDouble(
                                Plugin.GetMember(component, "warmup")),
                            Strength = Plugin.ToDouble(
                                Plugin.GetMember(component, "currentStrength")),
                            RequestedPowerWatts =
                                requestedPerTick * 60.0,
                            SuppliedPowerWatts =
                                capacityPerTick * 60.0,
                            CriticalPhotonOutputPerMinute =
                                productHeat > 0 &&
                                Plugin.ToInt(
                                    Plugin.GetMember(
                                        component,
                                        "productId")) ==
                                    CriticalPhotonItemId
                                    ? capacityPerTick * 3600.0 /
                                        productHeat
                                    : 0.0
                        };
                        string key = Key(planetId, entityId);
                        currentKeys.Add(key);
                        Queue<ReceiverSample> history;
                        if (!histories.TryGetValue(key, out history))
                        {
                            history = new Queue<ReceiverSample>();
                            histories[key] = history;
                        }
                        if (history.Count > 0)
                        {
                            ReceiverSample previous = Last(history);
                            if (gameTick < previous.GameTick)
                                history.Clear();
                        }
                        history.Enqueue(sample);
                        while (history.Count > 1 &&
                            (gameTick - history.Peek().GameTick) / 60.0 >
                                RetainedWindowSeconds)
                            history.Dequeue();
                    }
                }
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
            result["available"] =
                sampledGameData != null &&
                String.IsNullOrEmpty(lastFailure);
            result["source"] =
                "Ray Receiver PowerGeneratorComponent rolling observations";
            result["minimumWindowSeconds"] = MinimumWindowSeconds;
            result["minimumSamples"] = MinimumSamples;
            result["deployedCount"] = deployedCount;
            if (!String.IsNullOrEmpty(lastFailure))
                result["lastFailure"] = lastFailure;

            int configured = 0;
            int lensed = 0;
            int fullStrength = 0;
            int continuousNow = 0;
            int sustained = 0;
            double requestedPower = 0.0;
            double suppliedPower = 0.0;
            double criticalPhotonOutput = 0.0;
            double maximumWindow = 0.0;
            var devices = new List<object>();

            var orderedKeys = new List<string>(currentKeys);
            orderedKeys.Sort(StringComparer.Ordinal);
            foreach (string key in orderedKeys)
            {
                Queue<ReceiverSample> history;
                if (!histories.TryGetValue(key, out history) ||
                    history.Count == 0)
                    continue;
                ReceiverSample latest = Last(history);
                ReceiverSample first = history.Peek();
                double seconds =
                    (latest.GameTick - first.GameTick) / 60.0;
                maximumWindow = Math.Max(maximumWindow, seconds);
                bool windowReady =
                    seconds >= MinimumWindowSeconds &&
                    history.Count >= MinimumSamples;
                bool configuredNow =
                    latest.ProductId == CriticalPhotonItemId;
                bool lensedNow =
                    configuredNow &&
                    latest.CatalystId == GravitonLensItemId &&
                    latest.CatalystPoints > 0;
                bool fullNow =
                    configuredNow && latest.Strength >= 0.999;
                bool continuous =
                    configuredNow && latest.Warmup >= 0.999;
                bool sustainedHealthy = windowReady;
                bool lensSustained = windowReady;
                double minimumWarmup = 1.0;
                double minimumStrength = 1.0;
                foreach (ReceiverSample sample in history)
                {
                    bool sampleConfigured =
                        sample.ProductId == CriticalPhotonItemId;
                    bool sampleLensed =
                        sampleConfigured &&
                        sample.CatalystId == GravitonLensItemId &&
                        sample.CatalystPoints > 0;
                    lensSustained = lensSustained && sampleLensed;
                    sustainedHealthy =
                        sustainedHealthy &&
                        sampleConfigured &&
                        sampleLensed &&
                        sample.Strength >= 0.999 &&
                        sample.Warmup >= 0.999;
                    minimumWarmup =
                        Math.Min(minimumWarmup, sample.Warmup);
                    minimumStrength =
                        Math.Min(minimumStrength, sample.Strength);
                }

                if (configuredNow) configured++;
                if (lensedNow) lensed++;
                if (fullNow) fullStrength++;
                if (continuous) continuousNow++;
                if (sustainedHealthy) sustained++;
                if (configuredNow)
                {
                    requestedPower +=
                        latest.RequestedPowerWatts;
                    suppliedPower += latest.SuppliedPowerWatts;
                    criticalPhotonOutput +=
                        latest.CriticalPhotonOutputPerMinute;
                }

                devices.Add(new Dictionary<string, object> {
                    { "planetId", latest.PlanetId },
                    { "planetName", latest.PlanetName },
                    { "entityId", latest.EntityId },
                    { "sampleCount", history.Count },
                    { "windowSeconds", Math.Round(seconds, 3) },
                    { "windowReady", windowReady },
                    { "configuredForPhotonGeneration", configuredNow },
                    { "productId", latest.ProductId },
                    { "lensedNow", lensedNow },
                    { "lensSustained", lensSustained },
                    { "catalystId", latest.CatalystId },
                    { "catalystPoints", latest.CatalystPoints },
                    { "continuousReceivingNow", continuous },
                    { "warmupNow", latest.Warmup },
                    { "minimumWarmup", minimumWarmup },
                    { "fullStrengthNow", fullNow },
                    { "strengthNow", latest.Strength },
                    { "minimumStrength", minimumStrength },
                    { "sustainedHealthy", sustainedHealthy },
                    { "requestedDysonPowerWatts", latest.RequestedPowerWatts },
                    { "suppliedPowerWatts", latest.SuppliedPowerWatts },
                    { "criticalPhotonOutputPerMinute", latest.CriticalPhotonOutputPerMinute }
                });
            }

            result["configuredPhotonCount"] = configured;
            result["lensedPhotonCount"] = lensed;
            result["fullStrengthPhotonCount"] = fullStrength;
            result["continuousReceivingPhotonCount"] = continuousNow;
            result["sustainedPhotonCount"] = sustained;
            result["arrayRequestedDysonPowerWatts"] =
                requestedPower;
            result["arraySuppliedPowerWatts"] = suppliedPower;
            result["arrayCriticalPhotonOutputPerMinute"] =
                criticalPhotonOutput;
            result["maximumWindowSeconds"] =
                Math.Round(maximumWindow, 3);
            result["devices"] = devices;
            result["semantics"] =
                "Sustained health requires every retained sample across at least 60 game-seconds to remain in Photon Generation mode, lensed, fully exposed, full-strength, and fully warmed.";
            return result;
        }

        private static string Key(int planetId, int entityId)
        {
            return planetId + ":" + entityId;
        }

        private static ReceiverSample Last(
            Queue<ReceiverSample> samples)
        {
            ReceiverSample last = null;
            foreach (ReceiverSample sample in samples)
                last = sample;
            return last;
        }
    }
}
