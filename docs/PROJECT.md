# DSP Guide Check - Project Definition

## Product state

DSP Guide Check **3.0.104** implements the DSP Practical Progression Guide
**3.0** default critical path. The owner confirmed successful publication on
2026-09-19. The repository is in **maintenance mode**, with no active epic,
story or acceptance gate.

[ROADMAP.md](ROADMAP.md) is the maintenance placeholder. The completed roadmap,
implementation decisions and acceptance evidence were
[archived together](archive/project-management/guide-3.0/ROADMAP.md).

The published package came from source `ea3314b7ec33aef5073127675aa4f5c4a60abe70`,
[CI run 104](https://github.com/shytamir/dsp-beginner-guide/actions/runs/35415921325).
Its version is `3.0.104`; assembly/file metadata is `3.0.104.0`, and the
diagnostic label is `3.0.104.ea3314b`. Later CI builds do not change that
published baseline by themselves.

Maintenance intake is limited to:

- a meaningful change to the published guide;
- a reproducible defect or compatibility regression;
- an accepted feature request within the existing product scope;
- required maintenance of the game, BepInEx, build or package contract.

Historical evidence and limitations are retained in
[the archive](archive/README.md); they do not create active obligations or
override this document.

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

- Implemented guide authority: DSP Practical Progression Guide **3.0**.
- Published mod: **3.0.104**, covering the default critical path and Expert mode.
- Optional guide routes remain outside the mod's scope.
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
| ILS | The player selects Departure, Haulback or Automation. Stable objectives evaluate only that stage. Haulback checks finished outpost smelting, aboard cargo and cargo secured at home separately. Research follows stage-owned prerequisites without repeated queue/batch tasks. Automation counts two finished ILS towers/five Vessels and checks deployment separately. Home delivery requires configured endpoints, source materials and session-observed finished inputs at home. Pending projects at most three eligible, distinct stage-owned actions in prerequisite order. |
| YELLOW | Three configured Yellow-Cube Labs produce; separate input storage is not required. |
| PURPLE | Three configured Purple-Cube Labs produce; separate input storage is not required. |
| GREEN | Two configured Green-Cube Labs produce continuously; Quantum Chips and Graviton Lenses each have visible storage. |
| DYSON | Five objectives: Solar Sail production/launching, swarm power, receiver research, four sustained lensed Ray Receivers in Photon Generation, and observed Photon Materialization with stationary Antimatter. Hydrogen disposal and automatic science delivery remain guide advice, not panel objectives. |
| PHOTON | Five colored Cubes plus Antimatter sustain at least 40/min for 120 game seconds and 20 samples; 2,000 stationary Antimatter is required. Receivers remain diagnostic, with their construction gate in DYSON. |
| WHITE | White Cubes are researched; ten Labs sustain 40/min with the configured-Lab and stored-White-Cube counts shown; Mission Completed state and authoritative active progress are shown. |

Supporting production and soft reference paces appear only when they explain
a real, actionable shortfall. Healthy supporting chains do not create
completed clutter. Mission Completed changes WHITE to `Mission Accomplished!`
without navigating elsewhere.

For every Cube line, demand-driven production risk compares current production
with the lesser of actual consumption and the 40/min guide reference, using
the analyzer's established tolerance. Actual production and consumption remain
diagnostic, and actual net depletion still determines buffer runway when the
capped comparison identifies a deficit. BLUE, RED, and WHITE retain their
exact 20/min, 20/min, and 40/min phase goals; meeting an exact goal suppresses
demand-driven risk independently. Non-Cube demand remains uncapped.

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
- Receiver continuity uses a ready 60-second per-device history. Up to two
  unhealthy samples are tolerated; diagnostics retain the observed and allowed
  counts, and current configuration and lens checks remain immediate.
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

ILS has a separate I Departure / II Haulback / III Automation control inside
the collapsible body. `nav3` retains the selected stage and its origin across
phase changes and reloads. First ILS entry suggests III only for researched
1605, otherwise II for known remote location or finished remote production,
otherwise I. Runtime evidence never changes an initialized stage. Invalid
stage data preserves the phase and is initialized once on ILS entry.

ILS research reads the target technologies and their explicit/implicit prerequisites
from native `LDB.techs` once, retaining native prerequisite arrays and upgrade ranks.
Only the stage targets and advice policy belong to the mod. Departure requires
2902/1413; survey research remains optional. Haulback offers its support branches
without turning them into cargo gates. Automation follows 1414/1605 and catches
up their prerequisites; 2903 is excluded. Queued research produces waiting
status. The 200-Yellow-Cube batch is a reference before its research starts,
never a required stock counter. Missing research/queue evidence stays unknown.

Automation requires two finished ILS towers and five Logistics Vessels, counted
from birth-planet stock, Icarus while at home and deployed candidate endpoints.
Raw components cannot pass this objective. Deployment separately requires home
Remote Demand for both finished materials, source Remote Supply and five Vessels
assigned at home. Candidate selection prefers matching slots, then planet/station
ID; an unconfigured source is eligible only on the identified outpost. The old
protected-reserve claim is removed. Receipt flags use birth-planet cumulative input counters after observing matching endpoint policies and the home fleet. They survive quiet intervals and reset on reload, changed/lost configuration, counters or missing evidence. Planet totals corroborate delivery without attributing an exact sending station.

Station observations retain the native station-to-slot relationship; endpoint
checks inspect that station's own slots rather than searching every station's slots.
Receipt observation runs only while ILS Automation is selected. Leaving it clears
the receipt baseline; returning requires newly observed inputs. Panel analysis
does not materialize the unused full normalized-state diagnostic export.

## Snapshot contract

Snapshot schema 2.24 serializes the selected-phase conclusions used by the
panel plus only the evidence needed to audit implemented functions. It
includes provenance, playtime, research and Cube aggregates, selection
diagnostics including selected ILS stage/origin, Mission Completed progress when available, focused evidence,
resolved presentation-source settings, ordered production risks, collector
coverage, performance, and explicit omission or truncation markers.

Broad factory, technology, station-slot, inventory, topology, and all-item
dumps are excluded. Receiver detail is capped, and exports above 256 KiB are
rejected rather than written.

## Panel contract

`[General] ExpertMode` defaults to false and is read once at startup. True creates
only the existing Cube-rate bar and guide button under a non-intercepting root.
No adjoining panel, header/body, navigation, collapse, stage selector, risk glyph
or snapshot control is created. Expert mode fixes its effective phase to WHITE,
showing all six Cube counters with WHITE rate/color policy and the WHITE guide
anchor. This override is not persisted: normal mode retains its saved phase and
ILS stage. F8 remains on-demand; switching modes requires a restart.


The panel is click-through except for phase/ILS-stage navigation, collapse, explicit
scrolling, `DON'T PANIC`, and the diagnostic build's `Save snapshot` control.
The public Thunderstore build omits snapshot control and its interaction path
at compile time.

`DON'T PANIC` opens the selected phase in the published guide. For ILS, the
presentation model maps the selected Departure, Haulback or Automation stage
directly to `#flight`, `#titanium` or `#ils-automate`. The link keeps that target
when collapsed; clicking it never changes selection.

Panel text and Cube rates reuse the installed game's live vein-label Text
style at `UIRoot.instance.uiGame.veinDetail.nodePrefab.infoText`: font,
material, font style, line spacing, and attached Shadow or Outline mesh
effects. If that serialized Text is not ready, one bounded lookup uses a
loaded `UIVeinDetailNode.infoText`. Lookup occurs only when the panel is
created; missing or renamed resources fail softly to embedded Basic Regular
and one concise warning. Published-guide icons,
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

YELLOW and PURPLE have no separate storage objective. Three configured Labs
and positive available native production satisfy their existing Lab policy.
Direct inputs and Cubes remain eligible for actual draining/starved findings.
Missing recipe/item observations remain unknown. GREEN retains its two input
stores; internal branch progression is not modeled.

PHOTON input sampling retains at most 26 points per item for five colored
Cubes and Antimatter. Readiness requires 120 game seconds, 20 distinct samples
and every sampled native one-minute aggregate at least 40/min. Missing samples
break the item history; paused time does not fill it. PHOTON consumes this policy
in one combined input objective and a separate stationary-stock objective. This does not prove per-second throughput.

## Contracts

| Contract | Version |
|---|---:|
| Published mod | 3.0.104 |
| Snapshot schema | 2.24 |
| Normalized state | 2.8 |
| Guide selection | 1.7 |
| Guide analysis | 3.13 |
| Progression | 3.10 |
| Panel | 2.10 |

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
