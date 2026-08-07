# Thunderstore Package Contract

The hosted workflow produces one installable Thunderstore ZIP plus separate
diagnostic and public DLL artifacts. Only the public DLL belongs in the ZIP.

## ZIP layout

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

The required Thunderstore files are at the ZIP root. Source files, diagnostic
reports, game assemblies, and wrapper directories are excluded.

## Package sources

- `packaging/manifest.template.json`: manifest template; CI replaces only
  `{{VERSION_NUMBER}}`.
- `packaging/README.md`: player-facing store README.
- `packaging/icon.png`: 256 by 256 package icon.
- Public `DspGuideCheck.dll`: compiled with snapshot control disabled.
- Embedded `Basic-OFL.txt`: license notice for the packaged Basic Regular
  fallback font.

Manifest identity:

- Name: `DSPGuideCheck`.
- Website: `https://github.com/shytamir/dsp-beginner-guide`.
- Dependency: `xiaoye97-BepInEx-5.4.17`.
- Description: plain-text first paragraph of the repository README.
- Version: `M.m.N`.

## Version mapping

`VERSION` supplies `M` and `m`; the GitHub Actions run number supplies `N`.
Thunderstore, BepInEx, and product semantic versions use `M.m.N`. Assembly and
file metadata use `M.m.N.0`. The commit-bearing release label is diagnostic
metadata only.

## Build and validation

The workflow:

1. checks out the triggering commit and derives its build versions;
2. restores pinned compile references, including BepInEx 5.4.17;
3. builds and verifies the diagnostic DLL with snapshot control;
4. builds and verifies the public DLL without snapshot control;
5. packages only the public DLL with the exact files above;
6. verifies names, casing, count, manifest fields, dependency, license, UTF-8
   README, icon dimensions, and the packaged DLL hash;
7. uploads the ZIP, both DLL variants, and diagnostic reports.

The GitHub artifact is a transport container. Its
`DSPGuideCheck-M.m.N.zip` member is the installable package intended for a mod
manager or Thunderstore upload.

Completed packaging stories and release gates are retained in
[`docs/archive/project-management/THUNDERSTORE-PACKAGE-IMPLEMENTATION.md`](archive/project-management/THUNDERSTORE-PACKAGE-IMPLEMENTATION.md).
