# Guide 3.0 companion gap analysis

**Historical state:** This analysis was prepared on 2026-09-15. Its accepted
scope was implemented, accepted and published in 3.0.104. Subsequent decisions
were recorded in the [execution record](GUIDE3-EXECUTION.md).
**Decision authority:** [PROJECT.md](../../../PROJECT.md).
**Completed roadmap:** [ROADMAP.md](ROADMAP.md).

**GC3-13 B1 correction (2026-09-16):** The first correction failed owner retest.
The owner accepted the follow-up at roughly a 7 FPS cost and completed the
full playthrough on 2026-09-19 with no observable regressions. ILS was accepted
for this version. Expert mode's fixed-WHITE refinement did not block that test.
The workshop had found an unacceptable periodic slowdown. The original D3 copied prerequisite-table decision was
superseded by native `LDB.techs` data. Station observations were corrected to
preserve native slot ownership, receipt tracking was limited to selected Automation, and panel
analysis stopped exporting the full normalized diagnostic state. See the
[B1 execution record](GUIDE3-EXECUTION.md#gc3-13-b1--periodic-panel-slowdown)
for the changes, reasons, checks and owner disposition. The original
investigation below was retained as planning history.

## Purpose and evidence

Identify changes that improve a player's next decision, especially during ILS.
Guide authorship changes do not automatically become mod requirements. The
comparison covered the nine retained phases and their required receiver bridge;
optional routes and combat remained outside the mod contract.

The owner's subsequent Expert-mode request was a separate, bounded presentation
enhancement, not a discrepancy in the guide. D10 resolved its implementation
boundary for GC3-11 before candidate verification and the final workshop.

Sources inspected:

- The [production guide](https://dsp-beginner-guide.pages.dev/) was fetched on
  2026-09-15 and declared `guide-version=3.0`. Its nine critical-path sections
  and `receiver-antimatter-bridge` had the same extracted text as the local
  guide checkout at `c6d846b09f80564106ba221133a1c1d021267f2f`. Production HTML
  SHA-256: `692ba14b1f7e770841d228b969e782803cae638811272db33c1d7d75df87bbc1`.
- The guide's [v3.0 archive](https://github.com/shytamir/DSP_Guide/tree/c6d846b09f80564106ba221133a1c1d021267f2f/docs/archive/version-3.0),
  particularly NR-06, NR-07, NR-08, NR-12 and NR-13, explains chronology and
  ownership. Published content takes precedence over these historical stories.
- The guide's [technology reference](https://github.com/shytamir/DSP_Guide/blob/c6d846b09f80564106ba221133a1c1d021267f2f/assets/data/tech-reference.json)
  and [recipe evidence](https://github.com/shytamir/DSP_Guide/blob/c6d846b09f80564106ba221133a1c1d021267f2f/dsp_universal_end_product_dag_v1_0/dsp_universal_recipe_hyperedges_v1_0.csv)
  resolve prerequisite and material identities.
- Mod baseline: `d260eb1964bb05ba6a5cb39a940f89ff16867bef`;
  [gate engine](../../../../src/DspProgressionStatusExporter/GuideGateEngine.cs),
  [analysis](../../../../src/DspProgressionStatusExporter/GuideAnalyzer.cs),
  [panel model](../../../../src/DspProgressionStatusExporter/GuidePanelModel.cs),
  [selection](../../../../src/DspProgressionStatusExporter/ManualPhaseNavigation.cs),
  [normalized state](../../../../src/DspProgressionStatusExporter/ObservedGameState.cs),
  collectors, snapshot builder, regression scripts and release workflow.
- Read-only inspection of installed `Assembly-CSharp.dll`, SHA-256
  `ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85`:
  `StationComponent`, `AstroTrafficStat`, and `TrafficStat` confirm the native
  station and planet-traffic evidence described below. No game assembly or
  decompiled source was included in this planning change.

`build.cmd` passed with zero warnings/errors and all four existing suites.
The initial sandbox run failed to read the installed Microsoft SDK directory;
the same command succeeded with desktop access. Additional reflection fixtures
against that freshly built DLL reproduced the seven cases below. These were
deterministic reproductions, not new in-game observations.

| Case | Constructed evidence | Actual baseline result |
|---|---|---|
| R1 | Icarus was on planet 102; birth planet was 101; remote finished-material production exists; PLS research 1604 was complete | Stage III, although 1604 belongs to Stage II |
| R2 | 860 Titanium Ingots and 520 High-Purity Silicon remained in remote storage | Stage III before cargo was carried home |
| R3 | Move exactly that cargo into Icarus, leaving remote storage empty | Reverts to Stage II and reports both cargo amounts missing |
| R4 | Remote Titanium Ore and Silicon Ore production exists; finished-material production does not | Smelting objective was ready |
| R5 | Import/export traffic exists only on planets 201/202; no home stations exist on birth planet 101 | The arrive-home route objective was ready |
| R6 | Four sustained lensed receivers; 1 Photon/min and 1 Antimatter/min; 2,000 owned Antimatter; no colored-Cube evidence | PHOTON was complete |
| R7 | Three configured YELLOW Labs, recipe 27, produce 30/min; direct-input storage was empty | Lab objective was ready but the required storage objective blocks the phase |

Probe sources, extracted HTML and decompilation remained ignored under
`artifacts/guide3-planning/`. The table was the durable reproduction specification;
the implementation was required to encode the corrected outcomes in regression
tests without depending on those local artifacts.

## Gap disposition by phase

| Surface | Guide 3.0 versus the implemented contract | Disposition and player value |
|---|---|---|
| BLUE | Core factory/20-min science contract was retained. The checklist also gives roughly 15-20 MW as a planning reference. | Retain current objectives. A fixed generation gate would confuse capacity with actual adequacy; the approximate reference does not justify one. |
| RED | Two Labs/20-min remained the target; both refinery outputs should keep moving. New defense/research prose belongs to the guide. | Retain current science gate and existing congestion warning. Exact outlet attribution and combat tracking were excluded. Do not present the current aggregate warning as proof of each belt's condition. |
| ILS | Three chronological stages; aboard haulback; a finished transport package; actual delivery home. | Fix R1-R5, unsupported stage prerequisites, concurrent premature actions, and the raw-reserve completion claim: GC3-01 through GC3-06. |
| YELLOW | Three continuously supplied Labs were sufficient; research filler was explicitly discardable. | GC3-07 removes separate mandatory input-stock readiness and healthy-line buffer chores; retain shortage evidence. |
| PURPLE | Three continuously supplied Labs were sufficient. Input buffers remained useful operating advice. | GC3-07 makes the same bounded correction. No filler-upgrade tracker or additional 18/min hard target. |
| GREEN | Two continuous Labs plus visible Quantum Chip/Graviton Lens stores remained explicit. More research and power explanation does not change the outcome. | Retain the current gate, including its two storage checks. No Deuterium-route selector or fuel-system diagnosis. |
| DYSON and bridge | DYSON hands off through a required Receiver-to-Antimatter process, rather than ending at sail generation alone. | GC3-08 makes bridge work visible within DYSON and directs the guide link there when useful. No tenth selectable phase. |
| PHOTON | Five pre-WHITE Cube colors and Antimatter each sustain at least 40/min; 2,000 Antimatter was stored. 48/min was headroom. | GC3-09/10 replace the positive-rate completion loophole with this outcome and one first-shortage action. White Cubes were excluded. |
| WHITE | Universe Matrix, ten Labs/40-min, continuous supply, and Mission Completed remained the finish. | Retain the implemented contract and concise final-research treatment. No proof of exact belt connectivity or optional post-game panels. |
| Research upgrades, optional routes, guide widgets | Much v3.0 work improves explanations and discretionary methods. | No wholesale technology checklist, guide UI cloning, SPHERE/WARP/LOGISTICS controls, combat work, or generalized task scheduler. |

This was a bounded Guide 3.0 companion alignment. It does not promise that every
sentence in the guide has a corresponding runtime objective.

## ILS design decisions resolved for the draft

### D1. Player-owned stage selection

The baseline `EvaluateIls` reconstructed a stage on every evaluation from mutable
stock and technology proxies. A present inventory cannot prove when or where
the player carried a previous load. Persisting a guessed historical milestone
would preserve a wrong guess, and loading an earlier save could invalidate it.

The proposed control was one ILS-only row: **I Departure / II Haulback /
III Automation**. The selected stage was saved with the existing per-playthrough
phase preference. Runtime evidence evaluates that stage; it never changes it.
There were only two additional deliberate stage selections in the default trip.
Selecting a stage never completes its objectives, locks navigation, changes
the top-level phase, or modifies game state.

On first ILS entry without a stored stage, suggest once: III if technology
1605 was unlocked; otherwise II if a known non-birth player location or finished
remote ingot production was observed; otherwise I. Missing birth/location
evidence cannot prove an outpost. Queued technology, PLS 1604, Drive Engine
2903, raw ore and buffered remote cargo cannot seed III. Record the initial
suggestion's reason; users can immediately choose a different stage. This was
navigation assistance, not a claim that earlier work was performed.

`nav1`/`nav2` phase selections migrate without changing phase. `nav3` adds the
optional ILS stage; unknown/malformed stage values fall back to an uninitialized
stage, preserving valid phase data. Leaving ILS preserves its stage. Existing
save-identity behavior remained unchanged. No flight-event watcher, persisted
completion history or automatic stage transition was needed.

### D2. Finished materials, location and availability

The retained fields already separate `PlayerPlanetId`, `StarterPlanetId`,
`PlayerItemCounts`, `PlanetItemCounts` and per-factory production. Use finished
items 1106 and 1105 only; ore 1004/1003 does not prove smelting or completed
delivery. The starter planet must be positively known before excluding it.

For Stage II, select a known non-birth current planet first; otherwise select
an outpost with finished-material evidence, sorted by planet ID for ties.
Report the selected planet. The prescribed one-outpost/two-material model was
retained; no new multi-outpost mission planner was introduced.

Keep these facts separate:

| Fact | Permitted evidence |
|---|---|
| Remote smelting | Positive native one-minute production of both finished items on the selected non-birth planet |
| Cargo aboard | Icarus package counts, separately against 860/520 |
| Cargo secured at home | Known player-at-birth location; sum Icarus package plus birth-planet stationary stock, without double counting |
| Outpost stock | Selected planet's stationary stock, described as available for loading rather than aboard |
| Intent, fuel margin, free construction space | Explicit player judgment; no invented numerical checklist or automatic completion |

Unavailable location, research, inventory, production or station collections
had to remain distinguishable from valid zero counts. Add only the availability
flags needed by these changed consumers at the existing collection/normalization
boundary. The current default-zero conversion does not establish availability.

### D3. Research chronology and prerequisites

Use a small ILS-specific table, derived from the linked guide data, rather
than runtime graph discovery. The following IDs were technology IDs, not item
IDs. Explicit and implicit prerequisites both matter. The listed branches were
not one artificial linear chain.

| Stage | Bounded technology scope |
|---|---|
| I | Required departure endpoints: Drive Engine Lv2 2902 and Titanium Smelting 1413. Relevant drive prerequisites: 2901, Mecha Core Lv1 2101/Lv2 2102; smelting prerequisite 1411. Cosmic Exploration 4101/4102 and Engine 1805 were guide preparation advice, not additional departure gates. Engine items were a direct upgrade cost, not an invented technology prerequisite edge. |
| II | 1121 -> 1131; 1311 -> 1302; 1122 -> 1123 -> 1124; 1701 -> 1702 -> 1703; 1112 -> 1113 -> 1114; 1602 -> 1603; 3701; then 1604. Retain their explicit/implicit prerequisites from the evidence table. |
| III | 1414 -> 1605 after the first Yellow batch; catch up unmet real prerequisites, including 1604 and 1114. 2903 was not a prerequisite of 1605 and must leave the required chain. |

In particular, 1605 required 1604 and 1414, with implicit 1114; 1604 required
1603, with implicit 1113 and 3701; 1603 also required 1702 implicitly. Missing
inherited prerequisites were surfaced before their dependents. A queued target
was waiting, not a request to queue it again. Completed nodes disappear from
suggestions. Optional survey work and filler upgrades never block a stage.

Research collection already exposes unlocked and queued IDs plus progress;
no queue mutation was needed. Use explicit rank labels in the bounded table
where native prototype names omit them. Keep stage-owned research advice
distinct from hard cargo, hardware and route objectives.
The first 200 Yellow Cubes were a guide batch reference, not a perpetually
required inventory count. Once its research starts, advice supplies remaining
research rather than demanding that the original 200 be stocked again; no
new partial-research cost calculator was introduced.

### D4. Hardware as a separate checkpoint

`MissingIlsReserve` compares simultaneous stocks of intermediate components;
`fleetReady` counts stations/vessels globally. Neither proves that a protected
box contains the finished package. Consuming a component to build an ILS also
makes a raw-stock checklist regress.

Replace the required raw-reserve objective with **two ILS towers and five
Logistics Vessels**, item IDs 2104/5002. Count undeployed hardware accessible
at home (birth-planet stationary stock plus Icarus only while at home) and
hardware already assigned to the candidate home/source endpoints. Candidate
endpoints use the policy in D5; count each physical station/vessel once.
An undeployed tower was not a deployed endpoint; hardware elsewhere was not
described as locally available. A partly deployed but unconfigured source was
reported as deployment work until it satisfies endpoint policy.

The expanded component bill remained linked guide advice. Do not reconstruct
all nested recipe costs, track the handcrafting queue, identify a player's
protected box, or demand spent intermediate parts again. Before technology
1605 was unlocked, research/first-batch advice precedes assembly. After hardware
exists, configuration and delivery advice takes precedence.

### D5. Home delivery and evidence limits

`HasImportAndExport` then accepted matching item traffic anywhere. The
one-sided fallback also did not require a birth-planet destination. Native
`StationComponent` records planet input when a Vessel adds its cargo to the
receiving station; drone transfers use the separate internal-traffic register.
Thus planet-matched input was useful delivery evidence, although it does not
identify the exact sending station.

The bounded route proof was:

1. A positively identified birth planet with a stellar demand station for
   both 1106 and 1105, and at least five idle-plus-working Vessels assigned to
   that receiver. Other planets' fleets do not count.
2. A non-birth stellar supply station for both finished items, with matching
   finished-material stock or source production. The source need not be powered.
3. Positive native input for each finished item **on the birth planet** after
   the matching configuration was observed. Ore and internal traffic do not count.

For partial-progress reporting, select a birth-planet stellar receiver candidate
by most matching Demand slots, then station ID; select a non-birth source by
most matching Supply slots, then planet/station ID. An unconfigured source was
eligible only on the selected finished-material outpost from D2. Candidate
selection did not require an already complete fleet. Prefer the current pair
while it remained equally suitable; expose missing policies/fleet as conditions.
Only a pair satisfying all three proof steps qualifies as observed home supply.
Do not claim exact Vessel attribution from planet aggregates. Label the result
"home imports observed" with policy corroboration, not a reconstructed route.

Maintain only session-local receipt flags for the selected home/item/configuration
identity. Clear them on game-data replacement, lost/changed endpoint policy,
counter reset or unavailable required evidence. Quiet intervals after confirmed
receipt do not erase evidence; after reload, show "awaiting delivery evidence"
until another receipt. This was a current-configuration observation, not persistent
mission history. No active Vessel polling, Harmony patch, station write or
full transport graph was required.

### D6. Pending means useful work now

`GuidePanelModelBuilder.AddObjectives` then copied every unfinished
objective's action into Pending; only identical wording was deduplicated.
Consequently research, raw-component collection and route activation can all
appear before an ILS can be built.

Use bounded ILS action candidates with stable IDs, kind, eligibility and
priority. Evaluate prerequisites in analysis; the panel only projects them.
Priority was an available research/material prerequisite, the selected stage's
physical task, then deployment/verification. Display at most three eligible,
distinct tasks. A queued technology, warming sample or configured route awaiting
receipt contributes status text, not an instruction to repeat a completed step.
Do not hide unmet objectives merely because their next action was not yet useful.

## Other Guide 3.0 planning decisions

### D7. Readiness at YELLOW/PURPLE

The live guide explicitly allows departure once three Labs run continuously.
Make separate terminal-input stock objectives advisory or omit them when
healthy; do not retain mandatory buffer Pending tasks. Preserve input
telemetry and actionable draining/starved risks. GREEN still required its
two endpoint stores. No new research or exact-rate gates were warranted here.

### D8. Required receiver bridge inside DYSON

Keep nine phases and stable DYSON objectives. Add the bridge's receiver and
conversion evidence to DYSON; direct `DON'T PANIC` to the bridge after the
swarm objectives were ready. Prerequisite advice was 1504 -> 1505 -> 1506.
Reuse the four-receiver lens/warmup/continuity policy. Recipe 74 was Photon
Materialization: it consumes item 1208 and produces 1122 plus 1120.

Observed conversion readiness required a configured recipe-74 machine,
positive native Critical Photon production/consumption and Antimatter
production, and positive stationary Antimatter stock. Label those facts by
their actual cluster scope. They cannot prove that the same belt network
connects every producer and consumer or that returned Hydrogen has a safe outlet.
Retain one explicit required `player-check` row for the Hydrogen outlet and
automatic delivery to the science district. As with the existing BLUE manual
check, unobserved player judgment remained evidence-incomplete; phase navigation
stays unrestricted. Do not silently mark the physical handoff complete.

**Superseded at owner workshop (2026-09-19):** The manual Hydrogen/delivery
row was removed, including its Pending reminder. It adds lengthy, untracked work
that other science phases leave to the guide. DYSON was reduced to five objectives;
its receiver label explicitly names Photon Generation so power-mode receivers
cannot be mistaken for missing telemetry. Collection and continuity stay unchanged.

No fixed 1.655-GW gate: that reference assumes Ray Transmission Efficiency
Lv0. Existing live demand/generation evidence remained the relevant warning.

### D9. PHOTON outcome and sustained-rate interpretation

The six inputs were **6001-6005 and 1122**: five colored Cubes plus Antimatter.
They were not six Cube colors. WHITE's 6006 was excluded. Each required at least
40/min; 48/min remained Antimatter headroom. The second objective was 2,000
Antimatter in stationary storage, excluding Icarus inventory.

The current continuity fraction counts positive rates, not rates at 40/min.
For these six items only, derive a bounded readiness window from the existing
five-second samples of native one-minute aggregates: at least 120 game seconds
of evidence, at least 20 distinct samples, every sampled aggregate at least
40/min, and the latest value available and at least 40/min. Keep the boundary
sample needed to span 120 seconds; allow up to 26 sample points. A below-target
sample must age out before recovery; unavailable samples break readiness.
Game ticks prevent paused wall-clock time from filling the window.

This two-minute operational interpretation was a proposed mod policy, not a
number quoted from the guide and not a claim of per-second belt continuity.
Export elapsed game time, count, minimum rate and readiness reason. Do not use
inventory deltas, lifetime totals, or the ten-minute production-risk warmup to
decide this separate objective.

Show one combined six-input objective, compact deficient input evidence and
one first-shortage Pending action in BLUE -> RED -> YELLOW -> PURPLE -> GREEN
-> Antimatter order. Existing Current Status risk rows retain their separate
purpose. The four-receiver construction gate belongs to the bridge; PHOTON
retains receiver/power diagnostics without making efficiency ranks or a
particular construction count additional completion requirements.
`GuidePanelModelBuilder.CubeLevel` then used fixed early-phase bands, so
RED and GREEN can look comfortable at 20/min. While PHOTON was selected, use
40/min for the five displayed Cube-rate bands: below-target below 40,
comfortable at/above 40, unknown for unavailable item evidence. These colors
describe the latest native rate; the objective separately reports sustained
history. Other phases retain their current bands and icon layout.

## Owner-requested presentation option

### D10. Expert mode keeps only the Cube-rate bar and guide button

**Owner workshop refinement:** Expert mode fixes its effective phase to WHITE
so all six counters remained visible. The guide link also opens WHITE. The normal
phase/stage preference was retained without seeding or overwriting it in Expert
mode. This superseded the original selected-phase policy below.

`Plugin.Awake` already binds settings through BepInEx `Config.Bind`.
`GuidePanelController.EnsureCreated` then constructed the entire panel;
the Cube-rate bar and `DON'T PANIC` were children of that same root. The existing
collapse path retains the header, navigation and risk glyph, so it cannot
satisfy this requirement. `Apply` and `Layout` also directly access body/header
objects. Simply hiding the root would remove the requested bar and button.

Bind the boolean `[General] ExpertMode` with default false and capture it at
startup before controller creation. Apply configuration changes on game restart;
do not add a live watcher. Add a narrow mode branch within the existing
controller: create the shared overlay root, Cube views and guide button, skip
all other presentation objects, then update/layout only those retained objects.
Guard normal-only refresh, interaction and teardown paths against absent views.
The Expert root has no adjoining background/edge or invisible input surface;
its layout bounds fit the bar/button using the existing screen anchor. No
separate controller framework or telemetry fork was needed.

"Cube counter bar" means the existing production-rate display, with its existing
phase-dependent Cube set, values and colors (including GC3-10). The model's source
anchor remained authoritative. Omit the production-risk glyph as well as the
entire adjoining panel, header, collapse/navigation/ILS-stage/scroll controls
and diagnostic snapshot footer: only the two requested surfaces remained.

F8 continues to toggle an initially hidden overlay and never exports. The
normal selection persistence and first-selection seed still apply; Expert mode
does not infer or advance phases because navigation was unavailable. Returning
to normal mode after a restart gives access to the retained phase/ILS stage.
Collection/analysis continue unchanged, so this was not a promised performance
optimization. No snapshot contract change was justified by this display option.

Both builds required automated default/mode, presentation-policy, rate/anchor
parity and callback-eligibility coverage using synthetic inputs. Such tests do
not prove Unity visibility or pointer pass-through; those concrete checks and
1080p/4K layout were included in the final owner workshop, with no new earlier
human validation or investigation gate.

## Boundaries and evidence limits recorded during planning

- Baseline behavior was reproduced in fixtures; no new DSP playthrough,
  screenshot, performance measurement or user-utility acceptance was performed.
  Those observations were deliberately reserved at the planning stage for the roadmap's final owner
  workshop. Earlier gates made narrower automated claims.
- Planet traffic proves the destination's receipt, not exact source-station
  attribution. Current stock proves availability, not ownership of a protected
  box or historic hauling. The proposed wording and tests preserved these limits.
- Belt connectivity, a clear Hydrogen outlet, safe fuel margins and outpost
  construction space remained player judgments. Building a general topology or
  intent detector was excluded rather than left as a future investigation story.
- Existing configured-Lab counts plus native aggregate production do not prove
  simultaneous uptime of every individual Lab. The unchanged early-phase checks
  retain that bounded interpretation; per-machine uptime instrumentation was not
  part of the proposed fixes. The ILS mission also retains the guide's prescribed
  single outpost producing both finished materials, rather than adding support
  for arbitrary multi-outpost mission allocation.
- The draft made stage selection explicit. Its interaction cost and the
  chosen two-minute PHOTON window required owner acceptance of the roadmap and
  final workshop evaluation; their implementation semantics were fully specified.
- No known implementation question was deferred to an investigation gate.
  Unexpected future evidence can justify a bounded amendment; it cannot be
  silently replaced with guessed telemetry or treated as a passed check.
