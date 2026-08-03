using System;

/// <param name="p1">
/// 1-minute production rate (items/min). 
/// Source: FactoryStatData.itemRegisterPool[itemId].p1 (or index 0 count / 1.0f)
/// </param>
/// <param name="p10">
/// 10-minute production rate (items/min). 
/// Source: FactoryStatData.itemRegisterPool[itemId].p10 (or index 1 count / 10.0f)
/// </param>
/// <param name="p60">
/// 1-hour production rate (items/min). 
/// Source: FactoryStatData.itemRegisterPool[itemId].p60 (or index 2 count / 60.0f)
/// </param>
/// <param name="c1">
/// 1-minute consumption rate (items/min). 
/// Source: FactoryStatData.itemRegisterPool[itemId].c1
/// </param>
/// <param name="c10">
/// 10-minute consumption rate (items/min). 
/// Source: FactoryStatData.itemRegisterPool[itemId].c10
/// </param>
/// <param name="storageCurrent">
/// Aggregated item count in standard storage chests (StorageComponent) on the planet.
/// Calculation: Sum of StorageComponent.GetItemCount(itemId)
/// </param>
/// <param name="storageMax">
/// Aggregated total item capacity in allocated storage chest slots on the planet.
/// Calculation: Sum of (AllocatedSlots * ItemStackSize) for chests containing itemId.
/// </param>
/// <param name="ilsStockCurrent">
/// Aggregated item count stored in PLS/ILS towers (StationComponent) on the planet.
/// Calculation: Sum of StationStore.count where StationStore.itemId == itemId.
/// </param>
/// <param name="ilsStockMax">
/// Aggregated maximum configured capacity in PLS/ILS towers for this item.
/// Calculation: Sum of StationStore.max where StationStore.itemId == itemId.
/// </param>

namespace DSPGuideCheckMod.Analytics
{
    /// <summary>
    /// Computes continuous supply shortage and production stoppage risk using single-snapshot 
    /// multi-window statistics provided natively by Dyson Sphere Program's GameStatData.
    /// 
    /// REVISED EDITION: Fixed startup baseline zero-traps and logistics capacity scaling.
    /// </summary>
    public static class ProductionRiskAnalyzer
    {
        private const float EPSILON = 1e-4f;
        
        // Minimal production baseline (1 item/min) to consider a factory line "historically established".
        private const float ESTABLISHED_LINE_THRESHOLD = 1.0f; 

        // Minimum buffer runway (in seconds) required from logistics stock to trigger dampening.
        private const float SAFE_LOGISTICS_RUNWAY_SECONDS = 30.0f;

        /// <summary>
        /// Calculates a continuous risk score between 0.0 (Healthy/Normal) and 1.0 (Critical Shortage).
        /// </summary>
        public static float ComputeShortageRisk(
            float p1, float p10, float p60,
            float c1, float c10,
            float storageCurrent, float storageMax,
            float ilsStockCurrent, float ilsStockMax)
        {
            // =========================================================================
            // 1. DEFICIT DIVERGENCE CIRCUIT BREAKER
            // Immediate return: If current production satisfies current consumption,
            // downstream demand is met and no shortage exists.
            // =========================================================================
            if (c1 <= p1)
            {
                return 0.0f;
            }

            // =========================================================================
            // 2. SYSTEMIC DECAY & STARTUP FALLBACK (S)
            // Evaluates capacity collapse OR raw unmet demand if line is newly placed.
            // =========================================================================
            float systemicDecay;
            float historicalBaseline = Math.Max(p10, p60);

            if (historicalBaseline < ESTABLISHED_LINE_THRESHOLD)
            {
                // [FIX ISSUE #1]: Line has no historical baseline (new startup / low p10).
                // Instead of letting rawDrop clamp to 0 and kill the risk signal, we set decay 
                // equal to the immediate unmet demand ratio: (c1 - p1) / c1.
                systemicDecay = Math.Clamp((c1 - p1) / Math.Max(c1, EPSILON), 0.0f, 1.0f);
            }
            else
            {
                // Line is established. Calculate drop relative to historical capability.
                float rawDrop = Math.Clamp(1.0f - (p1 / historicalBaseline), 0.0f, 1.0f);
                systemicDecay = rawDrop * rawDrop; // Parabolic squashing for minor fluctuations
            }

            // =========================================================================
            // 3. DEFICIT DIVERGENCE (D)
            // [FIX ISSUE #3]: Include c1 in the denominator so sudden consumption spikes
            // aren't masked by historical low stats.
            // =========================================================================
            float demandScale = Math.Max(c1, Math.Max(c10, Math.Max(p10, EPSILON)));
            float deficitDivergence = Math.Clamp((c1 - p1) / demandScale, 0.0f, 1.0f);

            // =========================================================================
            // 4. BUFFER EXPOSURE & SCALED LOGISTICS RUNWAY (B)
            // =========================================================================
            
            // Local chest depletion ratio [0.0 = Full, 1.0 = Completely Empty / No Storage]
            float localDepletion = 1.0f;
            if (storageMax > EPSILON)
            {
                localDepletion = 1.0f - Math.Clamp(storageCurrent / storageMax, 0.0f, 1.0f);
            }

            // [FIX ISSUE #2]: Logistics dampening is now tied to actual RUNWAY SECONDS 
            // rather than raw fill percentage.
            float ilsRunwaySeconds = (ilsStockCurrent / Math.Max(c1, EPSILON)) * 60.0f;
            
            // Dampener scales linearly from 0.0 (no effect if runway < 0s) up to 1.0 (full dampening if runway >= 30s)
            float logisticsSafetyFactor = Math.Clamp(ilsRunwaySeconds / SAFE_LOGISTICS_RUNWAY_SECONDS, 0.0f, 1.0f);
            
            // Exposure decreases only if logistics stations offer a meaningful runway time buffer
            float bufferExposure = localDepletion * (1.0f - logisticsSafetyFactor);

            // =========================================================================
            // 5. COMPOSITE RISK SCORE
            // =========================================================================
            float finalRisk = systemicDecay * deficitDivergence * bufferExposure;

            return Math.Clamp(finalRisk, 0.0f, 1.0f);
        }
    }
}

/*
   IMPLEMENTATION HINTS & DSP ENGINE PITFALLS:
   
   1. TIMING / PERFORMANCE:
      Do NOT call this every frame. Aggregate factoryStorage and stationPool 
      stocks on a 1-second or 2-second timer tick (e.g., every 60 or 120 GameMain.tick).

   2. ARRAY SAFETY:
      DSP itemId values are non-contiguous (e.g. 1001, 1104). Ensure itemId is 
      within bounds of itemRegisterPool.Length and check for null elements before reading.

   3. SCOPE:
      Use PlanetFactory.factoryStat for local planet diagnostics. 
      Use GameMain.statistics.gameStat for cluster-wide totals.
*/