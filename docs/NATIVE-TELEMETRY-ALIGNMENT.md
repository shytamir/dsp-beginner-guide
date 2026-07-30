# TEL-01 - Native aggregate telemetry alignment

## Status

- Implemented in 1.17.0; compact-pool lookup corrected in 1.17.1 and runtime
  comparison pending.
- This is a separate runtime-evidence pass so production and Dyson semantics
  can be corrected and compared with the game UI as one coherent change.
- The audit used the installed DSP `Assembly-CSharp.dll` read-only.

## Implemented contract

- Production resolves each watched item through
  `FactoryProductionStat.productIndices[itemId]`, then reads
  `ProductStat.total[1]` and `total[8]` from the compact `productPool`.
- Entire-cluster values are the sum of the native one-minute factory
  aggregates. Planet-factory values are retained only for Titanium and Silicon
  route checks.
- Lifetime reads use `total[6]` and `total[13]` only for the six Cubes.
- A bounded sample history records continuity of the native one-minute windows;
  no rate is derived from lifetime-counter deltas.
- Dyson generation uses the exact `DysonSphere` aggregate fields, sail
  population uses `DysonSwarm.sailCount`, and construction progress sums the
  editor-facing `DysonNode.totalSp`, `totalSpMax`, `totalCp`, and `totalCpMax`
  getters.
- Construction-change rates are derived only from successive bounded samples
  of those native aggregate totals.
- Ejector and silo discovery now reads their dedicated component pools.
  Receiver continuity retains its accepted dedicated generator-pool sampler.
- Schema 2.1 records source, scope, period, watch-list coverage, and Dyson
  aggregate-node coverage without exporting broad item or topology maps.

## Original production mismatch

- The native Statistics Panel selects a period index as
  `timeLevel + 1` for production and adds `7` for consumption.
- Its one-minute view therefore consumes the game's ready-made
  `ProductStat.total[1]` production and `ProductStat.total[8]` consumption
  aggregates, summed across the factories in the selected UI scope.
- The prior collector read lifetime totals at indices `6` and `13`, retained
  broad per-factory item maps, and derived rates from successive samples.

## Original Dyson mismatch

- The native Dyson statistics/detail surface uses `DysonSwarm.sailCount`,
  `DysonSphere.energyGenCurrentTick`, layer counts, and the node aggregate
  getters `totalSp`, `totalSpMax`, `totalCp`, and `totalCpMax`.
- The prior collector separately walked layers, shells, nodes, and frames to
  reconstruct construction detail and then sampled those reconstructed totals
  for change rates.
- Launch-device state and PHOTON receiver continuity remain separate evidence;
  they are not replaced by Dyson-editor aggregates.

## Completed implementation work

- Replace lifetime-delta production rates with the same pre-aggregated
  one-minute production and consumption values used by the Statistics Panel.
- Match the panel's entire-cluster aggregation scope for guide analysis, with
  a deliberate planet scope only where a guide conclusion actually needs it.
- Collect only the item identifiers used by the selected-phase analysis,
  compact snapshot totals, and bounded continuity checks.
- Keep lifetime Cube totals separate from rolling rates and read only the
  required Cube identifiers.
- Retain a bounded history of native aggregate values only where an accepted
  continuity conclusion needs history.
- Replace hand-reconstructed Dyson construction totals with the aggregate
  values used by the Dyson statistics/detail surface.
- Use the game's aggregate sail count and generation values directly.
- If a construction-change rate remains useful, derive it only from successive
  bounded samples of the native aggregate construction totals.
- Preserve dedicated ejector, silo, receiver, and continuity collectors only
  for facts the aggregate Dyson surface does not provide.
- Remove obsolete broad maps, topology duplication, source notes, and snapshot
  fields after all consumers move to the new normalized evidence.
- Update collector provenance, normalized-state and snapshot contracts
  together.
- Fail softly and mark evidence unavailable when the expected aggregate member
  is absent; do not silently fall back to a different semantic.

## Acceptance

- A one-minute entire-cluster production checkpoint matches the values shown
  by the native Statistics Panel for representative Cubes and intermediates.
- Production conclusions no longer depend on lifetime-counter deltas.
- Sail population, generation, structure progress, and cell progress match the
  native Dyson statistics/editor values for swarm-only, partial-sphere, and
  developed-sphere saves.
- No full all-item history or duplicate shell/frame reconstruction is retained
  when an aggregate source is available.
- Existing manual navigation, SPHERE selection, PHOTON receiver continuity,
  snapshots, and panel behavior do not regress.
- Sampling duration and stationary gameplay show no new periodic hitch.
- A saved compact snapshot identifies the native source, scope, window,
  coverage, and only the evidence used by implemented conclusions.
