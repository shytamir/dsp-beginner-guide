# Native Telemetry Reference

This document records the maintained runtime evidence contract. Its original
derivation and acceptance record is archived at
[`docs/archive/technical/NATIVE-TELEMETRY-ALIGNMENT-DERIVATION.md`](archive/technical/NATIVE-TELEMETRY-ALIGNMENT-DERIVATION.md).

DSP Guide Check implements Guide 3.0 using the evidence policies below.

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

## ILS cargo scope and availability

The normalized state retains Icarus package counts separately from stationary
planet stock. The existing storage/grid, tank and station-pool collectors report
whether their input collections and relevant fields were available; a valid
empty collection differs from a missing collection. No extra inventory scan
or delta-based production estimator is introduced.

Stage II uses native one-minute production of finished items 1106/1105 only.
Prefer a known current non-birth planet; otherwise use the lowest-ID outpost
with finished production or known stock. Birth identity must be known. At home,
add package and birth-planet stationary counts once, against 860/520. This proves
current availability, not past hauling, fuel sufficiency or a protected reserve.
Raw location-field presence distinguishes a known space location (zero) from
missing evidence. Research rows retain per-ID availability of their unlocked
result, so missing reflection results cannot become locked technology claims.
Queue availability comes from the existing history queue collection. The
ILS-specific policy consumes these normalized observations and the native
`LDB.techs.Select(id)` definitions for its targets and their prerequisite closure.
The collector retains `PreTechs` and `PreTechsImplicit` arrays once and reads
the native name/Level for ranked advice; absent definitions stay unknown.
There is no production copy of the guide's prerequisite table or queue mutation.
Snapshots include only the selected stage's prerequisite closure and survey
advice where relevant, with availability, queued, unlocked and started flags.

Finished transport-package evidence reuses the normalized station list and
slots. It counts at most one selected home tower and one source tower, plus
their idle/working Vessel counts; inventory-held 2104/5002 is counted separately
at home. Pool and selected-station field availability remain explicit. Missing
fleet or policy fields cannot be treated as an observed zero/configuration.
Compact snapshots replace the old protected-reserve proxy with the counted
inventory/deployment sources and selected endpoint identities.

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

### ILS home receipt

The existing traffic sample exports the latest native finished-item input totals,
availability, game tick and reset epoch. A two-item session tracker establishes
its baseline after observing configured endpoints. It uses no ship polling or
new collection cadence. Native internal traffic and other destinations are
excluded. Missing data clears receipt flags; quiet valid samples retain them.
Source stock or production corroborates supply; source power is not required.
Observation is restricted to selected ILS Automation and clears its baseline
when leaving that stage. Native station-slot ownership is preserved
through normalization, so endpoint policy and source-stock checks inspect only
the owning station's slots. Route caches are not substituted for the existing
configuration objective, and no native route rebuild is invoked.

### DYSON receiver bridge

Reuse receiver continuity (including two unhealthy samples), recipe 74 and
available native Photon production/consumption plus Antimatter production.
Stationary Antimatter sums observed cluster storage and station inventories,
excluding Icarus. These aggregates do not establish conveyor connectivity,
Hydrogen disposal or science-district delivery. That guidance stays in the
source guide and does not create a panel objective.
Missing assembler/lab pools make recipe collection unavailable.

### PHOTON sustained inputs

Six existing native aggregate samples (6001-6005, 1122) feed a pure bounded
policy at the existing cadence. Keep the sample on/before 120 game seconds and
at most 26 points. At least 20 distinct ticks and no rate below 40/min are
required. Missing samples clear that item; game replacement clears all six.
This consumer does not alter ten-minute risk warmup or infer per-second flow.
