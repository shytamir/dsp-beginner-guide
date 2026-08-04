using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal sealed class GuidePanelRiskModel
    {
        public string Id;
        public int ItemId;
        public string Name;
        public string State;
        public bool DepletionMinutesAvailable;
        public double DepletionMinutes;
    }

    /// <summary>
    /// Preserves useful risk-row continuity across panel refreshes. Same-level
    /// rate changes never evict an incumbent; a new critical risk may replace
    /// an urgent row, and a phase change starts a fresh bounded selection.
    /// </summary>
    internal sealed class GuidePanelRiskStabilizer
    {
        public const int Limit = 3;

        private string phaseId;
        private readonly List<GuidePanelRiskModel> displayed =
            new List<GuidePanelRiskModel>();

        public void Reset()
        {
            phaseId = null;
            displayed.Clear();
        }

        public List<GuidePanelRiskModel> Select(
            string selectedPhaseId,
            List<GuidePanelRiskModel> orderedCandidates)
        {
            if (!String.Equals(
                phaseId,
                selectedPhaseId,
                StringComparison.OrdinalIgnoreCase))
            {
                phaseId = selectedPhaseId;
                displayed.Clear();
            }

            var retained = new List<GuidePanelRiskModel>();
            foreach (GuidePanelRiskModel incumbent in displayed)
            {
                GuidePanelRiskModel current = Find(
                    orderedCandidates, incumbent.Id);
                if (current != null)
                    retained.Add(current);
            }

            foreach (GuidePanelRiskModel candidate in orderedCandidates)
            {
                if (Find(retained, candidate.Id) != null)
                    continue;
                if (retained.Count < Limit)
                {
                    retained.Add(candidate);
                    continue;
                }

                int candidateRank = Rank(candidate.State);
                int replacement = -1;
                for (int i = retained.Count - 1; i >= 0; i--)
                    if (Rank(retained[i].State) < candidateRank)
                    {
                        replacement = i;
                        break;
                    }
                if (replacement >= 0)
                {
                    retained.RemoveAt(replacement);
                    retained.Add(candidate);
                }
            }

            displayed.Clear();
            AddSeverityBand(displayed, retained, "starved");
            AddSeverityBand(displayed, retained, "draining");
            return new List<GuidePanelRiskModel>(displayed);
        }

        private static void AddSeverityBand(
            List<GuidePanelRiskModel> destination,
            List<GuidePanelRiskModel> source,
            string state)
        {
            foreach (GuidePanelRiskModel risk in source)
                if (String.Equals(
                    risk.State, state, StringComparison.OrdinalIgnoreCase))
                    destination.Add(risk);
        }

        private static GuidePanelRiskModel Find(
            List<GuidePanelRiskModel> risks,
            string id)
        {
            foreach (GuidePanelRiskModel risk in risks)
                if (String.Equals(
                    risk.Id, id, StringComparison.OrdinalIgnoreCase))
                    return risk;
            return null;
        }

        private static int Rank(string state)
        {
            return String.Equals(
                state, "starved", StringComparison.OrdinalIgnoreCase)
                    ? 2 : 1;
        }
    }
}
