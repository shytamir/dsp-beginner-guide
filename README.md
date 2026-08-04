# DSP Guide Check

DSP Guide Check is an on-demand progression companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It reads the current save, evaluates the guide phase selected by the player,
and presents stable objectives plus concise, phase-aware status.

The player asks; the instrument answers. The panel is hidden by default,
never changes the factory or save, and never advances phases automatically.

The current development contract follows guide **1.22.2** and supports only
the ten critical-path phases from BOOTSTRAP through WHITE.

## Features

- Manual phase navigation through the ten critical-path phases.
- Stable phase objectives based on the
  [DSP Practical Progression Guide](https://dsp-beginner-guide.pages.dev/).
- Native Statistics Panel production evidence plus focused logistics, power,
  Dyson and Ray Receiver evidence.
- Sixty-second receiver continuity tracking for the PHOTON phase.
- Player-requested JSON snapshots for diagnostics and guide development.
- Native-styled, collapsible and scrollable panel.
- Embedded Basic Regular presentation font with the existing high-contrast
  outline; `DON'T PANIC` retains its separate Comic Sans treatment.
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
src\DspProgressionStatusExporter\bin\Release\net472\DspGuideCheck.dll
```

The game and BepInEx assemblies are referenced from the local installation;
they are not redistributed in this repository.

## Versioning and continuous integration

`VERSION` records the manually selected major and minor version numbers. Every
push to `main` runs the build, DLL verification, Thunderstore packaging, and
package verification workflow. The workflow sequence becomes the patch
number:

```text
Package/plugin version: M.m.N
Assembly/file version:  M.m.N.0
Diagnostic label:       M.m.N.X
```

For example, workflow run 42 publishes package and BepInEx version `1.18.42`,
assembly/file version `1.18.42.0`, and diagnostic label
`1.18.42.abcdef1`. The workflow sequence advances without committing a
generated version change back to `main`.

The hosted build downloads the official BepInEx 5 release as a compile
reference and restores pinned Unity reference packages. Local release builds
continue to use the assemblies supplied by the installed game and remain the
authoritative compatibility check.

BepInEx and the Thunderstore manifest receive the same three-number version.
Snapshots and reports retain the commit-bearing diagnostic label. The build
test rejects an invalid BepInEx identity, and the package test rejects an
incorrect manifest, icon, README, file name, or ZIP layout.

The exact deployment contract is documented in
[docs/THUNDERSTORE-PACKAGE.md](docs/THUNDERSTORE-PACKAGE.md).

## Install

Copy `DspGuideCheck.dll` into:

```text
Dyson Sphere Program\BepInEx\plugins\DSP-Guide-Check\
```

## Use

Press **F8** after loading a save:

- F8 opens or closes the panel.
- Previous and next move between phases.
- `Save snapshot` writes one JSON file. The control turns green for two
  seconds when the write succeeds or red when it fails.
- The bright-red `DON'T PANIC` control opens the source guide at the selected
  phase.

The initial phase is seeded once from the latest researched Cube. The selected
phase is then owned entirely by the player and retained separately per
playthrough, including across autosaves, renamed save slots and game restarts.

Critical-path sequence:

```text
BOOTSTRAP -> BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
          -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are checkpoints inside ILS. Optional WARP, SPHERE, and
LOGISTICS material remains in the source guide but does not receive a panel.

## Current contract

- Objectives come from each retained phase's local readiness checklist.
- Exact production gates are used only where that checklist names an exact
  pace: Blue 20/min, Red 20/min, and White 40/min.
- ILS presents only the active preparation, expedition, or research-rush
  checkpoint and uses planet-local cargo evidence rather than global stock.
- DYSON reports Solar Sail production, launches, and swarm generation;
  PHOTON reports actual Photon and Antimatter rates plus receiver demand
  against available Dyson generation.
- No phase displays a fixed factory-power objective.
- Production uses DSP's pre-aggregated one-minute and normalized ten-minute
  Statistics Panel values.
- Dyson generation, sail population, and construction progress use the native
  Dyson system and editor aggregates.
- Compact snapshot schema 2.4 exports conclusions, provenance, collector
  health, and only the focused evidence needed to audit those conclusions.
- The click-through panel uses Basic Regular with a dark outline. Only its
  explicit controls capture pointer input.

The critical-path migration, telemetry alignment, persistence, snapshot, and
panel contracts have completed in-game acceptance. Packaging has passed its
automated contract checks. See [CHANGELOG.md](CHANGELOG.md) for release
history.

## Active development

The Cube-rate column is implemented and accepted. Active development now
focuses on a conservative production-risk analyzer built through the existing
telemetry and analysis architecture:

1. verify and expose native multi-window evidence;
2. model accessible buffer runway and proven backpressure;
3. calculate and interpret production risk deterministically; and
4. add one quiet, click-through phase-health accent for expanded and collapsed
   panel states.

The package that inspired the feature is not an implementation dependency.
The adopted math, evidence safeguards, four user stories, and runtime gates
are defined in
[docs/PRODUCTION-RISK-ROADMAP.md](docs/PRODUCTION-RISK-ROADMAP.md).

`RISK-01` is implemented and awaits its native one-minute/ten-minute runtime
comparison gate before buffer or scoring work begins.

## Repository layout

```text
.
├── AGENTS.md
├── .github/ISSUE_TEMPLATE/
├── docs/
├── packaging/
├── scripts/
├── src/DspProgressionStatusExporter/
├── build.cmd
├── CHANGELOG.md
├── CONTRIBUTING.md
├── LICENSE
└── README.md
```

See [docs/PROJECT.md](docs/PROJECT.md) for product and evidence contracts and
[docs/RUNTIME-TESTING.md](docs/RUNTIME-TESTING.md) for the focused runtime
validation protocol. The current
[guide v1.22.2 gap analysis](docs/GUIDE-1.22.2-GAP-ANALYSIS.md), earlier
[guide authority and ingestion record](docs/GUIDE-REVISION-INGESTION.md), and
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

DSP Guide Check supports the guide's default critical path through Mission
Accomplished. It does not provide panels for optional paths, and it is not a
combat adviser, ratio calculator, build planner, or post-game dashboard.

## Residual issues

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
