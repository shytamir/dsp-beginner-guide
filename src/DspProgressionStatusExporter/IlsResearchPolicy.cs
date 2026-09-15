using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    internal static class IlsResearchPolicy
    {
        // Guide 3.0 tech-reference.json at c6d846b09f80564106ba221133a1c1d021267f2f.
        // https://github.com/shytamir/DSP_Guide/blob/c6d846b09f80564106ba221133a1c1d021267f2f/assets/data/tech-reference.json
        private sealed class Tech
        {
            public int Id;
            public string Name;
            public int[] Required;
            public int[] Implicit;
        }
        private static readonly Dictionary<int, Tech> Technologies = new Dictionary<int, Tech> {
            { 1, new Tech { Id = 1, Name = "Dyson Sphere Program", Required = new int[] {  }, Implicit = new int[] {  } } },
            { 1001, new Tech { Id = 1001, Name = "Electromagnetism", Required = new int[] { 1 }, Implicit = new int[] {  } } },
            { 1101, new Tech { Id = 1101, Name = "High-Efficiency Plasma Control", Required = new int[] { 1001 }, Implicit = new int[] {  } } },
            { 1102, new Tech { Id = 1102, Name = "Plasma Extract Refining", Required = new int[] { 1120, 1101 }, Implicit = new int[] {  } } },
            { 1111, new Tech { Id = 1111, Name = "Energy Matrix", Required = new int[] { 1102 }, Implicit = new int[] {  } } },
            { 1112, new Tech { Id = 1112, Name = "Hydrogen Fuel Rod", Required = new int[] { 1111 }, Implicit = new int[] {  } } },
            { 1113, new Tech { Id = 1113, Name = "Thruster", Required = new int[] { 1112 }, Implicit = new int[] {  } } },
            { 1114, new Tech { Id = 1114, Name = "Reinforced Thruster", Required = new int[] { 1113 }, Implicit = new int[] {  } } },
            { 1120, new Tech { Id = 1120, Name = "Fluid Storage Encapsulation", Required = new int[] { 1001 }, Implicit = new int[] {  } } },
            { 1121, new Tech { Id = 1121, Name = "Basic Chemical Engineering", Required = new int[] { 1120, 1102 }, Implicit = new int[] {  } } },
            { 1122, new Tech { Id = 1122, Name = "Polymer Chemical Engineering", Required = new int[] { 1121 }, Implicit = new int[] {  } } },
            { 1123, new Tech { Id = 1123, Name = "High-Strength Crystal", Required = new int[] { 1122 }, Implicit = new int[] {  } } },
            { 1124, new Tech { Id = 1124, Name = "Structure Matrix", Required = new int[] { 1123 }, Implicit = new int[] {  } } },
            { 1131, new Tech { Id = 1131, Name = "Applied Superconductor", Required = new int[] { 1121 }, Implicit = new int[] {  } } },
            { 1201, new Tech { Id = 1201, Name = "Basic Assembling", Required = new int[] { 1001 }, Implicit = new int[] {  } } },
            { 1302, new Tech { Id = 1302, Name = "Processor", Required = new int[] { 1311 }, Implicit = new int[] {  } } },
            { 1311, new Tech { Id = 1311, Name = "Semiconductor Material", Required = new int[] { 1201 }, Implicit = new int[] {  } } },
            { 1401, new Tech { Id = 1401, Name = "Automatic Metallurgy", Required = new int[] { 1001 }, Implicit = new int[] {  } } },
            { 1411, new Tech { Id = 1411, Name = "Steel Smelting", Required = new int[] { 1401 }, Implicit = new int[] {  } } },
            { 1413, new Tech { Id = 1413, Name = "Titanium Smelting", Required = new int[] { 1411 }, Implicit = new int[] {  } } },
            { 1414, new Tech { Id = 1414, Name = "High-Strength Titanium Alloy", Required = new int[] { 1413 }, Implicit = new int[] {  } } },
            { 1601, new Tech { Id = 1601, Name = "Basic Logistics System", Required = new int[] { 1001 }, Implicit = new int[] {  } } },
            { 1602, new Tech { Id = 1602, Name = "Upgraded Logistics System", Required = new int[] { 1601 }, Implicit = new int[] {  } } },
            { 1603, new Tech { Id = 1603, Name = "High-Efficiency Logistics System", Required = new int[] { 1602 }, Implicit = new int[] { 1702 } } },
            { 1604, new Tech { Id = 1604, Name = "Planetary Logistics System", Required = new int[] { 1603 }, Implicit = new int[] { 1113, 3701 } } },
            { 1605, new Tech { Id = 1605, Name = "Interstellar Logistics System", Required = new int[] { 1604, 1414 }, Implicit = new int[] { 1114 } } },
            { 1701, new Tech { Id = 1701, Name = "Electromagnetic Drive", Required = new int[] { 1001 }, Implicit = new int[] {  } } },
            { 1702, new Tech { Id = 1702, Name = "Magnetic Levitation", Required = new int[] { 1701 }, Implicit = new int[] {  } } },
            { 1703, new Tech { Id = 1703, Name = "Magnetic Particle Trap", Required = new int[] { 1702 }, Implicit = new int[] {  } } },
            { 2101, new Tech { Id = 2101, Name = "Mecha Core Lv1", Required = new int[] {  }, Implicit = new int[] {  } } },
            { 2102, new Tech { Id = 2102, Name = "Mecha Core Lv2", Required = new int[] { 2101 }, Implicit = new int[] {  } } },
            { 2901, new Tech { Id = 2901, Name = "Drive Engine Lv1", Required = new int[] {  }, Implicit = new int[] { 2101 } } },
            { 2902, new Tech { Id = 2902, Name = "Drive Engine Lv2", Required = new int[] { 2901 }, Implicit = new int[] { 2102 } } },
            { 3701, new Tech { Id = 3701, Name = "Vertical Construction Lv1", Required = new int[] {  }, Implicit = new int[] { 1601 } } },
            { 4101, new Tech { Id = 4101, Name = "Cosmic Exploration Lv1", Required = new int[] {  }, Implicit = new int[] {  } } },
            { 4102, new Tech { Id = 4102, Name = "Cosmic Exploration Lv2", Required = new int[] { 4101 }, Implicit = new int[] {  } } }
        };
        private static readonly int[] Departure = { 2902, 1413 };
        private static readonly int[] Haulback = { 1131, 1302, 1124, 1703, 1114, 1603, 3701, 1604 };
        private static readonly int[] Automation = { 1414, 1605 };

        private static int[] Targets(int stage)
        {
            return stage == 3 ? Automation : stage == 2 ? Haulback : Departure;
        }

        private static int FirstMissing(ObservedGameState state, int id)
        {
            if (state.UnlockedTechIds.Contains(id)) return 0;
            Tech tech = Technologies[id];
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
            bool known = state.AvailableTechIds.Contains(id) && (state.ResearchQueueAvailable || queued);
            string name = Technologies[id].Name;
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

        private static void AddClosure(int id, SortedSet<int> ids)
        {
            if (!ids.Add(id)) return;
            foreach (int prerequisite in Technologies[id].Required) AddClosure(prerequisite, ids);
            foreach (int prerequisite in Technologies[id].Implicit) AddClosure(prerequisite, ids);
        }

        public static List<object> Export(ObservedGameState state, int stage)
        {
            var ids = new SortedSet<int>();
            foreach (int target in Targets(stage)) AddClosure(target, ids);
            if (stage == 1) AddClosure(4102, ids);
            var rows = new List<object>();
            foreach (int id in ids)
                rows.Add(new Dictionary<string, object> {
                    { "id", id }, { "name", Technologies[id].Name },
                    { "required", Technologies[id].Required }, { "implicitRequired", Technologies[id].Implicit },
                    { "available", state.AvailableTechIds.Contains(id) },
                    { "unlocked", state.UnlockedTechIds.Contains(id) }, { "queued", state.QueuedTechIds.Contains(id) },
                    { "started", Started(state, id) }
                });
            return rows;
        }
    }
}
