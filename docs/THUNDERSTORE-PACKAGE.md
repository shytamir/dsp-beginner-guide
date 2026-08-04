# Thunderstore Package Contract

## Purpose

The hosted build produces one installable Thunderstore package for DSP Guide
Check plus separate diagnostic and public DLL artifacts. The repository source
layout and C# namespace remain unchanged; the public assembly and package
identity are `DspGuideCheck.dll` and `DSPGuideCheck`.

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
4. builds and verifies the default diagnostic DLL with snapshot control;
5. builds and verifies the public DLL without snapshot control;
6. renders the manifest template and packages only the public DLL in the exact
   ZIP layout;
7. verifies file names, casing, count, manifest fields, dependency, font
   license, UTF-8 README, 256 by 256 PNG icon, and an exact public-DLL hash
   match;
8. uploads the installable ZIP, both identifiable DLL variants, and their
   build and package reports.

The GitHub artifact is a transport container. Its
`DSPGuideCheck-M.m.N.zip` member is the package intended for a mod manager or
Thunderstore upload.

## Release-blocking next actions

Complete these stories in order before publishing the release candidate.

### STORE-README-01 — Give mod users a purpose-built store README

**Status:** Completed and accepted by the release owner.

**User story:** As a Thunderstore user, I want a concise README that tells me
how to install and open DSP Guide Check, so I can start using it without
reading development or telemetry documentation.

Acceptance criteria:

- `packaging/README.md` is the authoritative store README, and CI explicitly
  packages that file rather than the repository-root README;
- it links to the published practical progression guide and the mod source
  repository;
- its quick start encourages the player to install the mod, load a save, press
  F8, and discover the panel directly;
- it explains only the player-facing installation, controls, behavior, and
  support path needed on the store page;
- it does not mention snapshot export, schema details, build tooling, or other
  developer-facing internals;
- package validation proves the ZIP contains the dedicated README, and the
  rendered copy is presented to the release owner for final review.

### STORE-SNAPSHOT-01 — Omit snapshot export from the public package

**Status:** Implemented; human validation gate pending.

**User story:** As a Thunderstore user, I want the public panel to omit the
forensic snapshot control, so the normal mod surface stays focused on guide
progression while maintainers retain a diagnostic build when needed.

Acceptance criteria:

- a build-time switch disables creation, layout, visibility, and interaction
  of the `Save snapshot` control without panel exceptions or dead space;
- the ordinary diagnostic build retains snapshot export, while a distinct
  public build omits its control;
- the build-time switch and both local variants are implemented and validated
  before the hosted workflow is changed;
- CI produces identifiable diagnostic and public DLL variants, and only the
  public no-control DLL enters the Thunderstore ZIP;
- the store README contains no snapshot-export reference;
- an in-game gate confirms the diagnostic control remains available, the
  public control is absent, and the remaining panel layout and controls work.
