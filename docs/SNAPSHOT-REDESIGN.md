# Snapshot redesign worklist

Implemented in v1.16.0 and narrowed to the critical-path-only schema 2.2 for
guide v1.22.2.

- [x] Define a compact snapshot contract around diagnostic conclusions and the exact evidence used to reach them.
- [x] Remove duplicate normalized state from guide analysis and retain one authoritative normalized summary.
- [x] Replace full factory, technology, inventory, station-slot and all-item telemetry dumps with targeted aggregates.
- [x] Export the selected critical-path phase, stable playthrough identity version, persistence result and player-selection provenance.
- [x] Export objective and Current Status conclusions with their evidence availability, measurement window and supporting values.
- [x] Export phase-specific evidence only for the selected phase and implemented functions.
- [x] Add aggregate research totals, total playtime, lifetime Cube production and consumption by color, current Cube stock and relevant rolling rates.
- [x] Correct total-playtime collection to use the authoritative static game tick.
- [x] Retain compact collector health, sampling cadence, coverage and performance diagnostics so missing evidence remains visible.
- [x] Define focused summaries for production, ILS logistics, power, Dyson construction and PHOTON receiver continuity.
- [x] Remove player/mecha detail and broad reflection diagnostics unless an implemented conclusion directly depends on them.
- [x] Add explicit truncation and omission markers where detailed evidence is intentionally summarized.
- [x] Bump the snapshot schema when the compact contract replaces the current forensic structure.
- [x] Synchronize plugin, assembly, exporter and schema provenance in every snapshot.
- [x] Add deterministic contract checks for duplication, required aggregates, bounded detail and total export size.
- [x] Update README, project documentation, changelog and runtime testing status when the new contract is implemented.
