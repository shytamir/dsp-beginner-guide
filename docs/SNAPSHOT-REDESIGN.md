# Snapshot redesign worklist

- Define a compact snapshot contract around diagnostic conclusions and the exact evidence used to reach them.
- Remove duplicate normalized state from guide analysis and retain one authoritative normalized summary.
- Replace full factory, technology, inventory, station-slot and all-item telemetry dumps with targeted aggregates.
- Export the selected phase, route, stable playthrough identity version, persistence result and player-selection provenance.
- Export objective and Current Status conclusions with their evidence availability, measurement window and supporting values.
- Export phase-specific evidence only for the selected phase and implemented functions.
- Add aggregate research totals, total playtime, lifetime Cube production and consumption by color, current Cube stock and relevant rolling rates.
- Correct total-playtime collection to use the authoritative static game tick or game time.
- Retain compact collector health, sampling cadence, coverage and performance diagnostics so missing evidence remains visible.
- Define focused summaries for production, logistics, power, Dyson construction, SPHERE and PHOTON receiver continuity.
- Remove player/mecha detail and broad reflection diagnostics unless an implemented conclusion directly depends on them.
- Add explicit truncation and omission markers where detailed evidence is intentionally summarized.
- Bump the snapshot schema when the compact contract replaces the current forensic structure.
- Synchronize plugin, assembly, exporter and schema provenance in every snapshot.
- Add deterministic contract checks for duplication, required aggregates, bounded section sizes and phase-specific evidence.
- Update README, project documentation, changelog and runtime testing instructions when the new contract is implemented.
