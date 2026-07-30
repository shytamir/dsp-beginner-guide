using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DspProgressionStatusExporter
{
    [BepInPlugin(
        "local.dsp.progressionstatusexporter",
        "DSP Guide Check",
        BuildVersion.PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginVersion = BuildVersion.PluginVersion;
        private const string SchemaVersion = "2.0";
        private const float TelemetryIntervalSeconds = 5f;
        private const float PanelRefreshIntervalSeconds = 15f;
        private static ManualLogSource Log;
        private ConfigEntry<KeyboardShortcut> snapshotKey;
        private ConfigEntry<bool> includeDiagnostics;

        private static Type gameMainType;
        private static Type ldbType;
        private static readonly Dictionary<int, string> ItemNames = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> TechNames = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> RecipeNames = new Dictionary<int, string>();

        // Runtime-derived Phase 1 bundle authoritative prototype IDs.
        private const int ItemIdRayReceiver = 2208;
        private const int ItemIdEmRailEjector = 2311;
        private const int ItemIdVerticalLaunchingSilo = 2312;

        private sealed class DysonPopulationSample
        {
            public DateTime AtUtc;
            public double SailCount;
            public string MemberName;
        }

        private sealed class DysonConstructionSample
        {
            public DateTime AtUtc;
            public double ConstructedStructurePoints;
            public double ConstructedCellPoints;
            public double PermanentGenerationWatts;
        }

        private readonly Dictionary<int, Queue<DysonPopulationSample>> dysonPopulationSamples = new Dictionary<int, Queue<DysonPopulationSample>>();
        private readonly Dictionary<int, Queue<DysonConstructionSample>> dysonConstructionSamples =
            new Dictionary<int, Queue<DysonConstructionSample>>();
        private readonly Dictionary<int, Dictionary<string, object>>
            dysonTopologyCache =
                new Dictionary<int, Dictionary<string, object>>();
        private object dysonSampleData;
        private readonly ProductionTelemetry productionTelemetry = new ProductionTelemetry();
        private readonly TrafficTelemetry trafficTelemetry = new TrafficTelemetry();
        private readonly PowerTelemetry powerTelemetry = new PowerTelemetry();
        private readonly ReceiverTelemetry receiverTelemetry =
            new ReceiverTelemetry();
        private float nextTelemetryCycleAt;
        private bool telemetryCycleActive;
        private int telemetryStage;
        private readonly float[] lastTelemetryStageMs = new float[4];
        private readonly float[] maximumTelemetryStageMs = new float[4];
        private readonly GuidePanelController guidePanel =
            new GuidePanelController();
        private float nextGuidePanelRefreshAt;
        private Coroutine guidePanelRefreshCoroutine;
        private string lastSnapshotFileName;
        private string lastSnapshotDirectory;
        private string activePhaseSaveKey;
        private ConfigEntry<string> activePhaseSelectionEntry;
        private ManualPhaseSelection activePhaseSelection;

        private void Awake()
        {
            Log = Logger;
            snapshotKey = Config.Bind(
                "General",
                "SnapshotKey",
                new KeyboardShortcut(KeyCode.F8),
                "Press while playing to open or close the Guide Check panel. Snapshots are saved from the panel footer."
            );
            includeDiagnostics = Config.Bind(
                "General",
                "IncludeDiagnostics",
                true,
                "Include compact collector performance timings in saved snapshots. Recommended: true."
            );

            gameMainType = FindType("GameMain");
            ldbType = FindType("LDB");
            guidePanel.SetSnapshotAction(SaveSnapshotFromPanel);
            guidePanel.SetNavigationAction(HandleGuideNavigation);

            Log.LogInfo("DSP Guide Check loaded. Press " + snapshotKey.Value + " while playing.");
        }

        private void Update()
        {
            try
            {
                guidePanel.Tick(Time.unscaledDeltaTime);
                bool sampledThisFrame = false;
                if (!telemetryCycleActive &&
                    Time.realtimeSinceStartup >= nextTelemetryCycleAt)
                {
                    nextTelemetryCycleAt =
                        Time.realtimeSinceStartup + TelemetryIntervalSeconds;
                    telemetryCycleActive = true;
                    telemetryStage = 0;
                }
                if (telemetryCycleActive)
                    sampledThisFrame = SampleOneTelemetryStage();

                if (!sampledThisFrame &&
                    guidePanel.IsVisible &&
                    guidePanelRefreshCoroutine == null &&
                    Time.realtimeSinceStartup >= nextGuidePanelRefreshAt)
                {
                    object data = GetStatic(gameMainType, "data");
                    if (data == null)
                        guidePanel.Hide();
                    else
                    {
                        nextGuidePanelRefreshAt =
                            Time.realtimeSinceStartup +
                            PanelRefreshIntervalSeconds;
                        guidePanelRefreshCoroutine =
                            StartCoroutine(RefreshGuidePanelStaged(data));
                    }
                }

                if (snapshotKey.Value.IsDown())
                {
                    ToggleGuidePanel();
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Status exporter Update failed: " + ex);
            }
        }

        private void ToggleGuidePanel()
        {
            if (guidePanel.IsVisible)
            {
                if (guidePanelRefreshCoroutine != null)
                {
                    StopCoroutine(guidePanelRefreshCoroutine);
                    guidePanelRefreshCoroutine = null;
                }
                guidePanel.Hide();
                return;
            }

            guidePanel.Prepare();
            object data = GetStatic(gameMainType, "data");
            object player = GetStatic(gameMainType, "mainPlayer");
            GuidePanelModel model = BuildLiveGuidePanelModel(data, player);
            if (model == null) return;
            guidePanel.Show(model);
            nextGuidePanelRefreshAt =
                Time.realtimeSinceStartup + PanelRefreshIntervalSeconds;
        }

        private bool SampleOneTelemetryStage()
        {
            object data = GetStatic(gameMainType, "data");
            if (data == null)
            {
                telemetryCycleActive = false;
                if (guidePanel.IsVisible) guidePanel.Hide();
                return false;
            }
            long gameTick = ToLong(GetStatic(gameMainType, "gameTick"));
            int stage = telemetryStage;
            float started = Time.realtimeSinceStartup;
            if (stage == 0)
            {
                SampleDysonPopulation();
                receiverTelemetry.Sample(
                    data,
                    GetStatic(gameMainType, "history"),
                    gameTick);
            }
            else if (stage == 1)
                productionTelemetry.Sample(data, gameTick);
            else if (stage == 2)
                trafficTelemetry.SampleNow(data, gameTick);
            else
                powerTelemetry.SampleNow(data, gameTick);
            float elapsed =
                (Time.realtimeSinceStartup - started) * 1000f;
            lastTelemetryStageMs[stage] = elapsed;
            if (elapsed > maximumTelemetryStageMs[stage])
                maximumTelemetryStageMs[stage] = elapsed;

            telemetryStage++;
            if (telemetryStage >= 4)
                telemetryCycleActive = false;
            return true;
        }

        private GuidePanelModel ExportSnapshot()
        {
            try
            {
                if (gameMainType == null) gameMainType = FindType("GameMain");
                if (ldbType == null) ldbType = FindType("LDB");

                object data = GetStatic(gameMainType, "data");
                object player = GetStatic(gameMainType, "mainPlayer");

                if (data == null || player == null)
                {
                    Log.LogWarning("No active DSP game detected. Load a save, then press " + snapshotKey.Value + ".");
                    TryPopup("No active game detected.");
                    return null;
                }

                BuildProtoNameCaches();
                string outDir = Path.Combine(Paths.BepInExRootPath, "DSP-Status");
                Directory.CreateDirectory(outDir);
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                string planet = SafeFileName(ToStr(GetStatic(gameMainType, "localPlanet") != null
                    ? GetMember(GetStatic(gameMainType, "localPlanet"), "displayName", "name")
                    : null));
                if (String.IsNullOrEmpty(planet)) planet = "UnknownPlanet";
                string path = Path.Combine(outDir, "DSP-Status-" + stamp + "-" + planet + ".json");
                lastSnapshotFileName = Path.GetFileName(path);
                lastSnapshotDirectory = outDir;

                var live = new Dictionary<string, object>();
                Dictionary<string, object> research = ExportResearch();
                live["research"] = research;
                live["player"] = ExportPlayer(player);
                live["factories"] = ExportFactories(data);
                live["ownedInventorySummary"] =
                    ExportOwnedInventorySummary(data, player);
                live["dyson"] = ExportDyson(data);
                live["progressionSummary"] =
                    ExportProgressionSummary(data, player);
                Dictionary<string, object> production = productionTelemetry.Export();
                Dictionary<string, object> traffic = trafficTelemetry.Export();
                Dictionary<string, object> powerTelemetryExport = powerTelemetry.Export();
                Dictionary<string, object> recipes = RecipeTelemetry.Export(data);
                ObservedGameState observedState =
                    ObservedGameState.Build(
                        live, production, traffic,
                        powerTelemetryExport, recipes);
                ManualPhaseSelection selection =
                    EnsurePhaseSelection(data, observedState);
                Dictionary<string, object> guideAnalysis =
                    GuideAnalyzer.AnalyzeSelected(
                        observedState, selection.PhaseId);
                GuidePanelModel panelModel =
                    GuidePanelModelBuilder.Build(
                        guideAnalysis,
                        lastSnapshotFileName,
                        lastSnapshotDirectory);
                panelModel.SelectedLateRoute = selection.LateRoute;
                Dictionary<string, object> snapshot =
                    CompactSnapshotBuilder.Build(
                        SchemaVersion,
                        PluginVersion,
                        ExportSnapshotProvenance(),
                        ExportCompactGameInfo(data),
                        ExportCompactLocation(),
                        research,
                        observedState,
                        selection.Export(activePhaseSaveKey),
                        guideAnalysis,
                        ExportSamplingPerformance(),
                        includeDiagnostics.Value);
                string json = Json.Stringify(snapshot);
                int byteCount = Encoding.UTF8.GetByteCount(json);
                if (byteCount > 262144)
                    throw new InvalidOperationException(
                        "Compact snapshot exceeded 256 KiB (" +
                        byteCount.ToString(CultureInfo.InvariantCulture) +
                        " bytes).");
                File.WriteAllText(
                    path, json, new UTF8Encoding(false));

                Log.LogInfo("DSP progression status exported: " + path);
                return panelModel;
            }
            catch (Exception ex)
            {
                Log.LogError("DSP progression status export failed: " + ex);
                TryPopup("DSP status export failed. Check BepInEx LogOutput.log.");
                return null;
            }
        }

        private string SaveSnapshotFromPanel()
        {
            GuidePanelModel model = ExportSnapshot();
            if (model != null) guidePanel.UpdateModel(model);
            return model != null ? lastSnapshotDirectory : null;
        }

        private GuidePanelModel BuildLiveGuidePanelModel(
            object data,
            object player)
        {
            try
            {
                if (data == null || player == null)
                {
                    TryPopup("No active game detected.");
                    return null;
                }
                BuildProtoNameCaches();
                var live = new Dictionary<string, object>();
                live["research"] = ExportResearch();
                live["player"] = ExportPlayer(player);
                live["factories"] = ExportFactories(data);
                live["ownedInventorySummary"] =
                    ExportOwnedInventorySummary(data, player);
                live["dyson"] = ExportDyson(data);
                live["progressionSummary"] =
                    ExportProgressionSummary(data, player);

                Dictionary<string, object> production =
                    productionTelemetry.Export();
                Dictionary<string, object> traffic =
                    trafficTelemetry.Export();
                Dictionary<string, object> power = powerTelemetry.Export();
                Dictionary<string, object> recipes = RecipeTelemetry.Export(data);
                ObservedGameState observed =
                    ObservedGameState.Build(live, production, traffic, power, recipes);
                ManualPhaseSelection selection =
                    EnsurePhaseSelection(data, observed);
                Dictionary<string, object> analysis =
                    GuideAnalyzer.AnalyzeSelected(
                        observed, selection.PhaseId);
                GuidePanelModel model = GuidePanelModelBuilder.Build(
                    analysis,
                    lastSnapshotFileName,
                    lastSnapshotDirectory);
                model.SelectedLateRoute = selection.LateRoute;
                return model;
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check analysis failed: " + ex);
                return null;
            }
        }

        private IEnumerator RefreshGuidePanelStaged(object data)
        {
            // Let StartCoroutine return before any early-exit path clears the
            // active handle, and keep collector work off the scheduling frame.
            yield return null;
            object player = GetStatic(gameMainType, "mainPlayer");
            if (data == null || player == null)
            {
                guidePanel.Hide();
                guidePanelRefreshCoroutine = null;
                yield break;
            }

            var live = new Dictionary<string, object>();
            Dictionary<string, object> production = null;
            Dictionary<string, object> traffic = null;
            Dictionary<string, object> power = null;
            Dictionary<string, object> recipes = null;
            ObservedGameState observed = null;
            try
            {
                BuildProtoNameCaches();
                live["research"] = ExportResearch();
                live["player"] = ExportPlayer(player);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check research refresh failed: " + ex);
                guidePanelRefreshCoroutine = null;
                yield break;
            }
            yield return null;

            try
            {
                live["factories"] = ExportFactories(data);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check factory refresh failed: " + ex);
                guidePanelRefreshCoroutine = null;
                yield break;
            }
            yield return null;

            try
            {
                live["ownedInventorySummary"] =
                    ExportOwnedInventorySummary(data, player);
                live["dyson"] = ExportDyson(data);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check inventory refresh failed: " + ex);
                guidePanelRefreshCoroutine = null;
                yield break;
            }
            yield return null;

            try
            {
                live["progressionSummary"] =
                    ExportProgressionSummary(data, player);
                production = productionTelemetry.Export();
                traffic = trafficTelemetry.Export();
                power = powerTelemetry.Export();
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check telemetry refresh failed: " + ex);
                guidePanelRefreshCoroutine = null;
                yield break;
            }
            yield return null;

            try
            {
                recipes = RecipeTelemetry.Export(data);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check recipe refresh failed: " + ex);
                guidePanelRefreshCoroutine = null;
                yield break;
            }
            yield return null;

            try
            {
                observed = ObservedGameState.Build(
                    live, production, traffic, power, recipes);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check normalization failed: " + ex);
                guidePanelRefreshCoroutine = null;
                yield break;
            }
            yield return null;

            try
            {
                Dictionary<string, object> analysis =
                    GuideAnalyzer.AnalyzeSelected(
                        observed,
                        EnsurePhaseSelection(data, observed).PhaseId);
                GuidePanelModel model =
                    GuidePanelModelBuilder.Build(
                        analysis,
                        lastSnapshotFileName,
                        lastSnapshotDirectory);
                model.SelectedLateRoute =
                    activePhaseSelection != null
                        ? activePhaseSelection.LateRoute
                        : null;
                if (guidePanel.IsVisible)
                    guidePanel.UpdateModel(model);
            }
            catch (Exception ex)
            {
                Log.LogWarning("Guide Check analysis refresh failed: " + ex);
            }
            guidePanelRefreshCoroutine = null;
        }

        private ManualPhaseSelection EnsurePhaseSelection(
            object data,
            ObservedGameState observed)
        {
            PhaseSaveIdentity identity = BuildPhaseSaveIdentity(data);
            string saveKey = identity.SaveKey;
            if (!String.Equals(
                saveKey,
                activePhaseSaveKey,
                StringComparison.Ordinal))
            {
                activePhaseSaveKey = saveKey;
                activePhaseSelectionEntry = Config.Bind(
                    "Phase Selection",
                    saveKey,
                    "",
                    "Player-selected Guide Check phase for this playthrough. Managed by the in-game phase controls.");
                activePhaseSelection = ManualPhaseSelection.Parse(
                    activePhaseSelectionEntry.Value);
                if (activePhaseSelection != null)
                {
                    activePhaseSelection.IdentityVersion = identity.Version;
                    activePhaseSelection.PersistenceState =
                        "restored-stable-key";
                }
                else
                {
                    string legacyKey = BuildLegacyPhaseSaveKey(data);
                    ConfigEntry<string> legacyEntry = Config.Bind(
                        "Phase Selection",
                        legacyKey,
                        "",
                        "Legacy Guide Check phase selection retained for one-time migration.");
                    activePhaseSelection = ManualPhaseSelection.Parse(
                        legacyEntry.Value);
                    if (activePhaseSelection != null)
                    {
                        activePhaseSelection.IdentityVersion =
                            identity.Version;
                        activePhaseSelection.PersistenceState =
                            "migrated-current-legacy-key";
                        PersistPhaseSelection();
                    }
                }
            }

            if (activePhaseSelection == null)
            {
                HashSet<int> unlocked = observed != null
                    ? observed.UnlockedTechIds
                    : ReadCubeResearch();
                activePhaseSelection =
                    ManualPhaseNavigator.Seed(unlocked);
                activePhaseSelection.IdentityVersion = identity.Version;
                activePhaseSelection.PersistenceState =
                    identity.Stable
                        ? "seeded-stable-key"
                        : "seeded-fallback-key";
                PersistPhaseSelection();
            }
            return activePhaseSelection;
        }

        private void PersistPhaseSelection()
        {
            if (activePhaseSelectionEntry == null ||
                activePhaseSelection == null)
                return;
            activePhaseSelectionEntry.Value =
                activePhaseSelection.Serialize();
            Config.Save();
        }

        private void HandleGuideNavigation(string command)
        {
            object data = GetStatic(gameMainType, "data");
            object player = GetStatic(gameMainType, "mainPlayer");
            if (data == null || player == null) return;

            if (guidePanelRefreshCoroutine != null)
            {
                StopCoroutine(guidePanelRefreshCoroutine);
                guidePanelRefreshCoroutine = null;
            }

            ManualPhaseSelection selection =
                EnsurePhaseSelection(data, null);
            string current =
                ManualPhaseNavigator.NormalizePhase(selection.PhaseId);
            string target = current;
            if (String.Equals(
                command, "previous", StringComparison.OrdinalIgnoreCase))
            {
                target = ManualPhaseNavigator.Previous(
                    current, selection.LateRoute);
            }
            else if (String.Equals(
                command, "next", StringComparison.OrdinalIgnoreCase))
            {
                target = ManualPhaseNavigator.Next(
                    current, selection.LateRoute);
            }
            else if (String.Equals(
                command, "warp", StringComparison.OrdinalIgnoreCase) &&
                current == "purple")
            {
                target = "warp";
            }
            else if ((String.Equals(
                    command, "dyson", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(
                    command, "sphere", StringComparison.OrdinalIgnoreCase)) &&
                current == "green")
            {
                selection.LateRoute =
                    command.ToLowerInvariant();
                target = selection.LateRoute;
            }

            if (!String.Equals(
                current, target, StringComparison.OrdinalIgnoreCase))
            {
                selection.PhaseId = target;
                selection.SeedSource = "manual-control";
                selection.PersistenceState = "updated-by-player";
                PersistPhaseSelection();
                GuidePanelModel model =
                    BuildLiveGuidePanelModel(data, player);
                if (model != null && guidePanel.IsVisible)
                    guidePanel.UpdateModel(model);
            }
            nextGuidePanelRefreshAt =
                Time.realtimeSinceStartup +
                PanelRefreshIntervalSeconds;
        }

        private static HashSet<int> ReadCubeResearch()
        {
            var unlocked = new HashSet<int>();
            object history = GetStatic(gameMainType, "history");
            int[] ids = { 1002, 1111, 1124, 1312, 1705, 1507, 1508 };
            foreach (int id in ids)
            {
                object value = TryInvoke(history, "TechUnlocked", id);
                if (!(value is bool))
                    value = TryInvoke(
                        history, "TechUnlocked", id, false);
                if (value is bool && (bool)value)
                    unlocked.Add(id);
            }
            return unlocked;
        }

        private static PhaseSaveIdentity BuildPhaseSaveIdentity(object data)
        {
            object desc = GetMember(data, "gameDesc");
            return PhaseSaveIdentity.Build(
                PhaseIdentityValue(GetMember(desc, "creationTime")),
                PhaseIdentityValue(GetMember(
                    desc, "galaxySeed", "seed")),
                PhaseIdentityValue(GetMember(desc, "starCount")),
                PhaseIdentityValue(GetMember(
                    desc, "sandboxMode", "isSandboxMode")),
                ToStr(GetMember(
                    data, "gameName", "saveName", "name")));
        }

        private static string BuildLegacyPhaseSaveKey(object data)
        {
            object desc = GetMember(data, "gameDesc");
            Type gameSaveType = FindType("GameSave");
            return PhaseSaveIdentity.BuildLegacyKey(
                ToStr(GetMember(
                    data, "gameName", "saveName", "name")),
                ToStr(GetStatic(
                    gameSaveType,
                    "saveName",
                    "lastSaveName",
                    "currentSaveName")),
                ToStr(GetMember(desc, "galaxySeed", "seed")),
                ToStr(GetMember(desc, "starCount")),
                ToStr(GetMember(
                    desc, "sandboxMode", "isSandboxMode")));
        }

        private static string PhaseIdentityValue(object value)
        {
            if (value == null) return null;
            if (value is DateTime)
            {
                return ((DateTime)value).Ticks.ToString(
                    CultureInfo.InvariantCulture);
            }
            IFormattable formattable = value as IFormattable;
            return formattable != null
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
        }

        private Dictionary<string, object> ExportSamplingPerformance()
        {
            string[] names = {
                "dysonAndReceivers",
                "production",
                "traffic",
                "power"
            };
            var stages = new List<object>();
            for (int i = 0; i < names.Length; i++)
                stages.Add(new Dictionary<string, object> {
                    { "stage", names[i] },
                    { "lastMilliseconds", Math.Round(lastTelemetryStageMs[i], 3) },
                    { "maximumMilliseconds", Math.Round(maximumTelemetryStageMs[i], 3) }
                });
            return new Dictionary<string, object> {
                { "stages", stages },
                { "note", "Wall-clock timings measured around each staggered main-thread collection pass." }
            };
        }

        private void OnDestroy()
        {
            if (guidePanelRefreshCoroutine != null)
                StopCoroutine(guidePanelRefreshCoroutine);
            guidePanel.Destroy();
        }

        // --------------------------------------------------------------------
        // Top-level export sections
        // --------------------------------------------------------------------

        private static Dictionary<string, object> ExportRuntimeInfo()
        {
            var d = new Dictionary<string, object>();
            Assembly asm = gameMainType != null ? gameMainType.Assembly : null;

            d["unityVersion"] = Application.unityVersion;
            d["gameAssemblyFullName"] = asm != null ? asm.FullName : null;
            d["gameAssemblyLocation"] = asm != null ? asm.Location : null;
            d["bepInExRoot"] = Paths.BepInExRootPath;

            Type gameConfig = FindType("GameConfig");
            d["gameVersion"] = GetStatic(gameConfig, "gameVersion", "version", "gameVersionString");

            return d;
        }

        private static Dictionary<string, object>
            ExportSnapshotProvenance()
        {
            Type gameConfig = FindType("GameConfig");
            return new Dictionary<string, object> {
                { "pluginId", "local.dsp.progressionstatusexporter" },
                { "pluginVersion", PluginVersion },
                { "assemblyVersion", typeof(Plugin).Assembly
                    .GetName().Version.ToString() },
                { "snapshotSchemaVersion", SchemaVersion },
                { "unityVersion", Application.unityVersion },
                { "gameVersion", GetStatic(
                    gameConfig, "gameVersion", "version",
                    "gameVersionString") }
            };
        }

        private static Dictionary<string, object>
            ExportCompactGameInfo(object data)
        {
            object desc = GetMember(data, "gameDesc");
            long gameTick = ToLong(GetStatic(gameMainType, "gameTick"));
            return new Dictionary<string, object> {
                { "gameTick", gameTick },
                { "totalPlayTimeSeconds", gameTick / 60.0 },
                { "galaxySeed", Scalar(GetMember(
                    desc, "galaxySeed", "seed")) },
                { "starCount", Scalar(GetMember(desc, "starCount")) },
                { "resourceMultiplier", Scalar(GetMember(
                    desc, "resourceMultiplier")) },
                { "sandboxMode", Scalar(GetMember(
                    desc, "sandboxMode", "isSandboxMode")) },
                { "peaceMode", Scalar(GetMember(desc, "peaceMode")) }
            };
        }

        private static Dictionary<string, object>
            ExportCompactLocation()
        {
            return new Dictionary<string, object> {
                { "planet", ExportCelestialIdentity(
                    GetStatic(gameMainType, "localPlanet")) },
                { "star", ExportCelestialIdentity(
                    GetStatic(gameMainType, "localStar")) }
            };
        }

        private static Dictionary<string, object> ExportGameInfo(object data)
        {
            var d = new Dictionary<string, object>();
            object desc = GetMember(data, "gameDesc");

            d["gameTick"] = Scalar(GetMember(data, "gameTick"));
            d["gameTime"] = Scalar(GetMember(data, "gameTime", "time"));
            d["galaxySeed"] = Scalar(GetMember(desc, "galaxySeed", "seed"));
            d["starCount"] = Scalar(GetMember(desc, "starCount"));
            d["resourceMultiplier"] = Scalar(GetMember(desc, "resourceMultiplier"));
            d["sandboxMode"] = Scalar(GetMember(desc, "sandboxMode", "isSandboxMode"));
            d["peaceMode"] = Scalar(GetMember(desc, "peaceMode"));
            d["combatSettings"] = ExportScalarObject(GetMember(desc, "combatSettings"), 2, null);

            return d;
        }

        private static Dictionary<string, object> ExportLocation(object player)
        {
            var d = new Dictionary<string, object>();

            object planet = GetStatic(gameMainType, "localPlanet");
            object star = GetStatic(gameMainType, "localStar");

            d["planet"] = ExportCelestialIdentity(planet);
            d["star"] = ExportCelestialIdentity(star);
            d["playerPlanetId"] = Scalar(GetMember(player, "planetId"));
            d["playerStarId"] = Scalar(GetMember(player, "starId"));
            d["playerPosition"] = ToStr(GetMember(player, "position", "uPosition"));

            return d;
        }

        private static Dictionary<string, object> ExportResearch()
        {
            var result = new Dictionary<string, object>();
            object history = GetStatic(gameMainType, "history");
            if (history == null)
            {
                result["available"] = false;
                return result;
            }

            result["available"] = true;

            object techSet = GetStatic(ldbType, "techs");
            object dataArray = GetMember(techSet, "dataArray");

            var techRows = new List<object>();
            foreach (object proto in Enumerate(dataArray))
            {
                if (proto == null) continue;
                int id = ToInt(GetMember(proto, "ID"));
                if (id <= 0) continue;

                var row = new Dictionary<string, object>();
                row["id"] = id;
                row["name"] = ProtoName(proto);

                object unlocked = TryInvoke(history, "TechUnlocked", id);
                if (!(unlocked is bool))
                    unlocked = TryInvoke(history, "TechUnlocked", id, false);
                row["unlocked"] = unlocked is bool ? unlocked : null;

                object state = DictionaryLookup(GetMember(history, "techStates"), id);
                if (state != null)
                {
                    row["state"] = ExportNamedMembers(
                        state,
                        new string[] {
                            "curLevel", "currentLevel", "level", "maxLevel",
                            "hashUploaded", "hashNeeded", "uHashUploaded",
                            "unlocked", "isUnlocked"
                        }
                    );
                }

                techRows.Add(row);
            }

            result["technologies"] = techRows;
            result["currentTech"] = Scalar(GetMember(history, "currentTech", "currentTechId"));
            result["techQueue"] = ExportSimpleSequence(GetMember(history, "techQueue", "techQueueArray"));
            result["universeObserveLevel"] = Scalar(GetMember(history, "universeObserveLevel"));
            result["missionAccomplished"] = Scalar(GetMember(history, "missionAccomplished"));

            result["capabilityMetrics"] = ExportScalarObject(
                history,
                1,
                new string[] {
                    "tech", "universe", "observe", "mining", "research", "hash",
                    "logistic", "drone", "ship", "vessel", "courier", "storage",
                    "sorter", "stack", "blueprint", "construction", "solar",
                    "ray", "dyson", "warp", "sail", "walk", "core", "inventory",
                    "package", "shield", "fleet", "damage"
                }
            );

            return result;
        }

        private static Dictionary<string, object> ExportPlayer(object player)
        {
            var d = new Dictionary<string, object>();
            object mecha = GetMember(player, "mecha");

            d["identity"] = ExportNamedMembers(
                player,
                new string[] { "id", "planetId", "starId", "sandCount", "soilPile" }
            );

            d["mecha"] = ExportNamedMembers(
                mecha,
                new string[] {
                    "coreEnergy", "coreEnergyCap", "reactorPowerGen",
                    "walkSpeed", "maxSailSpeed", "maxWarpSpeed",
                    "thrusterLevel", "warpState", "warpSpeed",
                    "droneCount", "droneSpeed", "droneMovement",
                    "buildArea", "replicateSpeed", "inventorySize"
                }
            );

            d["mechaScalars"] = ExportScalarObject(
                mecha,
                1,
                new string[] {
                    "energy", "core", "reactor", "fuel", "walk", "sail", "warp",
                    "thruster", "drone", "build", "replicate", "inventory", "package",
                    "shield", "hp", "fleet"
                }
            );

            d["inventory"] = ExportStorage(GetMember(player, "package", "packageStorage"));
            d["deliveryPackage"] = ExportStorage(GetMember(player, "deliveryPackage"));
            d["fuelChamber"] = ExportStorage(
                GetMember(mecha, "reactorStorage", "fuelStorage", "fuelChamber", "reactor")
            );

            object replicator = GetMember(player, "replicator");
            d["replicator"] = ExportScalarObject(
                replicator,
                1,
                new string[] { "task", "queue", "time", "recipe", "count" }
            );

            return d;
        }

        private static List<object> ExportFactories(object data)
        {
            var rows = new List<object>();
            object factories = GetMember(data, "factories");

            foreach (object factory in Enumerate(factories))
            {
                if (factory == null) continue;

                var row = new Dictionary<string, object>();
                object planet = GetMember(factory, "planet");
                row["planet"] = ExportCelestialIdentity(planet);

                var buildingCounts = CountFactoryEntities(factory);
                row["buildingCounts"] = NamedCountRows(buildingCounts);

                row["ownedStorage"] = ExportOwnedStorage(factory);
                row["logistics"] = ExportLogistics(factory);
                row["power"] = ExportPower(factory);
                row["production"] = ExportProduction(factory);
                row["enemy"] = ExportEnemySummary(factory);

                rows.Add(row);
            }

            return rows;
        }

        private static Dictionary<string, object> ExportExploration(object data)
        {
            var d = new Dictionary<string, object>();
            object history = GetStatic(gameMainType, "history");
            d["universeObserveLevel"] = Scalar(GetMember(history, "universeObserveLevel"));

            var visited = new List<object>();
            object factories = GetMember(data, "factories");
            var seen = new HashSet<int>();

            foreach (object factory in Enumerate(factories))
            {
                if (factory == null) continue;
                object planet = GetMember(factory, "planet");
                int id = ToInt(GetMember(planet, "id", "planetId"));
                if (id > 0 && seen.Add(id))
                    visited.Add(ExportCelestialIdentity(planet));
            }

            object localPlanet = GetStatic(gameMainType, "localPlanet");
            if (localPlanet != null)
            {
                int id = ToInt(GetMember(localPlanet, "id", "planetId"));
                if (id > 0 && seen.Add(id))
                    visited.Add(ExportCelestialIdentity(localPlanet));
            }

            d["visitedOrFactoryPlanets"] = visited;
            d["note"] = "Exporter intentionally does not reveal resource data for unvisited/unobserved planets.";

            return d;
        }

        private void SampleDysonPopulation()
        {
            object data = GetStatic(gameMainType, "data");
            if (data == null) return;
            ResetDysonSamplingFor(data);

            string sphereMemberName;
            object spheres = FindEnumerableMember(data, new string[] { "dyson" }, out sphereMemberName);
            int slotIndex = 0;
            DateTime now = DateTime.UtcNow;

            foreach (object sphere in Enumerate(spheres))
            {
                if (sphere != null)
                {
                    dysonTopologyCache[slotIndex] =
                        ExportDysonTopology(sphere);
                    Queue<DysonConstructionSample> constructionSamples;
                    if (!dysonConstructionSamples.TryGetValue(
                        slotIndex, out constructionSamples))
                    {
                        constructionSamples =
                            new Queue<DysonConstructionSample>();
                        dysonConstructionSamples[slotIndex] =
                            constructionSamples;
                    }
                    constructionSamples.Enqueue(
                        new DysonConstructionSample {
                            AtUtc = now,
                            ConstructedStructurePoints = ToDouble(
                                GetMember(
                                    sphere,
                                    "totalConstructedStructurePoint")),
                            ConstructedCellPoints = ToDouble(
                                GetMember(
                                    sphere,
                                    "totalConstructedCellPoint")),
                            PermanentGenerationWatts = ToDouble(
                                GetMember(
                                    sphere,
                                    "energyGenCurrentTick_Layers")) * 60.0
                        });
                    while (constructionSamples.Count > 0 &&
                        (now - constructionSamples.Peek().AtUtc)
                            .TotalSeconds > 125.0)
                        constructionSamples.Dequeue();

                    string swarmMemberName;
                    object swarm = FindObjectMember(sphere, new string[] { "swarm" }, out swarmMemberName);
                    string countMemberName;
                    double sailCount;
                    if (TryFindNumericScalarMember(swarm, new string[] { "sail", "count" }, out countMemberName, out sailCount))
                    {
                        Queue<DysonPopulationSample> samples;
                        if (!dysonPopulationSamples.TryGetValue(slotIndex, out samples))
                        {
                            samples = new Queue<DysonPopulationSample>();
                            dysonPopulationSamples[slotIndex] = samples;
                        }
                        samples.Enqueue(new DysonPopulationSample { AtUtc = now, SailCount = sailCount, MemberName = countMemberName });
                        while (samples.Count > 0 && (now - samples.Peek().AtUtc).TotalSeconds > 75.0)
                            samples.Dequeue();
                    }
                }
                slotIndex++;
            }
        }

        private void ResetDysonSamplingFor(object data)
        {
            if (System.Object.ReferenceEquals(
                    data, dysonSampleData))
                return;
            dysonSampleData = data;
            dysonPopulationSamples.Clear();
            dysonConstructionSamples.Clear();
            dysonTopologyCache.Clear();
        }

        private Dictionary<string, object> ExportObservedDysonPopulationRate(int slotIndex)
        {
            var d = new Dictionary<string, object>();
            Queue<DysonPopulationSample> samples;
            if (!dysonPopulationSamples.TryGetValue(slotIndex, out samples) || samples.Count < 2) return d;

            DysonPopulationSample first = null;
            DysonPopulationSample last = null;
            foreach (DysonPopulationSample sample in samples)
            {
                if (first == null) first = sample;
                last = sample;
            }
            if (first == null || last == null || first == last) return d;

            double seconds = (last.AtUtc - first.AtUtc).TotalSeconds;
            if (seconds < 10.0) return d;
            d["sourceMember"] = last.MemberName;
            d["windowSeconds"] = seconds;
            d["netSailPopulationPerMinute"] = (last.SailCount - first.SailCount) * 60.0 / seconds;
            d["note"] = "Observed wall-clock net change in the live swarm sail-count member. It is not labeled as launch rate because expirations and sphere absorption can also change the population.";
            return d;
        }

        private Dictionary<string, object>
            ExportObservedDysonConstructionRate(int slotIndex)
        {
            var d = new Dictionary<string, object>();
            Queue<DysonConstructionSample> samples;
            if (!dysonConstructionSamples.TryGetValue(
                    slotIndex, out samples) ||
                samples.Count < 2)
                return d;

            DysonConstructionSample first = null;
            DysonConstructionSample last = null;
            foreach (DysonConstructionSample sample in samples)
            {
                if (first == null) first = sample;
                last = sample;
            }
            if (first == null || last == null || first == last) return d;

            double seconds = (last.AtUtc - first.AtUtc).TotalSeconds;
            if (seconds < 10.0) return d;
            double scale = 60.0 / seconds;
            d["windowSeconds"] = seconds;
            d["constructedStructurePointsPerMinute"] =
                (last.ConstructedStructurePoints -
                    first.ConstructedStructurePoints) * scale;
            d["constructedCellPointsPerMinute"] =
                (last.ConstructedCellPoints -
                    first.ConstructedCellPoints) * scale;
            d["permanentGenerationWattsChangePerMinute"] =
                (last.PermanentGenerationWatts -
                    first.PermanentGenerationWatts) * scale;
            d["note"] =
                "Observed changes in permanent sphere structure, shell cells, and layer generation. These are construction rates, not launch rates.";
            return d;
        }

        private Dictionary<string, object> ExportDyson(object data)
        {
            ResetDysonSamplingFor(data);
            var d = new Dictionary<string, object>();
            object history = GetStatic(gameMainType, "history");
            d["currentStar"] = ExportCelestialIdentity(GetStatic(gameMainType, "localStar"));
            d["researchModifiers"] = ExportNamedMembers(
                history,
                new string[] {
                    "solarSailLife", "solarEnergyLossRate", "useIonLayer",
                    "dysonNodeLatitude", "dysonNodeAbsorbInterval"
                }
            );

            string sphereMemberName;
            object spheres = FindEnumerableMember(data, new string[] { "dyson" }, out sphereMemberName);
            var systems = new List<object>();
            int slotIndex = 0;
            foreach (object sphere in Enumerate(spheres))
            {
                if (sphere != null)
                {
                    var row = new Dictionary<string, object>();
                    row["slotIndex"] = slotIndex;
                    row["runtimeType"] = sphere.GetType().FullName;

                    string starMemberName;
                    object star = FindObjectMember(sphere, new string[] { "star" }, out starMemberName);
                    if (star != null)
                    {
                        row["star"] = ExportCelestialIdentity(star);
                        row["starMember"] = starMemberName;
                    }

                    row["metrics"] = ExportScalarObject(
                        sphere,
                        1,
                        new string[] {
                            "energy", "power", "sail", "rocket", "node", "cell",
                            "layer", "frame", "structure", "request", "generate", "absorb"
                        }
                    );
                    Dictionary<string, object> topology;
                    if (!dysonTopologyCache.TryGetValue(
                            slotIndex, out topology))
                    {
                        topology = ExportDysonTopology(sphere);
                        dysonTopologyCache[slotIndex] = topology;
                    }
                    row["topology"] = topology;
                    Dictionary<string, object> constructionRate =
                        ExportObservedDysonConstructionRate(slotIndex);
                    if (constructionRate.Count > 0)
                        row["observedConstructionRate"] = constructionRate;

                    string swarmMemberName;
                    object swarm = FindObjectMember(sphere, new string[] { "swarm" }, out swarmMemberName);
                    if (swarm != null)
                    {
                        var swarmRow = new Dictionary<string, object>();
                        swarmRow["runtimeType"] = swarm.GetType().FullName;
                        swarmRow["member"] = swarmMemberName;
                        swarmRow["metrics"] = ExportScalarObject(
                            swarm,
                            1,
                            new string[] { "sail", "bullet", "orbit", "energy", "count", "cursor" }
                        );
                        Dictionary<string, object> observedRate = ExportObservedDysonPopulationRate(slotIndex);
                        if (observedRate.Count > 0) swarmRow["observedPopulationRate"] = observedRate;
                        row["swarm"] = swarmRow;
                    }

                    systems.Add(row);
                }
                slotIndex++;
            }
            d["systemCollectionMember"] = sphereMemberName;
            d["systems"] = systems;
            d["receiverContinuity"] = receiverTelemetry.Export();

            var planets = new List<object>();
            foreach (object factory in Enumerate(GetMember(data, "factories")))
            {
                if (factory == null) continue;

                HashSet<int> ejectorEntities = FindEntityIdsByProto(factory, ItemIdEmRailEjector);
                HashSet<int> siloEntities = FindEntityIdsByProto(factory, ItemIdVerticalLaunchingSilo);
                HashSet<int> receiverEntities = FindEntityIdsByProto(factory, ItemIdRayReceiver);
                if (ejectorEntities.Count == 0 && siloEntities.Count == 0 && receiverEntities.Count == 0)
                    continue;

                var row = new Dictionary<string, object>();
                row["planet"] = ExportCelestialIdentity(GetMember(factory, "planet"));
                if (ejectorEntities.Count > 0)
                    row["ejectors"] = ExportLaunchDeviceSummary(factory, ejectorEntities, ItemIdEmRailEjector, new string[] { "ejector", "pool" });
                if (siloEntities.Count > 0)
                    row["silos"] = ExportLaunchDeviceSummary(factory, siloEntities, ItemIdVerticalLaunchingSilo, new string[] { "silo", "pool" });
                if (receiverEntities.Count > 0)
                    row["receivers"] = ExportReceiverSummary(factory, receiverEntities);
                planets.Add(row);
            }
            d["planets"] = planets;
            d["note"] = "Dyson telemetry is deliberately narrow. It exports only live scalar state from established Dyson systems and deployed launch/receiver devices; unavailable runtime members are omitted rather than synthesized.";
            return d;
        }

        private static HashSet<int> FindEntityIdsByProto(object factory, int protoId)
        {
            var ids = new HashSet<int>();
            Array pool = GetMember(factory, "entityPool") as Array;
            int cursor = ToInt(GetMember(factory, "entityCursor"));
            if (pool == null) return ids;
            if (cursor <= 0 || cursor > pool.Length) cursor = pool.Length;

            for (int i = 1; i < cursor; i++)
            {
                object entity = pool.GetValue(i);
                if (entity == null) continue;
                if (ToInt(GetMember(entity, "protoId")) != protoId) continue;
                int entityId = ToInt(GetMember(entity, "id"));
                if (entityId > 0) ids.Add(entityId);
            }
            return ids;
        }

        private static Dictionary<string, object> ExportLaunchDeviceSummary(object factory, HashSet<int> entityIds, int protoId, string[] poolKeywords)
        {
            var d = new Dictionary<string, object>();
            d["protoId"] = protoId;
            d["name"] = ItemNames.ContainsKey(protoId) ? ItemNames[protoId] : null;
            d["deployedCount"] = entityIds.Count;

            object factorySystem = GetMember(factory, "factorySystem");
            string poolMemberName;
            object pool = FindEnumerableMember(factorySystem, poolKeywords, out poolMemberName);
            d["componentPoolMember"] = poolMemberName;

            int matched = 0;
            int supplied = 0;
            int targetAssigned = 0;
            int firing = 0;
            var distributions = new Dictionary<string, Dictionary<string, long>>();
            var counters = new Dictionary<string, double>();

            foreach (object component in Enumerate(pool))
            {
                if (component == null) continue;
                int entityId = ToInt(GetMember(component, "entityId"));
                if (entityId <= 0 || !entityIds.Contains(entityId)) continue;
                matched++;
                if (ToInt(GetMember(component, "bulletCount")) > 0)
                    supplied++;
                if (protoId == ItemIdVerticalLaunchingSilo &&
                    ToBool(GetMember(component, "hasNode")))
                    targetAssigned++;
                if (ToBool(GetMember(component, "fired")))
                    firing++;

                CollectScalarDistributions(
                    component,
                    new string[] {
                        "state", "orbit", "target", "auto", "node", "fired"
                    },
                    distributions);
                CollectNumericSums(component, new string[] { "bullet", "launch", "fire", "shoot" }, counters);
            }

            d["componentCount"] = matched;
            d["suppliedCount"] = supplied;
            d["targetAssignedCount"] = targetAssigned;
            d["firingNowCount"] = firing;
            if (distributions.Count > 0) d["stateDistributions"] = ExportDistributions(distributions);
            if (counters.Count > 0) d["counterSums"] = counters;
            return d;
        }

        private static Dictionary<string, object> ExportDysonTopology(
            object sphere)
        {
            var d = new Dictionary<string, object>();
            long layerCount = 0;
            long nodeCount = 0;
            long frameCount = 0;
            long designatedShellCount = 0;
            long cellReadyShellCount = 0;
            long shellCellPoints = 0;
            long shellCellPointCapacity = 0;
            var layers = new List<object>();

            object layerPool = GetMember(
                sphere, "layersIdBased", "layersSorted");
            foreach (object layer in Enumerate(layerPool))
            {
                if (layer == null) continue;
                int id = ToInt(GetMember(layer, "id"));
                if (id <= 0) continue;
                layerCount++;

                int layerNodes = ToInt(GetMember(layer, "nodeCount"));
                int layerFrames = ToInt(GetMember(layer, "frameCount"));
                int layerShells = ToInt(GetMember(layer, "shellCount"));
                nodeCount += layerNodes;
                frameCount += layerFrames;

                long layerReadyShells = 0;
                long layerCellPoints = 0;
                long layerCellCapacity = 0;
                foreach (object shell in Enumerate(
                    GetMember(layer, "shellPool")))
                {
                    if (shell == null ||
                        ToInt(GetMember(shell, "id")) <= 0)
                        continue;
                    long cellPoints = ToLong(
                        GetMember(shell, "cellPoint"));
                    long cellCapacity = ToLong(
                        GetMember(shell, "cellPointMax"));
                    bool boundaryReady =
                        cellPoints > 0 ||
                        IsShellBoundaryReady(shell);
                    designatedShellCount++;
                    layerCellPoints += cellPoints;
                    layerCellCapacity += cellCapacity;
                    if (boundaryReady) layerReadyShells++;
                }

                cellReadyShellCount += layerReadyShells;
                shellCellPoints += layerCellPoints;
                shellCellPointCapacity += layerCellCapacity;
                layers.Add(new Dictionary<string, object> {
                    { "layerId", id },
                    { "nodeCount", layerNodes },
                    { "frameCount", layerFrames },
                    { "designatedShellCount", layerShells },
                    { "cellReadyShellCount", layerReadyShells },
                    { "constructedCellPoints", layerCellPoints },
                    { "cellPointCapacity", layerCellCapacity }
                });
            }

            d["layerCount"] = layerCount;
            d["plannedNodeCount"] = nodeCount;
            d["plannedFrameCount"] = frameCount;
            d["designatedShellCount"] = designatedShellCount;
            d["cellReadyShellCount"] = cellReadyShellCount;
            d["constructedCellPoints"] = shellCellPoints;
            d["cellPointCapacity"] = shellCellPointCapacity;
            d["layers"] = layers;
            d["note"] =
                "A designated shell is counted from live layer shell objects. A shell is cell-ready only after its boundary nodes and frames are complete, or after permanent cell construction has already begun.";
            return d;
        }

        private static bool IsShellBoundaryReady(object shell)
        {
            bool sawNode = false;
            foreach (object node in Enumerate(GetMember(shell, "nodes")))
            {
                if (node == null ||
                    ToInt(GetMember(node, "id")) <= 0)
                    continue;
                sawNode = true;
                long maximum = ToLong(GetMember(node, "spMax"));
                long constructed = ToLong(GetMember(node, "sp"));
                if (maximum > 0 && constructed < maximum)
                    return false;
            }

            bool sawFrame = false;
            foreach (object frame in Enumerate(GetMember(shell, "frames")))
            {
                if (frame == null ||
                    ToInt(GetMember(frame, "id")) <= 0)
                    continue;
                sawFrame = true;
                long maximum = ToLong(GetMember(frame, "spMax"));
                long constructed =
                    ToLong(GetMember(frame, "spA")) +
                    ToLong(GetMember(frame, "spB"));
                if (maximum > 0 && constructed < maximum)
                    return false;
            }
            return sawNode && sawFrame;
        }

        private static Dictionary<string, object> ExportReceiverSummary(object factory, HashSet<int> entityIds)
        {
            var d = new Dictionary<string, object>();
            d["protoId"] = ItemIdRayReceiver;
            d["name"] = ItemNames.ContainsKey(ItemIdRayReceiver) ? ItemNames[ItemIdRayReceiver] : null;
            d["deployedCount"] = entityIds.Count;

            object power = GetMember(factory, "powerSystem");
            string poolMemberName;
            object generatorPool = FindEnumerableMember(power, new string[] { "gen", "pool" }, out poolMemberName);
            d["componentPoolMember"] = poolMemberName;

            var devices = new List<object>();
            foreach (object component in Enumerate(generatorPool))
            {
                if (component == null) continue;
                int entityId = ToInt(GetMember(component, "entityId"));
                if (entityId <= 0 || !entityIds.Contains(entityId)) continue;

                var row = new Dictionary<string, object>();
                row["entityId"] = entityId;
                row["metrics"] = ExportScalarObject(
                    component,
                    1,
                    new string[] {
                        "product", "catalyst", "warm", "ion", "ray", "receiver",
                        "generate", "energy", "power", "strength", "capacity", "state"
                    }
                );
                devices.Add(row);
            }
            d["componentCount"] = devices.Count;
            d["devices"] = devices;
            return d;
        }

        private static List<object> ExportDistributions(Dictionary<string, Dictionary<string, long>> distributions)
        {
            var rows = new List<object>();
            var names = new List<string>(distributions.Keys);
            names.Sort(StringComparer.Ordinal);
            foreach (string name in names)
            {
                var row = new Dictionary<string, object>();
                row["member"] = name;
                var values = new List<object>();
                var keys = new List<string>(distributions[name].Keys);
                keys.Sort(StringComparer.Ordinal);
                foreach (string value in keys)
                {
                    var v = new Dictionary<string, object>();
                    v["value"] = value;
                    v["count"] = distributions[name][value];
                    values.Add(v);
                }
                row["values"] = values;
                rows.Add(row);
            }
            return rows;
        }

        private static void CollectScalarDistributions(object obj, string[] keywords, Dictionary<string, Dictionary<string, long>> target)
        {
            if (obj == null) return;
            foreach (KeyValuePair<string, object> kv in GetScalarMembers(obj, keywords))
            {
                string value = ToStr(kv.Value);
                if (String.IsNullOrEmpty(value)) continue;
                Dictionary<string, long> values;
                if (!target.TryGetValue(kv.Key, out values))
                {
                    values = new Dictionary<string, long>();
                    target[kv.Key] = values;
                }
                if (!values.ContainsKey(value)) values[value] = 0;
                values[value]++;
            }
        }

        private static void CollectNumericSums(object obj, string[] keywords, Dictionary<string, double> target)
        {
            if (obj == null) return;
            foreach (KeyValuePair<string, object> kv in GetScalarMembers(obj, keywords))
            {
                if (!IsNumeric(kv.Value)) continue;
                double value = ToDouble(kv.Value);
                if (!target.ContainsKey(kv.Key)) target[kv.Key] = 0.0;
                target[kv.Key] += value;
            }
        }

        private static Dictionary<string, object> GetScalarMembers(object obj, string[] keywords)
        {
            var d = new Dictionary<string, object>();
            if (obj == null) return d;

            Type t = obj.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var names = new HashSet<string>();

            foreach (FieldInfo f in t.GetFields(flags))
            {
                if (names.Contains(f.Name) || !KeywordMatch(f.Name, keywords)) continue;
                names.Add(f.Name);
                try
                {
                    object scalar = Scalar(f.GetValue(obj));
                    if (scalar != null) d[f.Name] = scalar;
                }
                catch { }
            }

            foreach (PropertyInfo p in t.GetProperties(flags))
            {
                if (p.GetIndexParameters().Length != 0) continue;
                if (names.Contains(p.Name) || !KeywordMatch(p.Name, keywords)) continue;
                names.Add(p.Name);
                try
                {
                    object scalar = Scalar(p.GetValue(obj, null));
                    if (scalar != null) d[p.Name] = scalar;
                }
                catch { }
            }
            return d;
        }

        private static bool TryFindNumericScalarMember(object obj, string[] keywords, out string memberName, out double value)
        {
            memberName = null;
            value = 0.0;
            if (obj == null) return false;

            Dictionary<string, object> members = GetScalarMembers(obj, keywords);
            var names = new List<string>(members.Keys);
            names.Sort(delegate(string a, string b) {
                int lengthCompare = a.Length.CompareTo(b.Length);
                return lengthCompare != 0 ? lengthCompare : StringComparer.Ordinal.Compare(a, b);
            });

            foreach (string name in names)
            {
                object raw = members[name];
                if (!IsNumeric(raw)) continue;
                memberName = name;
                value = ToDouble(raw);
                return true;
            }
            return false;
        }

        private static bool IsNumeric(object value)
        {
            if (value == null) return false;
            TypeCode code = Type.GetTypeCode(value.GetType());
            return code == TypeCode.Byte || code == TypeCode.SByte ||
                   code == TypeCode.Int16 || code == TypeCode.UInt16 ||
                   code == TypeCode.Int32 || code == TypeCode.UInt32 ||
                   code == TypeCode.Int64 || code == TypeCode.UInt64 ||
                   code == TypeCode.Single || code == TypeCode.Double || code == TypeCode.Decimal;
        }

        private static Dictionary<string, object> ExportDarkFog(object data)
        {
            var d = new Dictionary<string, object>();
            object spaceSector = GetMember(data, "spaceSector");
            d["spaceSector"] = ExportScalarObject(
                spaceSector,
                1,
                new string[] {
                    "enemy", "hive", "combat", "threat", "level", "exp", "assault",
                    "relay", "seed", "core"
                }
            );

            var planets = new List<object>();
            foreach (object factory in Enumerate(GetMember(data, "factories")))
            {
                if (factory == null) continue;
                object enemy = GetMember(factory, "enemySystem");
                if (enemy == null) continue;

                var row = new Dictionary<string, object>();
                row["planet"] = ExportCelestialIdentity(GetMember(factory, "planet"));
                row["metrics"] = ExportScalarObject(
                    enemy,
                    1,
                    new string[] {
                        "enemy", "base", "camp", "relay", "threat", "level", "exp",
                        "assault", "unit", "count", "cursor"
                    }
                );
                planets.Add(row);
            }
            d["planetaryEnemySystems"] = planets;
            return d;
        }

        private static Dictionary<string, object> ExportProgressionSummary(object data, object player)
        {
            var d = new Dictionary<string, object>();
            object history = GetStatic(gameMainType, "history");

            int[] milestoneTechIds = new int[] {
                1002, // blue
                1111, // red
                2902, // Drive Engine Lv2
                1413, // Titanium Smelting
                1604, // PLS
                1414, // Titanium Alloy
                1605, // ILS
                1312, // purple
                1704, // Gravitational Wave Refraction
                2904, // mecha warp
                1705, // green
                3404, // vessel warp
                1505, // Planetary Ionosphere Utilization
                1506, // Dirac
                1507, // white
                1508  // mission
            };

            var milestones = new List<object>();
            foreach (int tid in milestoneTechIds)
            {
                var m = new Dictionary<string, object>();
                m["techId"] = tid;
                m["name"] = TechNames.ContainsKey(tid) ? TechNames[tid] : null;
                object unlocked = TryInvoke(history, "TechUnlocked", tid);
                if (!(unlocked is bool))
                    unlocked = TryInvoke(history, "TechUnlocked", tid, false);
                m["unlocked"] = unlocked is bool ? unlocked : null;
                milestones.Add(m);
            }
            d["milestoneTechs"] = milestones;

            int[] keyItemIds = new int[] {
                1003, 1004, 1105, 1106, 1118, // silicon/titanium chain
                1206, 1210,                   // particle container / warper
                6001, 6002, 6003, 6004, 6005, 6006
            };

            var allInventory = new Dictionary<int, long>();
            MergeStorageCounts(allInventory, GetMember(player, "package", "packageStorage"));
            object mecha = GetMember(player, "mecha");
            MergeStorageCounts(allInventory, GetMember(mecha, "reactorStorage", "fuelStorage", "fuelChamber"));

            var keyItems = new List<object>();
            foreach (int iid in keyItemIds)
            {
                var x = new Dictionary<string, object>();
                x["itemId"] = iid;
                x["name"] = ItemNames.ContainsKey(iid) ? ItemNames[iid] : null;
                x["playerCount"] = allInventory.ContainsKey(iid) ? allInventory[iid] : 0L;
                keyItems.Add(x);
            }
            d["keyPlayerItems"] = keyItems;

            var aggregateBuildings = new Dictionary<int, long>();
            foreach (object factory in Enumerate(GetMember(data, "factories")))
            {
                if (factory == null) continue;
                MergeCounts(aggregateBuildings, CountFactoryEntities(factory));
            }
            d["allFactoryBuildingCounts"] = NamedCountRows(aggregateBuildings);

            return d;
        }

        private static Dictionary<string, object> ExportDiagnostics(object data, object player)
        {
            var d = new Dictionary<string, object>();
            object history = GetStatic(gameMainType, "history");
            object statistics = GetStatic(gameMainType, "statistics");
            if (statistics == null) statistics = GetMember(data, "statistics");

            d["historyScalars"] = ExportScalarObject(history, 1, null);
            d["playerScalars"] = ExportScalarObject(player, 1, new string[] {
                "planet", "star", "sand", "soil", "position", "movement", "state"
            });
            d["statisticsScalars"] = ExportScalarObject(statistics, 2, new string[] {
                "production", "consume", "power", "research", "kill", "factory",
                "total", "count", "tick", "time"
            });

            return d;
        }

        private static Dictionary<string, object> ExportOwnedInventorySummary(object data, object player)
        {
            var d = new Dictionary<string, object>();
            var aggregate = new Dictionary<int, long>();

            // Personal inventory and fuel are included because they are also owned stock.
            MergeStorageCounts(aggregate, GetMember(player, "package", "packageStorage"));
            object mecha = GetMember(player, "mecha");
            MergeStorageCounts(aggregate, GetMember(mecha, "reactorStorage", "fuelStorage", "fuelChamber"));
            MergeStorageCounts(aggregate, GetMember(player, "deliveryPackage"));

            var byPlanet = new List<object>();

            foreach (object factory in Enumerate(GetMember(data, "factories")))
            {
                if (factory == null) continue;

                var planetCounts = new Dictionary<int, long>();
                MergeOwnedStorageCounts(planetCounts, factory);
                MergeLogisticsStorageCounts(planetCounts, factory);
                MergeCounts(aggregate, planetCounts);

                var row = new Dictionary<string, object>();
                row["planet"] = ExportCelestialIdentity(GetMember(factory, "planet"));
                row["contents"] = NamedCountRows(planetCounts);
                byPlanet.Add(row);
            }

            d["allOwnedItems"] = NamedCountRows(aggregate);
            d["factoryPlanetItems"] = byPlanet;
            d["scope"] = "Player inventory + delivery inventory + mecha fuel/reactor storage + factory depots/storage components + tanks + logistics station storage.";
            d["note"] = "Counts intentionally exclude materials currently inside machine input/output buffers, belts, sorters, miners, assemblers, labs, fractionators, ejectors, silos, and other in-process entities.";
            return d;
        }

        private static Dictionary<string, object> ExportOwnedStorage(object factory)
        {
            var d = new Dictionary<string, object>();
            object factoryStorage = GetMember(factory, "factoryStorage", "storageSystem");
            if (factoryStorage == null)
            {
                d["available"] = false;
                return d;
            }

            d["available"] = true;
            var aggregate = new Dictionary<int, long>();
            var containers = new List<object>();
            var tanks = new List<object>();

            Array storagePool = GetMember(factoryStorage, "storagePool") as Array;
            int storageCursor = ToInt(GetMember(factoryStorage, "storageCursor"));
            if (storagePool != null)
            {
                if (storageCursor <= 0 || storageCursor > storagePool.Length) storageCursor = storagePool.Length;

                for (int i = 1; i < storageCursor; i++)
                {
                    object component = storagePool.GetValue(i);
                    if (component == null) continue;

                    var counts = new Dictionary<int, long>();
                    MergeStorageCounts(counts, component);

                    int componentId = ToInt(GetMember(component, "id", "storageId"));
                    int entityId = ToInt(GetMember(component, "entityId"));
                    if (componentId <= 0 && entityId <= 0 && counts.Count == 0) continue;

                    var row = new Dictionary<string, object>();
                    row["storageId"] = componentId > 0 ? (object)componentId : i;
                    row["entityId"] = entityId > 0 ? (object)entityId : null;
                    row["building"] = ExportEntityBuildingIdentity(factory, entityId);
                    row["contents"] = NamedCountRows(counts);
                    row["metrics"] = ExportScalarObject(component, 1, new string[] {
                        "id", "entity", "size", "count", "bans", "filter", "storage"
                    });
                    containers.Add(row);
                    MergeCounts(aggregate, counts);
                }
            }

            Array tankPool = GetMember(factoryStorage, "tankPool") as Array;
            int tankCursor = ToInt(GetMember(factoryStorage, "tankCursor"));
            if (tankPool != null)
            {
                if (tankCursor <= 0 || tankCursor > tankPool.Length) tankCursor = tankPool.Length;

                for (int i = 1; i < tankCursor; i++)
                {
                    object tank = tankPool.GetValue(i);
                    if (tank == null) continue;

                    int componentId = ToInt(GetMember(tank, "id", "tankId"));
                    int entityId = ToInt(GetMember(tank, "entityId"));
                    int itemId = ToInt(GetMember(tank, "fluidId", "itemId", "itemID"));
                    long count = ToLong(GetMember(tank, "fluidCount", "count"));
                    long capacity = ToLong(GetMember(tank, "fluidCapacity", "capacity", "max"));

                    if (componentId <= 0 && entityId <= 0 && itemId <= 0 && count <= 0) continue;

                    var row = new Dictionary<string, object>();
                    row["tankId"] = componentId > 0 ? (object)componentId : i;
                    row["entityId"] = entityId > 0 ? (object)entityId : null;
                    row["building"] = ExportEntityBuildingIdentity(factory, entityId);
                    row["itemId"] = itemId > 0 ? (object)itemId : null;
                    row["name"] = itemId > 0 && ItemNames.ContainsKey(itemId) ? ItemNames[itemId] : null;
                    row["count"] = count;
                    row["capacity"] = capacity > 0 ? (object)capacity : null;
                    row["metrics"] = ExportScalarObject(tank, 1, new string[] {
                        "id", "entity", "fluid", "item", "count", "capacity", "input", "output"
                    });
                    tanks.Add(row);

                    if (itemId > 0 && count > 0)
                    {
                        if (!aggregate.ContainsKey(itemId)) aggregate[itemId] = 0;
                        aggregate[itemId] += count;
                    }
                }
            }

            d["containerCount"] = containers.Count;
            d["containers"] = containers;
            d["tankCount"] = tanks.Count;
            d["tanks"] = tanks;
            d["aggregateContents"] = NamedCountRows(aggregate);
            d["factoryStorageMetrics"] = ExportScalarObject(factoryStorage, 1, new string[] {
                "storage", "tank", "cursor", "count"
            });
            return d;
        }

        private static Dictionary<string, object> ExportEntityBuildingIdentity(object factory, int entityId)
        {
            var d = new Dictionary<string, object>();
            if (factory == null || entityId <= 0) return d;

            try
            {
                Array pool = GetMember(factory, "entityPool") as Array;
                if (pool == null || entityId >= pool.Length) return d;
                object entity = pool.GetValue(entityId);
                if (entity == null) return d;

                int protoId = ToInt(GetMember(entity, "protoId"));
                d["protoId"] = protoId > 0 ? (object)protoId : null;
                d["name"] = protoId > 0 && ItemNames.ContainsKey(protoId) ? ItemNames[protoId] : null;
            }
            catch { }
            return d;
        }

        private static void MergeOwnedStorageCounts(Dictionary<int, long> counts, object factory)
        {
            if (factory == null) return;
            object factoryStorage = GetMember(factory, "factoryStorage", "storageSystem");
            if (factoryStorage == null) return;

            Array storagePool = GetMember(factoryStorage, "storagePool") as Array;
            int storageCursor = ToInt(GetMember(factoryStorage, "storageCursor"));
            if (storagePool != null)
            {
                if (storageCursor <= 0 || storageCursor > storagePool.Length) storageCursor = storagePool.Length;
                for (int i = 1; i < storageCursor; i++)
                {
                    object component = storagePool.GetValue(i);
                    if (component != null) MergeStorageCounts(counts, component);
                }
            }

            Array tankPool = GetMember(factoryStorage, "tankPool") as Array;
            int tankCursor = ToInt(GetMember(factoryStorage, "tankCursor"));
            if (tankPool != null)
            {
                if (tankCursor <= 0 || tankCursor > tankPool.Length) tankCursor = tankPool.Length;
                for (int i = 1; i < tankCursor; i++)
                {
                    object tank = tankPool.GetValue(i);
                    if (tank == null) continue;
                    int itemId = ToInt(GetMember(tank, "fluidId", "itemId", "itemID"));
                    long count = ToLong(GetMember(tank, "fluidCount", "count"));
                    if (itemId <= 0 || count <= 0) continue;
                    if (!counts.ContainsKey(itemId)) counts[itemId] = 0;
                    counts[itemId] += count;
                }
            }
        }

        private static void MergeLogisticsStorageCounts(Dictionary<int, long> counts, object factory)
        {
            if (factory == null) return;
            object transport = GetMember(factory, "transport", "planetTransport");
            if (transport == null) return;

            Array stationPool = GetMember(transport, "stationPool") as Array;
            int cursor = ToInt(GetMember(transport, "stationCursor"));
            if (stationPool == null) return;
            if (cursor <= 0 || cursor > stationPool.Length) cursor = stationPool.Length;

            for (int i = 1; i < cursor; i++)
            {
                object station = stationPool.GetValue(i);
                if (station == null) continue;
                int id = ToInt(GetMember(station, "id"));
                if (id <= 0) continue;
                MergeStationStorage(counts, GetMember(station, "storage"));
            }
        }

        // --------------------------------------------------------------------
        // Factory summaries
        // --------------------------------------------------------------------

        private static Dictionary<int, long> CountFactoryEntities(object factory)
        {
            var counts = new Dictionary<int, long>();
            object poolObj = GetMember(factory, "entityPool");
            Array pool = poolObj as Array;
            int cursor = ToInt(GetMember(factory, "entityCursor"));

            if (pool == null) return counts;
            if (cursor <= 0 || cursor > pool.Length) cursor = pool.Length;

            for (int i = 1; i < cursor; i++)
            {
                object entity = pool.GetValue(i);
                if (entity == null) continue;

                int id = ToInt(GetMember(entity, "id"));
                int protoId = ToInt(GetMember(entity, "protoId"));

                if (id <= 0 || protoId <= 0) continue;

                if (!counts.ContainsKey(protoId)) counts[protoId] = 0;
                counts[protoId]++;
            }

            return counts;
        }

        private static Dictionary<string, object> ExportLogistics(object factory)
        {
            var d = new Dictionary<string, object>();
            object transport = GetMember(factory, "transport", "planetTransport");
            if (transport == null)
            {
                d["available"] = false;
                return d;
            }

            d["available"] = true;
            Array stationPool = GetMember(transport, "stationPool") as Array;
            int cursor = ToInt(GetMember(transport, "stationCursor"));

            var stations = new List<object>();
            var aggregateStorage = new Dictionary<int, long>();

            if (stationPool != null)
            {
                if (cursor <= 0 || cursor > stationPool.Length) cursor = stationPool.Length;

                for (int i = 1; i < cursor; i++)
                {
                    object station = stationPool.GetValue(i);
                    if (station == null) continue;
                    int id = ToInt(GetMember(station, "id"));
                    if (id <= 0) continue;

                    var row = new Dictionary<string, object>();
                    row["id"] = id;
                    row["gid"] = Scalar(GetMember(station, "gid"));
                    row["isStellar"] = Scalar(GetMember(station, "isStellar"));
                    row["isCollector"] = Scalar(GetMember(station, "isCollector"));
                    row["storage"] = ExportStationStorage(GetMember(station, "storage"));
                    row["fleet"] = ExportNamedMembers(
                        station,
                        new string[] {
                            "idleDroneCount", "workDroneCount",
                            "idleShipCount", "workShipCount",
                            "warperCount", "warperMaxCount"
                        }
                    );

                    MergeStationStorage(aggregateStorage, GetMember(station, "storage"));
                    stations.Add(row);
                }
            }

            d["stationCount"] = stations.Count;
            d["stations"] = stations;
            d["aggregateStationStorage"] = NamedCountRows(aggregateStorage);
            d["transportMetrics"] = ExportScalarObject(
                transport,
                1,
                new string[] { "station", "drone", "ship", "vessel", "courier", "dispenser", "cursor", "count" }
            );

            return d;
        }

        private static Dictionary<string, object> ExportPower(object factory)
        {
            object power = GetMember(factory, "powerSystem");
            var d = new Dictionary<string, object>();
            d["metrics"] = ExportScalarObject(
                power,
                2,
                new string[] {
                    "power", "energy", "generate", "consume", "consumer",
                    "generator", "network", "net", "node", "accumulator",
                    "exchanger", "charger", "cursor", "capacity"
                }
            );

            string networkPoolMember;
            object networkPool = FindEnumerableMember(power, new string[] { "net", "pool" }, out networkPoolMember);
            var networks = new List<object>();
            foreach (object network in Enumerate(networkPool))
            {
                if (network == null) continue;
                int id = ToInt(GetMember(network, "id"));
                if (id <= 0) continue;
                var metrics = ExportScalarObject(
                    network,
                    1,
                    new string[] {
                        "energy", "power", "generate", "consume", "capacity",
                        "accumulator", "charge", "discharge", "request", "supply",
                        "demand", "serve"
                    }
                );
                if (metrics.Count == 0) continue;
                var row = new Dictionary<string, object>();
                row["id"] = id;
                row["metrics"] = metrics;
                networks.Add(row);
            }
            if (!String.IsNullOrEmpty(networkPoolMember)) d["networkPoolMember"] = networkPoolMember;
            if (networks.Count > 0) d["networks"] = networks;
            return d;
        }

        private static Dictionary<string, object> ExportProduction(object factory)
        {
            var d = new Dictionary<string, object>();
            object fs = GetMember(factory, "factorySystem");
            d["factorySystemMetrics"] = ExportScalarObject(
                fs,
                1,
                new string[] {
                    "miner", "assembler", "lab", "fraction", "ejector", "silo",
                    "tank", "storage", "spray", "piler", "monitor", "cursor", "count"
                }
            );
            return d;
        }

        private static Dictionary<string, object> ExportEnemySummary(object factory)
        {
            object enemy = GetMember(factory, "enemySystem");
            return ExportScalarObject(
                enemy,
                1,
                new string[] {
                    "enemy", "base", "camp", "relay", "threat", "level",
                    "exp", "assault", "unit", "count", "cursor"
                }
            );
        }

        // --------------------------------------------------------------------
        // Inventory/storage helpers
        // --------------------------------------------------------------------

        private static List<object> ExportStorage(object storage)
        {
            var counts = new Dictionary<int, long>();
            MergeStorageCounts(counts, storage);
            return NamedCountRows(counts);
        }

        private static void MergeStorageCounts(Dictionary<int, long> counts, object storage)
        {
            if (storage == null) return;

            object grids = GetMember(storage, "grids");
            if (grids == null && storage is IEnumerable)
                grids = storage;

            foreach (object grid in Enumerate(grids))
            {
                if (grid == null) continue;
                int itemId = ToInt(GetMember(grid, "itemId", "itemID"));
                long count = ToLong(GetMember(grid, "count"));
                if (itemId <= 0 || count <= 0) continue;

                if (!counts.ContainsKey(itemId)) counts[itemId] = 0;
                counts[itemId] += count;
            }
        }

        private static List<object> ExportStationStorage(object storage)
        {
            var rows = new List<object>();
            foreach (object slot in Enumerate(storage))
            {
                if (slot == null) continue;
                int itemId = ToInt(GetMember(slot, "itemId", "itemID"));
                if (itemId <= 0) continue;

                var row = new Dictionary<string, object>();
                row["itemId"] = itemId;
                row["name"] = ItemNames.ContainsKey(itemId) ? ItemNames[itemId] : null;
                row["count"] = Scalar(GetMember(slot, "count"));
                row["max"] = Scalar(GetMember(slot, "max"));
                row["localLogic"] = ToStr(GetMember(slot, "localLogic"));
                row["remoteLogic"] = ToStr(GetMember(slot, "remoteLogic"));
                rows.Add(row);
            }
            return rows;
        }

        private static void MergeStationStorage(Dictionary<int, long> counts, object storage)
        {
            foreach (object slot in Enumerate(storage))
            {
                if (slot == null) continue;
                int itemId = ToInt(GetMember(slot, "itemId", "itemID"));
                long count = ToLong(GetMember(slot, "count"));
                if (itemId <= 0 || count <= 0) continue;

                if (!counts.ContainsKey(itemId)) counts[itemId] = 0;
                counts[itemId] += count;
            }
        }

        private static List<object> NamedCountRows(Dictionary<int, long> counts)
        {
            var rows = new List<object>();
            var ids = new List<int>(counts.Keys);
            ids.Sort();

            foreach (int id in ids)
            {
                var row = new Dictionary<string, object>();
                row["id"] = id;
                row["name"] = ItemNames.ContainsKey(id) ? ItemNames[id] : null;
                row["count"] = counts[id];
                rows.Add(row);
            }

            return rows;
        }

        private static void MergeCounts(Dictionary<int, long> target, Dictionary<int, long> source)
        {
            foreach (var kv in source)
            {
                if (!target.ContainsKey(kv.Key)) target[kv.Key] = 0;
                target[kv.Key] += kv.Value;
            }
        }

        // --------------------------------------------------------------------
        // Proto caches
        // --------------------------------------------------------------------

        private static void BuildProtoNameCaches()
        {
            if (ItemNames.Count == 0)
            {
                object itemSet = GetStatic(ldbType, "items");
                foreach (object proto in Enumerate(GetMember(itemSet, "dataArray")))
                {
                    int id = ToInt(GetMember(proto, "ID"));
                    if (id > 0 && !ItemNames.ContainsKey(id))
                        ItemNames[id] = ProtoName(proto);
                }
            }

            if (TechNames.Count == 0)
            {
                object techSet = GetStatic(ldbType, "techs");
                foreach (object proto in Enumerate(GetMember(techSet, "dataArray")))
                {
                    int id = ToInt(GetMember(proto, "ID"));
                    if (id > 0 && !TechNames.ContainsKey(id))
                        TechNames[id] = ProtoName(proto);
                }
            }
            if (RecipeNames.Count == 0)
            {
                object recipes = GetStatic(ldbType, "recipes");
                object dataArray = GetMember(recipes, "dataArray");
                foreach (object proto in Enumerate(dataArray))
                {
                    int id = ToInt(GetMember(proto, "ID", "Id", "id"));
                    if (id > 0 && !RecipeNames.ContainsKey(id))
                        RecipeNames[id] = ProtoName(proto);
                }
            }

        }

        private static string ProtoName(object proto)
        {
            if (proto == null) return null;

            string raw = ToStr(GetMember(proto, "Name"));
            object nameObj = GetMember(proto, "name");
            string localized = ToStr(nameObj);

            if (!String.IsNullOrEmpty(localized)) return localized;
            return raw;
        }

        // --------------------------------------------------------------------
        // Safe reflection helpers
        // --------------------------------------------------------------------

        private static Type FindType(string fullName)
        {
            if (String.IsNullOrEmpty(fullName)) return null;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type t = asm.GetType(fullName, false);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        internal static object GetStatic(Type type, params string[] names)
        {
            if (type == null) return null;
            foreach (string name in names)
            {
                object value;
                if (TryGetMember(type, null, name, true, out value))
                    return value;
            }
            return null;
        }

        internal static object GetMember(object obj, params string[] names)
        {
            if (obj == null) return null;
            Type type = obj.GetType();

            foreach (string name in names)
            {
                object value;
                if (TryGetMember(type, obj, name, false, out value))
                    return value;
            }
            return null;
        }

        private static bool TryGetMember(Type type, object obj, string name, bool isStatic, out object value)
        {
            value = null;
            try
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                    (isStatic ? BindingFlags.Static : BindingFlags.Instance) |
                    BindingFlags.FlattenHierarchy;

                Type t = type;
                while (t != null)
                {
                    FieldInfo f = t.GetField(name, flags);
                    if (f != null)
                    {
                        value = f.GetValue(obj);
                        return true;
                    }
                    t = t.BaseType;
                }

                PropertyInfo p = type.GetProperty(name, flags);
                if (p != null && p.GetIndexParameters().Length == 0)
                {
                    value = p.GetValue(obj, null);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static object FindEnumerableMember(object obj, string[] keywords, out string memberName)
        {
            memberName = null;
            if (obj == null) return null;
            Type t = obj.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo f in t.GetFields(flags))
            {
                if (!NameContainsAll(f.Name, keywords)) continue;
                try
                {
                    object value = f.GetValue(obj);
                    if (value != null && !(value is string) && value is IEnumerable)
                    {
                        memberName = f.Name;
                        return value;
                    }
                }
                catch { }
            }

            foreach (PropertyInfo p in t.GetProperties(flags))
            {
                if (p.GetIndexParameters().Length != 0) continue;
                if (!NameContainsAll(p.Name, keywords)) continue;
                try
                {
                    object value = p.GetValue(obj, null);
                    if (value != null && !(value is string) && value is IEnumerable)
                    {
                        memberName = p.Name;
                        return value;
                    }
                }
                catch { }
            }
            return null;
        }

        private static object FindObjectMember(object obj, string[] keywords, out string memberName)
        {
            memberName = null;
            if (obj == null) return null;
            Type t = obj.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo f in t.GetFields(flags))
            {
                if (!NameContainsAll(f.Name, keywords)) continue;
                try
                {
                    object value = f.GetValue(obj);
                    if (value == null || Scalar(value) != null || value is IEnumerable) continue;
                    memberName = f.Name;
                    return value;
                }
                catch { }
            }

            foreach (PropertyInfo p in t.GetProperties(flags))
            {
                if (p.GetIndexParameters().Length != 0) continue;
                if (!NameContainsAll(p.Name, keywords)) continue;
                try
                {
                    object value = p.GetValue(obj, null);
                    if (value == null || Scalar(value) != null || value is IEnumerable) continue;
                    memberName = p.Name;
                    return value;
                }
                catch { }
            }
            return null;
        }

        private static bool NameContainsAll(string name, string[] keywords)
        {
            if (String.IsNullOrEmpty(name)) return false;
            if (keywords == null || keywords.Length == 0) return true;
            string lower = name.ToLowerInvariant();
            foreach (string keyword in keywords)
            {
                if (String.IsNullOrEmpty(keyword)) continue;
                if (!lower.Contains(keyword.ToLowerInvariant())) return false;
            }
            return true;
        }

        internal static object TryInvoke(object obj, string methodName, params object[] args)
        {
            if (obj == null) return null;
            try
            {
                Type t = obj.GetType();
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                MethodInfo[] methods = t.GetMethods(flags);

                foreach (MethodInfo m in methods)
                {
                    if (m.Name != methodName) continue;
                    ParameterInfo[] ps = m.GetParameters();
                    if (ps.Length != args.Length) continue;

                    object[] converted = new object[args.Length];
                    bool ok = true;
                    for (int i = 0; i < args.Length; i++)
                    {
                        try
                        {
                            if (args[i] == null)
                            {
                                converted[i] = null;
                            }
                            else if (ps[i].ParameterType.IsInstanceOfType(args[i]))
                            {
                                converted[i] = args[i];
                            }
                            else
                            {
                                converted[i] = Convert.ChangeType(args[i], ps[i].ParameterType, CultureInfo.InvariantCulture);
                            }
                        }
                        catch
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (!ok) continue;
                    return m.Invoke(obj, converted);
                }
            }
            catch { }
            return null;
        }

        private static object DictionaryLookup(object dictionaryObj, int key)
        {
            if (dictionaryObj == null) return null;

            IDictionary dictionary = dictionaryObj as IDictionary;
            if (dictionary != null)
            {
                try
                {
                    if (dictionary.Contains(key)) return dictionary[key];
                }
                catch { }
            }

            try
            {
                PropertyInfo indexer = dictionaryObj.GetType().GetProperty("Item");
                if (indexer != null)
                    return indexer.GetValue(dictionaryObj, new object[] { key });
            }
            catch { }

            return null;
        }

        internal static IEnumerable Enumerate(object value)
        {
            if (value == null) yield break;
            if (value is string) yield break;

            IEnumerable e = value as IEnumerable;
            if (e == null) yield break;

            foreach (object x in e) yield return x;
        }

        // --------------------------------------------------------------------
        // Compact generic diagnostics
        // --------------------------------------------------------------------

        private static Dictionary<string, object> ExportNamedMembers(object obj, string[] names)
        {
            var d = new Dictionary<string, object>();
            if (obj == null) return d;

            foreach (string name in names)
            {
                object v = GetMember(obj, name);
                object scalar = Scalar(v);
                if (scalar != null) d[name] = scalar;
            }

            return d;
        }

        private static Dictionary<string, object> ExportScalarObject(object obj, int maxDepth, string[] keywords)
        {
            var d = new Dictionary<string, object>();
            if (obj == null) return d;

            Type t = obj.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var names = new HashSet<string>();

            foreach (FieldInfo f in t.GetFields(flags))
            {
                if (names.Contains(f.Name)) continue;
                if (!KeywordMatch(f.Name, keywords)) continue;
                names.Add(f.Name);

                try
                {
                    object v = f.GetValue(obj);
                    object scalar = Scalar(v);
                    if (scalar != null) d[f.Name] = scalar;
                    else if (maxDepth > 1 && v != null && IsSmallObject(v.GetType()))
                        d[f.Name] = ExportScalarObject(v, maxDepth - 1, keywords);
                }
                catch { }
            }

            foreach (PropertyInfo p in t.GetProperties(flags))
            {
                if (p.GetIndexParameters().Length != 0) continue;
                if (names.Contains(p.Name)) continue;
                if (!KeywordMatch(p.Name, keywords)) continue;
                names.Add(p.Name);

                try
                {
                    object v = p.GetValue(obj, null);
                    object scalar = Scalar(v);
                    if (scalar != null) d[p.Name] = scalar;
                    else if (maxDepth > 1 && v != null && IsSmallObject(v.GetType()))
                        d[p.Name] = ExportScalarObject(v, maxDepth - 1, keywords);
                }
                catch { }
            }

            return d;
        }

        private static bool KeywordMatch(string name, string[] keywords)
        {
            if (keywords == null || keywords.Length == 0) return true;
            string lower = name.ToLowerInvariant();

            foreach (string k in keywords)
            {
                if (lower.Contains(k.ToLowerInvariant())) return true;
            }
            return false;
        }

        private static bool IsSmallObject(Type t)
        {
            if (t == null) return false;
            if (t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal))
                return false;
            if (typeof(IEnumerable).IsAssignableFrom(t))
                return false;
            return true;
        }

        private static object Scalar(object value)
        {
            if (value == null) return null;

            Type t = value.GetType();
            if (t.IsEnum) return value.ToString();

            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                case TypeCode.Char:
                    return value;
                default:
                    return null;
            }
        }

        private static List<object> ExportSimpleSequence(object seq)
        {
            var list = new List<object>();
            foreach (object x in Enumerate(seq))
            {
                object scalar = Scalar(x);
                if (scalar != null) list.Add(scalar);
            }
            return list;
        }

        private static Dictionary<string, object> ExportCelestialIdentity(object obj)
        {
            var d = new Dictionary<string, object>();
            if (obj == null) return d;

            d["id"] = Scalar(GetMember(obj, "id", "planetId", "starId"));
            d["name"] = Scalar(GetMember(obj, "displayName", "name"));
            d["type"] = ToStr(GetMember(obj, "type", "typeString", "planetType", "spectr"));
            d["starId"] = Scalar(GetMember(obj, "starId"));
            d["index"] = Scalar(GetMember(obj, "index"));
            d["orbitRadius"] = Scalar(GetMember(obj, "orbitRadius"));
            d["luminosity"] = Scalar(GetMember(obj, "luminosity"));
            d["windStrength"] = Scalar(GetMember(obj, "windStrength"));
            d["solarEnergyMultiplier"] = Scalar(GetMember(obj, "solarEnergyMultiplier"));

            return d;
        }

        private static string ToStr(object value)
        {
            if (value == null) return null;
            try { return value.ToString(); }
            catch { return null; }
        }

        internal static int ToInt(object value)
        {
            if (value == null) return 0;
            try { return Convert.ToInt32(value, CultureInfo.InvariantCulture); }
            catch { return 0; }
        }

        internal static long ToLong(object value)
        {
            if (value == null) return 0L;
            try { return Convert.ToInt64(value, CultureInfo.InvariantCulture); }
            catch { return 0L; }
        }

        internal static double ToDouble(object value)
        {
            if (value == null) return 0.0;
            try { return Convert.ToDouble(value, CultureInfo.InvariantCulture); }
            catch { return 0.0; }
        }

        internal static bool ToBool(object value)
        {
            if (value == null) return false;
            try
            {
                return Convert.ToBoolean(
                    value, CultureInfo.InvariantCulture);
            }
            catch { return false; }
        }

        internal static string ItemName(int itemId)
        {
            string name;
            return ItemNames.TryGetValue(itemId, out name) ? name : null;
        }

        internal static string RecipeName(int recipeId)
        {
            string name;
            return RecipeNames.TryGetValue(recipeId, out name) ? name : null;
        }

        private static string SafeFileName(string s)
        {
            if (String.IsNullOrEmpty(s)) return "";
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }

        private static void TryPopup(string message)
        {
            try
            {
                Type t = FindType("UIRealtimeTip");
                if (t == null) return;

                MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (MethodInfo m in methods)
                {
                    if (m.Name != "Popup") continue;
                    ParameterInfo[] ps = m.GetParameters();
                    if (ps.Length == 0 || ps[0].ParameterType != typeof(string)) continue;

                    object[] args = new object[ps.Length];
                    args[0] = message;
                    for (int i = 1; i < ps.Length; i++)
                    {
                        if (ps[i].HasDefaultValue) args[i] = ps[i].DefaultValue;
                        else if (ps[i].ParameterType == typeof(bool)) args[i] = false;
                        else if (ps[i].ParameterType == typeof(int)) args[i] = 0;
                        else if (ps[i].ParameterType.IsValueType) args[i] = Activator.CreateInstance(ps[i].ParameterType);
                        else args[i] = null;
                    }

                    m.Invoke(null, args);
                    return;
                }
            }
            catch { }
        }
    }

    // ------------------------------------------------------------------------
    // Minimal JSON writer. It only serializes the dictionaries/lists/scalars
    // created by this plugin, so it cannot accidentally walk the live game
    // object graph or create a giant/cyclic dump.
    // ------------------------------------------------------------------------
    internal static class Json
    {
        public static string Stringify(object value)
        {
            var sb = new StringBuilder(1024 * 1024);
            WriteValue(sb, value, 0);
            sb.Append('\n');
            return sb.ToString();
        }

        private static void WriteValue(StringBuilder sb, object value, int indent)
        {
            if (value == null)
            {
                sb.Append("null");
                return;
            }

            string s = value as string;
            if (s != null)
            {
                WriteString(sb, s);
                return;
            }

            if (value is bool)
            {
                sb.Append((bool)value ? "true" : "false");
                return;
            }

            Type t = value.GetType();
            if (IsNumber(t))
            {
                if (value is double && (Double.IsNaN((double)value) || Double.IsInfinity((double)value)))
                {
                    sb.Append("null");
                    return;
                }
                if (value is float && (Single.IsNaN((float)value) || Single.IsInfinity((float)value)))
                {
                    sb.Append("null");
                    return;
                }

                sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            IDictionary dict = value as IDictionary;
            if (dict != null)
            {
                WriteObject(sb, dict, indent);
                return;
            }

            IEnumerable seq = value as IEnumerable;
            if (seq != null)
            {
                WriteArray(sb, seq, indent);
                return;
            }

            WriteString(sb, value.ToString());
        }

        private static void WriteObject(StringBuilder sb, IDictionary dict, int indent)
        {
            sb.Append("{");
            bool first = true;

            foreach (DictionaryEntry entry in dict)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\n");
                Indent(sb, indent + 1);
                WriteString(sb, Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
                sb.Append(": ");
                WriteValue(sb, entry.Value, indent + 1);
            }

            if (!first)
            {
                sb.Append("\n");
                Indent(sb, indent);
            }
            sb.Append("}");
        }

        private static void WriteArray(StringBuilder sb, IEnumerable seq, int indent)
        {
            sb.Append("[");
            bool first = true;

            foreach (object item in seq)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\n");
                Indent(sb, indent + 1);
                WriteValue(sb, item, indent + 1);
            }

            if (!first)
            {
                sb.Append("\n");
                Indent(sb, indent);
            }
            sb.Append("]");
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            if (s == null)
            {
                sb.Append("null");
                return;
            }

            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32)
                            sb.Append("\\u" + ((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        private static bool IsNumber(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        private static void Indent(StringBuilder sb, int n)
        {
            for (int i = 0; i < n; i++) sb.Append("  ");
        }
    }
}
