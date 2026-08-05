# DSP Guide Check - Maintenance Regression Protocol

This is the reusable runtime protocol for the maintained product. Historical
feature gates and their accepted evidence are archived under
[`docs/archive/validation/`](archive/validation/).

Use only Dyson Sphere Program with the current mod installed through BepInEx.
Use the diagnostic DLL when a JSON snapshot is requested; use the public DLL
when validating the Thunderstore surface.

## Preparation

1. Install the DLL variant being tested.
2. Load a representative save and allow at least one minute of production
   history before judging normal rates. Allow ten minutes when testing
   production-risk history explicitly.
3. Open the panel with F8 and change phases only with Previous and Next.
4. Capture only the screenshots and diagnostic snapshots required by the
   change under test.

## Core regression

- F8 opens and closes the panel without saving or changing game state.
- Navigation follows `BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN ->
  DYSON -> PHOTON -> WHITE` and stops at both endpoints.
- Runtime evidence, pausing, research, objectives, and Mission Completed never
  change the selected phase.
- Selection persists after a normal restart of the same playthrough.
- Removed and optional phases never appear as navigation destinations,
  objectives, findings, or snapshot phase contracts.
- Objectives remain stable while the selected phase is unchanged.
- Missing runtime evidence fails softly without a plugin exception.

## Panel regression

- Titles use the published-guide icon, bracketed phase tag, color, and readable
  wrapping.
- The body scrolls when needed and remains click-through outside explicit
  controls.
- The Cube-rate rail remains visible and click-through while collapsed.
- The risk glyph is absent when quiet and uses distinct native draining and
  starved signals when actionable.
- `DON'T PANIC` sits directly below the last visible Cube with their right
  edges aligned, follows Cube-count changes, remains visible and interactive
  while collapsed, and opens the selected guide anchor.
- On a short panel, `DON'T PANIC` does not collide with Cube rates or body
  content.
- On a full six-Cube panel, it follows the White Cube without clipping.
- The diagnostic `Save snapshot` control remains independent and usable.
- The public DLL has no snapshot control, dead interaction, or empty footer.
- F8, collapse, navigation, scrolling, and controls do not introduce a visible
  hitch or new log error.

## Phase spot checks

- BLUE: starter inputs and routine hardware replenish; Blue Cubes sustain
  20/min; research is not hand-fed.
- RED: two configured Labs sustain 20 Red Cubes/min; oil congestion is status,
  not another objective.
- ILS: exactly one preparation, expedition, or research-rush checkpoint is
  active.
- YELLOW and PURPLE: three configured Cube Labs run continuously.
- GREEN: two configured Cube Labs run continuously and Quantum Chip and
  Graviton Lens storage is visible.
- DYSON: Solar Sails are produced and launched and the swarm generates power.
- PHOTON: lensed receiver supply, Critical Photon and Antimatter production,
  the 48/min reference, and the 2,000-Antimatter midpoint remain distinct.
- WHITE: Universe Matrix research, ten Labs at 40/min, stored White Cubes, and
  Mission Completed remain the stable contract.

## Production-risk spot check

Use a mature GREEN or WHITE line with ready ten-minute history:

1. Confirm a healthy or backpressured line adds no actionable row or glyph.
2. Reduce production below continuing consumption while accessible stock
   remains. Confirm a compact `draining` row, paired Next Action, and draining
   glyph.
3. Exhaust accessible stock while demand remains established. Confirm a
   compact `starved` row, paired restart action, and starved glyph.
4. Restore production and confirm the finding clears promptly.
5. If testing list stability, create at least three simultaneous risks and
   observe several refreshes. Existing same-severity members must not churn;
   a newly starved risk may displace one draining member.

## Snapshot and performance

- `Save snapshot` writes one JSON only when clicked and reports success or
  failure for two seconds.
- The JSON is no larger than 256 KiB and names snapshot schema `2.9`.
- It contains focused selected-phase evidence and explicit coverage or
  omission diagnostics, not broad factory dumps.
- Leave the panel visible for one minute and hidden for one minute. Report any
  visible hitch, plugin exception, or new log error.

Report the tested DLL variant, DSP and mod versions, save/phase used, checks
performed, and any mismatch. Provide screenshots for presentation failures and
a diagnostic snapshot only when runtime analysis requires repository-side
audit.
