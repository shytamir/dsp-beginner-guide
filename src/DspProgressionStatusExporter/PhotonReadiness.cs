using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class SustainedInputReadiness
    {
        public int ItemId;
        public double ElapsedGameSeconds;
        public int SampleCount;
        public double MinimumRate;
        public string Reason = "unavailable";
        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object> {
                { "itemId", ItemId }, { "elapsedGameSeconds", ElapsedGameSeconds },
                { "sampleCount", SampleCount }, { "minimumRate", SampleCount > 0 ? (object)MinimumRate : null },
                { "reason", Reason }, { "targetPerMinute", 40 },
                { "source", "Sampled native one-minute production aggregates; not per-second throughput." }
            };
        }
    }

    internal sealed class PhotonReadiness
    {
        internal static readonly int[] ItemIds = { 6001, 6002, 6003, 6004, 6005, 1122 };
        private sealed class Point { public long Tick; public double Rate; }
        private readonly Dictionary<int, List<Point>> history = new Dictionary<int, List<Point>>();
        private long lastTick = -1;

        public void Clear() { history.Clear(); lastTick = -1; }

        public void Sample(long gameTick, Dictionary<int, double> rates)
        {
            if (gameTick < lastTick) Clear();
            lastTick = gameTick;
            foreach (int id in ItemIds)
            {
                List<Point> points;
                if (!history.TryGetValue(id, out points)) history[id] = points = new List<Point>();
                double rate;
                if (rates == null || !rates.TryGetValue(id, out rate) || Double.IsNaN(rate) || Double.IsInfinity(rate))
                { points.Clear(); continue; }
                if (points.Count > 0 && points[points.Count - 1].Tick == gameTick)
                    points[points.Count - 1].Rate = rate;
                else points.Add(new Point { Tick = gameTick, Rate = rate });
                // Keep the sample on/before the window boundary so elapsed time
                // can reach 120 seconds despite small cadence variations.
                long boundary = gameTick - 7200;
                while (points.Count > 1 && points[1].Tick <= boundary) points.RemoveAt(0);
                while (points.Count > 26) points.RemoveAt(0);
            }
        }

        public SustainedInputReadiness Evaluate(int itemId)
        {
            var result = new SustainedInputReadiness { ItemId = itemId };
            List<Point> points;
            if (!history.TryGetValue(itemId, out points) || points.Count == 0) return result;
            result.SampleCount = points.Count;
            result.ElapsedGameSeconds = (points[points.Count - 1].Tick - points[0].Tick) / 60.0;
            result.MinimumRate = Double.MaxValue;
            foreach (Point point in points) result.MinimumRate = Math.Min(result.MinimumRate, point.Rate);
            result.Reason = result.MinimumRate < 40 ? "below-target"
                : result.ElapsedGameSeconds >= 120 && points.Count >= 20 ? "ready" : "warming";
            return result;
        }

        public List<object> Export()
        {
            var result = new List<object>();
            foreach (int id in ItemIds) result.Add(Evaluate(id).Export());
            return result;
        }
    }
}
