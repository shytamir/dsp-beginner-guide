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
| GC3-11 | Technically complete; pushed as `07007a6` |
| GC3-12 | Technically complete; candidate source pushed as `e89f73e` |
| G1 / M1 | Passed: stage, cargo and research technically coherent |
| G2 / M2 | Passed: complete ILS journey and eligible Pending tasks |
| G3 / M3 | Passed: Guide 3.0 handoffs and PHOTON readiness |
| G4 | Passed: both variants, local/hosted checks and retained candidate ready |
| GC3-13 / G5 / M5 | Complete; owner full-playthrough acceptance 2026-09-19 with B2/B3 corrected; publication pending |

## GC3-13 — Full-playthrough acceptance and publication preparation

**Owner report (2026-09-19):** Full playthrough completed with no observable
regressions. ILS is accepted for this version. The only reported issues are the
lengthy manual DYSON delivery objective and the receiver label omitting Photon
Generation. The owner requested these corrections and preparation for publishing.

**Evidence:** The supplied snapshot/screenshot pairs are indexed in the
[workshop record](GUIDE3-WORKSHOP.md#full-playthrough-and-final-dyson-corrections--2026-09-19).
Both snapshots identify `2.2.101`. The first has four deployed but zero Photon
Generation receivers; the later snapshot has four configured, lensed and
sustained receivers. Screenshots show the corresponding `0/4` and `4/4` values
and the manual objective. The evidence supports a wording correction.

**Changes and decisions:**

- Remove `dyson-handoff` and its Pending reminder. Routing and Hydrogen advice
  stays in the guide. Five stable objectives remain; observed DYSON readiness
  no longer stays evidence-incomplete solely because of that manual row.
- Name Photon Generation in the receiver objective. Preserve existing native
  data, sampling, tolerance and counts. No new collection or performance work.
- Update the store README's stale Expert description to the already implemented
  WHITE behavior; this is documentation only.
- Analysis 3.13 and progression 3.10 identify the changed objective contract.
  Snapshot schema remains 2.24 because its structure is unchanged.

**Validation and handoff:** Release `dotnet build` passed with zero warnings or
errors. `Test-GuideHandoffs.ps1` passed five-objective completion, no redundant
Pending action, explicit receiver mode, `0/4` and `4/4` evidence, existing
receiver thresholds, conversion failures, stable IDs and guide anchors.
The clean-source handoff command is:

```powershell
pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1 -Sequence 103 -OutputDirectory artifacts/guide3/publish-ready
```

Its manifest and reports bind public/diagnostic DLLs, snapshot parity and the
validated public ZIP to the final commit. User snapshots remain external.

**Disposition:** GC3-13/G5/M5 complete under the owner's acceptance with these
two corrections. Publication is pending. No post-fix game session, installed-DLL
hash or exhaustive acceptance-case matrix is claimed. Expert fixed-WHITE remains
automatically tested without later owner runtime confirmation.

## 3.0 version promotion — 2026-09-19

**Authorization:** The owner requested promotion to 3.0 with strict artifact
version verification. Update only `VERSION`, local `BuildVersion` constants,
project version defaults and the corresponding release records. Preserve the
accepted behavior, snapshot schema 2.24 and all analysis contracts.

**Procedure:** Build the local defaults and verify the compiled plugin identity.
Commit and push the promotion, then use that commit's actual CI run number as
`N`. Verify both DLL variants and the public ZIP downloaded from that run:
package/BepInEx/exporter `3.0.N`, assembly/file `3.0.N.0`, and diagnostic product
label `3.0.N.<commit>`. The packaged DLL must hash-match the public artifact;
the diagnostic build must retain snapshot control and the public build omit it.
Run the existing clean-source candidate command with the same sequence against
installed game references. Retain manifests and validation reports under
`artifacts/release-3.0/`; report actual run/commit/version identities at handoff.
The historical 2.2.x candidate artifacts remain unchanged. Store publication
and any release/tag remain separate from this source/artifact promotion.

## GC3-13 B1 — periodic panel slowdown

**Report and authorization (2026-09-16):** The owner reported roughly 55 to
22 FPS every 15 seconds for nearly two seconds on a mature factory, with
ExpertMode false and WHITE visible. The prior installed version had no
noticeable periodic dip. The owner authorized bounded native-data corrections,
tests, an installable candidate and a push to main; explicitly no new measurements.
This is the workshop's first blocking issue, not a new roadmap epic or story.

### First correction — failed owner retest

**Changes and reasons:**

- Preserve `StationComponent.storage` ownership through the existing collector
  and normalized station. Endpoint policy/source-stock checks use the station's
  own slots; they no longer search the entire cluster's slot list for each station.
  The normalized flat list shares those same slot objects for existing consumers.
- Observe ILS receipt baselines only in selected Automation. Leaving the stage
  clears the baseline so returning cannot certify deliveries during an unobserved
  interval. Native planet input counters and existing completion thresholds remain.
- Replace the production prerequisite table with the relevant native
  `LDB.techs.Select(id)` closure, read once. Retain the native prerequisite arrays;
  use native names and levels with the existing `LvN` presentation. The stage
  targets and advice order remain guide policy. Missing definitions stay unknown.
  The old table is now synthetic test input only, not compiled mod data.
- Remove the full `ObservedGameState.Export()` from panel analysis. Pass the
  queue-availability flag directly to the compact snapshot builder, preserving
  its external schema and deliberate export behavior. No broad snapshot is added.
- Reuse station fleet/storage fields, recipe pools, traffic pools and the research
  queue already read for collection rather than reading them again for availability.

**Scope decisions:** Native route pairs are not a substitute for the existing
endpoint-configuration objective; do not change completion semantics or call
native route rebuilds. Native inventory extra-info caches have different scope
and refresh behavior, so existing precise storage reads remain. Keep the small
PHOTON history over native rates and the existing Expert presentation contract.
No telemetry redesign, broad collector rewrite, measurement system, game/save
mutation, player-facing prose rewrite or new acceptance gate.

**Validation (2026-09-16):**

- `dotnet build src/DspProgressionStatusExporter/DspProgressionStatusExporter.csproj -c Release`
  passed with zero warnings/errors against the installed DSP/BepInEx references.
- `pwsh -NoProfile -File scripts/Test-IlsJourney.ps1` passed, including changed
  native prerequisite inputs, scoped receipt resets and no full-state analysis export.
- `pwsh -NoProfile -File scripts/Test-IlsEvidenceCollection.ps1` passed after
  correcting a PowerShell dictionary construction in the added fixture. It checks
  native explicit/implicit arrays, rank labels, missing metadata, normalization
  reference reuse and station-slot isolation.
- `pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1 -Sequence 99 -OutputDirectory artifacts/guide3/blocker01-preflight -AllowWorkingTree`
  passed for public and diagnostic variants: retained/Guide 3.0 suites, native
  collector fixtures, BepInEx configuration, hidden lifecycle, compiled identity,
  snapshot parity and public package validation. Both builds had zero warnings/errors.
  This preflight is not the delivered binary; the clean-source handoff below
  rebuilds and verifies its exact DLLs and writes the final reports/identity manifest.
- No game session, live performance claim, new measurement or diagnostic export
  from a player save was used. B1 remains subject to owner retest.

**Handoff:** Run `scripts/Test-Guide3Candidate.ps1 -Sequence 99 -OutputDirectory
artifacts/guide3/workshop-blocker-01` from the clean correction commit. Its
`candidate.json` binds the tested public/diagnostic DLLs and public ZIP to that
source. This folder replaces the GC3-12 candidate for retest; original records
below remain historical. The final source archive and reports stay local/ignored.

**Acceptance:** Rejected at runtime: the owner reported no improvement. The
following follow-up supersedes this candidate. GC3-13/G5 remain open; no release,
tag or publication is authorized by this repair.

### Follow-up — remove discarded collection (2026-09-16)

**Verified comparison:** The installed correction's binary identifies itself as
`2.2.100.ff71d06`; its compiled code includes the scoped ILS correction. The
[public package](https://thunderstore.io/c/dyson-sphere-program/p/DSPGuideCheckMod/DSPGuideCheck/)
identifies its DLL as `2.1.83.22a5998`, matching the owner's mod-manager version.
Read-only decompilation of both binaries confirms the same 15-second broad
collection stages. This was not a stale correction DLL. The owner then verified:

- Hiding the candidate overlay stops the periodic dip.
- Public `2.1.83` also drops by approximately 19 FPS with WHITE visible, versus
  approximately 33 FPS for the candidate. This replaces the initial no-dip
  baseline report; both inherited cost and additional degradation need acceptance.

**Scope extension:** The owner explicitly included the public version's roughly
19 FPS hit in this build: review and correct inherited native-data underuse and
overloaded 15-second collection as part of the same B1 issue.

**Confirmed work with no consumer:** `ObservedGameState.Build` never reads the
legacy `player` export. From factory rows it reads tanks and stations only;
power and production come from their existing telemetry inputs. It discards
factory-level building counts, container details, generic power/production/enemy
metrics and station-stock aggregates. Progression's aggregate building counts
are normalized but have no active guide or compact-snapshot consumer either.
Removing only the final normalized export in the first correction left this
upstream work intact.

**Change and scope:** Stop those unused exports in the shared collection used
by panel opening, timed refresh and deliberate compact snapshots. Reduce tank
rows to consumed item/count/capacity and station fleet reads to the consumed
ship counts. Remove both building-count passes from active collection. No phase
rules, player prose, refresh cadence, native
sampler, snapshot schema, diagnostic mechanism or game/save state changes.
No new cache, scheduling framework, approximate inventory source or measurement.
Existing unused helper definitions are left alone; they are no longer called by
these paths. This patch removes confirmed waste, not a demonstrated explanation
of the full difference between the two versions.

**Remaining 15-second contributors and native decisions:**

| Contributor | Verified native source and decision |
|---|---|
| Research | `GameHistoryData.TechUnlocked(id)` returns `techStates[id].unlocked` or false for an absent entry. Read that table once per refresh with cached prototype names and only consumed hash progress; omit generic capability reflection. Missing tables remain unknown. |
| Inventory | WHITE needs White Cube stock; DYSON/PHOTON need stationary Antimatter. Use native read-only `StorageComponent.GetItemCount(int)` for those depot/package/fuel reads, preserving existing tank/station reads and inventory boundaries. Missing methods retain the existing reflective fallback. Other phases and deliberate snapshots retain full inventory collection. |
| Statistics inventory cache | `ProductStat.storageCount`/detailed counts are refreshed by the on-request `ProductionExtraInfoCalculator` and have different scope. Its refresh scans factories and writes native statistics. Do not force a rebuild or substitute potentially stale/different stock. |
| Dyson | Live guidance needs native sphere energy fields, `DysonSwarm.sailCount` and the existing receiver sampler export. Omit construction-node and launcher detail walks from live refresh; retain them for deliberate compact snapshots. The separate existing five-second sampler is unchanged. |
| Recipes | No equivalent maintained recipe-count aggregate was found in `FactorySystem`. Read native component recipe IDs up to their cursors. The live panel reads assemblers only for DYSON's conversion check; Cube lab checks use labs. Native reference speed is not a configured-machine count. |
| Stations/tanks | Slot policy, capacity, stock and ship counts still come from their dedicated native pools. They supply local-buffer conclusions and ILS objectives; aggregate traffic cannot replace station configuration. Removed unused diagnostic and duplicate-stock reads. |
| Production/traffic/power | Existing exports use their already collected native statistics/sampler results. No new sampling or per-entity collection is added here. |
| Normalization/analysis/UI | Preserve selected-phase rules, station ownership and row reuse. Removing unrelated inventory and recipe rows from live inputs does not alter the selected objectives; deliberate snapshots retain their fuller evidence. |

Native inspection used installed `Assembly-CSharp.dll`, SHA-256
`ae0ba95f75bd879a62aa4ce253b2ab78eaa4fb3c7c595f5e1fee75ebe0e0ef85`.
No game assembly is copied into tracked files.

**Validation:**

- `dotnet build src/DspProgressionStatusExporter/DspProgressionStatusExporter.csproj -c Release`
  passed with zero warnings/errors after rerunning with access to the installed
  SDK cache; the sandbox initially denied that SDK directory.
- `pwsh -NoProfile -File scripts/Test-IlsEvidenceCollection.ps1` passed. New
  fixtures verify preserved tank amounts/capacities, station identity, fleet,
  stock/policies, live building counts and unavailable pools. Fixture getters
  also assert that the factory-row collector never touches depot grids, entity
  counts or generic factory diagnostics. A fixture type dependency was corrected
  once before the passing run; there was no runtime code repair.
- Expanded fixtures execute the installed `StorageComponent.GetItemCount` on
  isolated synthetic native grids, comparing its totals with the original grid
  reader, including known-zero and fallback cases. They also verify native
  research flags/hash progress, missing metadata, recipe cursor bounds, WHITE's
  omitted assembler reads, retained DYSON conversion recipes and native power
  reads without construction-node access.
- Running that script against the retained `ff71d06` public DLL passed the same
  tank/station/fleet/stock assertions, then failed the new discarded-work guard
  with `Factory rows still duplicate the entity count`, as expected. The guard
  detects the previous behavior; it is not a timing benchmark.
- `pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1 -Sequence 101 -OutputDirectory artifacts/guide3/blocker01-refresh-preflight -AllowWorkingTree`
  passed both variants, all retained/story/native-collector fixtures, configuration,
  snapshot parity, identity and public package checks. Both builds had zero
  warnings/errors. Preflight artifacts are not the owner candidate.

**Handoff:** The same candidate command without `-AllowWorkingTree`, targeting
`artifacts/guide3/workshop-blocker-01-refresh`, binds the final clean commit to
the `2.2.101` DLLs, package, source archive, manifest and validation reports.

**Owner acceptance (2026-09-16):** The owner accepted the remaining roughly
7 FPS impact and confirmed the ExpertMode flag works. B1 is closed. The retained
`2.2.101.5cc8f33` candidate will be used for the full playthrough regardless of
the Expert fixed-WHITE follow-up. No agent-run live measurement is claimed;
the remaining cost has not been isolated. GC3-13/G5 remain open for playthrough
and final owner acceptance.

## GC3-13 — Expert fixed-WHITE follow-up

**Owner request:** Expert mode must use WHITE so all Cube counters are displayed.
This is a bounded workshop correction, independent of the accepted candidate's
full playthrough, and supersedes GC3-11's original selected-phase behavior.

**Implementation and decision:** Return an effective WHITE selection before
normal-mode selection binding, seeding or persistence. Collection and analysis
therefore both use WHITE; all six counters keep existing WHITE values/colors and
DON'T PANIC opens WHITE. Preserve the normal phase and ILS stage for a later
restart with ExpertMode false. No telemetry, cadence, schema or UI layout
change; no new selection is written to the config or game/save state.

**Validation:**

- `dotnet build src/DspProgressionStatusExporter/DspProgressionStatusExporter.csproj -c Release`
  passed with zero warnings/errors.
- `pwsh -NoProfile -File scripts/Test-ExpertConfiguration.ps1` passed, including
  all nine normal phases, six Expert counters, WHITE anchor, repeated refresh,
  actual plugin selection with an ILS preference, no persistence/config access
  during Expert selection, default/false/true config and inert hidden controls.
- Final handoff uses `scripts/Test-Guide3Candidate.ps1 -Sequence 102 -OutputDirectory
  artifacts/guide3/workshop-expert-white` on the clean commit; its reports and
  manifest bind both variants and the public package to the tested source.

**Acceptance:** Corrected-build in-game counters/link confirmation is pending.
The owner already accepted the flag's operation. Neither this follow-up nor
technical validation closes the full-playthrough workshop or authorizes publication.

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

The 2026-09-19 owner closeout above supersedes this story's manual handoff row.
The original implementation evidence below is retained as history.

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

## GC3-12 — Verified workshop candidate

**Outcome:** Added Test-Guide3Candidate as the single local verification entry
point. It builds both variants from installed references, isolates fixture
processes, verifies actual plugin identity and public/diagnostic markers, checks
compact snapshot parity and bounds, and invokes the existing artifact/package
validators. Clean runs retain source.zip, candidate.json with hashes, reports
and the workshop procedure. Hosted CI now runs the portable suites for both
DLLs, including receiver continuity and all pure story outcomes.

**Decisions:** Use PowerShell 7 for the existing hidden-controller fixture. Keep
game-dependent production lookup, phase/typography, collection, BepInEx config
and snapshot checks local; hosted compile references cannot validate Unity
interaction. Generate a candidate-specific BuildVersion.cs under ignored
artifacts without dirtying tracked source. The 2.2.x line keeps the existing CI
run-number patch; no versioning framework or release workflow was added.
Preflight output from dirty source is explicitly unsuitable for acceptance.

**Local validation (2026-09-16):**

- `pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1 -Sequence 98 -OutputDirectory D:/Shy/dsp-beginner-guide/artifacts/guide3/preflight-98 -AllowWorkingTree` passed.
- Both builds: zero warnings/errors. Both variants passed the four retained
  suites, all story fixture chains, variant/compiled-identity checks and existing
  artifact validation. Diagnostic/public snapshot fixtures match exactly.
- Eleven phase/stage snapshots retain selection, availability, receipt and rate
  provenance; the largest is 43,529 bytes, below 256 KiB. Unrelated evidence is
  omitted. Package layout, manifest, icon, license and DLL hash checks passed.
- PowerShell 7.6.5 and the installed SDK/game references ran under the
  authenticated desktop context. No interactive game, player save or new
  screenshot was used.

**Final verification / G4 (2026-09-16):**

- Candidate source: `e89f73e65570caf9599f5837377ed8056951fc52`;
  package/plugin `2.2.98`, assembly/file `2.2.98.0`, label `2.2.98.e89f73e`.
- `pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1 -Sequence 98` passed
  on that clean source with the same complete matrix and snapshot bounds above.
- [Hosted run 98](https://github.com/shytamir/dsp-beginner-guide/actions/runs/35029750217)
  succeeded: both builds had zero warnings/errors; portable suites, identity,
  variants, ZIP validation and artifact upload passed.
- Locally retained under `artifacts/guide3/candidate-e89f73e-98/`: both DLLs,
  public ZIP, exact source archive, generated version file, candidate.json, all
  local reports/fixtures, hosted report extracted from job 104585211792, and
  workshop procedure. Hosted compile-reference DLLs have their own hashes; the
  local candidate below is the one reserved for the workshop.

| Workshop artifact | SHA-256 |
|---|---|
| Diagnostic DLL | `70d81576533c0ad75e40951f7afb963e8f76a8fdaeece972d85fa959a7b0b26f` |
| Public DLL | `f2d46655643f426d8d48719314d960863e2ac6997d1bde1ea802f3efb35f0fdf` |
| Public ZIP | `5c232dc0554587f9a906dd0265c777140fa97e0a4db57aed8da04eb8f33cba5a` |

**Management closeout:** A documentation-only follow-up records these observed
post-push results and closes G4 without changing the pinned candidate source or
binaries. All twelve technical stories are complete. GC3-13/G5 remain pending
explicit owner validation; no game deployment, runtime acceptance, release,
tag or Thunderstore publication was performed.
