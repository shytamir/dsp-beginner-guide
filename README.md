# DSP Guide Check

DSP Guide Check is an on-demand progression companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It reads the current save, evaluates the guide phase selected by the player,
and presents stable objectives plus concise, phase-aware status.

The player asks; the instrument answers. The panel is hidden by default,
never changes the factory or save, and never advances phases automatically.

Version **1.16.0** introduces compact diagnostic snapshots and retains the
per-playthrough phase-persistence repair from 1.15.1.

## Features

- Manual phase navigation from BOOTSTRAP through WHITE.
- Explicit DYSON/SPHERE and optional WARP route selection.
- Stable phase objectives based on the
  [DSP Practical Progression Guide](https://dsp-beginner-guide.pages.dev/).
- Rolling production, logistics, power, Dyson and Ray Receiver evidence.
- Sixty-second receiver continuity tracking for the PHOTON phase.
- Player-requested JSON snapshots for diagnostics and guide development.
- Native-styled, collapsible and scrollable panel.
- No unsolicited notifications, automatic phase changes or factory actions.

## Requirements

- Dyson Sphere Program.
- BepInEx 5 installed in the game directory.
- .NET SDK capable of building .NET Framework 4.7.2 projects, if building from
  source.

The public release was validated with DSP Early Access `0.10.34.28529`.

## Build

Run `build.cmd` from the repository root. By default it uses the standard
Steam installation:

```text
C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program
```

To use another game directory:

```text
build.cmd "D:\Games\Dyson Sphere Program"
```

The DLL is written to:

```text
src\DspProgressionStatusExporter\bin\Release\net472\
```

The game and BepInEx assemblies are referenced from the local installation;
they are not redistributed in this repository.

## Versioning and continuous integration

`VERSION` records the manually selected major and minor version numbers. Every
push to `main` runs the build, artifact-verification, and packaging workflow.
The workflow sequence becomes the patch number and the triggering commit is
included in both published forms:

```text
Release/artifact label: M.m.N.X
Semantic version:       M.m.N+X
```

For example, workflow run 42 for commit `abcdef1` publishes the artifact label
`1.16.42.abcdef1` and embeds semantic version `1.16.42+abcdef1`. The assembly
and file version remain numeric (`1.16.42.0`). The workflow sequence advances
without committing a generated version change back to `main`.

The hosted build downloads the official BepInEx 5 release as a compile
reference and restores pinned Unity reference packages. Local release builds
continue to use the assemblies supplied by the installed game and remain the
authoritative compatibility check.

## Install

Copy `DspProgressionStatusExporter.dll` into a folder beneath:

```text
Dyson Sphere Program\BepInEx\plugins\
```

For example:

```text
Dyson Sphere Program\BepInEx\plugins\DSPGuideCheck\
```

## Use

Press **F8** after loading a save:

- F8 opens or closes the panel.
- Previous and next move between phases.
- Route controls select DYSON or SPHERE and enter or leave WARP.
- `Save snapshot` writes one JSON file and opens its directory.
- The bright-red `DON'T PANIC` control opens the source guide at the selected
  phase.

The initial phase is seeded once from the latest researched Cube. The selected
phase is then owned entirely by the player and retained separately per
playthrough, including across autosaves, renamed save slots and game restarts.

Main sequence:

```text
BOOTSTRAP → BLUE → RED → FLIGHT → TITANIUM → YELLOW → ILS
→ PURPLE → GREEN → DYSON or SPHERE → PHOTON → WHITE
```

WARP is an optional detour.

## Version 1.16.0

Saved snapshots now use schema 2.0. They contain the selected phase and route,
objective and Current Status conclusions, aggregate research and Cube totals,
and focused evidence for the selected phase. Broad factory, player, technology,
inventory, station and all-item dumps are deliberately omitted.

Snapshots include compact collector coverage so unavailable or incomplete
evidence remains visible. PHOTON receiver detail is capped and marked when
truncated, and the entire JSON export is bounded to 256 KiB.

## Version 1.15.1

This maintenance release replaces the mutable save-name persistence key with
an identity based on the playthrough creation time and galaxy descriptor.
Existing selection data for the currently loaded legacy key is migrated once
when available. Snapshot selection diagnostics identify whether the phase was
restored, migrated or initially seeded.

## Version 1.15.0

This release includes the complete manual-navigation, SPHERE and PHOTON
roadmap:

- automatic phase transitions and the redundant COMPLETE phase are removed;
- SPHERE recognizes active construction without requiring an arbitrary rocket
  rate;
- PHOTON tracks receiver mode, lenses, warmup, strength, requested and
  supplied Dyson power, Critical Photon output and sustained continuity;
- receiver discovery uses the dedicated gamma-generator pool instead of
  scanning every factory entity;
- the guide link is now the two-line Comic Sans `DON'T PANIC` control.

In late-game validation, the receiver collection pass fell from roughly
67-139 ms to about 1.5-1.7 ms in captured frames, with a 7.2 ms recorded
maximum. A faint periodic hitch remained perceptible on that save, so this is
reported as a substantial reduction rather than complete elimination.

## Repository layout

```text
.
├── AGENTS.md
├── .github/ISSUE_TEMPLATE/
├── docs/
├── src/DspProgressionStatusExporter/
├── build.cmd
├── CHANGELOG.md
├── CONTRIBUTING.md
├── LICENSE
└── README.md
```

See [docs/PROJECT.md](docs/PROJECT.md) for product and evidence contracts and
[docs/RUNTIME-TESTING.md](docs/RUNTIME-TESTING.md) for the focused runtime
validation protocol. The prepared
[guide-revision ingestion contract](docs/GUIDE-REVISION-INGESTION.md) and
[native telemetry alignment pass](docs/NATIVE-TELEMETRY-ALIGNMENT.md) define
the next evidence work without adopting the forthcoming guide revision early.

## Development and contributions

Before making changes, read [AGENTS.md](AGENTS.md) for repository-specific
engineering and agent instructions. Contribution expectations and bug-report
evidence are described in [CONTRIBUTING.md](CONTRIBUTING.md).

## Safety and privacy

The plugin is read-only with respect to DSP state. It writes a snapshot only
when `Save snapshot` is clicked. Snapshots may contain planet names, selected
phase conclusions and focused factory evidence; review them before publishing.

## Scope

DSP Guide Check supports practical progression through Mission Accomplished.
It is not a combat adviser, ratio calculator, build planner or general
post-game dashboard.

## Residual issues

- The current guide-derived phase contracts have not yet been reconciled with
  the forthcoming guide revision. The revision will be ingested as a new,
  reviewable contract rather than layered onto existing rules.
- Production rates are still reconstructed from lifetime game-stat counters,
  and Dyson construction detail is still reconstructed below the aggregate
  level used by the native Statistics and Dyson panels. `TEL-01` records the
  required native-aggregate alignment and runtime comparison.
- The v1.15.1 persistence repair and snapshot schema 2.0 still share a deferred
  full-playthrough runtime checkpoint; defects will be handled when they
  surface during the next appropriate test cycle.
- Hosted CI builds against pinned public Unity references and official BepInEx
  5 references. A release build against the installed game's assemblies
  remains the authoritative compatibility check.

## License

DSP Guide Check is licensed under the
[Apache License 2.0](LICENSE).

## Disclaimer

This is an unofficial community project. Dyson Sphere Program and its assets
belong to their respective owners. BepInEx and the game are required but are
not included.
