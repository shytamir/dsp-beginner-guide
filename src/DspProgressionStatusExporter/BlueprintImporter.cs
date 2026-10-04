using System;
using System.IO;
using System.IO.Compression;

namespace DspProgressionStatusExporter
{
    internal static class BlueprintImporter
    {
        private const string ResourceName =
            "DspGuideCheck.Blueprints.DSP-Guide-Blueprint-Collection-Ready.zip";

        internal static void Import(string blueprintFolder)
        {
            if (String.IsNullOrWhiteSpace(blueprintFolder) ||
                !Path.IsPathRooted(blueprintFolder))
                throw new InvalidOperationException("DSP's blueprint folder is unavailable.");

            string root = Path.GetFullPath(Path.Combine(blueprintFolder, "Guide Check"));
            string prefix = root + Path.DirectorySeparatorChar;
            using (Stream resource = typeof(BlueprintImporter).Assembly.GetManifestResourceStream(ResourceName))
            {
                if (resource == null)
                    throw new InvalidOperationException("The bundled blueprint pack is unavailable.");
                using (var archive = new ZipArchive(resource, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string relative = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                        string destination = Path.GetFullPath(Path.Combine(root, relative));
                        if (Path.IsPathRooted(relative) ||
                            !destination.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException("A blueprint pack path leaves Guide Check.");

                        RejectLinks(destination, root);
                        if (String.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destination);
                            continue;
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        using (Stream input = entry.Open())
                        using (var output = new FileStream(destination, FileMode.Create, FileAccess.Write))
                            input.CopyTo(output);
                    }
                }
            }
        }

        private static void RejectLinks(string destination, string root)
        {
            for (string path = destination; path != null && path.Length >= root.Length;
                path = Path.GetDirectoryName(path))
            {
                if ((File.Exists(path) || Directory.Exists(path)) &&
                    (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Guide Check contains a redirected file or folder.");
            }
        }
    }
}
