using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DspProgressionStatusExporter
{
    internal sealed class EmbeddedBasicFont : IDisposable
    {
        private const string ResourceName =
            "DspGuideCheck.Fonts.Basic-Regular.ttf";

        private IntPtr fontMemory;
        private IntPtr fontResource;
        private bool ownsFont;

        public Font Font { get; private set; }
        public string Source { get; private set; }

        private EmbeddedBasicFont()
        {
        }

        public static EmbeddedBasicFont Load(Font fallback, int size)
        {
            Font loaded = FindLoadedFont();
            if (loaded != null)
            {
                return new EmbeddedBasicFont
                {
                    Font = loaded,
                    Source = "loaded-basic-font"
                };
            }

            var result = new EmbeddedBasicFont
            {
                Font = fallback,
                Source = "native-fallback"
            };

            try
            {
                byte[] bytes = ReadResource();
                result.fontMemory = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, result.fontMemory, bytes.Length);

                uint fontCount = 0;
                result.fontResource = AddFontMemResourceEx(
                    result.fontMemory,
                    (uint)bytes.Length,
                    IntPtr.Zero,
                    ref fontCount);
                if (result.fontResource == IntPtr.Zero || fontCount == 0)
                {
                    result.ReleaseResource();
                    return result;
                }

                Font font = Font.CreateDynamicFontFromOSFont(
                    new string[] { "Basic", "Basic Regular" },
                    size);
                if (font == null)
                {
                    result.ReleaseResource();
                    return result;
                }

                result.Font = font;
                result.Source = "embedded-google-fonts-basic";
                result.ownsFont = true;
            }
            catch
            {
                result.ReleaseResource();
            }

            return result;
        }

        public void Dispose()
        {
            if (ownsFont && Font != null)
                UnityEngine.Object.Destroy(Font);

            Font = null;
            ownsFont = false;
            ReleaseResource();
        }

        private static Font FindLoadedFont()
        {
            Font[] loaded = Resources.FindObjectsOfTypeAll<Font>();
            if (loaded == null)
                return null;

            foreach (Font candidate in loaded)
            {
                if (candidate != null &&
                    candidate.name.IndexOf(
                        "basic",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate;
            }

            return null;
        }

        private static byte[] ReadResource()
        {
            Assembly assembly = typeof(EmbeddedBasicFont).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(
                ResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        "Embedded Basic font resource was not found.");
                }

                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

        private void ReleaseResource()
        {
            if (fontResource != IntPtr.Zero)
            {
                try
                {
                    RemoveFontMemResourceEx(fontResource);
                }
                catch
                {
                    // The presentation safely retains the captured native font.
                }

                fontResource = IntPtr.Zero;
            }

            if (fontMemory != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(fontMemory);
                fontMemory = IntPtr.Zero;
            }
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(
            IntPtr fontData,
            uint dataSize,
            IntPtr reserved,
            ref uint fontCount);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveFontMemResourceEx(
            IntPtr fontResourceHandle);
    }
}
