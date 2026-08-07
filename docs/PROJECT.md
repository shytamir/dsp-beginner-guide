# DSP Guide Check - Project Definition

## Product state

DSP Guide Check is complete for the adopted DSP Practical Progression Guide
2.0 contract and is in maintenance mode.

An accepted, bounded maintenance cycle is active following a full critical-path
playthrough. Its ordered scope and story gates are recorded in the
[`Maintenance roadmap`](MAINTENANCE-ROADMAP.md). Work outside that list still
resumes only for:

- a meaningful change to the published guide;
- a reproducible defect or compatibility regression;
- an accepted feature request within the existing product scope;
- required maintenance of the game, BepInEx, build, or package contract.

Completed stories, migrations, roadmaps, and validation gates are historical
records under [`docs/archive/`](archive/README.md). They do not represent open
work or override this document.

The active maintenance roadmap does not reopen the guide 2.0 product design or
make temporary playthrough observations authoritative. Guide-authoring work is
tracked separately and is not part of the mod roadmap.

## Purpose and scope

DSP Guide Check is the passive, on-demand runtime companion to the
[DSP Practical Progression Guide](https://dsp-beginner-guide.pages.dev/).
F8 recalls the phase selected for the current playthrough, evaluates that
phase's stable readiness objectives, and presents a bounded set of concise
status conclusions and immediate actions.

The player asks; the instrument answers. It is not an autopilot, factory
score, build designer, combat adviser, optional-route tracker, or unsolicited
warning system.

## Product invariants

- The player owns phase selection.
- Runtime evidence evaluates the selected phase but never changes it.
- Objectives remain stable while a phase is selected.
- Hard objectives, reference paces, warnings, and player judgments remain
  distinct.
- Current Status shows at most three actionable conclusions; recommendations
  remain separate in Next Actions.
- The panel is hidden by default and never alerts by itself.
- F8 never saves; `Save snapshot` is the deliberate diagnostic export.
- The mod is read-only with respect to game and save state.
- Collection, normalization, analysis, panel modeling, and UI remain separate.
- Missing or renamed runtime evidence fails softly.
- Combat and optional guide routes remain outside scope.

## Guide authority

- Adopted authority: public guide 2.0 edition.
- Published `guide-version` metadata: `1.23.0`.
- Current implementation: the nine-phase default critical path.

```text
BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
     -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are checkpoints inside ILS. WARP, SPHERE, LOGISTICS,
COMPLETE, and other optional or post-completion routes have no navigation
control, panel, objective contract, finding, or snapshot phase contract.

The phase-local `Ready to move on when` checklist is authoritative. The ILS
mission additionally uses its manifest, `Before flying home`, and `Done when`
checkpoints. A numerical pace becomes a hard objective only when the local
readiness text states it.

| Phase | Stable readiness contract |
|---|---|
| BLUE | Starter inputs and routine hardware replenish; Blue Cubes run continuously at 20/min or better; research is not hand-fed. |
| RED | Two Labs sustain 20 Red Cubes/min while Refined Oil retains a continuing outlet. |
| ILS | Show one active checkpoint at a time: preparation before launch; same-planet Titanium and Silicon production plus return cargo during the expedition; then research, protected components, and active Titanium and Silicon ILS routes during the rush. |
| YELLOW | Three configured Yellow-Cube Labs produce continuously. |
| PURPLE | Three configured Purple-Cube Labs produce continuously. |
| GREEN | Two configured Green-Cube Labs produce continuously; Quantum Chips and Graviton Lenses each have visible storage. |
| DYSON | The Photon swarm produces and launches Solar Sails and generates useful power. |
| PHOTON | Critical Photon and Antimatter production runs; actual rates appear against the 48/min receiver-array reference; 2,000 stored Antimatter marks the midpoint. |
| WHITE | White Cubes are researched; ten Labs sustain 40/min with the configured-Lab and stored-White-Cube counts shown; Mission Completed state and authoritative active progress are shown. |

Supporting production and soft reference paces appear only when they explain
a real, actionable shortfall. Healthy supporting chains do not create
completed clutter. Mission Completed changes WHITE to `Mission Accomplished!`
without navigating elsewhere.

## Architecture

```text
Live DSP runtime
    |
Collectors and rolling samplers
    |
Normalized ObservedGameState
    |-- compact diagnostic JSON snapshot
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

## Evidence contract

- Production rates come from DSP's pre-aggregated one-minute Statistics Panel
  values for a bounded watch set, not inventory deltas or lifetime-rate math.
- Scope-matched ten-minute values support production-risk interpretation only
  after their current-session history is ready.
- Lifetime counters remain separate and are used only for aggregate Cube
  totals.
- Dyson generation, sail population, structure progress, and cell progress
  come from native Dyson system and editor aggregates.
- Ejectors, silos, and Ray Receivers retain dedicated component-pool
  collectors for facts absent from those aggregates.
- ILS station configuration, vessel deployment, traffic, research, inventory,
  and power evidence are collected only for retained consumers.
- Unavailable expected evidence remains unknown; another proxy is not silently
  substituted.

See [Native telemetry reference](NATIVE-TELEMETRY-ALIGNMENT.md) for exact
runtime sources.

## Selection and persistence

The latest researched Cube seeds a phase only when the playthrough has no
valid stored selection. Thereafter Previous and Next are the sole phase-change
authority. Selection is keyed by playthrough creation time and a stable galaxy
descriptor so autosaves, renamed slots, pauses, and restarts retain it.

Legacy selections normalize once: BOOTSTRAP to BLUE; FLIGHT and TITANIUM to
ILS; SPHERE to DYSON; WARP to GREEN; LOGISTICS and COMPLETE to WHITE.

## Snapshot contract

Snapshot schema 2.11 serializes the selected-phase conclusions used by the
panel plus only the evidence needed to audit implemented functions. It
includes provenance, playtime, research and Cube aggregates, selection
diagnostics, Mission Completed progress when available, focused evidence,
resolved presentation-source settings, ordered production risks, collector
coverage, performance, and explicit omission or truncation markers.

Broad factory, technology, station-slot, inventory, topology, and all-item
dumps are excluded. Receiver detail is capped, and exports above 256 KiB are
rejected rather than written.

## Panel contract

The panel is click-through except for phase navigation, collapse, explicit
scrolling, `DON'T PANIC`, and the diagnostic build's `Save snapshot` control.
The public Thunderstore build omits snapshot control and its interaction path
at compile time.

Panel text and Cube rates reuse the installed game's live vein-label Text
style at `UIRoot.instance.uiGame.veinDetail.nodePrefab.infoText`: font,
material, font style, line spacing, and Outline settings. The lookup occurs
once when the panel is created; missing or renamed resources fail softly to
embedded Basic Regular and one concise warning. Published-guide icons,
bracketed phase tags, and colors identify all nine phases. The fixed Cube-rate
rail uses six cached Matrix icons with outlined threshold-colored rate text
and a text-only soft fallback.

A native draining or starved glyph appears beside that rail for the strongest
displayed risk; quiet states show no glyph. `DON'T PANIC` retains its bright-red
Comic Sans treatment directly below the last visible Cube, right-aligned with
the Cube square. The entire rail, risk glyph, and guide control remain visible
when the panel body is collapsed.

Current Status presents at most three stable risk rows containing only the
item name plus `draining` or `starved`. Paired recommendations appear in Next
Actions. Initial ordering is severity, trustworthy net-depletion time, then
phase item order. Incumbents retain membership and same-severity order while
actionable; only a newly starved risk may displace a displayed draining risk.
A tracked objective may show one short buffer estimate when authoritative
local stock and net-deficit rates support it.

WHITE uses concise White-Cube language. Its production evidence contains the
configured-Lab and stored-Cube counts without repeating the rate rail, and its
single Pending action is to complete Mission Completed research. The objective
shows queued, authoritative active-progress, or complete state when available.

## Contracts

| Contract | Version |
|---|---:|
| Release line | 2.0.x |
| Snapshot schema | 2.11 |
| Normalized state | 2.1 |
| Guide selection | 1.6 |
| Guide analysis | 3.0 |
| Progression | 2.9 |
| Panel | 2.7 |

The CI run number supplies the release patch. BepInEx and Thunderstore use the
same three-number version; assembly and file metadata add `.0`, and diagnostic
metadata includes the triggering commit hash.

## Release and maintenance

Hosted builds produce separately verified diagnostic and public DLLs. Only
the public DLL enters the Thunderstore-compatible ZIP. Game and Unity
assemblies are build inputs only and are never redistributed. See the
[Thunderstore package contract](THUNDERSTORE-PACKAGE.md).

The maintained regression procedure is [RUNTIME-TESTING.md](RUNTIME-TESTING.md).
Compilation alone never proves in-game presentation or runtime conclusions.

When maintenance work is accepted:

1. preserve the product invariants and layer boundaries above;
2. update only the contracts affected by the change;
3. run deterministic checks and both applicable build variants;
4. require a focused DSP checkpoint for runtime or presentation behavior;
5. archive completed story and gate records instead of accumulating them here.
