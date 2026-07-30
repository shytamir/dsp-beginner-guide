# DSP Guide Check

DSP Guide Check is an on-demand progression companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It reads the current save, evaluates the guide phase selected by the player,
and presents stable objectives plus concise, phase-aware status.

The player asks; the instrument answers. The panel is hidden by default,
never changes the factory or save, and never advances phases automatically.

Version **1.18.1** adds a focused usability pass to the published guide
contract: outlined panel text, background-free phase controls with bounded
hover growth, selected-route outlines, and click-through non-control surfaces.

## Features

- Manual phase navigation from BOOTSTRAP through WHITE and the
  post-completion LOGISTICS phase.
- Explicit DYSON/SPHERE and optional WARP route selection.
- Stable phase objectives based on the
  [DSP Practical Progression Guide](https://dsp-beginner-guide.pages.dev/).
- Native Statistics Panel production evidence plus focused logistics, power,
  Dyson and Ray Receiver evidence.
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

BepInEx receives the numeric assembly version (`M.m.N.0`), while snapshots and
build reports retain the semantic and release labels above. The artifact test
rejects a generated BepInEx identity that `System.Version` cannot parse.

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
- `Save snapshot` writes one JSON file. The control turns green for two
  seconds when the write succeeds or red when it fails.
- The bright-red `DON'T PANIC` control opens the source guide at the selected
  phase.

The initial phase is seeded once from the latest researched Cube. The selected
phase is then owned entirely by the player and retained separately per
playthrough, including across autosaves, renamed save slots and game restarts.

Main sequence:

```text
BOOTSTRAP → BLUE → RED → FLIGHT → TITANIUM → YELLOW → ILS
→ PURPLE → GREEN → DYSON or SPHERE → PHOTON → WHITE → LOGISTICS
```

WARP is an optional reference route with no completion gate. LOGISTICS is
entered manually after the main progression route.

## Version 1.18.1

All panel text now carries a dark outline for legibility over bright factory
and sky backgrounds. Phase navigation controls use transparent hit areas
instead of block backgrounds, grow to one fixed hover size, and show the
selected DYSON/SPHERE route through a colored outline.

The panel background, edge, viewport, rows, and text are click-through. Only
the collapse, phase navigation, explicit scroll, snapshot, and guide-link
controls capture pointer input.

## Version 1.18.0

GUIDE-01 re-derived the entire phase contract from the published guide rather
than layering new thresholds onto older rules. Objectives now come from each
phase's readiness checklist. Exact production gates are used only where the
checklist names an exact pace; honest player checks remain visible when runtime
state cannot prove intent, preparation, or understanding.

The new LOGISTICS phase covers automated refill of logistics infrastructure,
personal construction resupply, and provider/receiver route literacy. Manual
phase ownership, per-playthrough persistence, compact snapshots, and native
Statistics/Dyson evidence remain unchanged.

## Version 1.17.1

Production telemetry now reads DSP's pre-aggregated one-minute Statistics
Panel values for a bounded guide-relevant item set. Lifetime counters remain
separate and are retained only for lifetime Cube totals. The 1.17.1 hotfix
corrects the compact native pool lookup used for watched items.

Dyson generation, sail population and construction progress now come from the
native Dyson system and node aggregates. Dedicated launch-device and Ray
Receiver collectors remain separate because they answer operational questions
that the aggregate construction totals do not.

Snapshot schema 2.1 and normalized state 1.5 expose the source, scope, period
and coverage of this evidence. The corrected lookup was accepted against the
Statistics, Dyson, and PHOTON runtime checkpoints.

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
validation protocol. The adopted
[guide authority and ingestion record](docs/GUIDE-REVISION-INGESTION.md) and
[native telemetry alignment contract](docs/NATIVE-TELEMETRY-ALIGNMENT.md)
record the published source, the re-derived phase contracts, and their
evidence mapping.

## Development and contributions

Before making changes, read [AGENTS.md](AGENTS.md) for repository-specific
engineering and agent instructions. Contribution expectations and bug-report
evidence are described in [CONTRIBUTING.md](CONTRIBUTING.md).

## Safety and privacy

The plugin is read-only with respect to DSP state. It writes a snapshot only
when `Save snapshot` is clicked. Snapshots may contain planet names, selected
phase conclusions and focused factory evidence; review them before publishing.

## Scope

DSP Guide Check supports practical progression through Mission Accomplished
and the guide's focused post-completion LOGISTICS phase. It is not a combat
adviser, ratio calculator, build planner or general post-game dashboard.

## Residual issues

- GUIDE-01 requires the representative in-game acceptance pass documented in
  `docs/RUNTIME-TESTING.md`; compilation alone cannot validate player-facing
  conclusions or manual checklist ergonomics.
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
