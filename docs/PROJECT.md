# DSP Guide Check - Project Definition

## Purpose

DSP Guide Check is the passive, on-demand runtime companion to the DSP
Practical Progression Guide. F8 recalls the phase selected for the current
playthrough, measures that phase's stable readiness objectives, and presents
one concise, useful status conclusion.

The player asks; the instrument answers. It is not an autopilot, factory
score, build designer, combat adviser, optional-route tracker, or unsolicited
warning system.

## Product invariants

- The player owns phase selection.
- Runtime evidence evaluates the selected phase but never changes it.
- Objectives remain stable while a phase is selected.
- Hard objectives, reference paces, warnings, and player judgments remain
  distinct.
- Current Status communicates an actionable conclusion rather than every
  available rate.
- The panel is hidden by default and never alerts by itself.
- F8 never saves; `Save snapshot` is the deliberate forensic export.
- The mod is read-only with respect to the game and save state.
- Collection, normalization, analysis, panel modeling, and UI remain separate.
- Missing or renamed runtime evidence fails softly.
- Combat and optional guide routes remain outside scope.

## Current guide authority

- Published guide: <https://dsp-beginner-guide.pages.dev/>
- Adopted development authority: public guide 2.0 edition
- Published `guide-version` metadata: `1.23.0`
- Detailed derivation and roadmap: `docs/GUIDE-2.0-GAP-ANALYSIS.md`
- Current runtime implementation: guide 2.0 topology, phase contracts, and
  Matrix-icon column with superseded-contract cleanup complete
- Completed 1.22.2 derivation: `docs/GUIDE-1.22.2-GAP-ANALYSIS.md`
- Earlier guide v1.1 record: `docs/GUIDE-REVISION-INGESTION.md` (historical)

The target phase sequence is:

```text
BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
     -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are checkpoints inside ILS. WARP, SPHERE, LOGISTICS,
COMPLETE, and other optional or post-completion routes have no navigation
control, panel, objective contract, finding, or snapshot phase contract.

## Project status

**Guide 2.0 migration:** complete.

`GUIDE2-01`, `GUIDE2-02`, `VIS2-01`, and `GUIDE2-03` have passed their focused
runtime gates. The current runtime retains legacy selection migration and
historical records without exposing superseded phase contracts. See
`docs/GUIDE-2.0-GAP-ANALYSIS.md`.

The production-risk roadmap is paused because of a blocking concern.
`RISK-01` remains accepted and `RISK-02` remains implemented but has not passed
its runtime gate. Neither is part of the guide 2.0 migration.

## Future considerations

These ideas are recorded but are not active work:

- `RATE-01` and `BUFFER-01` remain superseded by `RISK-01` through `RISK-04`,
  but that roadmap is currently paused.
- Keep completed objectives to compact single lines while reserving supporting
  detail for incomplete objectives.
- Limit Pending to the few highest-value actions, ordered by what unlocks or
  constrains progress, without repeating every incomplete objective.
- Limit Current Status to exceptional findings rather than every available
  healthy fact.
- Use a quiet-success conclusion such as `No immediate constraints found`
  when the analyzer has nothing actionable to report.
- Stabilize conclusions across insignificant fluctuations so the panel does
  not rewrite itself on minor rate noise.
- Reserve explicit player checks for important judgments the runtime genuinely
  cannot observe, and present each only once.

## Objective authority

The phase-local `Ready to move on when` checklist is authoritative. The ILS
mission additionally uses its manifest, `Before flying home`, and `Done when`
checkpoints. A numerical pace becomes a hard objective only when the local
readiness text states it.

| Phase | Stable readiness contract |
|---|---|
| BLUE | Starter inputs and routine hardware replenish; Blue Cubes run continuously at 20/min or better; research is not hand-fed. The 15–20 MW planning target is not a fixed objective. |
| RED | Two Labs sustain 20 Red Cubes/min while Refined Oil retains a continuing outlet; Hydrogen or Refined Oil congestion remains a concise diagnostic warning rather than another objective row. |
| ILS | Show one active checkpoint at a time: required technology and carried outpost essentials before launch; same-planet Titanium and Silicon production plus locally stored return cargo during the expedition; then the current research-chain target, missing protected components, and activated Titanium and Silicon ILS routes during the rush. |
| YELLOW | Three configured Yellow-Cube Labs produce continuously. |
| PURPLE | Three configured Purple-Cube Labs produce continuously. |
| GREEN | Two configured Green-Cube Labs produce continuously; Quantum Chips and Graviton Lenses each have visible storage. |
| DYSON | The Photon swarm produces and launches Solar Sails and generates useful power. Reliable Antimatter is a handoff cue to PHOTON, not a duplicate DYSON objective. |
| PHOTON | Critical Photon and Antimatter production is running; actual rates are shown against the 48/min receiver-array reference; 2,000 stored Antimatter marks the midpoint. Current Status compares total receiver demand with available Dyson generation. |
| WHITE | Universe Matrix is researched; ten Labs sustain 40 White Cubes/min with the stored White Cube count shown; Mission Completed consumes or has consumed 4,000 White Cubes. |

Supporting production and soft reference paces appear only when they explain
a real, actionable shortfall. Healthy supporting chains do not create rows of
completed clutter. Mission Completed changes the WHITE presentation to
`Mission Accomplished!` without navigating elsewhere.

## Architecture

```text
Live DSP runtime
    |
Collectors and rolling samplers
    |
Normalized ObservedGameState
    |-- compact forensic JSON snapshot
    `-- selected-phase analysis
            |
        GuidePanelModel
            |
        on-demand Unity panel
```

`Plugin.cs` owns lifecycle and export orchestration. Focused telemetry classes
collect runtime evidence. `ObservedGameState.cs` normalizes it.
`GuideAnalyzer.cs` and `GuideGateEngine.cs` interpret only the player-selected
phase. `ManualPhaseNavigation.cs` owns per-playthrough selection.
`GuidePanelModel.cs` is presentation-ready data, and
`GuidePanelController.cs` contains Unity UI behavior only.

## Evidence policy

- Production rates come from DSP's pre-aggregated one-minute Statistics Panel
  values for a bounded watch set, not inventory deltas or lifetime-rate math.
- Lifetime counters are separate and retained only where aggregate Cube totals
  are useful.
- Dyson generation, sail population, structure progress, and cell progress
  come from native Dyson system and editor aggregates.
- Ejectors, silos, and Ray Receivers retain dedicated component-pool collectors
  for operational facts absent from those aggregates.
- ILS station configuration, vessel deployment, traffic, research, inventory,
  and power evidence are collected only for retained consumers.
- Evidence unavailable through the expected runtime member remains unknown;
  a different proxy is not silently substituted.

The native telemetry derivation and accepted comparison are documented in
`docs/NATIVE-TELEMETRY-ALIGNMENT.md`.

## Selection and persistence

The latest researched Cube seeds a phase only when the playthrough has no
valid stored selection. Thereafter Previous and Next are the sole phase-change
authority. Selection is keyed by playthrough creation time and stable galaxy
descriptor, so autosaves, renamed slots, pauses, and normal restarts retain
the same selection.

BOOTSTRAP is in the legacy normalization set and maps once to BLUE. Existing
mappings remain: FLIGHT and TITANIUM to ILS, SPHERE to
DYSON, WARP to GREEN, and LOGISTICS or COMPLETE to WHITE.

## Snapshot contract

Snapshot schema 2.7 serializes the same selected-phase conclusions used by the
panel plus only the evidence needed to audit implemented functions. Every
snapshot includes provenance, playtime, research and Cube aggregates,
selection diagnostics, objective/status conclusions, focused selected-phase
evidence, collector coverage and performance, and explicit omission or
truncation markers.

Broad factory, technology, station-slot, inventory, topology, and all-item
dumps are excluded. Receiver detail is capped, and the export is rejected
rather than written above 256 KiB. See `docs/SNAPSHOT-REDESIGN.md`.

## Panel contract

The panel is click-through except for phase navigation, collapse, explicit
scrolling, `Save snapshot`, and `DON'T PANIC`. It has no obstructive panel
background. Text uses embedded Basic Regular with a two-pixel dark outline.
The fixed Cube-rate column uses six cached embedded Matrix icons with outlined
threshold-colored rate text and a text-only soft fallback. All established
sizes, spacing, and bounded hover behavior remain. `DON'T PANIC` retains its
separate bright-red Comic Sans presentation.

The Basic font falls back softly to the captured native Goal font if private
runtime registration is unavailable. Its SIL Open Font License notice is
embedded in the assembly and exposed as `Basic-OFL.txt` in public packages.

## Contracts

| Contract | Version |
|---|---:|
| Release line | 1.18.x |
| Snapshot schema | 2.7 |
| Normalized state | 1.9 |
| Guide selection | 1.6 |
| Guide analysis | 2.8 |
| Progression | 2.8 |
| Panel | 2.1 |

The CI run number supplies the release patch. BepInEx and Thunderstore use
the same three-number version; assembly/file metadata adds `.0`, and
diagnostics append the triggering commit hash.

## Validation status

Accepted runtime work for the current implementation includes:

- player-owned navigation and per-playthrough persistence;
- the nine-phase guide 2.0 critical-path objective inventory;
- native one-minute production comparisons against the Statistics Panel;
- native Dyson aggregate comparisons against the Dyson editor;
- focused ILS, DYSON, PHOTON, and WHITE conclusions;
- compact schema 2.7 snapshots and bounded collector diagnostics;
- panel layout, click-through behavior, scrolling, footer actions, and hover
  behavior;
- Basic Regular rendering with the retained outline and separate
  `DON'T PANIC` treatment;
- an extensive representative user test with no reported functional,
  navigation, persistence, layout, footer, or performance defects.

`GUIDE2-01` passed its early-save and working-Blue-factory in-game checkpoint
with correct objectives, prompt completion, and no visible regression.
`GUIDE2-02` passed its focused runtime checkpoint with correct navigation,
phase contracts, exceptional supporting-branch findings, DYSON title and
objectives, and PHOTON receiver status.
`VIS2-01` passed its bright- and dark-background presentation checkpoint with
one, three, and six Matrix icons plus interaction and refresh checks.
`GUIDE2-03` passed its navigation, persistence, removed-contract, control,
performance, and focused BLUE/ILS/WHITE snapshot checkpoint.

The release candidate now has two ordered Thunderstore blockers. First,
`STORE-README-01` replaces the packaged copy with a concise player-facing
README and proves CI uses it. Second, `STORE-SNAPSHOT-01` adds a safe
build-time public variant without the forensic export control, then changes CI
to package only that variant. Their user stories and acceptance criteria are
authoritative in `docs/THUNDERSTORE-PACKAGE.md`.

`STORE-README-01` is implemented and awaits release-owner review of the
packaged copy. `STORE-SNAPSHOT-01` remains queued behind that review.

`docs/RUNTIME-TESTING.md` remains the regression protocol for future runtime
changes. Compilation alone never proves in-game presentation or conclusions.

## Release packaging

Hosted builds publish `DspGuideCheck.dll` in the Thunderstore-compatible ZIP
defined by `docs/THUNDERSTORE-PACKAGE.md`. Game and Unity assemblies are build
inputs only and are never redistributed.

## Historical records

- `docs/GUIDE-REVISION-INGESTION.md`: superseded guide v1.1 adoption record.
- `docs/GUIDE-1.22.2-GAP-ANALYSIS.md`: derivation and completed migration
  stories for the previous critical path.
- `docs/GUIDE-2.0-GAP-ANALYSIS.md`: current authority comparison and nine-phase
  migration acceptance record.
- `CHANGELOG.md`: release-by-release behavior, including retired optional
  route contracts.

Historical documents describe the release in which they were written; they
do not override this current project definition.
