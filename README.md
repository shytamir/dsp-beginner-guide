# DSP Guide Check

DSP Guide Check is an on-demand progression companion for
[Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/).
It reads the current save, evaluates the guide phase selected by the player,
and presents stable objectives plus concise, phase-aware status.

The player asks; the instrument answers. The panel is hidden by default,
never changes the factory or save, and never advances phases automatically.

The released baseline implements guide **2.3**. The **2.2.x candidate** adds the
accepted Guide 3.0 improvements and awaits owner runtime acceptance. See the
[workshop checklist](docs/management/GUIDE3-WORKSHOP.md). The candidate retains nine phases; its implementation state is described under
[Project status](#project-status).

## Features

- Manual phase navigation through the implemented nine critical-path
  phases.
- In the current candidate, explicit ILS Departure / Haulback / Automation
  selection persists per playthrough and controls the guide link.
- Stable phase objectives based on the
  [DSP Practical Progression Guide](https://dsp-beginner-guide.pages.dev/).
- Native Statistics Panel production evidence plus focused logistics, power,
  Dyson and Ray Receiver evidence.
- Sixty-second receiver continuity tracking for the DYSON bridge, with up to
  two unhealthy samples treated as noise.
- Player-requested JSON snapshots for diagnostics and guide development.
- Native-styled, collapsible and scrollable panel.
- Embedded Matrix icons in the click-through Cube-rate column.
- Distinct native signal glyphs for a developing shortage or stopped supply;
  quiet production states add no indicator.
- Up to three stable, compact production-risk conclusions with immediate
  recommendations in a separate Next Actions section.
- DSP's live vein-label typography with an embedded Basic Regular fallback;
  `DON'T PANIC` retains its separate Comic Sans treatment.
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

### Verify the workshop candidate

From a clean checkout, run with PowerShell 7:

```powershell
pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1
```

Use `-GameRoot` for another DSP installation and `-Sequence N` to match a
GitHub workflow run number. The default sequence 1 is local verification only;
CI keeps using its run number as the patch. Output under ignored
`artifacts/guide3/candidate-<source>-<sequence>/` includes both DLLs, public ZIP,
source archive, versions/hashes, test logs, snapshot fixtures and workshop copy.
The command verifies installed game references and uses the existing local SDK.
This desktop's sandbox cannot read the SDK registry paths: run the build under
the authenticated desktop context. PowerShell 7 is required for the local
hidden-controller fixture; Windows PowerShell 5 cannot resolve its Unity calls.
No game session or player save is needed. `-AllowWorkingTree` is a preflight
option; its output is explicitly not a workshop candidate.

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

For example, workflow run 42 produces package and BepInEx version `2.2.42`,
assembly/file version `2.2.42.0`, and diagnostic label
`2.2.42.abcdef1`. The workflow sequence advances without committing a
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
- The collapse-proof bright-red `DON'T PANIC` control below the Cube-rate
  column opens the source guide at the selected phase. During ILS it opens the
  preparation, expedition, or automation stage currently shown by the panel.

The repository's default diagnostic build also includes `Save snapshot`. It
writes one JSON file and gives two seconds of green or red footer feedback.
The public Thunderstore build omits that control.

The initial phase is seeded once from the latest researched Cube. The selected
phase is then owned entirely by the player and retained separately per
playthrough, including across autosaves, renamed save slots and game restarts.

Critical-path sequence:

```text
BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
     -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are checkpoints inside ILS. Optional WARP, SPHERE, and
LOGISTICS material remains in the source guide but does not receive a panel.

### Expert mode

In the mod's BepInEx config file, set:

```ini
[General]
ExpertMode = true
```

Restart DSP after changing the setting. It defaults to false. Expert mode shows
only the existing Cube-rate bar and `DON'T PANIC` button when F8 opens it.
It omits the adjoining panel and every other control, including navigation,
collapse, ILS stages, risk glyph and diagnostic snapshot button. Stored phase
and stage still determine rates and the guide link. Set false and restart to
restore normal controls. F8 starts hidden and never saves in either mode.

## Current contract

- Objectives come from each retained phase's local readiness checklist.
- BLUE consolidates starter-input continuity and routine-hardware
  replenishment with the Blue science loop; it does not impose a fixed power
  target or enumerate every healthy mall product.
- Exact production gates are used only where that checklist names an exact
  pace: Blue 20/min, Red 20/min, and White 40/min.
- For those exact-rate Cube phases, demand-driven risk stays quiet while the
  current Cube rate meets or exceeds the goal, even if faster research draws
  down stored surplus.
- WHITE keeps its configured-Lab and stored-White-Cube evidence concise and
  reports the strongest authoritative Mission Completed state available.
- ILS presents only the active preparation, expedition, or research-rush
  checkpoint, excludes the starter planet from outpost evidence, and uses
  planet-local cargo evidence rather than global stock.
- YELLOW and PURPLE require three supplied Labs without a separate storage
  objective. Their direct inputs remain eligible for genuine shortage warnings.
- DYSON includes swarm production, receiver research/continuity and conversion
  to stationary Antimatter. Hydrogen disposal and science delivery remain player
  checks; the guide link opens the bridge once the swarm is ready.
- PHOTON requires five colored Cubes and Antimatter at sustained 40/min plus
  2,000 stationary Antimatter. Its Cube colors use current 40/min readiness;
  receiver power remains diagnostic.
- No phase displays a fixed factory-power objective.
- Production uses DSP's pre-aggregated one-minute and normalized ten-minute
  Statistics Panel values.
- Dyson generation, sail population, and construction progress use the native
  Dyson system and editor aggregates.
- Compact snapshot schema 2.24 exports conclusions, production-risk terms,
  provenance, collector health, and only the focused evidence needed to audit
  those conclusions.
- The click-through panel reuses DSP's vein-label font, material, and outline,
  with embedded Basic Regular as a soft fallback, and embeds the six Matrix
  icons beside Cube rates. Only its explicit controls capture pointer input.

The critical-path migration, telemetry alignment, persistence, snapshot, and
baseline panel contracts completed in-game acceptance; the Guide 3.0 candidate
has not. Baseline packaging passed its
automated contract checks. See [CHANGELOG.md](CHANGELOG.md) for release
history.

## Project status

The released product is complete for the adopted guide 2.3 contract. The owner
authorized [the Guide 3.0 roadmap](docs/ROADMAP.md); GC3-01 through GC3-11 are technically
complete; G1/G2/G3 passed. GC3-12 passed local preflight and awaits the final
committed candidate and hosted CI confirmation. [Execution records](docs/management/GUIDE3-EXECUTION.md)
separate automated completion from the pending final in-game owner workshop.
[docs/PROJECT.md](docs/PROJECT.md) remains the authority for current state and
scope; the [gap analysis](docs/management/GUIDE3-GAP-ANALYSIS.md) explains the
implemented fixes and deliberate exclusions.

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

See [docs/PROJECT.md](docs/PROJECT.md) for the current product contract,
[docs/RUNTIME-TESTING.md](docs/RUNTIME-TESTING.md) for maintenance regression,
and [docs/NATIVE-TELEMETRY-ALIGNMENT.md](docs/NATIVE-TELEMETRY-ALIGNMENT.md) for
runtime evidence sources. Completed planning and validation records are kept
under [docs/archive/](docs/archive/README.md).

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
