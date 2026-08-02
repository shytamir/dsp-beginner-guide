# Thunderstore Package Contract

## Purpose

The hosted build produces one installable Thunderstore package for DSP Guide
Check. The repository source layout and C# namespace remain unchanged; the
public assembly and package identity are `DspGuideCheck.dll` and
`DSPGuideCheck`.

## ZIP layout

The installable ZIP contains exactly these files:

```text
manifest.json
README.md
icon.png
Basic-OFL.txt
BepInEx/
  plugins/
    DSP-Guide-Check/
      DspGuideCheck.dll
```

The three required Thunderstore files are at the ZIP root. No source,
diagnostic reports, game assemblies, or additional wrapper directory belongs
inside the installable package.

## Manifest

`packaging/manifest.template.json` is the source template. CI replaces the
single `{{VERSION_NUMBER}}` placeholder before packaging.

- Name: `DSPGuideCheck`
- Website: `https://github.com/shytamir/dsp-beginner-guide`
- Dependency: `xiaoye97-BepInEx-5.4.17`
- Description: the plain-text first paragraph of the repository README
- Version: `M.m.N`

The package README is maintained separately at `packaging/README.md`. It uses
portable Markdown and absolute public links so it renders outside GitHub.
`packaging/icon.png` is a 256 by 256 PNG with no game-owned assets.

## Version mapping

`VERSION` provides `M` and `m`. The GitHub Actions run number supplies `N`.
The same `M.m.N` value is used by the Thunderstore manifest, BepInEx plugin
identity, and product semantic version.

Assembly and file metadata retain the required four-number representation
`M.m.N.0`. The commit-bearing `M.m.N.X` release label remains diagnostic
metadata only and is not a Thunderstore version.

## Build and validation

The workflow:

1. checks out the triggering commit;
2. derives the build versions;
3. restores compile references;
4. builds `DspGuideCheck.dll`;
5. verifies DLL identity and version metadata;
6. renders the manifest template and creates the exact ZIP layout;
7. verifies file names, casing, count, manifest fields, dependency, font
   license, UTF-8
   README, 256 by 256 PNG icon, and non-empty DLL;
8. uploads the installable ZIP with build and package reports beside it.

The GitHub artifact is a transport container. Its
`DSPGuideCheck-M.m.N.zip` member is the package intended for a mod manager or
Thunderstore upload.
