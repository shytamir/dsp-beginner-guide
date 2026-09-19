# Changelog

## Unreleased

### Guide 3.0 candidate

- GC3-13 closeout (2026-09-19): The owner completed the full playthrough on
  2.2.101 with no observable regressions and accepted ILS for this version.
  Removed DYSON's manual Hydrogen/science-delivery objective and its Pending
  reminder; this advice stays in the guide. The receiver objective now names
  Photon Generation explicitly. Receiver sampling and completion thresholds
  are unchanged. Analysis 3.13 and progression 3.10; snapshot schema stays 2.24.
  Final corrections are technically validated; publication is pending.

- GC3-13 owner checkpoint: Accepted performance at roughly a 7 FPS cost and
  confirmed ExpertMode works. Fixed Expert mode to use WHITE and display all six
  Cube counters, preserving normal-mode phase/stage preferences. The accepted
  candidate remains the full-playthrough build; this follow-up does not block it.

- GC3-13 B1 follow-up: The first correction did not resolve the reported slowdown.
  The inherited public-version slowdown is included in this correction.
  Removed discarded player/factory diagnostics, unused building counting,
  full storage-container exports and redundant station-stock aggregation from
  panel collection. Live WHITE/Antimatter stock uses native item counts; research
  reads native flags, and Dyson guidance uses native power without construction
  or launcher walks. Recipe reads respect cursors and query assemblers only for
  DYSON. Preserve consumed evidence, compact snapshots, prose and
  refresh cadence. The owner subsequently accepted the roughly 7 FPS impact.

- GC3-13 B1: Corrected the periodic refresh's ILS station lookup to reuse each
  station's own slots and observe receipts only in selected ILS Automation.
  Replaced the copied research-prerequisite table with native technology data;
  removed unused full-state diagnostic materialization from panel analysis and
  redundant availability reads. No new timing or diagnostic collection. The
  owner retest failed; the follow-up above supersedes this candidate.

- GC3-12: Prepared the 2.2.x candidate line with one PowerShell 7 command for
  both local variants, retained/story fixtures, bounded snapshot parity, identity
  and package checks. Hosted CI runs the portable suites for both DLLs. Clean
  candidates retain source, hashes, reports and the final owner workshop locally.
  Owner playthrough acceptance is recorded above; publication remains pending.

- GC3-11: Added default-off `[General] ExpertMode`, applied after restart. It
  creates only the Cube-rate bar and DON'T PANIC button, retaining selected-phase
  rates and guide anchors. All adjoining guidance controls and snapshot export
  callbacks are omitted/inert. Normal presentation and snapshot schema remain.

- GC3-10: PHOTON now evaluates six sustained WHITE inputs and 2,000 stationary
  Antimatter. Pending names the first current shortage; the five Cube colors
  use 40/min without changing other phases. Receiver construction belongs to
  DYSON. Analysis 3.12 and progression 3.9.

- GC3-09: Added bounded 120-game-second/20-sample readiness at 40/min for five
  colored Cubes and Antimatter, using existing native samples. Missing evidence
  breaks history; pause cannot fill it. Normalized 2.8 and snapshot 2.24 expose
  elapsed time, sample count, minimum rate and reason.

- GC3-08: DYSON includes the required receiver-to-Antimatter bridge, ordered
  research and originally an explicit Hydrogen/science-delivery player check
  (removed at owner closeout above). The bridge
  link follows swarm readiness. Snapshot 2.23 adds scoped conversion evidence;
  analysis 3.11 and progression 3.8 retain the existing receiver tolerance.

- GC3-07: YELLOW/PURPLE no longer require separate input stores. Three supplied
  Labs can pass without buffers; genuine input shortages remain visible.
  Missing Lab/item evidence stays unknown. Analysis 3.10, progression 3.7.

- GC3-06: ILS Pending projects at most three eligible stage-owned tasks. Research,
  assembly, deployment and receipt respect prerequisites; waiting evidence adds
  no repeat work. Analysis 3.9, panel 2.10 and snapshot 2.22 export candidates.

- GC3-05: Home delivery now requires finished source material, matching home/outpost
  policies, five home Vessels and subsequent home inputs for both materials.
  Session receipt flags survive quiet periods; diagnostics expose reset reasons
  and planet-level attribution limits. Snapshot 2.21, normalized 2.7, analysis 3.8,
  progression 3.6.

- GC3-04: Automation counts finished towers and Vessels at home or assigned to
  selected endpoints. Deployment/configuration is separate; consumed components
  no longer need replacing. Normalized state 2.6, analysis 3.7, progression 3.5
  and snapshot 2.20 include endpoint/count provenance.

- GC3-03: ILS research follows stage-owned branches and their real prerequisites.
  Queued research waits without repeat actions; Drive Engine Lv3 is no longer
  required. The first 200 Yellow Cubes remain a one-time guide batch reference.
  Normalized state 2.5, analysis 3.6, progression 3.4 and snapshot 2.19.

- GC3-02: Haulback distinguishes finished outpost production, Icarus cargo and
  cargo available at home. Unloading preserves the total; ore and remote stock
  cannot stand in for finished cargo aboard. Missing observations remain unknown.
  Normalized state 2.4, analysis 3.5, progression 3.3 and snapshot 2.18.

- GC3-01: ILS stages are selected explicitly and retained per playthrough.
  A one-time first-entry suggestion replaces repeated automatic stage changes;
  `DON'T PANIC` follows the selected stage. Selection 1.7, analysis 3.4,
  progression 3.2, panel 2.9 and snapshot 2.17 carry stage provenance.
  Automated checks pass in both variants; owner runtime acceptance is pending.

### 2.1 maintenance summary

- Changed: concise WHITE status, native panel typography, useful YELLOW and
  PURPLE terminal-input tracking, safer ILS stage evidence, exact-target Cube
  risk suppression, a 40/min Cube demand-reference ceiling, and two-sample
  PHOTON continuity tolerance.
- Not changed: the adopted guide authority, player-owned nine-phase path,
  public/diagnostic packaging split, intermediate branch modeling, RED
  coproduct diagnosis, or the rejected Drive Engine claim.

### Changed

- Adopted the published guide 2.3 edition as the current development authority
  without changing the retained nine-phase runtime contract. During ILS,
  `DON'T PANIC` now opens the published preparation, expedition, or automation
  stage that matches the checkpoint currently presented by the panel; other
  phases retain their phase anchor and unknown ILS evidence falls back to
  `#ils`.
- Accepted the `CUBE-DEMAND-CEILING-01` gate after release-owner in-game
  validation. Every Cube line now compares production with the lesser of
  actual consumption and the 40/min guide reference for demand-risk
  presentation. Actual rates and net buffer depletion remain uncapped, exact
  BLUE, RED, and WHITE goals retain authority, and non-Cube demand is
  unchanged. Archived the completed maintenance roadmap and returned the
  documented project state to maintenance mode with no active work.
- Accepted the `PHOTON-CONTINUITY-01` gate after deterministic checks proved
  the two-sample boundary in both builds and release-owner runtime validation
  proved sustained failure and natural recovery. A schema-2.16 snapshot also
  confirmed that recovered current receiver state remained blocked while six
  to eight unhealthy samples were retained. Reconciled the completed
  maintenance roadmap as a historical record and returned the project to
  maintenance mode with no active tasks.
- Implemented `PHOTON-CONTINUITY-01`: a ready 60-second receiver history now
  tolerates up to two unhealthy samples as telemetry noise, while the third
  revokes sustained continuity. Current Photon Generation configuration and
  lens presence remain immediate objective requirements. Diagnostics expose
  unhealthy and allowed sample counts. Advanced normalized state to 2.3 and
  compact snapshot schema to 2.16.
- Accepted the `CUBE-TARGET-RISK-01` runtime gate after ready RED diagnostics
  proved target-met demand suppression and below-target draining, and the
  public DLL reproduced the pertinent presentation states without regression.
  The shared exact-target mechanism and deterministic phase coverage made a
  separate WHITE runtime capture unnecessary for acceptance.
- Implemented `CUBE-TARGET-RISK-01`: BLUE, RED, and WHITE no longer present a
  demand-driven Cube risk while current production equals or exceeds their
  exact 20/min, 20/min, or 40/min phase goal. Diagnostics preserve the raw
  demand deficit and now identify target satisfaction separately. Rejected RED
  coproduct diagnosis as contrary to the adopted contract and disproportionate
  to attribute reliably. Advanced guide analysis to 3.3, production-risk
  output to 1.2, and compact snapshot schema to 2.15.
- Accepted the `CUBE-BRANCH-01` runtime gate, including the GREEN-style YELLOW
  and PURPLE terminal-input presentation and the ILS starter-planet exclusion,
  without a reported regression. Rejected the supposed RED-to-ILS Drive Engine
  II story because current code distinguishes the preparation Lv2 requirement
  from the later intentional Lv3 research-rush prerequisite; any recurrence
  now requires a same-state screenshot and diagnostic snapshot.
- Hotfixed ILS stage selection so production or storage on DSP's native birth
  planet cannot stand in for an interplanetary outpost. The birth-planet ID is
  collected defensively from `GameMain.galaxy.birthPlanetId`; unavailable
  evidence fails softly. Advanced normalized state to 2.2, guide analysis to
  3.2, progression to 3.1, and compact snapshot schema to 2.14.
- Implemented `CUBE-BRANCH-01` without new branch progression modeling:
  YELLOW now checks visible Diamond and Titanium Crystal storage, PURPLE checks
  Processors and Particle Broadband, and both reuse GREEN's combined objective
  plus independent terminal-item and Cube risks. Removed Carbon Nanotubes from
  PURPLE's selected-phase evidence set. Advanced guide analysis to 3.1,
  progression to 3.0, and compact snapshot schema to 2.13.
- Corrected the `NATIVE-TYPE-01` runtime gate failure: native capture no longer
  rejects a valid vein-label Text when its visible edge is material- or
  Shadow-driven rather than a Unity Outline. It now copies all attached Shadow
  and Outline mesh effects and performs one bounded lookup for a loaded
  `UIVeinDetailNode` when the serialized prefab Text is not ready. Advanced
  panel presentation to 2.8 and compact snapshot schema to 2.12.
- Implemented `NATIVE-TYPE-01`: panel headings, content, navigation, and Cube
  rates now reuse the installed game's live vein-label font, material, font
  style, line spacing, and outline settings. Lookup is a one-time read of
  `UIRoot.instance.uiGame.veinDetail.nodePrefab.infoText`; missing resources
  fail softly to embedded Basic Regular with one warning. `DON'T PANIC`
  retains its separate Comic Sans treatment. Advanced panel presentation to
  2.7 and compact snapshot schema to 2.11 for the combined WHITE/typography
  runtime gate.
- Implemented `WHITE-CONCISE-01`: WHITE now uses compact White-Cube wording,
  keeps configured-Lab and stored-Cube evidence without duplicating the rate
  rail, presents only one Mission Completed action, and reports authoritative
  queued, active-progress, or complete research state. Advanced normalized
  state to 2.1, progression to 2.9, panel presentation to 2.6, and compact
  snapshot schema to 2.10 for the focused runtime gate.
- Accepted `PANIC-01` in both runtime variants: the source-guide control is
  collision-free, collapse-proof, responsive to Cube-count changes,
  independent of diagnostic snapshot control, and clean in the plugin log.
- Declared the adopted guide 2.0 product complete and in maintenance mode.
  Moved completed roadmaps, migration stories, implementation audits, and
  feature-specific validation gates under `docs/archive/`; current docs now
  contain only maintained contracts and reusable operating procedures.
- Moved `DON'T PANIC` from the collapsible footer to the fixed Cube-rate rail,
  directly below and right-aligned with its last visible Cube. Public builds
  no longer reserve an otherwise empty footer, and panel presentation is 2.5.
- Replaced the single verbose production-risk conclusion with up to three
  stable compact condition rows and paired recommendations in a separate Next
  Actions section. Initial priority is starvation, trustworthy net-depletion
  time, then phase item order; same-severity newcomers cannot churn a full
  incumbent list.
- Added truthful accessible-stock/net-deficit depletion estimates for tracked
  objectives, advanced guide analysis to 3.0, panel presentation to 2.4, and
  compact snapshot schema to 2.9 for the RISK-05 runtime gate.
- Accepted the RISK-05 three-risk GREEN presentation, paired actions, critical
  promotion, recovery, reset, buffer-note, interaction, layout, performance,
  and log gate with no discovered regression. Four-candidate churn remains a
  non-blocking deterministic regression checkpoint because no natural
  four-end-product case was reproduced in game.
- Added a fixed, click-through native DSP signal glyph beside the Cube-rate
  rail for the analyzer-selected production risk. Draining and starved use
  distinct embedded signals; quiet states display no glyph. Advanced the
  panel presentation contract to 2.3.
- Accepted the RISK-04 quiet, draining, starved, collapsed-body, 4K,
  interaction, navigation, layout, performance, and log gate.
- Added deterministic, scope-matched production-risk scoring and
  interpretation for selected-phase items. Unknown, warming, backpressured,
  balanced, draining, and starved states remain distinct, while Current Status
  is capped at the strongest actionable conclusion.
- Retained per-factory ten-minute rates in normalized state so planet-local
  runway is never combined with a cluster-wide history baseline, and added
  deterministic coverage for startup, backpressure, pulsed output, noise,
  chronic and exact-target deficits, draining buffers, and starvation.
- Advanced normalized state to 2.0, guide analysis to 2.9, and compact
  snapshot schema to 2.8 for the RISK-03 runtime gate.
- Accepted the RISK-02 full, draining, empty, and mixed-storage runtime gate;
  remote-only exclusion remains a focused regression checkpoint if remote
  logic changes rather than a blocker for the guide's local component flows.
- Accepted the public `2.0.52` release gate: BepInEx loaded without the former
  compile-target warning, and BLUE, ILS, DYSON, and PHOTON screenshots passed
  the published-guide title-presentation checkpoint.
- Aligned the hosted compile reference with the declared Thunderstore
  dependency, `xiaoye97-BepInEx-5.4.17`, and added an assembly-reference
  version check to prevent the loader warning from returning.
- Matched all nine panel phase tags to the published guide's bracketed labels,
  exact colors, and phase icons, including ILS, DYSON, and PHOTON, and advanced
  the panel presentation contract to 2.2.
- Accepted the STORE-SNAPSHOT-01 public-build gate and advanced the release
  line from 1.18 to 2.0.
- Added distinct diagnostic and public build variants. The diagnostic DLL
  retains the forensic snapshot control; the public Thunderstore DLL omits
  the control and its interaction path at compile time.
- Updated hosted packaging to test both variant markers and package only the
  exact validated public DLL.
- Accepted the GUIDE2-03 navigation, persistence, removed-contract, and focused
  BLUE/ILS/WHITE snapshot gate, completing the guide 2.0 migration.
- Accepted the VIS2-01 Matrix-icon presentation gate on bright and dark
  backgrounds with one, three, and six visible Cubes, including interaction
  and refresh checks.
- Removed orphaned optional-route presentation translations and obsolete
  finding suppression from the current nine-phase runtime path.
- Replaced the superseded guide-1.22.2 runtime protocol with the guide 2.0
  objectives, schema 2.7 export contract, Matrix icons, and final migration
  gate.
- Replaced the Cube-rate column's flat color tiles with six cached, embedded
  Matrix icons while retaining outlined threshold-colored rate text and a
  text-only soft fallback.
- Advanced the panel presentation contract to 2.1 for the VIS2-01 icon column.
- Accepted the GUIDE2-02 nine-phase runtime gate, including focused RED,
  PURPLE, GREEN, DYSON, PHOTON, and WHITE checkpoints.
- Corrected the panel-facing DYSON title to `Build the Photon swarm` and added
  deterministic coverage so analysis and progression cannot diverge again.
- Realigned the nine retained phase contracts with guide 2.0: RED now treats
  refinery congestion as status rather than a hard gate, DYSON uses the
  Photon-swarm contract, and PHOTON explicitly requires four sustained lensed
  receivers.
- Added one exceptional draining-input conclusion for PURPLE, GREEN, and
  WHITE while keeping healthy supporting chains out of the panel.
- Advanced guide analysis and progression to 2.8 and compact snapshot schema
  to 2.7 for the GUIDE2-02 contract.
- Consolidated the former BOOTSTRAP phase into BLUE, making BLUE the first of
  nine player-selected phases and normalizing stored BOOTSTRAP selections.
- Merged compact starter-input and routine-hardware readiness into BLUE without
  adding a fixed power objective or listing every healthy mall product.
- Advanced guide selection to 1.6, guide analysis and progression to 2.7, and
  compact snapshot schema to 2.6 for the guide 2.0 phase contract.
- Added conservative per-planet buffer evidence from item-configured logistics
  slots set to local Supply. Remote-only, non-supply, and unproven tank
  aggregates are excluded and identified in compact diagnostics.
- Added runway against planet-local native one-minute demand and tri-state
  backpressure evidence without changing player-facing risk interpretation.
- Advanced normalized state to 1.9 and compact snapshot schema to 2.5 for the
  RISK-02 runtime gate.
- Added independent native one-minute and ten-minute production and
  consumption evidence for the bounded watch set. Ten-minute totals are
  normalized to items per minute, carry explicit per-item observation-age
  readiness, and remain distinct from unavailable or legitimate zero values.
- Advanced normalized state to 1.8 and compact snapshot schema to 2.4 with
  focused multi-window provenance and selected-phase diagnostic evidence.
- Reworked ILS into compact preparation, expedition, and research-rush
  checkpoints backed by carried inventory and planet-local cargo evidence.
- Replaced DYSON's premature Antimatter objectives with Solar Sail production,
  launch, population, and swarm-generation status.
- Replaced PHOTON's manual pace check with actual Critical Photon and
  Antimatter rates, the 2,000-Antimatter midpoint, and receiver demand versus
  available Dyson generation.
- Removed fixed factory-power rows from every phase and removed WHITE's
  redundant six-input objective while adding its stored White Cube count.
- Advanced the normalized state contract to 1.7 and guide analysis/progression
  contracts to 2.6 and snapshot schema to 2.3, including focused ILS
  player-inventory, planet-local cargo, and protected-reserve evidence.
- Added the CUBE-01 click-through Cube-rate column. It grows from Blue through
  White with the selected phase, reuses native one-minute production rates,
  and presents per-minute rates with phase-aware threshold colors.
- Advanced the panel presentation contract to 2.0; phase objectives,
  navigation, telemetry collection, and snapshot schema are unchanged.
- Adopted the embedded Basic Regular font for panel presentation while
  preserving every established size, spacing, color, outline, and control
  behavior; `DON'T PANIC` remains Comic Sans.
- Added the Basic font's SIL Open Font License notice to public packages.
- Accepted Basic Regular rendering and the guide 1.22.2 critical-path build
  through an extensive representative user test with no reported defects.
- Adopted the guide 1.22.2 critical path as the active ten-phase contract.
- Consolidated FLIGHT and TITANIUM into one ILS expedition checklist and
  aligned every retained objective inventory with its phase-local readiness
  checks.
- Reduced hard production-rate objectives to Blue 20/min, Red 20/min, and
  White 40/min; late Dyson and receiver metrics remain diagnostic evidence.
- Removed WARP, SPHERE, LOGISTICS, and other optional-route panels, controls,
  findings, and compact snapshot phase contracts.
- Added compact ILS station and deployed-vessel evidence and advanced the
  snapshot schema to 2.2.
- Renamed the public build output to `DspGuideCheck.dll` without changing the
  source namespace or runtime behavior.
- Added a Thunderstore manifest template, portable package README, 256 by 256
  icon contract, exact BepInEx install layout, and package validator.
- Changed hosted packaging to publish an installable `DSPGuideCheck-M.m.N.zip`
  plus diagnostic reports.
- Reused the same three-number `M.m.N` version for Thunderstore and BepInEx
  while retaining `M.m.N.0` for assembly/file metadata and the commit hash in
  diagnostics.

## 1.18.3 - 2026-07-30

### Fixed

- Strengthened the panel text outline so it remains visible against bright
  terrain and clouds.
- Restored visible selected-route emphasis for the preserved DYSON/SPHERE
  choice on GREEN.
- Applied bounded hover growth to collapse, scroll, snapshot, and guide-link
  controls.
- Removed the remaining filled backgrounds from the scroll controls.
- Changed the active LOGISTICS phase label to bright green.

### Historical validation request

- Confirm outline visibility, selected-route emphasis, transparent controls,
  hover behavior, and the LOGISTICS color in DSP.

## 1.18.2 - 2026-07-30

### Fixed

- Kept the Next phase control available in WHITE so the player can enter
  LOGISTICS manually. Next is now hidden only in LOGISTICS.

### Historical validation request

- Confirm WHITE shows Next, Next selects LOGISTICS, and LOGISTICS hides Next
  in DSP.

## 1.18.1 - 2026-07-30

### Changed

- Added a dark outline to panel text for visibility over bright backgrounds.
- Replaced phase-navigation backgrounds with transparent hit areas and
  bounded, non-compounding hover growth.
- Moved selected DYSON/SPHERE emphasis from the control background to its
  text outline.
- Made all non-interactive panel surfaces click-through.

### Historical validation request

- Confirm text clarity, hover behavior, selected-route emphasis, and
  click-through input in DSP.

## 1.18.0 - 2026-07-30

### Changed

- Adopted published guide version 1.1 as the GUIDE-01 authority contract.
- Replaced every selected-phase objective inventory with the corresponding
  readiness checklist.
- Kept exact paces, comfort references, optional paths, warnings, and
  unprovable player checks distinct.
- Removed WARP completion criteria and retained it as a player-selected
  reference route.
- Re-derived DYSON, SPHERE, PHOTON, and WHITE objectives from the revised
  late-game route contracts.
- Added the manually selected post-completion LOGISTICS phase and focused
  logistics evidence.
- Updated guide titles, terminology, evidence watch lists, compact snapshot
  selection, and documentation for the revised phase inventory.

### Historical validation request

- Exercise representative early, middle, optional-route, late, WHITE, and
  LOGISTICS saves with the v1.18 runtime protocol.

## 1.17.2 - 2026-07-30

### Changed

- Replaced the `Save snapshot` Explorer launch with two-second footer feedback:
  green when the file write succeeds and red when it fails.

## 1.17.1 - 2026-07-30

### Fixed

- Resolved watched Statistics Panel items through DSP's native
  `productIndices` map before reading the compact `productPool`.
- Reported active production factories instead of allocated pool capacity.
- Retained a concise production-collector failure reason in normalized and
  compact snapshot diagnostics.

## 1.17.0 - 2026-07-30

### Changed

- Replaced production-rate reconstruction from lifetime counters with DSP's
  native one-minute Statistics Panel aggregates.
- Limited production collection to the guide-relevant watch set and retained
  factory-scoped evidence only for route checks that require it.
- Kept lifetime counters separate and limited them to lifetime Cube totals.
- Replaced reconstructed Dyson topology with native generation, sail,
  structure and cell aggregates.
- Kept launch-device and Ray Receiver telemetry on their dedicated component
  pools.
- Added production and Dyson source, scope, period and coverage provenance to
  normalized state 1.5 and snapshot schema 2.1.

### Historical validation request

- Compare saved evidence with the in-game one-minute, entire-cluster
  Statistics Panel and Dyson editor/detail surfaces.
- Recheck SPHERE and PHOTON behavior, manual phase ownership, snapshot
  compactness and sampling performance during the same runtime pass.

### Fixed

- Kept the generated semantic build version out of the BepInEx plugin
  attribute, which only accepts a numeric `System.Version`.
- Added an artifact test that rejects invalid generated BepInEx versions before
  upload.

## 1.16.0 - 2026-07-30

### Changed

- Replaced the million-byte forensic dump with snapshot schema 2.0: a compact,
  bounded diagnostic contract containing conclusions and the evidence used by
  implemented guide functions.
- Removed repeated factory, player, technology, inventory, station, telemetry,
  normalized-state, analysis and panel structures from saved JSON.
- Added authoritative total playtime, research totals, lifetime Cube totals,
  current Cube stock and rolling Cube rates.
- Added selected-phase item and logistics evidence, a worst-planet power
  summary, and focused DYSON, SPHERE and PHOTON evidence.
- Added compact collector coverage and optional performance timing diagnostics,
  explicit omission markers, receiver-detail truncation and a 256 KiB export
  limit.
- Synchronized plugin, assembly, exporter and schema provenance in subsequent
  snapshots.

## 1.15.1 - 2026-07-30

### Fixed

- Prevented autosaves, renamed save slots and restarts from rebinding phase
  selection to a newly seeded phase.
- Added one-time migration from the currently loaded legacy phase key.
- Added compact phase-persistence provenance to snapshot selection diagnostics.

### Documentation

- Added the snapshot-redesign worklist that subsequently produced the compact
  diagnostic contract. The completed record now resides under `docs/archive/`.

## 1.15.0 - 2026-07-27

First public repository release.

### Added

- Player-controlled phase and optional-route navigation.
- Stable objectives and concise Current Status for all main phases.
- Dedicated SPHERE guidance.
- PHOTON receiver configuration, production and 60-second continuity evidence.
- On-demand JSON snapshots.
- Collapsible, scrollable panel with footer actions.
- Two-line Comic Sans `DON'T PANIC` source-guide control.

### Changed

- Removed automatic phase transitions and the redundant COMPLETE phase.
- Made SPHERE construction activity, rather than a nominal rocket rate, the
  meaningful objective.
- Reduced receiver sampling cost by using the dedicated gamma-generator pool
  instead of scanning every factory entity.

### Known limitation

- A faint periodic hitch can remain perceptible in a large late-game save,
  although the receiver pass was reduced from 67-139 ms to approximately
  1.5-1.7 ms in captured validation frames.
