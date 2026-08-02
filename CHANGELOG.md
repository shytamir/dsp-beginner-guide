# Changelog

## Unreleased

### Changed

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

### Validation pending

- Confirm outline visibility, selected-route emphasis, transparent controls,
  hover behavior, and the LOGISTICS color in DSP.

## 1.18.2 - 2026-07-30

### Fixed

- Kept the Next phase control available in WHITE so the player can enter
  LOGISTICS manually. Next is now hidden only in LOGISTICS.

### Validation pending

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

### Validation pending

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

### Validation pending

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

### Validation pending

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
- Synchronized plugin, assembly, exporter and schema provenance in every
  future snapshot.

## 1.15.1 - 2026-07-30

### Fixed

- Prevented autosaves, renamed save slots and restarts from rebinding phase
  selection to a newly seeded phase.
- Added one-time migration from the currently loaded legacy phase key.
- Added compact phase-persistence provenance to snapshot selection diagnostics.

### Planned

- Added `docs/SNAPSHOT-REDESIGN.md` as the high-level implementation worklist
  for replacing the oversized forensic snapshot with a compact diagnostic
  contract.

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
