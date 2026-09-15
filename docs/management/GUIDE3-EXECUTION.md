# Guide 3.0 execution record

**Authority:** [PROJECT.md](../PROJECT.md) and the accepted [roadmap](../ROADMAP.md).
**Authorization:** 2026-09-15, implement GC3-01 through GC3-12 in order, record
decisions and push to main after each completed story. Stop at GC3-13 for human
validation. Technical completion is not owner runtime acceptance or publication.

## Current state

| Stories / gate | State |
|---|---|
| GC3-01 | Technically complete; pushed as `522a5fc` |
| GC3-02 | Technically complete; pushed as `9195e21` |
| GC3-03 | Technically complete; pushed as `4896d81` |
| GC3-04 | Technically complete; pushed as `696d79e` |
| GC3-05 | Technically complete; pushed as `2b9db24` |
| GC3-06 | Technically complete; pushed as `c75b2c6` |
| GC3-07 | Technically complete; pushed as `2fb9770` |
| GC3-08 | Technically complete; pushed as `d69843e` |
| GC3-09 | Technically complete; pushed as `05bd817` |
| GC3-10 | Technically complete; pushed as `b8dea50` |
| GC3-11 | Technically complete |
| GC3-12 | Next |
| G1 / M1 | Passed: stage, cargo and research technically coherent |
| G2 / M2 | Passed: complete ILS journey and eligible Pending tasks |
| G3 / M3 | Passed: Guide 3.0 handoffs and PHOTON readiness |
| G4 | Pending |
| GC3-13 / G5 | Reserved for owner workshop; not started |

## GC3-01 — Select and retain the ILS stage

**Outcome:** `nav3` persists the stage and suggestion/manual origin alongside
the existing phase and playthrough identity. I/II/III controls dispatch through
the same navigation callback, remain inside the collapsible body and immediately
rebuild the selected-stage model. The fixed guide link reads the stored stage.

**Decisions:** Reuse BepInEx selection persistence, the existing button factory
and normalized evidence. Seed only on first ILS entry with an uninitialized
stage; missing birth identity cannot prove an outpost. Invalid stage data does
not discard a valid phase. No history latch, runtime event hook or automatic
advancement was added. The analysis/gate input now explicitly includes stage.
Existing phase-only fixture calls specify Stage I; ILS fixtures choose their
stage deliberately rather than expecting automatic inference.

**Contracts:** selection 1.7 (`nav3`), analysis 3.4, progression 3.2, panel 2.9,
snapshot 2.17. Normalized runtime evidence and release numbering are unchanged.

**Validation:** Diagnostic compilation via `build.cmd`: zero warnings/errors;
its four retained suites passed (the phase suite was rerun after updating all
reflection calls for the added input; receiver suite then run separately).
`Test-IlsStages.ps1` and `Test-SnapshotControlVariant.ps1` passed. Public build
via `dotnet build ... -p:IncludeSnapshotControl=false -o artifacts/guide3/public`
also had zero warnings/errors; all four retained suites, ILS-stage suite and
variant verification passed against that DLL. Fixtures cover migration,
invalid values, one-time seeds, R1-R3 evidence changes, command eligibility,
leaving/reentering ILS, independent selections, stable objective IDs and anchors.

**Reservation:** Actual button placement, pointer behavior and save-reload
interaction will be checked at GC3-13. Cargo and research rules are intentionally
unchanged here and remain the next stories. G1 is not yet complete.

## GC3-02 — Distinguish outpost production, aboard cargo and cargo at home

**Outcome:** Stable Haulback objectives report outpost smelting, return cargo
and cargo at home separately. Only 1106/1105 production qualifies. Outpost
storage is loading context; at home, package and stationary stock are summed
once. Missing evidence produces unknown status without a restocking action.

**Decisions:** Reused the existing grid/pool collection pass and native
Statistics Panel fields. Added collection availability, per-ID research
availability and location presence at normalization. Known current outpost wins;
otherwise the lowest eligible planet ID wins. Spent cargo cannot prove past
hauling: the player can select Automation after completing that trip. Equipment
presence does not claim enough fuel or safe construction space.

**Contracts:** normalized state 2.4, analysis 3.5, progression 3.3, snapshot
2.18. Stage ownership, panel contract and native sampling cadence are unchanged.

**Validation:** Diagnostic and public builds completed with zero warnings and
errors. All four retained suites and both variant checks passed; the phase
suite's normalized-version expectation was updated for 2.4. `Test-IlsCargo.ps1`
includes the retained stage fixtures plus cargo/loading/unloading/transit,
finished-versus-ore, deterministic outpost, spent stock and unavailable/zero
cases. `Test-IlsEvidenceCollection.ps1` additionally exercises real collector
methods with synthetic grids/pools and normalization with missing/valid-empty
inputs. It passed against both DLLs using local game references. The public
command remains the GC3-01 command with the new collection suite included.

**Reservation:** No new game session or save was used. Runtime presentation and
utility remain reserved for GC3-13; G1 awaits the research story.

## GC3-03 — Suggest research at the stage where it is useful

**Outcome:** Departure, Haulback and Automation consume their own prerequisite
branches. The first unmet prerequisite supplies one concise research action;
queued work waits, completed targets disappear from suggestions, and missing
research/queue observations remain unknown. Survey research is explicitly
optional. Early Automation suppresses unusable hardware/route tasks.

**Decisions:** Pinned the declared targets' full explicit/implicit closure from
guide commit `c6d846b09f80564106ba221133a1c1d021267f2f` in
`IlsResearchPolicy`. All included `pretechsMax` values are false. An observed
unlocked node is authoritative and needs no retrospective prerequisite proof;
unfinished nodes traverse their prerequisites in reference order. Stage II's
support research is advice, separate from required cargo objectives. No live
LDB graph, research-cost estimator or queue mutation was added. The 200-Cube
reference ends when either batch-consuming target is queued, partly progressed
or unlocked; it never reads current Cube inventory as a replenishment target.

**Contracts:** normalized state 2.5, analysis 3.6, progression 3.4, snapshot
2.19. Research provenance is limited to the selected stage's closure. Other
phase research behavior and player-owned navigation remain unchanged.

**Validation / G1:** Both variants built with zero warnings/errors. The four
retained suites, `Test-IlsEvidenceCollection.ps1`, `Test-IlsResearch.ps1` and
variant checks passed against both final DLLs. The new research suite exercises
each stage endpoint, inherited implicit prerequisites, exact 1603/1604/1605
edges, queue/complete/unknown states, partial batch consumption, optional rank
labels and early-Automation eligibility. The prerequisite table initializer and
a test array-count assertion were corrected before these successful runs.

**Milestone M1:** Stage selection, cargo and research are technically coherent.
G1 passed without a game session or human validation. Actual usability remains
reserved for GC3-13; finished hardware and home-route proof are next.

## GC3-04 — Track the finished transport package

**Outcome:** Removed the simultaneous raw-component reserve gate. The package
counts two finished towers and five Vessels from home inventory and the selected
deployed endpoints. Configuration/fleet deployment has a separate objective.
Building hardware cannot create a demand to replenish its consumed components.

**Decisions:** Reused stationary stock, the Icarus package and normalized station
fields. Select home by matching Demand slots, source by matching Supply slots,
then stable IDs. An unconfigured source must be on the identified outpost.
Each selected tower and idle/working Vessel is counted once; unrelated off-world
hardware is excluded. Known sufficient hardware can pass even if additional
inventory is unavailable, but an incomplete unknown count generates no build
order. Persistent preference for equally suitable endpoints and current-policy
receipt tracking belong to GC3-05. No raw-recipe reconstruction or protected-box
claim remains in the package diagnostics.

**Contracts:** normalized state 2.6, analysis 3.7, progression 3.5 and snapshot
2.20. Compact evidence records inventory counts, selected endpoint IDs, assigned
Vessels and availability. The linked guide retains the expanded component bill.

**Validation:** Both builds had zero warnings/errors; all four retained suites,
`Test-IlsHardware.ps1`, `Test-IlsEvidenceCollection.ps1` and both variant checks
passed. Hardware fixtures include zero/one/two towers, four/five Vessels, raw
components only, partial deployment, assigned plus working Vessels, unrelated
hardware, unavailable collections, and build/deployment prerequisite ordering.
Collection fixtures distinguish valid zero stations from missing station fields.

**Reservation:** No game runtime claims. G2 still awaits receipt verification
and the full ILS Pending journey; owner acceptance remains GC3-13.

## GC3-05 — Verify finished materials reaching home

**Outcome:** Automation requires a configured birth-planet Demand receiver with
five Vessels, a non-birth Supply source with finished stock/production, and new
home inputs for both finished materials. Configuring a route produces waiting
status, not a repeated activation task.

**Decisions:** Reuse native cumulative counters, not rolling input rates that
could predate configuration. Preserve equally suitable endpoint identities.
Keep only two session receipt flags and their baseline; reset on game identity,
configuration, counter epoch/regression or unavailable evidence. Current source
stock/production corroborates readiness without requiring source power. The
tracker observes existing model refreshes; it cannot attribute individual ships
or see station changes between observations. No game writes or extra scans.

**Contracts:** snapshot 2.21, normalized 2.7, analysis 3.8, progression 3.6.
Compact stage evidence includes receipt status, reset reason, sample tick/epoch
and the explicit planet-level attribution limit.

**Validation:** Both variants built with zero warnings/errors. Four retained
suites, Test-IlsReceipt (including prior ILS suites), Test-IlsEvidenceCollection
and variant checks passed against both. Fixtures cover old/one/both/quiet
receipts, source zero, preferred endpoints, reload, missing evidence, policy and
counter changes, other destinations, fleets, ore and internal traffic. Collector
fixtures exercise real sampling and normalization with native-shaped synthetic
objects. A misplaced normalization insertion was repaired before the final
successful matrix. No live save or runtime acceptance was claimed.

## GC3-06 — Actionable ILS Pending tasks

**Outcome:** Analysis emits a bounded candidate list with stable IDs, kind,
priority and eligibility. The panel projects at most three distinct eligible
tasks while retaining all objective evidence. Queued research and awaiting
receipt are status only. A later selected stage still obeys prerequisites.

**Decisions:** Reuse objective actions and their existing prerequisite guards;
add only ILS-specific projection. Loading becomes eligible when source material
or smelting is observed, or cargo is being assembled at home. Other phases'
Pending behavior and production-risk ordering are unchanged.

**Contracts:** analysis 3.9, panel 2.10, snapshot 2.22; no new runtime inputs.

**Validation / G2:** Both variants compile with zero warnings/errors. Four
retained suites, native collection fixtures, variant checks and Test-IlsJourney
(including all prior ILS fixtures) pass. The journey exercises departure,
construction, loading, return, early/queued/completed research, partial/full
hardware, configured waiting and confirmed receipt. Stable IDs/order, three-task
limit, unique labels and stage anchors pass.

**Milestone M2:** The complete synthetic ILS journey is technically coherent.
G2 passed; player-facing utility and actual Unity behavior remain GC3-13.

## GC3-07 — Healthy YELLOW/PURPLE without extra storage

**Outcome:** Removed the separate terminal-storage objectives from these two
phases. Existing three-Lab positive-production readiness remains; missing recipe
or per-item production evidence is unknown without an invented construction task.

**Decisions:** Input collection and shared risk calculation are unchanged.
Actual draining/starved risks still qualify. GREEN stores and BLUE/RED/WHITE
exact targets are unchanged. No new rate or continuity threshold was invented.

**Contracts:** analysis 3.10 and progression 3.7; snapshot shape unchanged.

**Validation:** Both variants compiled with zero warnings/errors. Four retained
suites and Test-GuideHandoffs passed for both, covering zero storage, stopped
production, incomplete Labs, missing recipe/item evidence and unchanged GREEN.
Retained phase fixtures verify actionable input depletion and exact-target
suppression. The new fixture's configured-machine field name was corrected
before success. The touched retained test also removes an extra EOF blank line
introduced in GC3-06. Runtime utility remains reserved for GC3-13.

## GC3-08 — Required receiver bridge

**Outcome:** DYSON keeps its swarm objectives and adds stable research, receiver,
conversion and player-check rows. Once swarm objectives pass, the guide link
opens receiver-antimatter-bridge. Nine selectable phases remain unchanged.

**Decisions:** Reuse the tested four-receiver policy, including its two-sample
tolerance. Research follows 1504/1505/1506; queued or unknown work creates no
repeat action. Conversion requires recipe 74, native Photon production and
consumption, Antimatter production and positive stationary stock. Observed
cluster stock excludes Icarus. Missing component pools now retain uncertainty.
No belt topology, source-power threshold or efficiency-rank gate was added.
The required Hydrogen/delivery check always remains evidence-incomplete.

**Contracts:** snapshot 2.23, analysis 3.11 and progression 3.8. DYSON compact
evidence now includes recipe 74, material rates, receiver details and stationary
stock scope. No presentation-shape or normalized-state shape change.

**Validation:** Both variants compiled without warnings/errors; four retained
suites and Test-GuideHandoffs passed. After the collector availability fix, both
variants rebuilt and passed handoff, collector and receiver-tolerance suites.
Fixtures cover absent/partial/full arrays, wrong recipe, no conversion, Icarus
stock, missing inputs, ordered research, stable IDs, anchors and the unresolved
player check. GC3-13 retains actual connectivity and presentation acceptance.

## GC3-09 — Sustained PHOTON input sampling

**Outcome:** The existing production sample feeds a pure six-input evaluator.
It retains a boundary sample, at most 26 points per item, and requires 120 game
seconds plus 20 distinct observations at or above 40/min. Each item reports
time, count, minimum and ready/warming/below-target/unavailable.

**Decisions:** Use game ticks and the already computed native galaxy aggregates.
Repeated ticks replace the current sample without increasing count or age.
Unavailable samples clear only the affected history; game replacement, explicit
clear and backwards ticks reset the window. Keep existing risk sampling intact.
The two-minute rule is the accepted mod policy, not a per-second belt guarantee.

**Contracts:** normalized 2.8 and snapshot 2.24. Only PHOTON compact evidence
includes the six summaries; raw histories and White-Cube readiness are excluded.

**Validation:** Diagnostic/public builds have zero warnings/errors. Four
retained suites and the new pure/collector suites pass for both variants.
Cases include 119/120 seconds, 19/20 samples, 0/39.99/40/48 rates, isolated low
sample expiry, missing data/recovery, repeated paused ticks, fixed memory bound
and game-data replacement. Synthetic native-shaped objects verify the actual
collector and normalization connection. PHOTON objectives consume this in GC3-10.

## GC3-10 — WHITE-input readiness in PHOTON

**Outcome:** Two stable objectives require all six sustained inputs and 2,000
stationary Antimatter. Receiver construction remains in DYSON; receiver/power
evidence is retained. Five Cube colors now use current 40/min availability.

**Decisions:** Pending selects the first current shortage in Blue/Red/Yellow/
Purple/Green/Antimatter order. A recovered current rate waiting for its low
history to age out does not prompt another upgrade. Warming/unknown inputs
remain status. The independent storage task counts cluster storage/stations,
excluding Icarus and making no science-district delivery claim. Current Status
risks keep their existing separate cap and purpose.

**Contracts:** analysis 3.12 and progression 3.9; current snapshot/panel shapes
remain unchanged. PHOTON compact item evidence includes the five colored Cubes.

**Validation / G3:** Both variants compiled with zero warnings/errors. Four
retained suites, Test-PhotonOutcome and Test-PhotonReadiness pass. Handoff
fixtures are included by the outcome suite. Cases cover each individual input,
several deficits, all ready, 1,999/2,000 stationary stock, Icarus-only stock,
40/48 semantics, missing rates, waiting recovery, R6 and coexistence with three
Current Status risks. Existing BLUE/RED/WHITE exact targets and GREEN stores
remain covered by retained fixtures.

**Milestone M3:** G3 passes without runtime/owner validation. Expert presentation
and candidate packaging are next; actual utility remains reserved for GC3-13.

## GC3-11 — Configurable Expert overlay

**Outcome:** Default-off ExpertMode is bound through BepInEx before Prepare.
Expert creation skips the panel background/edge, title/header/body, navigation,
stages, collapse, scrolling, risk glyph and diagnostic snapshot control.
Only the existing bar and guide button are created, sized without an empty
panel/header. Normal and Expert reuse bar/button construction and rate models.

**Decisions:** Startup-only immutable presentation policy; no live watcher or
new telemetry path. Plugin does not register navigation/snapshot actions in
Expert mode, and controller callbacks independently reject them. Refresh uses
the same selected phase/stage, source anchor and rates. Root has no Graphic;
only the guide button is interactive. Hidden refresh stays hidden.

**Contracts:** No snapshot or analysis version change for presentation-only
work. Normal mode is retained by default; returning to it requires restart.

**Validation:** Both variants compile with zero warnings/errors. Four retained
suites, variant checks and Test-ExpertConfiguration pass. Its portable policy
suite includes prior phase outcomes and checks default/false/true, callback
eligibility and rate/link parity. Local fixtures bind actual BepInEx config,
exercise omitted callback guards, and check hidden update/tick/teardown without
creating Unity objects. A ConfigFile enumeration issue in the fixture and the
retained shared-parent source assertion were corrected before final success.
The hidden-lifecycle fixture requires PowerShell 7; Windows PowerShell 5 eagerly
resolves Unity native calls outside the game and cannot run that fixture.
Actual Show/creation, visibility, focus, pointer pass-through and 1080p/4K layout
remain explicitly unvalidated until GC3-13; compilation is not that evidence.
