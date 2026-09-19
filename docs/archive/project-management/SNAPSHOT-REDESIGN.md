# Completed snapshot redesign

Implemented in v1.16.0, narrowed to the critical-path-only contract for guide
v1.22.2, extended by schema 2.4 with compact native one-minute and ten-minute
production evidence, extended by schema 2.8 with the selected production-risk
state and score terms, and extended by schema 2.9 with the bounded ordered
actionable-risk list and trustworthy net-depletion estimate.

- Defined a compact snapshot contract around diagnostic conclusions and the exact evidence used to reach them.
- Removed duplicate normalized state from guide analysis and retained one authoritative normalized summary.
- Replaced full factory, technology, inventory, station-slot and all-item telemetry dumps with targeted aggregates.
- Exported the selected critical-path phase, stable playthrough identity version, persistence result and player-selection provenance.
- Exported objective and Current Status conclusions with their evidence availability, measurement window and supporting values.
- Exported phase-specific evidence only for the selected phase and implemented functions.
- Added aggregate research totals, total playtime, lifetime Cube production and consumption by color, current Cube stock and relevant rolling rates.
- Corrected total-playtime collection to use the authoritative static game tick.
- Retained compact collector health, sampling cadence, coverage and performance diagnostics so missing evidence remained visible.
- Defined focused summaries for production, ILS logistics, power, Dyson construction and PHOTON receiver continuity.
- Removed player/mecha detail and broad reflection diagnostics unless an implemented conclusion directly depended on them.
- Added explicit truncation and omission markers where detailed evidence was intentionally summarized.
- Bumped the snapshot schema when the compact contract replaced the earlier forensic structure.
- Synchronized plugin, assembly, exporter and schema provenance in every snapshot.
- Added deterministic contract checks for duplication, required aggregates, bounded detail and total export size.
- Updated README, project documentation, changelog and runtime testing status when the new contract was implemented.
