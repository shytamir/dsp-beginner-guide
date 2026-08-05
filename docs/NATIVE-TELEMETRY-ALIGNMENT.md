# Native Telemetry Reference

This document records the maintained runtime evidence contract. Its original
derivation and acceptance record is archived at
[`docs/archive/technical/NATIVE-TELEMETRY-ALIGNMENT-DERIVATION.md`](archive/technical/NATIVE-TELEMETRY-ALIGNMENT-DERIVATION.md).

## Production statistics

- Resolve watched items through
  `FactoryProductionStat.productIndices[itemId]` into the compact
  `productPool`.
- Read `ProductStat.total[1]` and `total[8]` for native one-minute production
  and consumption.
- Read `total[2]` and `total[9]` for ten-minute production and consumption,
  dividing by ten once in the collector to normalize both to items/minute.
- Sum factory aggregates for entire-cluster conclusions. Retain planet-local
  values only for guide functions that explicitly require matched local scope.
- Mark ten-minute history ready only after the watched item has remained
  observable for 600 game seconds in the current mod session. A native zero
  remains a real zero and is not a readiness proxy.
- Use `total[6]` and `total[13]` only for lifetime totals of the six Cubes.
- Retain bounded one-minute samples for continuity; never derive production
  rates from inventory or lifetime-counter deltas.

## Dyson and component evidence

- Generation uses `DysonSphere.energyGenCurrentTick`.
- Sail population uses `DysonSwarm.sailCount`.
- Structure and cell progress sum the editor-facing `DysonNode.totalSp`,
  `totalSpMax`, `totalCp`, and `totalCpMax` getters.
- Construction-change rates use successive bounded samples of those native
  aggregate totals.
- Ejectors and silos use their dedicated component pools.
- Ray Receiver configuration and continuity use the dedicated generator pool.

## Collection boundaries

- Collect only item identifiers used by selected-phase analysis, compact Cube
  totals, and bounded continuity checks.
- Preserve source, scope, period, readiness, and coverage provenance.
- Do not retain full all-item history, broad topology maps, or duplicate Dyson
  reconstruction when a native aggregate exists.
- If an expected member is unavailable, mark the evidence unknown and fail
  softly; do not substitute a semantically different proxy.
