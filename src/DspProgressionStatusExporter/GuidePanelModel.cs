using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DspProgressionStatusExporter
{
    internal sealed class GuidePanelRowModel
    {
        public string Id;
        public string Status;
        public string Label;
        public string Detail;
        public bool Required;
        public bool Completed;

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object> {
                { "id", Id },
                { "status", Status },
                { "label", Label },
                { "detail", Detail },
                { "required", Required },
                { "completed", Completed }
            };
        }
    }

    internal enum CubeRateLevel
    {
        Unknown,
        BelowMinimum,
        Minimum,
        Comfortable,
        Later
    }

    internal sealed class GuidePanelCubeRateModel
    {
        public string CubeId;
        public string RateText;
        public CubeRateLevel Level;
    }

    internal sealed class GuidePanelModel
    {
        public string PhaseId;
        public string Title;
        public string Subtitle;
        public string SnapshotFileName;
        public string SnapshotDirectory;
        public readonly List<GuidePanelRowModel> Objectives =
            new List<GuidePanelRowModel>();
        public readonly List<GuidePanelRowModel> Pending =
            new List<GuidePanelRowModel>();
        public readonly List<GuidePanelRowModel> Context =
            new List<GuidePanelRowModel>();
        public readonly List<GuidePanelCubeRateModel> CubeRates =
            new List<GuidePanelCubeRateModel>();

        public Dictionary<string, object> Export()
        {
            var objectives = new List<object>();
            foreach (GuidePanelRowModel row in Objectives)
                objectives.Add(row.Export());
            var pending = new List<object>();
            foreach (GuidePanelRowModel row in Pending)
                pending.Add(row.Export());
            var context = new List<object>();
            foreach (GuidePanelRowModel row in Context)
                context.Add(row.Export());

            return new Dictionary<string, object> {
                { "contractVersion", "2.2" },
                { "phaseId", PhaseId },
                { "phaseSelectionAuthority", "player" },
                { "title", Title },
                { "subtitle", Subtitle },
                { "snapshotFileName", SnapshotFileName },
                { "snapshotDirectory", SnapshotDirectory },
                { "objectives", objectives },
                { "pending", pending },
                { "currentStatus", context },
                { "stabilityRule",
                    "Objective identities and order remain fixed while phaseId is unchanged; only their measured status and detail update." }
            };
        }
    }

    /// <summary>
    /// Converts the pure analyzer result into a presentation contract.
    /// It deliberately contains no Unity or live-game access.
    /// </summary>
    internal static class GuidePanelModelBuilder
    {
        private sealed class CubeRateSpec
        {
            public string CubeId;
            public int ItemId;
            public double Minimum;
            public double Comfortable;
            public double? Later;
        }

        private static readonly CubeRateSpec[] CubeRateSpecs = {
            new CubeRateSpec {
                CubeId = "blue", ItemId = 6001,
                Minimum = 20, Comfortable = 40, Later = 60
            },
            new CubeRateSpec {
                CubeId = "red", ItemId = 6002,
                Minimum = 10, Comfortable = 20, Later = 60
            },
            new CubeRateSpec {
                CubeId = "yellow", ItemId = 6003,
                Minimum = 15, Comfortable = 22.5, Later = 60
            },
            new CubeRateSpec {
                CubeId = "purple", ItemId = 6004,
                Minimum = 12, Comfortable = 24, Later = 40
            },
            new CubeRateSpec {
                CubeId = "green", ItemId = 6005,
                Minimum = 10, Comfortable = 20, Later = 40
            },
            new CubeRateSpec {
                CubeId = "white", ItemId = 6006,
                Minimum = 40, Comfortable = 40, Later = null
            }
        };

        public static GuidePanelModel Build(
            Dictionary<string, object> analysis,
            ObservedGameState observedState,
            string snapshotFileName,
            string snapshotDirectory)
        {
            var model = new GuidePanelModel {
                PhaseId = "unknown",
                Title = "Guide Check",
                Subtitle = "Live guide objectives",
                SnapshotFileName = snapshotFileName,
                SnapshotDirectory = snapshotDirectory
            };
            if (analysis == null) return model;

            Dictionary<string, object> phase = AsDictionary(Get(analysis, "phase"));
            Dictionary<string, object> progression =
                AsDictionary(Get(analysis, "progression"));
            model.PhaseId = Text(Get(phase, "id"), "unknown");
            model.Title = PlayerFacingText.Normalize(
                Text(Get(phase, "title"), "Guide Check"));
            AddCubeRates(model, observedState);

            Dictionary<string, object> currentGate =
                FindCurrentGate(progression, model.PhaseId);
            if (currentGate != null)
            {
                string gateTitle = Text(Get(currentGate, "title"), null);
                if (!String.IsNullOrEmpty(gateTitle))
                    model.Title = PlayerFacingText.Normalize(gateTitle);
                AddObjectives(model, currentGate);
            }

            bool completedWhite =
                String.Equals(
                    model.PhaseId,
                    "white",
                    StringComparison.OrdinalIgnoreCase) &&
                HasCompletedObjective(model, "mission-completed");
            model.Subtitle = completedWhite
                ? "Main progression complete."
                : "Current phase objectives";

            AddContext(model, AsList(Get(analysis, "findings")));
            return model;
        }

        private static void AddCubeRates(
            GuidePanelModel model,
            ObservedGameState state)
        {
            int visibleCount;
            int focusIndex;
            CubeRangeForPhase(model.PhaseId, out visibleCount, out focusIndex);
            for (int i = 0; i < visibleCount; i++)
            {
                CubeRateSpec spec = CubeRateSpecs[i];
                ObservedItemFlow flow = null;
                bool available =
                    state != null &&
                    state.ProductionWindowReady &&
                    state.ItemFlows.TryGetValue(spec.ItemId, out flow);
                if (!available)
                {
                    model.CubeRates.Add(new GuidePanelCubeRateModel {
                        CubeId = spec.CubeId,
                        RateText = "--/m",
                        Level = CubeRateLevel.Unknown
                    });
                    continue;
                }

                double perMinute = Math.Max(0.0, flow.ProducedPerMinute);
                model.CubeRates.Add(new GuidePanelCubeRateModel {
                    CubeId = spec.CubeId,
                    RateText = perMinute.ToString(
                        "0.##", CultureInfo.InvariantCulture) + "/m",
                    Level = CubeLevel(spec, perMinute, i == focusIndex)
                });
            }
        }

        private static CubeRateLevel CubeLevel(
            CubeRateSpec spec,
            double perMinute,
            bool focused)
        {
            if (perMinute < spec.Minimum)
                return focused
                    ? CubeRateLevel.BelowMinimum
                    : CubeRateLevel.Minimum;
            if (spec.Later.HasValue && perMinute >= spec.Later.Value)
                return CubeRateLevel.Later;
            if (perMinute >= spec.Comfortable)
                return CubeRateLevel.Comfortable;
            return CubeRateLevel.Minimum;
        }

        private static void CubeRangeForPhase(
            string phaseId,
            out int visibleCount,
            out int focusIndex)
        {
            visibleCount = 0;
            focusIndex = -1;
            if (String.Equals(phaseId, "blue", StringComparison.OrdinalIgnoreCase))
            {
                visibleCount = 1;
                focusIndex = 0;
            }
            else if (String.Equals(phaseId, "red", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(phaseId, "ils", StringComparison.OrdinalIgnoreCase))
            {
                visibleCount = 2;
                focusIndex = 1;
            }
            else if (String.Equals(phaseId, "yellow", StringComparison.OrdinalIgnoreCase))
            {
                visibleCount = 3;
                focusIndex = 2;
            }
            else if (String.Equals(phaseId, "purple", StringComparison.OrdinalIgnoreCase))
            {
                visibleCount = 4;
                focusIndex = 3;
            }
            else if (String.Equals(phaseId, "green", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(phaseId, "dyson", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(phaseId, "photon", StringComparison.OrdinalIgnoreCase))
            {
                visibleCount = 5;
                focusIndex = 4;
            }
            else if (String.Equals(phaseId, "white", StringComparison.OrdinalIgnoreCase))
            {
                visibleCount = 6;
                focusIndex = 5;
            }
        }

        private static bool HasCompletedObjective(
            GuidePanelModel model,
            string id)
        {
            foreach (GuidePanelRowModel row in model.Objectives)
                if (row.Completed &&
                    String.Equals(
                        row.Id, id, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static Dictionary<string, object> FindCurrentGate(
            Dictionary<string, object> progression,
            string phaseId)
        {
            if (progression == null) return null;
            Dictionary<string, object> fallback = null;
            List<object> gates = AsList(Get(progression, "gateEvaluations"));
            if (gates != null)
            {
                foreach (object item in gates)
                {
                    Dictionary<string, object> gate = AsDictionary(item);
                    if (gate == null) continue;
                    fallback = gate;
                    if (String.Equals(
                        Text(Get(gate, "id"), null),
                        phaseId,
                        StringComparison.OrdinalIgnoreCase))
                        return gate;
                }
            }

            return fallback;
        }

        private static void AddObjectives(
            GuidePanelModel model,
            Dictionary<string, object> gate)
        {
            List<object> conditions = AsList(Get(gate, "conditions"));
            if (conditions == null) return;
            foreach (object item in conditions)
            {
                Dictionary<string, object> condition = AsDictionary(item);
                if (condition == null) continue;
                string status = Text(Get(condition, "status"), "unknown");
                bool required = Boolean(Get(condition, "required"), true);
                string evidence = Text(Get(condition, "evidence"), null);
                string action = Text(Get(condition, "action"), null);
                string id = Text(
                    Get(condition, "id"), "objective-" + model.Objectives.Count);
                string label = PlayerFacingText.Normalize(
                    Text(Get(condition, "label"), "Guide objective"));
                string detail = PlayerFacingText.CleanEvidence(
                    evidence, label, status, id);
                model.Objectives.Add(new GuidePanelRowModel {
                    Id = id,
                    Status = status,
                    Label = label,
                    Detail = detail,
                    Required = required,
                    Completed = IsCompleted(status)
                });
                if (!IsCompleted(status) && !String.IsNullOrEmpty(action))
                    AddPending(model, id, status, required, action);
            }
        }

        private static void AddPending(
            GuidePanelModel model,
            string objectiveId,
            string status,
            bool required,
            string action)
        {
            string normalized = PlayerFacingText.Normalize(action);
            foreach (GuidePanelRowModel existing in model.Pending)
                if (String.Equals(
                    existing.Label, normalized, StringComparison.OrdinalIgnoreCase))
                    return;
            model.Pending.Add(new GuidePanelRowModel {
                Id = "pending-" + objectiveId,
                Status = status,
                Label = normalized,
                Detail = null,
                Required = required,
                Completed = false
            });
        }

        private static void AddContext(
            GuidePanelModel model,
            List<object> findings)
        {
            if (findings == null) return;
            var candidates = new List<ContextCandidate>();
            int sourceOrder = 0;
            foreach (object item in findings)
            {
                Dictionary<string, object> finding = AsDictionary(item);
                if (finding == null) continue;
                string id = Text(Get(finding, "id"), null);
                if (String.IsNullOrEmpty(id))
                    continue;
                string claim = Text(Get(finding, "claim"), null);
                if (String.IsNullOrEmpty(claim)) continue;
                string status = Text(Get(finding, "status"), "context");
                GuidePanelRowModel row = CreateContextRow(
                    id,
                    status,
                    claim,
                    Text(Get(finding, "evidence"), null));
                if (row == null) continue;
                candidates.Add(new ContextCandidate {
                    Priority = Integer(Get(finding, "priority"), 100),
                    SourceOrder = sourceOrder++,
                    Row = row
                });
            }
            candidates.Sort(delegate(ContextCandidate left, ContextCandidate right) {
                int priority = left.Priority.CompareTo(right.Priority);
                return priority != 0
                    ? priority
                    : left.SourceOrder.CompareTo(right.SourceOrder);
            });
            int count = Math.Min(6, candidates.Count);
            for (int i = 0; i < count; i++)
                model.Context.Add(candidates[i].Row);
        }

        private static GuidePanelRowModel CreateContextRow(
            string id,
            string status,
            string claim,
            string evidence)
        {
            string label = PlayerFacingText.Normalize(claim);
            string detail = PlayerFacingText.CleanEvidence(
                evidence, label, status, id);

            if (String.Equals(
                id, "refined-oil-congestion", StringComparison.OrdinalIgnoreCase))
            {
                detail = PlayerFacingText.NetOilRate(evidence);
                if (!String.IsNullOrEmpty(detail))
                    detail += " Use Hydrogen or expand storage to avoid bottlenecks.";
                else
                    detail =
                        "Use Hydrogen or expand storage to avoid bottlenecks.";
            }

            return new GuidePanelRowModel {
                Id = id,
                Status = status,
                Label = label,
                Detail = detail,
                Required = false,
                Completed = IsPositiveContext(status)
            };
        }

        private sealed class ContextCandidate
        {
            public int Priority;
            public int SourceOrder;
            public GuidePanelRowModel Row;
        }

        private static bool IsCompleted(string status)
        {
            return String.Equals(status, "ready", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(status, "complete", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(status, "complete-inferred", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPositiveContext(string status)
        {
            return IsCompleted(status) ||
                String.Equals(status, "context", StringComparison.OrdinalIgnoreCase);
        }

        private static object Get(Dictionary<string, object> dictionary, string key)
        {
            object value;
            return dictionary != null && dictionary.TryGetValue(key, out value)
                ? value
                : null;
        }

        private static Dictionary<string, object> AsDictionary(object value)
        {
            return value as Dictionary<string, object>;
        }

        private static List<object> AsList(object value)
        {
            return value as List<object>;
        }

        private static string Text(object value, string fallback)
        {
            return value == null ? fallback : Convert.ToString(value);
        }

        private static bool Boolean(object value, bool fallback)
        {
            try { return value == null ? fallback : Convert.ToBoolean(value); }
            catch { return fallback; }
        }

        private static int Integer(object value, int fallback)
        {
            try { return value == null ? fallback : Convert.ToInt32(value); }
            catch { return fallback; }
        }
    }

    internal static class PlayerFacingText
    {
        private sealed class MatrixTerm
        {
            public string Color;
            public string Singular;
            public string Plural;
        }

        private static readonly MatrixTerm[] MatrixTerms = new MatrixTerm[] {
            new MatrixTerm {
                Color = "Blue",
                Singular = "Electromagnetic Matrix",
                Plural = "Electromagnetic Matrices"
            },
            new MatrixTerm {
                Color = "Red",
                Singular = "Energy Matrix",
                Plural = "Energy Matrices"
            },
            new MatrixTerm {
                Color = "Yellow",
                Singular = "Structure Matrix",
                Plural = "Structure Matrices"
            },
            new MatrixTerm {
                Color = "Purple",
                Singular = "Information Matrix",
                Plural = "Information Matrices"
            },
            new MatrixTerm {
                Color = "Green",
                Singular = "Gravity Matrix",
                Plural = "Gravity Matrices"
            },
            new MatrixTerm {
                Color = "White",
                Singular = "Universe Matrix",
                Plural = "Universe Matrices"
            }
        };

        public static string CleanEvidence(
            string evidence,
            string label,
            string status,
            string id)
        {
            if (String.IsNullOrEmpty(evidence)) return evidence;
            string result = Regex.Replace(
                evidence,
                @"; active in [^;]+;",
                ";",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            result = Regex.Replace(
                result,
                @"\s{2,}",
                " ",
                RegexOptions.CultureInvariant);
            result = Regex.Replace(
                result,
                @"\s*Next:\s*[^.]+\.?",
                "",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            Match technology = Regex.Match(
                result,
                @"^Technology\s+\d+(?:\s+\([^)]+\))?\s+is\s+(complete|incomplete)\.$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (technology.Success)
            {
                string subject = ObjectiveSubject(label);
                result = String.Equals(
                    technology.Groups[1].Value,
                    "complete",
                    StringComparison.OrdinalIgnoreCase)
                    ? subject + " is complete."
                    : subject + " is not yet researched.";
            }
            else
            {
                Match booleanTechnology = Regex.Match(
                    result,
                    @"^Technology\s+\d+\s+complete:\s+(True|False)\.$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (booleanTechnology.Success)
                {
                    string subject = ObjectiveSubject(label);
                    result = String.Equals(
                        booleanTechnology.Groups[1].Value,
                        "True",
                        StringComparison.OrdinalIgnoreCase)
                        ? subject + " is complete."
                        : subject + " is not yet researched.";
                }
            }

            if (String.Equals(id, "power", StringComparison.OrdinalIgnoreCase))
                result = ReplaceInsensitive(
                    result, "Minimum satisfaction", "Lowest satisfaction");
            else if (String.Equals(
                id, "automated-titanium", StringComparison.OrdinalIgnoreCase))
                result = String.Equals(
                    status, "ready", StringComparison.OrdinalIgnoreCase)
                    ? "An active Titanium route was found."
                    : "An automated Titanium route wasn't found.";
            else if (String.Equals(
                id, "automated-silicon", StringComparison.OrdinalIgnoreCase))
                result = String.Equals(
                    status, "ready", StringComparison.OrdinalIgnoreCase)
                    ? "An active Silicon route was found."
                    : "A sustainable Silicon route wasn't found.";

            result = ReplaceInsensitive(result, "Observed", "Found");
            result = ReplaceInsensitive(result, "minimum", "desired");
            result = ReplaceInsensitive(result, "guide minimum", "desired rate");
            result = ReplaceInsensitive(result, "guide target", "desired target");
            result = ReplaceInsensitive(
                result, "guide starter target", "starter target");
            result = ReplaceInsensitive(
                result, "guide practical", "desired");
            result = ReplaceInsensitive(
                result,
                "A full byproduct path can stop Hydrogen production.",
                "Use Hydrogen or expand storage to avoid bottlenecks.");
            return TrimSentences(Normalize(result));
        }

        public static string NetOilRate(string evidence)
        {
            if (String.IsNullOrEmpty(evidence)) return null;
            Match match = Regex.Match(
                evidence,
                @"Refined Oil:\s*([0-9]+(?:[.,][0-9]+)?)/min produced,\s*" +
                @"([0-9]+(?:[.,][0-9]+)?)/min consumed",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            double produced;
            double consumed;
            if (!match.Success ||
                !Double.TryParse(
                    match.Groups[1].Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out produced) &&
                !Double.TryParse(
                    match.Groups[1].Value.Replace(',', '.'),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out produced) ||
                (!Double.TryParse(
                    match.Groups[2].Value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out consumed) &&
                !Double.TryParse(
                    match.Groups[2].Value.Replace(',', '.'),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out consumed)))
                return null;
            double net = produced - consumed;
            return "Found net " +
                (net >= 0d ? "production" : "consumption") +
                " rate of " +
                Math.Abs(net).ToString("0.0", CultureInfo.InvariantCulture) +
                "/min.";
        }

        public static string Normalize(string text)
        {
            if (String.IsNullOrEmpty(text)) return text;
            string result = text;
            result = ReplaceInsensitive(
                result,
                "A Yellow-dependent ILS-path technology is complete.",
                "A technology on the ILS path that consumes Yellow Cubes (Structure Matrices) is complete.");
            result = ReplaceInsensitive(
                result,
                "The research queue is explicitly pointed toward Purple",
                "The research queue is explicitly pointed toward Purple Cube (Information Matrix) research");
            result = ReplaceInsensitive(
                result,
                "the Green tier",
                "the Green Cube (Gravity Matrix) tier");
            result = ReplaceInsensitive(
                result,
                "meet the guide minimum",
                "meet the desired rate");
            result = ReplaceInsensitive(
                result,
                "reaches the guide target",
                "reaches the desired target");
            result = ReplaceInsensitive(
                result,
                "guide starter target",
                "desired starter rate");
            result = ReplaceInsensitive(
                result,
                "weakest guide-relative intermediate",
                "weakest intermediate");
            result = ReplaceInsensitive(
                result,
                "Verify the refinery output cannot deadlock",
                "Make sure refineries don't bottleneck");
            result = ReplaceInsensitive(
                result,
                "Verify fuel, buildings, and destination power in the guide checklist",
                "Check fuel, buildings, and destination power");
            result = ReplaceInsensitive(
                result,
                "practical target",
                "desired target");
            result = ReplaceInsensitive(result, "â€“", "\u2013");
            var protectedTerms = new Dictionary<string, string>();
            int token = 0;

            foreach (MatrixTerm term in MatrixTerms)
            {
                string singularPhrase =
                    term.Color + " Cube (" + term.Singular + ")";
                string pluralPhrase =
                    term.Color + " Cubes (" + term.Plural + ")";
                result = ReplaceInsensitive(
                    result, term.Color + " matrices", pluralPhrase);
                result = ReplaceInsensitive(
                    result, term.Color + " matrix", singularPhrase);
                result = ReplaceInsensitive(
                    result, term.Color + " production",
                    singularPhrase + " production");
                result = ReplaceInsensitive(
                    result, "200-" + term.Color,
                    "200 " + pluralPhrase);

                string singularToken = "\u001fM" + token++ + "\u001f";
                string pluralToken = "\u001fM" + token++ + "\u001f";
                result = ReplaceInsensitive(result, singularPhrase, singularToken);
                result = ReplaceInsensitive(result, pluralPhrase, pluralToken);
                protectedTerms[singularToken] = singularPhrase;
                protectedTerms[pluralToken] = pluralPhrase;
            }

            foreach (MatrixTerm term in MatrixTerms)
            {
                result = ReplaceInsensitive(
                    result,
                    term.Plural,
                    term.Color + " Cubes (" + term.Plural + ")");
                result = ReplaceInsensitive(
                    result,
                    term.Singular,
                    term.Color + " Cube (" + term.Singular + ")");
            }

            foreach (KeyValuePair<string, string> pair in protectedTerms)
                result = result.Replace(pair.Key, pair.Value);
            return TrimSentences(result);
        }

        private static string TrimSentences(string text)
        {
            if (String.IsNullOrEmpty(text)) return text;
            string result = Regex.Replace(
                text,
                @"\s+([,.;:])",
                "$1",
                RegexOptions.CultureInvariant);
            result = Regex.Replace(
                result,
                @"\s{2,}",
                " ",
                RegexOptions.CultureInvariant);
            return result.Trim();
        }

        private static string ObjectiveSubject(string label)
        {
            string result = label ?? "This technology";
            string[] suffixes = new string[] {
                " is researched",
                " is complete",
                " is available"
            };
            foreach (string suffix in suffixes)
                if (result.EndsWith(
                    suffix, StringComparison.OrdinalIgnoreCase))
                    return result.Substring(0, result.Length - suffix.Length);
            return result;
        }

        private static string ReplaceInsensitive(
            string text,
            string oldValue,
            string newValue)
        {
            return Regex.Replace(
                text,
                Regex.Escape(oldValue),
                delegate(Match match) { return newValue; },
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
    }
}
