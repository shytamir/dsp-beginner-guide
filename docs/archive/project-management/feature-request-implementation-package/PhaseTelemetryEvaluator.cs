using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DSPGuideCheckMod.Analytics;
using DSPGuideCheckMod.UI;

namespace DSPGuideCheckMod.Telemetry
{
    /// <summary>
    /// Background evaluation ticker that polls active phase objectives on a 1.5s cadence.
    /// Aggregates individual shortage risk scores and updates global health indicators
    /// (both expanded navigation pip and hanging collapsed rate frame).
    /// </summary>
    public class PhaseTelemetryEvaluator : MonoBehaviour
    {
        [Header("Evaluation Settings")]
        [SerializeField, Range(0.5f, 5.0f)] 
        private float evaluationCadence = 1.5f;

        [Header("UI Target Bindings")]
        [SerializeField] private GuideNavigationController navController;
        [SerializeField] private Image hangingFramePip; // 4px border glow on hanging rate column

        // Runtime State
        private float _cadenceTimer = 0.0f;
        private GuidePhaseData _activePhaseData;
        private Dictionary<int, float> _phaseTargetRates = new Dictionary<int, float>();

        // Fallback color if TryParseHtmlString fails
        private static readonly Color FALLBACK_PIP_COLOR = new Color(0.0f, 1.0f, 0.8f, 1.0f); // Cyan

        // Cache evaluated result to avoid unnecessary UI redraws
        public DiagnosticSeverity CurrentWorstSeverity { get; private set; } = DiagnosticSeverity.Balanced;

        #region Unity Lifecycle

        private void Update()
        {
            if (_activePhaseData == null || _activePhaseData.TargetItemIds == null || _activePhaseData.TargetItemIds.Count == 0)
            {
                return;
            }

            _cadenceTimer += Time.deltaTime;
            if (_cadenceTimer >= evaluationCadence)
            {
                _cadenceTimer = 0.0f;
                EvaluateActivePhaseHealth();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Binds the active phase and optional per-item target rates for evaluation.
        /// Triggers an immediate zero-delay evaluation tick.
        /// </summary>
        public void SetActivePhase(GuidePhaseData phaseData, Dictionary<int, float> itemTargetRates = null)
        {
            _activePhaseData = phaseData;
            _phaseTargetRates = itemTargetRates ?? new Dictionary<int, float>();
            _cadenceTimer = 0.0f;
            
            // Execute immediate evaluation for zero-delay UI response upon switching phases
            EvaluateActivePhaseHealth();
        }

        /// <summary>
        /// Binds the UI image element on DSP's hanging cube rate panel for collapsed state glow.
        /// </summary>
        public void BindHangingFramePip(Image hangingImage)
        {
            hangingFramePip = hangingImage;
            ApplyPipColor(CurrentWorstSeverity);
        }

        #endregion

        #region Core Evaluation Logic

        /// <summary>
        /// Scans active phase target items across local planet statistics, computes shortage risk,
        /// and determines the single highest severity score.
        /// </summary>
        private void EvaluateActivePhaseHealth()
        {
            // Engine context validation
            if (GameMain.mainPlayer == null || GameMain.mainPlayer.factory == null)
            {
                return;
            }

            PlanetFactory factory = GameMain.mainPlayer.factory;
            FactoryStatData statData = GameMain.history?.GetPlanetFactoryStatData(factory.planetId);

            if (statData == null || statData.itemRegisterPool == null)
            {
                return;
            }

            DiagnosticSeverity worstSeverity = DiagnosticSeverity.Balanced;

            for (int i = 0; i < _activePhaseData.TargetItemIds.Count; i++)
            {
                int itemId = _activePhaseData.TargetItemIds[i];
                
                // Bounds check against item register pool length
                if (itemId <= 0 || itemId >= statData.itemRegisterPool.Length)
                {
                    continue;
                }

                var itemRegister = statData.itemRegisterPool[itemId];
                if (itemRegister == null)
                {
                    continue;
                }

                // Extract raw production rates (Already normalized to items/min by DSP stat engine)
                // Register time indices: [0] = 1m, [1] = 10m, [2] = 60m
                float p1  = itemRegister.pRegister[0];
                float p10 = itemRegister.pRegister[1];
                float p60 = itemRegister.pRegister[2];

                // Extract raw consumption rates
                float c1  = itemRegister.cRegister[0];
                float c10 = itemRegister.cRegister[1];

                // Query planet storage and PLS/ILS stock with true proto stack limits
                GetLocalInventoryBuffers(factory, itemId, out float storageCurrent, out float storageMax, out float ilsCurrent, out float ilsMax);

                // Target rate configured by phase (used solely by diagnostic formatter for text advice)
                float targetRate = _phaseTargetRates.TryGetValue(itemId, out float rate) ? rate : 0.0f;

                // Compute stateless risk score [0.0 - 1.0] using correct function signature
                float risk = ProductionRiskAnalyzer.ComputeShortageRisk(
                    p1, p10, p60,
                    c1, c10,
                    storageCurrent, storageMax,
                    ilsCurrent, ilsMax
                );

                // Format state to extract severity tier
                ProductionDiagnosticResult diag = ProductionDiagnosticFormatter.Evaluate(
                    targetRate, p1, p10, p60, c1, c10,
                    storageCurrent, storageMax, ilsCurrent, ilsMax, risk
                );

                // Track worst severity ordinal (Starved > Draining > Ramping > Sufficient > Balanced)
                if (diag.Severity > worstSeverity)
                {
                    worstSeverity = diag.Severity;
                }
            }

            CurrentWorstSeverity = worstSeverity;

            // Update UI targets
            if (navController != null)
            {
                navController.UpdateStatusPip(CurrentWorstSeverity);
            }

            ApplyPipColor(CurrentWorstSeverity);
        }

        #endregion

        #region Helper Methods & Inventory Resolution

        /// <summary>
        /// Queries local planet chests and PLS/ILS towers, looking up item prototype stack sizes 
        /// to accurately compute storage capacity.
        /// </summary>
        private void GetLocalInventoryBuffers(PlanetFactory factory, int itemId, out float storageCurrent, out float storageMax, out float ilsCurrent, out float ilsMax)
        {
            storageCurrent = 0.0f;
            storageMax = 0.0f;
            ilsCurrent = 0.0f;
            ilsMax = 0.0f;

            if (factory == null) return;

            // Lookup actual item stack size from DSP Database
            ItemProto proto = LDB.items?.Select(itemId);
            int stackSize = (proto != null && proto.StackSize > 0) ? proto.StackSize : 50;

            // 1. Scan local planet factory storage chests
            if (factory.factoryStorage != null)
            {
                var storagePool = factory.factoryStorage.storagePool;
                if (storagePool != null)
                {
                    for (int i = 1; i < factory.factoryStorage.storageCursor; i++)
                    {
                        if (storagePool[i] != null && storagePool[i].id == i && storagePool[i].itemId == itemId)
                        {
                            storageCurrent += storagePool[i].GetItemCount(itemId);
                            storageMax += storagePool[i].size * stackSize; // Accurate capacity calculation
                        }
                    }
                }
            }

            // 2. Scan local PLS/ILS transport hubs
            if (factory.transport != null && factory.transport.stationPool != null)
            {
                for (int i = 1; i < factory.transport.stationCursor; i++)
                {
                    var station = factory.transport.stationPool[i];
                    if (station != null && station.id == i && station.storage != null)
                    {
                        for (int j = 0; j < station.storage.Length; j++)
                        {
                            if (station.storage[j].itemId == itemId)
                            {
                                ilsCurrent += station.storage[j].count;
                                ilsMax += station.storage[j].max; // Pre-configured max capacity slot limit
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tints the hanging 4px frame pip image based on severity tier. Includes defensive parsing fallback.
        /// </summary>
        private void ApplyPipColor(DiagnosticSeverity severity)
        {
            if (hangingFramePip == null) return;

            string hexCode;
            switch (severity)
            {
                case DiagnosticSeverity.Starved:
                    hexCode = ProductionDiagnosticFormatter.COLOR_STARVED;
                    break;
                case DiagnosticSeverity.Draining:
                    hexCode = ProductionDiagnosticFormatter.COLOR_DRAINING;
                    break;
                case DiagnosticSeverity.Ramping:
                    hexCode = ProductionDiagnosticFormatter.COLOR_RAMPING;
                    break;
                case DiagnosticSeverity.Sufficient:
                    hexCode = ProductionDiagnosticFormatter.COLOR_SUFFICIENT;
                    break;
                default:
                    hexCode = ProductionDiagnosticFormatter.COLOR_BALANCED;
                    break;
            }

            if (ColorUtility.TryParseHtmlString(hexCode, out Color parsedColor))
            {
                hangingFramePip.color = parsedColor;
            }
            else
            {
                hangingFramePip.color = FALLBACK_PIP_COLOR;
            }
        }

        #endregion
    }
}

/*
================================================================================
PARAMETER & FIELD MAPPING SPECIFICATION
================================================================================
| Field / Property        | Source / Target Type       | Purpose & Description |
| :---------------------- | :------------------------- | :-------------------- |
| evaluationCadence       | float (Default: 1.5s)      | Update cadence timer preventing high-frequency frame drops. |
| navController           | GuideNavigationController  | Receives UpdateStatusPip calls for expanded header pip. |
| hangingFramePip         | UnityEngine.UI.Image       | 4px border glow attached to hanging matrix rate panel. |
| CurrentWorstSeverity    | DiagnosticSeverity (Enum)  | Evaluated worst status tier across active phase objectives. |
| itemRegister.pRegister  | float[3] array             | Raw 1m [0], 10m [1], 60m [2] production rates (items/min). |
| itemRegister.cRegister  | float[3] array             | Raw 1m [0], 10m [1], 60m [2] consumption rates (items/min). |
| LDB.items.Select        | ItemProto                  | Reads native stack limit (proto.StackSize) for accurate capacity. |
================================================================================
IMPLEMENTATION HINTS & DSP ENGINE PITFALLS
================================================================================
1. RAW REGISTER RATE ACCURACY:
   - DSP's itemRegisterPool automatically averages pRegister and cRegister slots to items-per-minute.
     Do NOT divide pRegister[1] by 10 or pRegister[2] by 60, as this corrupts baseline calculations.

2. ACCURATE CHEST BUFFER CALCULATIONS:
   - Storage chest capacity varies based on item stack size (e.g., 200 for Iron, 20 for Buildings).
     Multiplying storage.size by LDB.items.Select(itemId).StackSize ensures exact max capacity.

3. CANVAS VERTEX TINTING (PREVENTING MEMORY LEAKS):
   - Always modify hangingFramePip.color rather than hangingFramePip.material.color. Modifying 
     material properties dynamically instantiates new Material instances in Unity memory.

4. PLANET SWITCHING SAFETY:
   - Always fetch GameMain.mainPlayer.factory inside the evaluation tick. If a player warps or 
     switches planets, reading a cached factory reference will query stale or uninitialized arrays.
================================================================================
*/