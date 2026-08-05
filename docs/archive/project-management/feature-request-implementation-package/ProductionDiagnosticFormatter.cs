using System;

namespace DSPGuideCheckMod.Analytics
{
    /// <summary>
    /// Severity classification tiers for player-facing diagnostic indicators.
    /// </summary>
    public enum DiagnosticSeverity
    {
        /// <summary>Risk &lt; 0.3, C1 &lt;= P1. Green/Cyan.</summary>
        Balanced,

        /// <summary>Risk &lt; 0.3, C1 &gt; P1 but covered by ample buffer or direct flow. Green.</summary>
        Sufficient,

        /// <summary>P10 &lt; 1.0, C1 &gt; P1. Orange/Yellow (Startup phase).</summary>
        Ramping,

        /// <summary>0.3 &lt;= Risk &lt; 0.7, C1 &gt; P1. Yellow/Amber.</summary>
        Draining,

        /// <summary>Risk &gt;= 0.7, Severe drop/halt &amp; buffers empty. Red.</summary>
        Starved
    }

    /// <summary>
    /// Holds the formatted diagnostic output ready for direct binding to uGUI Text components.
    /// </summary>
    public struct ProductionDiagnosticResult
    {
        public DiagnosticSeverity Severity;
        public float CalculatedRisk;
        public string StatusLabel;       // "STARVED", "DRAINING", "RAMPING", "SUFFICIENT", "BALANCED"
        public string HexColor;          // Color code for uGUI rich text styling (e.g. "#FF3333")
        public string SummaryTitle;      // e.g., "Upstream Supply Failure"
        public string PrimaryIssueText;  // Detailed explanation of the primary math driver
        public string ActionableHint;    // Direct player recommendation
        public string LocalRateText;     // Formatted throughput text (e.g., "120 / 600 per min")
        public string BufferRunwayText;  // Formatted runway text (e.g., "0s remaining (Chests & ILS empty)")
    }

    /// <summary>
    /// Evaluates raw telemetry and calculated shortage risk from ProductionRiskAnalyzer,
    /// translating mathematical states into human-readable diagnostic text and visual color badges.
    /// </summary>
    public static class ProductionDiagnosticFormatter
    {
        private const float EPSILON = 1e-4f;
        private const float ESTABLISHED_LINE_THRESHOLD = 1.0f; // 1 item/min to consider line established

        // Configurable Risk Thresholds (Coupled to ProductionRiskAnalyzer scale)
        public const float RISK_THRESHOLD_HIGH = 0.7f;
        public const float RISK_THRESHOLD_MODERATE = 0.3f;
        
        // Minimum runway (in seconds) to consider a minor deficit safely "buffered"
        public const float BUFFERED_RUNWAY_THRESHOLD_SECONDS = 15.0f;

        // UI Color Palette (DSP Aesthetic Hex Codes)
        public const string COLOR_BALANCED   = "#00FFCC"; // Cyan Accent
        public const string COLOR_SUFFICIENT = "#55FF55"; // Vibrant Green
        public const string COLOR_RAMPING    = "#FF9900"; // Warm Orange
        public const string COLOR_DRAINING   = "#FFCC00"; // Warning Yellow
        public const string COLOR_STARVED    = "#FF3333"; // Alert Red

        /// <summary>
        /// Translates raw statistics and risk metrics into intuitive player diagnostics.
        /// </summary>
        /// <param name="targetRate">
        /// Desired per-minute throughput target defined by the active guide phase.
        /// </param>
        /// <param name="p1">
        /// 1-minute production rate (items/min). Source: FactoryStatData.itemRegisterPool[itemId].pRegister[0]
        /// </param>
        /// <param name="p10">
        /// 10-minute production rate (items/min). Source: FactoryStatData.itemRegisterPool[itemId].pRegister[1] / 10.0f
        /// </param>
        /// <param name="p60">
        /// 1-hour production rate (items/min). Source: FactoryStatData.itemRegisterPool[itemId].pRegister[2] / 60.0f
        /// </param>
        /// <param name="c1">
        /// 1-minute consumption rate (items/min). Source: FactoryStatData.itemRegisterPool[itemId].cRegister[0]
        /// </param>
        /// <param name="c10">
        /// 10-minute consumption rate (items/min). Source: FactoryStatData.itemRegisterPool[itemId].cRegister[1] / 10.0f
        /// </param>
        /// <param name="storageCurrent">
        /// Total item count across standard storage chests (StorageComponent) on local planet.
        /// </param>
        /// <param name="storageMax">
        /// Total item capacity allocated in storage chests on local planet.
        /// </param>
        /// <param name="ilsStockCurrent">
        /// Total item count stored in PLS/ILS towers (StationComponent) on local planet.
        /// </param>
        /// <param name="ilsStockMax">
        /// Total configured maximum capacity across PLS/ILS towers on local planet.
        /// </param>
        /// <param name="calculatedRisk">
        /// Pre-computed risk score [0.0 - 1.0] from ProductionRiskAnalyzer.ComputeShortageRisk().
        /// </param>
        /// <returns>Populated DiagnosticResult struct containing formatted text and color codes.</returns>
        public static ProductionDiagnosticResult Evaluate(
            float targetRate,
            float p1, float p10, float p60,
            float c1, float c10,
            float storageCurrent, float storageMax,
            float ilsStockCurrent, float ilsStockMax,
            float calculatedRisk)
        {
            ProductionDiagnosticResult result = new ProductionDiagnosticResult
            {
                CalculatedRisk = calculatedRisk
            };

            float historicalBaseline = Math.Max(p10, p60);
            float totalStock = storageCurrent + ilsStockCurrent;
            float totalDemand = Math.Max(c1, EPSILON);
            
            // Calculate buffer runway in seconds
            float runwaySeconds = (totalStock / totalDemand) * 60.0f;

            // =========================================================================
            // 1. DIAGNOSTIC DRIVER EVALUATION & STATE SELECTION
            // =========================================================================

            if (c1 <= p1 && calculatedRisk < RISK_THRESHOLD_MODERATE)
            {
                // STATE A: BALANCED
                result.Severity = DiagnosticSeverity.Balanced;
                result.StatusLabel = "BALANCED";
                result.HexColor = COLOR_BALANCED;
                result.SummaryTitle = "Production Satisfactory";
                
                if (targetRate > EPSILON && p1 < targetRate)
                {
                    result.PrimaryIssueText = $"Output meets current demand ({c1:F0}/min), but is below the guide target of {targetRate:F0}/min.";
                    result.ActionableHint = "Expand production facilities to fulfill future phase targets.";
                }
                else
                {
                    result.PrimaryIssueText = "Current output fully satisfies downstream demand.";
                    result.ActionableHint = "No action required. Production line is operating normally.";
                }
            }
            else if (historicalBaseline < ESTABLISHED_LINE_THRESHOLD && c1 > p1)
            {
                // STATE B: RAMPING
                result.Severity = DiagnosticSeverity.Ramping;
                result.StatusLabel = "RAMPING";
                result.HexColor = COLOR_RAMPING;
                result.SummaryTitle = "New Line Bootstrapping";

                if (p1 <= EPSILON)
                {
                    result.PrimaryIssueText = $"New line is completely offline (0/min) while facing active demand ({c1:F1}/min).";
                    result.ActionableHint = "Check power connections, sorter configurations, and initial belt feeds.";
                }
                else
                {
                    result.PrimaryIssueText = $"Production ({p1:F1}/min) is ramping up toward active demand ({c1:F1}/min).";
                    result.ActionableHint = "Ensure all newly placed assemblers are fully fed with input materials.";
                }
            }
            else if (calculatedRisk >= RISK_THRESHOLD_HIGH)
            {
                // STATE C: STARVED
                result.Severity = DiagnosticSeverity.Starved;
                result.StatusLabel = "STARVED";
                result.HexColor = COLOR_STARVED;
                result.SummaryTitle = "Upstream Supply Failure";

                if (p1 < (historicalBaseline * 0.3f))
                {
                    float dropPercent = (1.0f - (p1 / Math.Max(historicalBaseline, EPSILON))) * 100.0f;
                    result.PrimaryIssueText = $"Production collapsed by {dropPercent:F0}% relative to 10m average.";
                }
                else
                {
                    result.PrimaryIssueText = "Downstream consumption severely outstrips production with no buffer safety net.";
                }
                
                result.ActionableHint = "Check raw material supply, power grid saturation, or belt bottlenecks upstream.";
            }
            else if (calculatedRisk >= RISK_THRESHOLD_MODERATE)
            {
                // STATE D: DRAINING
                result.Severity = DiagnosticSeverity.Draining;
                result.StatusLabel = "DRAINING";
                result.HexColor = COLOR_DRAINING;

                if (runwaySeconds <= EPSILON)
                {
                    result.SummaryTitle = "Storage Depleted";
                    result.PrimaryIssueText = $"Consumption ({c1:F1}/min) exceeds output ({p1:F1}/min). Storage buffers are completely dry.";
                    result.ActionableHint = "Increase upstream production capacity immediately to eliminate active deficit.";
                }
                else
                {
                    result.SummaryTitle = "Buffer Deficit Cushion";
                    result.PrimaryIssueText = $"Consumption ({c1:F1}/min) exceeds output ({p1:F1}/min). Storage is actively depleting.";
                    result.ActionableHint = "Increase upstream production capacity before storage buffers run dry.";
                }
            }
            else
            {
                // STATE E: SUFFICIENT
                result.Severity = DiagnosticSeverity.Sufficient;
                result.StatusLabel = "SUFFICIENT";
                result.HexColor = COLOR_SUFFICIENT;

                if (runwaySeconds >= BUFFERED_RUNWAY_THRESHOLD_SECONDS)
                {
                    result.SummaryTitle = "Buffered Deficit Coverage";
                    result.PrimaryIssueText = $"Minor consumption excess ({c1:F1}/min vs {p1:F1}/min) is comfortably absorbed by storage reserves.";
                    result.ActionableHint = "Monitor buffer levels over time; no immediate action required.";
                }
                else
                {
                    result.SummaryTitle = "Unbuffered Flow";
                    result.PrimaryIssueText = "Demand is met by direct belt flow, but local storage buffer is minimal or non-existent.";
                    result.ActionableHint = "Consider placing a small buffer chest or ILS slot to absorb temporary supply hiccups.";
                }
            }

            // =========================================================================
            // 2. TEXT FORMATTING (LOCAL RATES & RUNWAY)
            // =========================================================================

            result.LocalRateText = targetRate > EPSILON 
                ? $"{p1:F0} / {targetRate:F0} per min" 
                : $"{p1:F0} per min";

            if (runwaySeconds <= EPSILON)
            {
                result.BufferRunwayText = "0s remaining (Chests & ILS are empty)";
            }
            else if (runwaySeconds >= 3600.0f)
            {
                result.BufferRunwayText = "> 1 hour buffer available";
            }
            else if (runwaySeconds >= 60.0f)
            {
                result.BufferRunwayText = $"~{(runwaySeconds / 60.0f):F1}m buffer remaining";
            }
            else
            {
                result.BufferRunwayText = $"~{runwaySeconds:F0}s buffer remaining";
            }

            return result;
        }
    }
}

/*
================================================================================
IMPLEMENTATION HINTS & DSP ENGINE PITFALLS FOR THE DEVELOPER
================================================================================

1. UI LAYOUT & RICH TEXT BINDING:
   - Use Unity's rich text tags to bind colors directly to text components:
     string headerString = $"<color={result.HexColor}>[{result.StatusLabel}]</color> {itemName}";
   - For 4K resolution usability, ensure the collapsed Status Pip image element uses 
     a minimum width/thickness of 4px (e.g., LayoutElement.minWidth = 4f) so high-DPI 
     displays don't render it as a microscopic hairline.

2. NON-ALLOCATING ACCORDION UPDATES (PREVENTING GC STUTTERS):
   - Do NOT construct new string objects inside Update() or FixedUpdate().
   - Run the Evaluate() method ONLY when:
     a) The drawer is expanded and the 1-2 second evaluation timer ticks.
     b) The user clicks to expand an accordion row.
   - Cache formatted strings or use standard StringBuilder buffers if updating multiple 
     rows in batch to avoid triggering Unity garbage collector spikes.

3. DYNAMIC CONTAINER RESIZING (uGUI Setup):
   - To make the accordion expand smoothly without manual offset math:
     * Parent Container: Requires VerticalLayoutGroup + ContentSizeFitter (Vertical Fit = Preferred Size).
     * Item Row: Contains Header Button + Collapsible Child Drawer (GameObject active toggle).
     * Child Drawer: Contains Text components for PrimaryIssueText, ActionableHint, LocalRateText.
   - Calling childDrawer.SetActive(!childDrawer.activeSelf) will automatically cause 
     the parent ScrollRect to adjust scroll height on the next UI layout pass.

4. SCOPE BINDING (Planet vs Global):
   - Remember that p1/c1 values passed into Evaluate() should represent the LOCAL 
     PlanetFactory stats for targeted diagnostics.
   - Displaying Global rates alongside local rates (e.g., "Local: 120/min | Global: 1,200/min") 
     is helpful for players, but local rates must drive the DiagnosticSeverity evaluation 
     to ensure planet-specific shortages aren't masked by off-world production.
================================================================================
*/