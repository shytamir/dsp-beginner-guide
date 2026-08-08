using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class ProductionRiskInput
    {
        public int ItemId;
        public string Name;
        public string Scope;
        public int PlanetId;
        public string PlanetName;
        public bool OneMinuteAvailable;
        public double ProducedPerMinute;
        public double ConsumedPerMinute;
        public bool TenMinuteAvailable;
        public bool TenMinuteReady;
        public double TenMinuteProducedPerMinute;
        public double TenMinuteConsumedPerMinute;
        public bool RunwayAvailable;
        public double RunwayMinutes;
        public double AccessibleCount;
        public string BackpressureStatus;
        public double ExactTargetPerMinute;
        public double DemandCeilingPerMinute;
    }

    internal sealed class ProductionRiskResult
    {
        public int ItemId;
        public string Name;
        public string Scope;
        public int PlanetId;
        public string PlanetName;
        public string State;
        public string Severity;
        public bool Actionable;
        public double Score;
        public double BaselinePerMinute;
        public double Drop;
        public double Thinness;
        public bool DemandDeficit;
        public bool TargetDeficit;
        public bool TargetSatisfied;
        public bool RunwayAvailable;
        public double RunwayMinutes;
        public bool DepletionMinutesAvailable;
        public double DepletionMinutes;
        public string BackpressureStatus;
        public double ProducedPerMinute;
        public double ConsumedPerMinute;
        public double ExactTargetPerMinute;

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object> {
                { "itemId", ItemId },
                { "name", Name },
                { "scope", Scope },
                { "planetId", PlanetId > 0 ? (object)PlanetId : null },
                { "planetName", PlanetName },
                { "state", State },
                { "severity", Severity },
                { "actionable", Actionable },
                { "score", Math.Round(Score, 4) },
                { "baselinePerMinute", Math.Round(BaselinePerMinute, 3) },
                { "drop", Math.Round(Drop, 4) },
                { "thinness", Math.Round(Thinness, 4) },
                { "demandDeficit", DemandDeficit },
                { "targetDeficit", TargetDeficit },
                { "targetSatisfied", TargetSatisfied },
                { "producedPerMinute", Math.Round(ProducedPerMinute, 3) },
                { "consumedPerMinute", Math.Round(ConsumedPerMinute, 3) },
                { "exactTargetPerMinute", ExactTargetPerMinute > 0.0
                    ? (object)Math.Round(ExactTargetPerMinute, 3) : null },
                { "runwayAvailable", RunwayAvailable },
                { "runwayMinutes", RunwayAvailable
                    ? (object)Math.Round(RunwayMinutes, 3) : null },
                { "depletionMinutesAvailable", DepletionMinutesAvailable },
                { "depletionMinutes", DepletionMinutesAvailable
                    ? (object)Math.Round(DepletionMinutes, 3) : null },
                { "backpressureStatus", BackpressureStatus }
            };
        }
    }

    /// <summary>
    /// Pure production-risk calculation. Inputs are normalized, scope-matched
    /// items-per-minute rates and conservative accessible-buffer evidence.
    /// </summary>
    internal static class ProductionRiskAnalyzer
    {
        private const double Epsilon = 0.001;
        private const double RelativeTolerance = 0.05;
        private const double AbsoluteTolerance = 0.5;
        private const double RunwayFloorMinutes = 0.5;
        private const double StarvedRunwayMinutes = 0.05;
        private const double StarvedScore = 0.7;

        public static ProductionRiskResult Evaluate(ProductionRiskInput input)
        {
            var result = new ProductionRiskResult {
                ItemId = input.ItemId,
                Name = input.Name,
                Scope = input.Scope,
                PlanetId = input.PlanetId,
                PlanetName = input.PlanetName,
                State = "unknown",
                Severity = "quiet",
                BackpressureStatus = String.IsNullOrEmpty(input.BackpressureStatus)
                    ? "unknown" : input.BackpressureStatus,
                ProducedPerMinute = Math.Max(0.0, input.ProducedPerMinute),
                ConsumedPerMinute = Math.Max(0.0, input.ConsumedPerMinute),
                ExactTargetPerMinute = Math.Max(0.0, input.ExactTargetPerMinute),
                RunwayAvailable = input.RunwayAvailable,
                RunwayMinutes = Math.Max(0.0, input.RunwayMinutes)
            };

            if (!input.OneMinuteAvailable)
                return result;

            if (String.Equals(
                result.BackpressureStatus,
                "proven",
                StringComparison.OrdinalIgnoreCase))
            {
                result.State = "backpressured";
                return result;
            }

            if (!input.TenMinuteAvailable)
                return result;
            if (!input.TenMinuteReady)
            {
                result.State = "warming";
                return result;
            }

            result.BaselinePerMinute = Math.Max(
                0.0, input.TenMinuteProducedPerMinute);
            double baseline = Math.Max(result.BaselinePerMinute, Epsilon);
            double dropRaw = Clamp01(
                1.0 - result.ProducedPerMinute / baseline);
            result.Drop = dropRaw * dropRaw;

            if (result.ConsumedPerMinute <= Epsilon)
                result.Thinness = 0.0;
            else if (result.RunwayAvailable)
                result.Thinness = Clamp01(
                    1.0 - result.RunwayMinutes / RunwayFloorMinutes);
            else
                result.Thinness = 1.0;
            result.Score = Clamp01(result.Drop * result.Thinness);

            result.DemandDeficit = !DemandSatisfied(
                result.ProducedPerMinute,
                result.ConsumedPerMinute,
                input.DemandCeilingPerMinute);
            result.TargetDeficit = result.ExactTargetPerMinute > 0.0 &&
                ExceedsTolerance(
                    result.ExactTargetPerMinute,
                    result.ProducedPerMinute);
            result.TargetSatisfied = result.ExactTargetPerMinute > 0.0 &&
                result.ProducedPerMinute >= result.ExactTargetPerMinute;

            if (result.TargetSatisfied)
            {
                result.State = "balanced";
                return result;
            }

            double netDepletionPerMinute =
                result.ConsumedPerMinute - result.ProducedPerMinute;
            if (result.RunwayAvailable && result.DemandDeficit &&
                netDepletionPerMinute > Tolerance(result.ConsumedPerMinute))
            {
                result.DepletionMinutesAvailable = true;
                result.DepletionMinutes = Math.Max(
                    0.0,
                    input.AccessibleCount / netDepletionPerMinute);
            }

            if (!result.DemandDeficit && !result.TargetDeficit)
            {
                result.State = "balanced";
                return result;
            }

            bool collapsed = result.BaselinePerMinute > AbsoluteTolerance &&
                result.ProducedPerMinute <= Tolerance(result.BaselinePerMinute);
            bool emptyDemandBuffer = result.DemandDeficit &&
                result.RunwayAvailable &&
                result.RunwayMinutes <= StarvedRunwayMinutes;
            if ((emptyDemandBuffer &&
                    (collapsed || result.Score >= StarvedScore)) ||
                (result.TargetDeficit && collapsed))
            {
                result.State = "starved";
                result.Severity = "red";
                result.Actionable = true;
                return result;
            }

            result.State = "draining";
            result.Severity = "amber";
            result.Actionable = true;
            return result;
        }

        private static bool ExceedsTolerance(double expected, double actual)
        {
            return expected - actual > Tolerance(expected);
        }

        internal static bool DemandSatisfied(
            double producedPerMinute,
            double consumedPerMinute,
            double demandCeilingPerMinute)
        {
            double referenceDemand = Math.Max(0.0, consumedPerMinute);
            if (demandCeilingPerMinute > 0.0)
            {
                referenceDemand = Math.Min(
                    referenceDemand,
                    demandCeilingPerMinute);
            }
            return !ExceedsTolerance(
                referenceDemand,
                Math.Max(0.0, producedPerMinute));
        }

        private static double Tolerance(double reference)
        {
            return Math.Max(
                AbsoluteTolerance,
                Math.Abs(reference) * RelativeTolerance);
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0) return 0.0;
            if (value > 1.0) return 1.0;
            return value;
        }
    }
}
