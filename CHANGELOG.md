# Changelog

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
