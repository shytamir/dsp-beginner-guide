# Guide 3.0 companion improvements roadmap

**Status:** Accepted for implementation — GC3-01 through GC3-07 technically complete; G1/G2 passed; GC3-08 next.
**Prepared:** 2026-09-15.
**Implementation:** Owner authorized GC3-01 through GC3-12 on 2026-09-15, with a push after each story. GC3-13 remains the human validation stop.
**Execution record:** [Story decisions and validation](management/GUIDE3-EXECUTION.md).
**Authority:** [PROJECT.md](PROJECT.md).
**Evidence and resolved designs:** [Guide 3.0 gap analysis](management/GUIDE3-GAP-ANALYSIS.md).

## Outcome and scope

Help players follow the first interplanetary mission without contradictory
stage changes or premature tasks, and make the remaining route's completion
signals useful against the published Guide 3.0.

The delivered result has player-owned ILS stages; truthful cargo, hardware
and home-delivery checks; prerequisite-aware Pending tasks; non-blocking
YELLOW/PURPLE buffer advice; a visible required receiver bridge; PHOTON
readiness based on five colored Cubes plus Antimatter at 40/min; and an opt-in
Expert mode showing only the existing Cube-rate bar and `DON'T PANIC` button.

This is the sole active roadmap. At the inspected mod baseline there was no
standalone placeholder roadmap file: the no-active-work state lived in
`PROJECT.md`. This document replaces that planning state. Archived roadmaps
remain historical and unchanged; the separate guide repository is a read-only
source, not a target of this work.

### Preserved boundaries

- Nine top-level phases; player-owned phase selection; no factory/save writes.
- On-demand panel, F8 never exports, no unsolicited alerts.
- Stable objectives within the selected phase and, for ILS, selected stage.
- Existing passive observation, forgiving reflection and native Statistics
  Panel rate sources; no compile-time game-assembly dependency.
- Separate hard objectives, soft references, production risks and player checks.
- Public/diagnostic build separation and compact bounded snapshots.
- No combat, optional-route panels, general prerequisite/task framework,
  all-factory topology analysis, recipe expansion engine, release-pipeline
  overhaul, or unrelated UI cleanup.

## Acceptance and execution rules

The owner accepted the roadmap for implementation on 2026-09-15. This authorizes
the technical stories through GC3-12, not runtime acceptance or publication.
Each completed story is recorded and pushed to main before the next begins.

Within execution, **GC3-13 is the only human validation story**. The owner's
2026-09-15 direction defers the repository's normal per-change DSP screenshots
and user-run checkpoints to that final workshop. No earlier story requires
owner sign-off, a supplied save, an interactive game session, or manual timing.
Gameplay instructions that ask the player to check a belt or choose a stage
are product behavior, not development approval gates.

Earlier gates are agent-run technical gates. A story may become technically
complete after its automated definition of done passes; that status never
means owner accepted, runtime validated or published. Each milestone reports
what was tested and retains the final workshop reservation.

Every implementation story includes its focused fixtures, applicable contract
updates and documentation in its scope. Do not defer broken intermediate
contracts to the release story. Tests must exercise behavior and boundary
cases, including missing evidence, rather than merely compare source strings.

Unexpected implementation failures are repaired within the affected story.
Unknown facts must be recorded accurately; no investigation epic or gate is
reserved in this plan. A material change to the agreed product outcome requires
a roadmap amendment rather than silent scope expansion.

## Epics, phases, gates and milestones

Epics describe outcomes containing several independently verifiable stories.
Execution phases order the work; gates are checks, not additional stories.
Milestones record the outcome after a gate passes.

| Execution phase | Epic and value | Stories | Exit gate | Milestone |
|---|---|---|---|---|
| A — Understand the selected ILS stage | E1: Stable ILS context and accurate preparation/haulback | GC3-01, GC3-02, GC3-03 | G1 | M1: ILS stage, cargo and research guidance are technically coherent |
| B — Finish ILS with useful actions | E2: A real transport package and corroborated home delivery | GC3-04, GC3-05, GC3-06 | G2 | M2: Complete ILS fixture journey with actionable Pending tasks |
| C — Repair consequential Guide 3.0 handoffs | E3: Trustworthy mid/late-route readiness | GC3-07, GC3-08, GC3-09, GC3-10 | G3 | M3: Selected Guide 3.0 outcomes pass automated regression |
| D — Prepare and accept the candidate | E4: A configurable, reproducible candidate for owner acceptance | GC3-11, GC3-12, then GC3-13 | G4, then G5 | M4: Normal/Expert candidate ready for workshop; M5: Owner decision recorded |

### Dependency schedule

Default serial order is GC3-01 through GC3-13. The table identifies actual
dependencies; it does not authorize parallel agents or concurrent edits.

| Story | Prerequisites |
|---|---|
| GC3-01 | Accepted roadmap and implementation authorization |
| GC3-02 | GC3-01 |
| GC3-03 | GC3-01; reuse GC3-02 availability handling |
| GC3-04 | G1 |
| GC3-05 | GC3-04 |
| GC3-06 | GC3-02, GC3-03, GC3-04, GC3-05 |
| GC3-07 | G2; otherwise functionally independent of the ILS work |
| GC3-08 | G2 |
| GC3-09 | G2; otherwise functionally independent of GC3-07/08 |
| GC3-10 | GC3-08 and GC3-09 |
| GC3-11 | G3; reuse GC3-01 selection/anchor and GC3-10 Cube-rate rules |
| GC3-12 | GC3-11 |
| GC3-13 | G4 |

## E1 — Stable ILS context and accurate preparation/haulback

### GC3-01 — Select and retain the ILS stage

**Value:** The player can keep the panel on the work they are doing while
cargo moves, research finishes or the game reloads.

**Scope:** Implement gap-analysis D1 in `ManualPhaseNavigation`, `Plugin`,
`GuideGateEngine`, `GuidePanelModel` and `GuidePanelController`. Add the compact
ILS-only I/II/III selector below phase navigation; persist an optional stage
in `nav3`. Initialize it once using the documented seed policy. Pass the
selected stage into analysis explicitly, and map it directly to `#flight`,
`#titanium` or `#ils-automate`. Keep the control inside the collapsible body;
the fixed guide link continues to use the stored stage while collapsed.

**Definition of done:**

- Each button changes only the ILS stage, immediately reevaluates it and
  persists it through the existing playthrough identity. Selecting a stage
  never asserts completion or changes the top-level phase.
- `nav1`/`nav2` migration, invalid stage fallback, first-entry seeding,
  leaving/reentering ILS and independent playthrough preferences have fixtures.
- Research, moving/unloading cargo, missing evidence, F8 and refresh cannot
  change a stored stage. R1-R3 cannot cause navigation changes.
- Selector visibility, selected-state modeling, callback dispatch and source
  anchors are tested in both variants; existing controls retain their contracts.
- Selection, analysis input and snapshot stage provenance are versioned where
  changed. Compilation succeeds with zero errors; visual acceptance is reserved.

**Out of scope:** Automatic stage advancement, checkpoint-history latches,
new top-level phases, flight detection, stage lockouts, UI redesign.

### GC3-02 — Distinguish outpost production, aboard cargo and cargo at home

**Value:** Loading the correct finished materials into Icarus becomes progress
rather than a new shortage, and mining ore cannot pass a smelting objective.

**Scope:** Implement D2 in the existing collection/normalization, ILS gate and
compact evidence paths. Retain the two researched departure requirements and
bounded loadout presence checks. Stage II reports selected-outpost finished
production, cargo aboard, and home location/available haulback distinctly.
Add explicit availability only for inputs used by the revised checks.

**Definition of done:**

- Only 1106/1105 production counts as remote smelting. Birth-planet production
  cannot satisfy it; unknown birth identity cannot prove a non-birth outpost.
- Remote storage, Icarus inventory and home stock are not conflated. Thresholds
  are exactly 860 Titanium Ingots and 520 High-Purity Silicon; counts are not
  duplicated when unloading. Current stage remains player-owned throughout.
- Fixtures cover before loading, partial/full aboard, transit, partial/full
  unloading at home, spent cargo, raw ore only, starter Silicon, two candidate
  planets, absent fields and valid zero counts.
- Below threshold, advice names the missing finished material/amount. Full
  aboard cargo abroad suggests return; secured cargo at home points to Stage
  III. Unavailable evidence produces uncertainty rather than a restocking order.
- Presence of loadout items is described as observed equipment, never proof
  of adequate fuel, a chosen destination or a safe powered layout.

**Out of scope:** Fuel calculations, historical hauling proof, multi-outpost
mission planning, belt/power topology, an expanded packing calculator.

### GC3-03 — Suggest research at the stage where it is useful

**Value:** The player sees a research action they can use now, with no redundant
queue instruction or unnecessary Drive Engine Lv3 detour.

**Scope:** Add the bounded stage-owned technology policy in D3 to ILS analysis.
Commit its explicit and implicit prerequisite data as a small local table
with source attribution and exact rank labels. Include the prerequisite
closure of the listed targets from the pinned reference; no live web fetch is
needed when the mod runs. Export only relevant research evidence.

**Definition of done:**

- The required departure endpoints remain 2902 and 1413; survey recommendations
  are optional. Stage II owns its support branches and 1604. Stage III owns
  1414/1605, with missing real prerequisites shown before locked dependents.
- Technology 2903 is absent from required ILS objectives/actions. The exact
  1605/1604/1603 prerequisite relationships in D3 have regression assertions.
- A completed target generates no task; a queued target shows waiting status;
  a missing prerequisite supersedes an unavailable dependent action.
- The 200-Yellow-Cube batch is a reference before research begins, never a
  repeated stock requirement after the batch is partly or fully consumed.
- A player entering Stage III early receives a bounded catch-up action without
  stage changes, a full technology dump or a premature route-activation task.
- Fixtures exercise every declared branch, an incomplete inherited prerequisite,
  queued versus completed states, absent research evidence and deterministic
  ordering. Tests do not require native LDB initialization.

**Out of scope:** Editing the game's queue, tracking discretionary upgrades in
other phases, a generalized research planner, exact research ETA.

## E2 — A real transport package and corroborated home delivery

### GC3-04 — Track the finished transport package

**Value:** Assembling the two towers and five Vessels advances the mission
without asking the player to replace components already spent on that hardware.

**Scope:** Replace the required raw-reserve calculation with the bounded
finished-hardware evidence in D4. Add distinct Stage III hardware and deployment
conditions. Preserve the guide link to the expanded bill; expose counted source
scope in diagnostics. Gate assembly advice behind its required research.

**Definition of done:**

- The checkpoint requires two ILS items/stations and five Vessel items/assigned
  Vessels accessible at home or at the identified deployment endpoints; each
  physical unit is counted once. Undeployed hardware cannot pass deployment.
- Components in arbitrary storage cannot pass the finished-package checkpoint.
  Completing hardware never requires regenerating its consumed intermediate bill.
- Fixtures cover zero, one/two towers, four/five Vessels, partial deployment,
  unrelated off-world hardware, assigned/working Vessels, missing inventory and
  missing station evidence.
- Wording states observed inventory/deployment, never "protected" or "stored
  together" without evidence. Once the package is available, its next task is
  deployment/configuration rather than collecting the old component bill.

**Out of scope:** Protected-box selection, recursive recipe accounting,
handcrafting-queue inspection, automatic construction or station placement.

### GC3-05 — Verify that finished materials reach the home planet

**Value:** An unrelated route can no longer claim the first home supply is
complete, and a configured route waiting for a shipment gets useful status.

**Scope:** Implement D5 in ILS-specific analysis and the existing traffic
normalization. Resolve and report candidate endpoint IDs, policies and fleet
counts. Retain bounded session-local receipt evidence for the two finished
items; expose reset/unavailable status already known by the collector.

**Definition of done:**

- A home Demand endpoint, non-birth Supply endpoint, five assigned home Vessels,
  matching finished source stock/production and observed birth-planet inputs
  are required for the corroborated result. An unpowered source is allowed.
- R5 fails correctly. Other destinations, wrong policies, ore imports, internal
  drone traffic and a fleet on another planet cannot satisfy home delivery.
- Configuration, awaiting receipt, confirmed receipt and unavailable evidence
  remain distinct; no instruction repeatedly "activates" a configured route.
- Positive receipt persists through quiet intervals in the same valid session
  configuration, and resets on the conditions defined in D5. Fixtures cover
  reload, counter reset, endpoint changes and one-item-only delivery.
- Diagnostics disclose planet-level corroboration; no exact sending-station
  or Vessel attribution is asserted from these aggregates.

**Out of scope:** A logistics dashboard, route optimization, warpers, active
ship tracking, game-method patches, persistent shipment history.

### GC3-06 — Show only actionable Pending tasks for the selected ILS stage

**Value:** The player gets a short next step instead of simultaneous instructions
to research, rebuild already spent parts and activate unavailable hardware.

**Scope:** Implement D6 using an ILS-only ordered action-candidate list from
analysis. The panel projects at most three eligible distinct tasks with stable
IDs. Preserve complete objective evidence separately. Other phases keep their
current Pending behavior unless explicitly changed by a later story.

**Definition of done:**

- Stage I handles flight/loadout needs; Stage II handles support research,
  smelting, loading and return; Stage III handles prerequisite/batch work,
  hardware, deployment and receipt in prerequisite order.
- No route instruction precedes available hardware; no hardware instruction
  precedes its required research; queued research and waiting telemetry do not
  generate repeat actions. Selecting a later stage cannot bypass eligibility.
- The fixture journey covers departure -> remote construction -> load ->
  return -> research -> partial/full package -> configuration -> home receipt,
  including deliberate player stage selections and retries of incomplete work.
- Repeated identical evidence yields the same IDs/order; there are at most
  three tasks, no duplicate labels, and each task has a currently unmet purpose.
- Both build variants produce equivalent objectives/actions/guide anchors.

**Out of scope:** A global task engine, automatic stage/phase progression,
inventory reservation, reordering existing non-ILS production-risk rows.

## E3 — Trustworthy mid/late-route readiness

### GC3-07 — Stop healthy YELLOW/PURPLE lines from failing a storage gate

**Value:** A player running the guide's three supplied Labs can move on without
building buffers solely to satisfy the mod.

**Scope:** Apply D7 to `EvaluateYellow`, `EvaluatePurple` and their panel
projection. Retain terminal-item collection and production-risk eligibility.

**Definition of done:**

- Three continuously producing configured Labs can satisfy the phase with zero
  separately stored direct inputs. R7 and its PURPLE equivalent are corrected.
- Empty storage alone does not create a mandatory buffer task on a healthy line.
  Genuine draining/starved input findings remain eligible.
- GREEN's storage objectives and BLUE/RED/WHITE exact rate goals are unchanged.
- Fixtures cover healthy/no-buffer, stopped production, an actionable input
  shortage, incomplete Lab configuration and unavailable evidence.

**Out of scope:** New exact YELLOW/PURPLE rate targets, branch progression,
filler-research gates, changes to the shared production-risk calculation.

### GC3-08 — Carry DYSON through the required receiver bridge

**Value:** The player can find the missing Receiver-to-Antimatter work before
PHOTON asks them to strengthen a line they have not built.

**Scope:** Apply D8. Preserve current swarm conditions and append stable
receiver, conversion and explicit player-check conditions to DYSON. Use
existing recipe/receiver/item evidence, with only the necessary compact
snapshot additions. Resolve the bridge guide anchor from the presentation
model once the swarm conditions are ready.

**Definition of done:**

- The nine-phase sequence and manual navigation are unchanged. DYSON's bridge
  requirements do not disappear or swap IDs when evidence changes.
- Missing 1504/1505/1506 advice respects prerequisites. Receiver checks retain
  the tested two-unhealthy-sample tolerance; recipe 74 and the scoped material
  evidence in D8 are checked without conflating configuration with production.
- The Hydrogen outlet/science-district delivery remains an explicit required
  player check and cannot be auto-completed from global output. The panel
  distinguishes observed readiness from evidence still requiring judgment.
- Fixtures cover absent/partial/healthy arrays, wrong Collider recipe, no
  conversion, Icarus-only Antimatter, unknown evidence, source anchors and the
  unresolved player check. No fixed 1.655-GW or efficiency-rank gate appears.

**Out of scope:** A tenth phase, automatic navigation, Hydrogen blockage
attribution, conveyor connectivity proof, redesigning receiver continuity.

### GC3-09 — Measure sustained 40/min readiness for the six PHOTON inputs

**Value:** A brief production burst cannot tell the player that the final
science inputs are ready.

**Scope:** Implement D9's 120-game-second/20-sample policy for items 6001-6005
and 1122 using the existing native-aggregate samples. Add a small pure
readiness evaluator and bounded normalized/snapshot output for this consumer.
Retain the boundary sample, at most 26 points; do not add another scan/cadence.

**Definition of done:**

- Fixtures distinguish 119/120 seconds, fewer than 20/20 samples, 39.99/40/48
  per minute, an isolated below-target sample, recovery after it leaves the
  window, missing samples, native zero, pause and game-data replacement.
- Every relevant sampled one-minute rate is at least 40/min before readiness;
  missing evidence cannot count as zero or successful history.
- Per-item output includes elapsed game time, sample count, minimum rate and
  ready/warming/below-target/unavailable reason. White Cubes are not sampled
  for this policy. Other phase objectives and risk-history timing are unchanged.
- The pure evaluator runs without Unity/game initialization. Collector fixtures
  verify the connection to existing sampling and its fixed memory bound.

**Out of scope:** Per-second throughput guarantees, a configurable arbitrary
rate engine, new inventory-delta rates, extending ten-minute risk warmup.

### GC3-10 — Make PHOTON report WHITE-input readiness

**Value:** PHOTON points to the first input that will starve WHITE and stays
quiet about healthy production already meeting the required pace.

**Scope:** Consume GC3-09 in two stable PHOTON objectives: all six specified
inputs sustain 40/min, and stationary Antimatter reaches 2,000. Move the
receiver construction gate to its bridge owner while retaining useful receiver
and live-power diagnostics. Update compact evidence and PHOTON action selection.

**Definition of done:**

- R6 cannot complete. All five pre-WHITE Cubes and Antimatter must be ready;
  there is no White-Cube, four-receiver-count or upgrade-rank completion gate.
- Icarus-only Antimatter does not satisfy the stationary reserve. Totals are
  labeled at their actual scope; no science-district delivery is inferred.
- The first below-target input produces one actionable task in D9 order;
  warming/unavailable inputs show status. Healthy lines create no upgrade tasks.
  The storage task remains distinct when reserve is below 2,000.
- PHOTON's five Cube-rate colors use the 40/min reference and per-item
  availability, with unchanged layout. Colors reflect current rate, not history
  completion; other phases keep their existing bands.
- Fixtures cover each individual deficient input, several deficits, all ready,
  1,999/2,000 stock, 40 versus 48 semantics, absent evidence and coexistence
  with the existing maximum-three Current Status risks.

**Out of scope:** Rate gates for White Cubes within PHOTON, efficiency-rank
requirements, branch tracing, Antimatter transportation automation.

## E4 — A configurable, reproducible candidate for owner acceptance

### GC3-11 — Choose a minimal Expert overlay through configuration

**Value:** Experienced players can keep Cube production rates and the guide
shortcut visible without the adjoining guidance panel or its controls.

**Scope:** Implement D10. Bind `[General] ExpertMode = false` in `Plugin` and
pass the startup value to `GuidePanelController` before its first `Prepare`.
Use a bounded Expert creation/apply/layout path that creates only the existing
Cube-rate bar and `DON'T PANIC` button under a non-intercepting overlay root.
Document configuration and the required game restart. Retain the current
telemetry/model pipeline, selected-phase rate policy and source-guide anchor.

**Definition of done:**

- An absent entry defaults to false. False retains the normal panel and its
  existing controls in both public and diagnostic builds; true takes effect
  on the next game launch. No in-panel toggle or live config watcher is added.
- Expert mode creates no adjoining background/edge, title/phase icon,
  objectives, Pending/Current Status/Next Actions body, scroll controls,
  collapse control, previous/next arrows, ILS stage selector, risk glyph or
  diagnostic `Save snapshot` control. These controls have no active callbacks,
  focus targets or invisible pointer interception areas.
- The bar retains its existing Cube set, native rates, unknown-value handling
  and colors for the selected phase, including GC3-10's PHOTON policy. The
  guide button retains the model's source anchor. Layout fits the retained
  bar/button without reserving the adjoining panel or an empty header.
- The overlay starts hidden; F8 still opens/closes it and never saves. Refresh,
  reopening and save changes cannot recreate the omitted UI or alter a stored
  phase/ILS stage. Existing first-selection seeding remains unchanged; returning
  to normal mode after restart restores access to the stored selection.
- Automated fixtures exercise default/false/true configuration, presentation
  policy and callback eligibility, selected-phase rate/link parity, and
  show/hide/refresh model behavior in both variants. Guarded controller creation,
  update and teardown paths compile with zero errors. Tests use synthetic
  inputs; actual Unity visibility, pointer behavior and layout remain reserved
  for GC3-13, rather than being claimed from policy tests or compilation.

**Out of scope:** Automatic phase/stage advancement, new navigation shortcuts,
always-on startup display, inventory counters, extra alerts, live mode switching,
telemetry/performance redesign, snapshot schema changes or unrelated UI cleanup.

### GC3-12 — Produce the automatically verified workshop candidate

**Value:** The owner receives a reproducible candidate with known automated
coverage and precise workshop tasks, rather than becoming its first debugger.

**Scope:** Add a focused local verification entry point that builds both
variants, runs the retained four suites plus new story suites against each,
checks snapshot parity/size and invokes existing artifact/package validation.
Wire the new Unity-independent suites and receiver policy suite into CI for
both DLLs. Keep game-dependent reflection/presentation checks in the automated
local path using installed game references; do not pretend hosted stubs are
in-game validation. Prepare the workshop checklist and candidate hashes.

**Definition of done:**

- One documented local command succeeds on the installed toolchain with zero
  build errors and all applicable deterministic suites passing for both variants,
  including normal/Expert configuration and presentation-policy coverage.
  Game-reference discovery and the known desktop SDK access requirement are
  documented; no owner-provided save is required by this command.
- Snapshot fixtures contain stage/availability/receipt/rate provenance, omit
  unrelated data, and remain within 256 KiB. Presentation-only additions do
  not trigger unrelated contract version changes.
- CI executes the portable behavior suites for both variants; local-only tests
  are listed explicitly. Existing BepInEx identity, version, public marker,
  ZIP layout, license, icon and packaged-DLL hash checks pass for the candidate.
- Set the candidate release line to 2.2.x using the existing CI patch numbering;
  synchronize plugin/assembly/package metadata through existing scripts.
  Update README, changelog, PROJECT, telemetry and runtime-testing documents to
  distinguish implemented candidate behavior from owner acceptance/publication.
- Retain the candidate, exact source revision, hashes, test reports and workshop
  procedure locally under ignored artifacts; do not depend solely on CI's
  30-day artifact retention. No player data or game binaries are committed.

**Out of scope:** Thunderstore upload, release/tag creation, game deployment,
new CI infrastructure, UI screenshot claims from compilation, owner acceptance.

### GC3-13 — Owner acceptance workshop

**Value:** The owner decides whether the technically complete candidate actually
helps during play and whether its remaining evidence limits are acceptable.

**Scope:** This is the final execution story and the only human validation
workshop. Use the exact GC3-12 candidate in DSP through BepInEx. Gather focused
screenshots/snapshots only here, and repair defects within the already agreed
criteria before presenting affected results for acceptance again.

**Definition of done:**

- Workshop covers ILS stage selection/persistence and correction of an initial
  suggestion; preparation, loading, return and consumed cargo; useful research
  and hardware actions; configured versus observed home delivery; and no
  unrelated-planet success. Purpose-built saves may be used at this stage.
- Owner exercises healthy no-buffer YELLOW/PURPLE, the DYSON bridge/player-check
  wording, PHOTON warming/deficit/recovery/40-min readiness, and unchanged WHITE.
- Both variants in normal mode are checked for navigation, source links,
  collapse, pointer behavior, representative 1080p/4K layout, diagnostic-only
  snapshot control, save reload, missing-evidence behavior and visible
  performance/log regressions.
- In both variants, enable Expert mode through the config and restart: only
  the Cube-rate bar and working `DON'T PANIC` button appear when requested.
  Check rates/anchors, F8, refresh, save reload, 1080p/4K placement and pointer
  pass-through over the former panel. All omitted controls remain absent and
  inert. Disable the setting and restart to confirm normal controls and stored
  phase/ILS stage return. No snapshot can be triggered in Expert mode.
- Feedback includes the cost of manual ILS stage choice and the two-minute
  rate window. Before/after utility is judged against the specific problems in
  R1-R7, not full textual conformance to the guide.
- In-scope failures are repaired and automated checks rerun where affected.
  Acceptance requires an explicit owner decision and no unresolved acceptance
  blocker. Rejection leaves this story open; a material new feature is separately
  scoped rather than silently added.
- Record the accepted source/artifact identity and known limitations, reconcile
  current management state, and archive this roadmap only on explicit owner
  closeout instruction. Technical completion alone cannot close this story.

**Out of scope:** An exhaustive new playthrough, optional/combat paths,
unrequested publication, unrelated feature requests or automatic acceptance.

## Gate definitions

| Gate | Agent-verifiable exit evidence | Human involvement |
|---|---|---|
| G1 | GC3-01/02/03 done; retained local suites plus new selection/cargo/research fixtures pass against both variants; source links and serialization cases covered | None |
| G2 | GC3-04/05/06 done; complete synthetic ILS journey, unrelated-home negative cases, receipt reset/quiet cases and Pending eligibility/order pass in both variants | None |
| G3 | GC3-07 through GC3-10 done; all phase regression, bridge uncertainty and PHOTON sample-boundary cases pass; no top-level phase additions or removed exact legacy targets | None |
| G4 | GC3-11/12 done; both modes covered in both variants; candidate versions/hashes, local and portable-CI reports, valid public ZIP, bounded snapshots and workshop procedure available | None |
| G5 | GC3-13 workshop criteria satisfied and explicit owner acceptance recorded | Final owner workshop only |

Before GC3-12's wrapper exists, build diagnostic/public outputs separately with
`IncludeSnapshotControl=true/false`, run each relevant PowerShell suite using
its `-DllPath`, and supply the installed `-GameRoot` to game-dependent suites.
Use `Test-SnapshotControlVariant.ps1` for each variant. These are technical gate
commands; no deployment or screenshot is required. Each new story suite must
accept `-DllPath`, use synthetic inputs, and report failures as nonzero exits.

## Planning readiness check

- Every admitted gap maps to a bounded story; no-op/deferred areas are explicit.
- Each story states value, implementation boundary, done criteria and exclusions.
- Dependencies are acyclic; the default sequence respects all gates.
- Research is already resolved in the linked analysis; no discovery story is
  placed in the execution path.
- All pre-workshop checks can run without human-provided game state.
- Runtime, interaction, performance and utility acceptance remain accurately
  reserved for GC3-13.
- This draft and its publication do not change the shipped product contract.
