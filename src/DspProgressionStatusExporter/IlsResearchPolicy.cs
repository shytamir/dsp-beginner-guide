using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal static class IlsResearchPolicy
    {
        private static readonly int[] Departure = { 2902, 1413 };
        private static readonly int[] Haulback = { 1131, 1302, 1124, 1703, 1114, 1603, 3701, 1604 };
        private static readonly int[] Automation = { 1414, 1605 };

        internal static IEnumerable<int> ResearchTargets
        {
            get
            {
                foreach (int id in Departure) yield return id;
                foreach (int id in Haulback) yield return id;
                foreach (int id in Automation) yield return id;
                yield return 4102;
            }
        }

        private static int[] Targets(int stage)
        {
            return stage == 3 ? Automation : stage == 2 ? Haulback : Departure;
        }

        private static int FirstMissing(ObservedGameState state, int id)
        {
            if (state.UnlockedTechIds.Contains(id)) return 0;
            ObservedTechDefinition tech;
            if (!state.ResearchDefinitions.TryGetValue(id, out tech)) return id;
            foreach (int prerequisite in tech.Required)
            {
                int missing = FirstMissing(state, prerequisite);
                if (missing != 0) return missing;
            }
            foreach (int prerequisite in tech.Implicit)
            {
                int missing = FirstMissing(state, prerequisite);
                if (missing != 0) return missing;
            }
            return id;
        }

        public static GuideGateCondition Evaluate(ObservedGameState state, int stage)
        {
            int missing = 0;
            foreach (int target in Targets(stage))
            {
                missing = FirstMissing(state, target);
                if (missing != 0) break;
            }
            GuideGateCondition condition = ResearchCondition(state, missing);
            condition.Id = stage == 3 ? "ils-rush-tech" : stage == 2 ? "ils-outpost-research" : "ils-departure-research";
            condition.Label = stage == 3 ? "ILS research" : stage == 2 ? "Outpost research" : "Departure research";
            condition.Required = stage != 2;
            if (stage == 3 && (missing == 1414 || missing == 1605) &&
                !Started(state, 1414) && !Started(state, 1605))
                condition.Evidence += " Guide batch: 200 Yellow Cubes.";
            return condition;
        }

        public static GuideGateCondition Survey(ObservedGameState state)
        {
            GuideGateCondition condition = ResearchCondition(state, FirstMissing(state, 4102));
            condition.Id = "ils-survey-research";
            condition.Label = "Survey research (optional)";
            condition.Required = false;
            if (condition.Action != null) condition.Action = "Consider " + condition.Action.Substring(9);
            return condition;
        }

        private static GuideGateCondition ResearchCondition(ObservedGameState state, int id)
        {
            if (id == 0) return new GuideGateCondition { Status = "ready", Evidence = "Research complete.", EvidenceKind = "observed" };
            bool queued = state.QueuedTechIds.Contains(id);
            ObservedTechDefinition tech;
            bool definitionKnown = state.ResearchDefinitions.TryGetValue(id, out tech);
            bool known = definitionKnown && state.AvailableTechIds.Contains(id) && (state.ResearchQueueAvailable || queued);
            string name = definitionKnown ? tech.Name : null;
            return new GuideGateCondition {
                Status = !known ? "unknown" : queued ? "watch" : "blocked",
                Evidence = !known ? "Research state unavailable." : queued ? name + " queued." : name + " is next.",
                EvidenceKind = "observed",
                Action = known && !queued ? "Research " + name + "." : null
            };
        }

        private static bool Started(ObservedGameState state, int id)
        {
            ObservedTechProgress progress;
            return state.UnlockedTechIds.Contains(id) || state.QueuedTechIds.Contains(id) ||
                state.TechProgress.TryGetValue(id, out progress) && progress.HashUploaded > 0;
        }

        private static void AddClosure(ObservedGameState state, int id, SortedSet<int> ids)
        {
            if (!ids.Add(id)) return;
            ObservedTechDefinition tech;
            if (!state.ResearchDefinitions.TryGetValue(id, out tech)) return;
            foreach (int prerequisite in tech.Required) AddClosure(state, prerequisite, ids);
            foreach (int prerequisite in tech.Implicit) AddClosure(state, prerequisite, ids);
        }

        public static List<object> Export(ObservedGameState state, int stage)
        {
            var ids = new SortedSet<int>();
            foreach (int target in Targets(stage)) AddClosure(state, target, ids);
            if (stage == 1) AddClosure(state, 4102, ids);
            var rows = new List<object>();
            foreach (int id in ids)
            {
                ObservedTechDefinition tech;
                bool known = state.ResearchDefinitions.TryGetValue(id, out tech);
                rows.Add(new Dictionary<string, object> {
                    { "id", id }, { "name", known ? tech.Name : null },
                    { "required", known ? tech.Required : null }, { "implicitRequired", known ? tech.Implicit : null },
                    { "available", known && state.AvailableTechIds.Contains(id) },
                    { "unlocked", state.UnlockedTechIds.Contains(id) }, { "queued", state.QueuedTechIds.Contains(id) },
                    { "started", Started(state, id) }
                });
            }
            return rows;
        }
    }
}
