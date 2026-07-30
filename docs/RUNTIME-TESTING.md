# DSP Guide Check 1.17 - Runtime Validation

This protocol uses only Dyson Sphere Program with the mod installed through
BepInEx. It requires no coding task or other software.

TEL-01 changes the evidence beneath existing guide behavior. The test therefore
checks the native telemetry alignment and the accepted navigation, analysis,
snapshot, SPHERE, and PHOTON contracts together.

## Preparation

1. Install the v1.17 DLL in the usual BepInEx plugin location.
2. Load a save with active production and let it run for at least one minute.
3. Keep the Statistics Panel set to `1 minute` and `Entire star cluster` for
   production comparisons.
4. Use `Save snapshot` only at the checkpoints below.

## Checkpoint A - Native production aggregates

Choose a phase with a Cube and at least two relevant intermediates visible in
the phase evidence, such as PURPLE, GREEN, PHOTON, or WHITE.

1. Open the Statistics Panel's Production page.
2. Record or screenshot the one-minute production and consumption rates for
   the phase Cube and at least two relevant intermediates.
3. Without changing the period or scope, click `Save snapshot`.

Expected:

- snapshot production provenance identifies the native one-minute Statistics
  Panel source and `entire-star-cluster` scope;
- the compared production and consumption rates match the Statistics Panel;
- lifetime production totals appear only for the six Cubes;
- selected-phase analysis and Current Status remain sensible;
- an idle but buffered older Cube does not become a false danger merely
  because its current rate is zero.

## Checkpoint B - Native Dyson aggregates

Use the SPHERE playthrough if it still contains a partial or developed sphere.

1. Select SPHERE manually.
2. Open the Dyson editor or statistics surface.
3. Record or screenshot total generation, swarm sail population, constructed
   and planned structure points, and constructed and planned cell points.
   Several screenshots are fine if the native UI cannot show them together.
4. Click `Save snapshot`.

Expected:

- snapshot Dyson provenance names the native Dyson statistics/editor
  aggregates;
- generation and sail population match the native UI;
- structure and cell progress match the native editor totals;
- aggregate-node coverage is present and reports no missing nodes;
- SPHERE recognizes established construction without requiring the 5/min
  reference rocket pace;
- no shell, frame, or raw topology dump is present.

If a swarm-only save is readily available, one additional snapshot is useful
to confirm zero permanent construction does not make Dyson evidence
unavailable.

## Checkpoint C - PHOTON continuity

Use a save with configured Photon Generation receivers and select PHOTON
manually.

Expected:

- receiver deployment, mode, lenses, strength, requested and supplied power,
  Critical Photon output, and the accepted 60-second continuity behavior are
  unchanged;
- a mode or lens interruption still breaks continuity and recovery still
  requires a healthy window;
- production evidence for Critical Photons and Antimatter uses the native
  one-minute source.

Save one PHOTON snapshot from the most informative continuity state. A
before/after pair is welcome if continuity changes during the test.

## Cross-contract regression check

During the checkpoints, also confirm:

- F8 never saves a snapshot;
- phase and route selections change only through player controls;
- the selected phase does not change while paused, after research, or after
  completing an objective;
- navigation, collapse/expand, scrolling, footer controls, and panel layout
  behave normally;
- `Save snapshot` writes one JSON and opens its directory;
- `DON'T PANIC` opens the selected source-guide phase;
- no noticeable new five-second hitch occurs with the panel closed or open;
- each saved JSON is no larger than 256 KiB.

## Snapshot contract checks

The returned files will be checked for:

- exporter 1.17 and snapshot schema 2.1;
- normalized state 1.5 provenance reflected in collector evidence;
- production source, scope, period, sample count, bounded watch-list coverage,
  and only selected-phase item evidence;
- Cube lifetime totals remaining separate from native one-minute rates;
- native Dyson aggregate source and node coverage;
- focused SPHERE or PHOTON evidence where applicable;
- unchanged guide-selection persistence provenance;
- compact research, Cube, power, logistics, objectives, and Current Status;
- no raw factory/entity model, broad all-item history, duplicate normalized
  state, or shell/frame topology;
- collector timings consistent with no new periodic hitch.

## Testing handoff

Please return:

- the Checkpoint A snapshot and Statistics Panel screenshot(s);
- the Checkpoint B snapshot and Dyson UI screenshot(s);
- the most informative Checkpoint C snapshot, or a before/after pair;
- whether any rate or Dyson total disagreed with the native UI;
- whether navigation, persistence, SPHERE, PHOTON, layout, footer, or
  performance behavior regressed;
- any player-facing conclusion that did not fit the visible game state.

The comparison is intentionally broader than TEL-01 alone: the snapshots will
be reviewed for collection accuracy, normalized provenance, compactness,
analysis congruence, and preservation of all accepted product contracts.
