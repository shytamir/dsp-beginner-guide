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
- Adopted guide version: `1.22.2`
- Detailed derivation: `docs/GUIDE-1.22.2-GAP-ANALYSIS.md`
- Earlier guide v1.1 record: `docs/GUIDE-REVISION-INGESTION.md` (historical)

The active phase sequence is:

```text
BOOTSTRAP -> BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
          -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are checkpoints inside ILS. WARP, SPHERE, LOGISTICS,
COMPLETE, and other optional or post-completion routes have no navigation
control, panel, objective contract, finding, or snapshot phase contract.

## Project status

**Active:** information-quality pass (`RATE-01` and `BUFFER-01`).

The current implementation is accepted against the guide 1.22.2 critical
path. The active work improves how supporting production evidence becomes a
player-facing conclusion; it does not change phase objectives, navigation, or
the underlying native telemetry sources.

### RATE-01 - Show rates only when they explain a problem

**User story:** As a guide reader, I want supporting production rates to appear
only when they explain an actionable shortfall, so the panel gives me useful
knowledge instead of reproducing the Statistics Panel.

Scope:

- Preserve exact checklist rates as phase objectives where the guide makes
  them hard requirements.
- Suppress healthy supporting-rate findings.
- When supporting production is genuinely limiting the selected phase,
  present one causal conclusion naming the constraint and affected goal.
- Include a current and desired rate only when those numbers help the player
  judge the shortfall; otherwise prefer the conclusion alone.
- Continue using native one-minute Statistics Panel aggregates and retain the
  focused evidence in snapshots for auditability.

Acceptance:

- Healthy supporting chains add no rate-dashboard rows.
- A sustained, causally relevant shortfall produces a concise actionable
  finding rather than a list of every related rate.
- Hard objective completion, manual phase ownership, and snapshot evidence do
  not regress.
- Missing or immature rate evidence remains unknown and does not create a
  confident diagnosis.

### BUFFER-01 - Interpret idle production in demand and buffer context

**User story:** As a guide reader, I want an idle but adequately buffered line
to be treated as healthy, so normal production shutdown does not become a
false shortage warning.

Scope:

- Evaluate supporting production together with current consumption, net
  deficit, and relevant owned or stored stock.
- Treat zero or low production as non-problematic when there is no active
  deficit or the available buffer credibly covers current demand.
- Warn only when active demand, net depletion, and inadequate buffer agree on
  a real risk; do not infer failure from production rate alone.
- Report unavailable demand or stock evidence honestly instead of substituting
  inventory deltas or speculative restart assumptions.
- Export the compact demand, deficit, buffer, and conclusion evidence needed
  to validate the decision.

Acceptance:

- A full, idle, or demand-free buffered line does not produce a shortage
  warning.
- A consuming line with a sustained deficit and insufficient runway produces
  a concise warning identifying the endangered phase need.
- The same evidence yields the same deterministic conclusion in the panel,
  analyzer, and snapshot.
- No additional factory-wide scan, automatic phase behavior, or unsolicited
  alert is introduced.

## Future considerations

These ideas are recorded but are not active work:

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
| BOOTSTRAP | Inputs arrive continuously; routine mall hardware replenishes automatically; the grid supplies roughly 5-10 MW. |
| BLUE | Blue Cubes run continuously at 20/min or better; research is not hand-fed; roughly 5-10 MW is available. |
| RED | Two Labs sustain 20 Red Cubes/min while Hydrogen and Refined Oil both leave the Refineries. |
| ILS | Prepare the trip; establish powered remote Titanium and Silicon smelting; return with 860 Titanium Ingots and 520 High-Purity Silicon; complete the finite 200-Yellow-Cube purchase; deploy two ILS towers and five Vessels; make both resources arrive home automatically. |
| YELLOW | Three configured Yellow-Cube Labs produce continuously. |
| PURPLE | Three configured Purple-Cube Labs produce continuously. |
| GREEN | Two configured Green-Cube Labs produce continuously; Quantum Chips and Graviton Lenses each have visible storage. |
| DYSON | Critical Photons become Antimatter reliably, and Antimatter reaches the science district without hand-carrying. |
| PHOTON | At least 2,000 Antimatter is stored, plus an explicit player check that the rising trend can support WHITE. |
| WHITE | Universe Matrix is researched; all six inputs reach the Labs continuously; ten Labs sustain 40 White Cubes/min; Mission Completed consumes or has consumed 4,000 White Cubes. |

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

Legacy removed selections normalize once: FLIGHT and TITANIUM to ILS, SPHERE
to DYSON, WARP to GREEN, and LOGISTICS or COMPLETE to WHITE.

## Snapshot contract

Snapshot schema 2.2 serializes the same selected-phase conclusions used by the
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
background. Text uses embedded Basic Regular with a two-pixel dark outline;
all established sizes, spacing, colors, and bounded hover behavior remain.
`DON'T PANIC` retains its separate bright-red Comic Sans presentation.

The Basic font falls back softly to the captured native Goal font if private
runtime registration is unavailable. Its SIL Open Font License notice is
embedded in the assembly and exposed as `Basic-OFL.txt` in public packages.

## Contracts

| Contract | Version |
|---|---:|
| Release line | 1.18.x |
| Snapshot schema | 2.2 |
| Normalized state | 1.6 |
| Guide selection | 1.5 |
| Guide analysis | 2.5 |
| Progression | 2.5 |
| Panel | 1.9 |

The CI run number supplies the release patch. BepInEx and Thunderstore use
the same three-number version; assembly/file metadata adds `.0`, and
diagnostics append the triggering commit hash.

## Validation status

Accepted runtime work includes:

- player-owned navigation and per-playthrough persistence;
- the ten-phase guide 1.22.2 critical-path objective inventory;
- native one-minute production comparisons against the Statistics Panel;
- native Dyson aggregate comparisons against the Dyson editor;
- focused ILS, DYSON, PHOTON, and WHITE conclusions;
- compact schema 2.2 snapshots and bounded collector diagnostics;
- panel layout, click-through behavior, scrolling, footer actions, and hover
  behavior;
- Basic Regular rendering with the retained outline and separate
  `DON'T PANIC` treatment;
- an extensive representative user test with no reported functional,
  navigation, persistence, layout, footer, or performance defects.

`docs/RUNTIME-TESTING.md` remains the regression protocol for future runtime
changes. Compilation alone never proves in-game presentation or conclusions.

## Release packaging

Hosted builds publish `DspGuideCheck.dll` in the Thunderstore-compatible ZIP
defined by `docs/THUNDERSTORE-PACKAGE.md`. Game and Unity assemblies are build
inputs only and are never redistributed.

## Historical records

- `docs/GUIDE-REVISION-INGESTION.md`: superseded guide v1.1 adoption record.
- `docs/GUIDE-1.22.2-GAP-ANALYSIS.md`: derivation and completed migration
  stories for the current critical path.
- `CHANGELOG.md`: release-by-release behavior, including retired optional
  route contracts.

Historical documents describe the release in which they were written; they
do not override this current project definition.
