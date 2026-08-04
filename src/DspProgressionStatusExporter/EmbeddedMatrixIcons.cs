using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace DspProgressionStatusExporter
{
    internal sealed class EmbeddedMatrixIcons : IDisposable
    {
        private const string ResourcePrefix = "DspGuideCheck.MatrixIcons.";

        private static readonly Dictionary<string, string> ResourceNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "blue", "t-matrix.png" },
                { "red", "e-matrix.png" },
                { "yellow", "c-matrix.png" },
                { "purple", "i-matrix.png" },
                { "green", "g-matrix.png" },
                { "white", "u-matrix.png" },
                { "ils", "1605.png" },
                { "dyson", "solar-collector.png" },
                { "photon", "photon-capacitor-full.png" }
            };

        private readonly Dictionary<string, Sprite> sprites =
            new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private readonly List<Texture2D> textures = new List<Texture2D>();

        private EmbeddedMatrixIcons()
        {
        }

        public static EmbeddedMatrixIcons Load()
        {
            var result = new EmbeddedMatrixIcons();
            foreach (KeyValuePair<string, string> resource in ResourceNames)
                result.TryLoad(resource.Key, ResourcePrefix + resource.Value);
            return result;
        }

        public Sprite Get(string cubeId)
        {
            Sprite sprite;
            return !String.IsNullOrEmpty(cubeId) &&
                sprites.TryGetValue(cubeId, out sprite)
                    ? sprite
                    : null;
        }

        public void Dispose()
        {
            foreach (Sprite sprite in sprites.Values)
                if (sprite != null)
                    UnityEngine.Object.Destroy(sprite);
            foreach (Texture2D texture in textures)
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            sprites.Clear();
            textures.Clear();
        }

        private void TryLoad(string cubeId, string resourceName)
        {
            Texture2D texture = null;
            Sprite sprite = null;
            try
            {
                byte[] bytes = ReadResource(resourceName);
                texture = new Texture2D(
                    2, 2, TextureFormat.RGBA32, false);
                texture.name = "DSPGuideCheck-" + cubeId + "-phase-icon";
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                if (!texture.LoadImage(bytes, true) ||
                    texture.width <= 0 || texture.height <= 0)
                    throw new InvalidOperationException(
                        "Embedded Matrix icon could not be decoded.");

                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                if (sprite == null)
                    throw new InvalidOperationException(
                        "Embedded Matrix icon sprite could not be created.");
                sprite.name = texture.name;
                textures.Add(texture);
                sprites[cubeId] = sprite;
            }
            catch
            {
                if (sprite != null)
                    UnityEngine.Object.Destroy(sprite);
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }
        }

        private static byte[] ReadResource(string resourceName)
        {
            Assembly assembly = typeof(EmbeddedMatrixIcons).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(
                resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        "Embedded Matrix icon resource was not found.");
                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }
    }
}
