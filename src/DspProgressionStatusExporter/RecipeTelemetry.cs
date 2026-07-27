using System;
using System.Collections.Generic;

namespace DspProgressionStatusExporter
{
    /// <summary>
    /// Captures configured production recipes at export time. This is not a
    /// production-rate proxy: it answers which route a machine is configured
    /// to use, while ProductionTelemetry remains authoritative for throughput.
    /// </summary>
    internal static class RecipeTelemetry
    {
        public static Dictionary<string, object> Export(object gameData)
        {
            var result = new Dictionary<string, object>();
            var factories = new List<object>();
            result["available"] = false;
            result["source"] =
                "GameData.factories[*].factorySystem assembler/lab component recipe identifiers";
            result["semantics"] =
                "Configured machine counts identify recipe choice; they do not prove current production.";
            result["factories"] = factories;

            try
            {
                int factoryIndex = 0;
                foreach (object factory in Plugin.Enumerate(Plugin.GetMember(gameData, "factories")))
                {
                    if (factory != null)
                    {
                        Dictionary<int, int> configured = new Dictionary<int, int>();
                        object factorySystem = Plugin.GetMember(factory, "factorySystem");
                        CountRecipes(Plugin.GetMember(factorySystem, "assemblerPool"), configured);
                        CountRecipes(Plugin.GetMember(factorySystem, "labPool"), configured);

                        var row = new Dictionary<string, object>();
                        row["factoryIndex"] = factoryIndex;
                        object planet = Plugin.GetMember(factory, "planet");
                        row["planetId"] = Plugin.ToInt(Plugin.GetMember(planet, "id", "planetId"));
                        object name = Plugin.GetMember(planet, "displayName", "name");
                        row["planetName"] = name != null ? name.ToString() : null;
                        row["recipes"] = ExportRecipes(configured);
                        factories.Add(row);
                    }
                    factoryIndex++;
                }
                result["available"] = true;
            }
            catch (Exception ex)
            {
                result["lastFailure"] = ex.GetType().Name + ": " + ex.Message;
            }
            return result;
        }

        private static void CountRecipes(object pool, Dictionary<int, int> configured)
        {
            foreach (object component in Plugin.Enumerate(pool))
            {
                if (component == null) continue;
                int id = Plugin.ToInt(Plugin.GetMember(component, "id"));
                int recipeId = Plugin.ToInt(Plugin.GetMember(component, "recipeId"));
                if (id <= 0 || recipeId <= 0) continue;
                int count;
                configured.TryGetValue(recipeId, out count);
                configured[recipeId] = count + 1;
            }
        }

        private static List<object> ExportRecipes(Dictionary<int, int> configured)
        {
            var ids = new List<int>(configured.Keys);
            ids.Sort();
            var rows = new List<object>();
            foreach (int recipeId in ids)
            {
                rows.Add(new Dictionary<string, object> {
                    { "recipeId", recipeId },
                    { "name", Plugin.RecipeName(recipeId) },
                    { "configuredMachineCount", configured[recipeId] }
                });
            }
            return rows;
        }
    }
}
