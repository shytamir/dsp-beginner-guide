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
- ILS: exactly one preparation, non-starter-planet expedition, or
  research-rush checkpoint is active. Starter-planet Silicon production does
  not count as an outpost.
- YELLOW: three configured Cube Labs run continuously; Diamond and Titanium
  Crystal storage is visible.
- PURPLE: three configured Cube Labs run continuously; Processor and Particle
  Broadband storage is visible.
- GREEN: two configured Cube Labs run continuously and Quantum Chip and
  Graviton Lens storage is visible.
- DYSON: Solar Sails are produced and launched and the swarm generates power.
- PHOTON: lensed receiver supply, Critical Photon and Antimatter production,
  the 48/min reference, and the 2,000-Antimatter midpoint remain distinct.
- WHITE: concise White-Cube research, configured-Lab and stored-Cube evidence,
  the 40/min gate, and Mission Completed state remain the stable contract.

## WHITE-CONCISE-01 focused gate

This gate also validates `NATIVE-TYPE-01`. Use the diagnostic DLL and one
WHITE-ready playthrough. Capture the panel and one snapshot in each available
state; reloading purpose-built saves is fine.

1. Before queuing Mission Completed, confirm `Mission Completed not queued`.
2. Queue it and let research consume at least one White Cube. Confirm the row
   shows `Mission Completed queued` briefly or an integer `% done` once exact
   progress is available.
3. Complete the research and confirm `Mission Completed complete`.
4. In every state, confirm the lab evidence contains only the configured-Lab
   and stored-White-Cube counts, the objective text contains no Matrix alias,
   and Pending contains only `Complete Mission Completed research.` while the
   research is incomplete.
5. Confirm the Cube-rate rail still presents the live rate and both DLL
   variants retain their expected snapshot-control behavior.
6. At 1080p and 4K, compare panel text and Cube rates with a visible planet
   vein label. Confirm the same compact weight and dark outline, with no new
   clipping, overlap, unstable wrapping, or excessive panel height.
7. At both resolutions, exercise expanded and collapsed layouts with different
   visible Cube counts. Confirm navigation, scrolling, risk glyph, Cube rail,
   `DON'T PANIC`, and diagnostic snapshot control remain correctly placed and
   interactive.
8. Save one diagnostic snapshot for repository-side verification of the
   resolved native Text source, font, material, and attached mesh effects.
   Report any fallback warning or new plugin error.

## CUBE-BRANCH-01 focused gate

Use the diagnostic DLL and saves where YELLOW and PURPLE production already
exist. For each phase, keep the three Cube Labs configured throughout and use
ordinary storage boxes to control only the terminal-input evidence.

1. Before the Cube checks, load a pre-flight starter-planet save that produces
   High-Purity Silicon from Stone. Select ILS and confirm the preparation
   checkpoint remains active. If Drive Engine II can be completed in that
   save, confirm the same result after research; no snapshot is required.
2. Stop the two terminal-input belts before their storage boxes, empty those
   boxes and remove those items from Icarus and logistics-station storage.
   Select the phase and confirm its combined input objective is blocked.
3. Put at least one unit of only the first named input into a storage box:
   Diamond for YELLOW, Processor for PURPLE. Leave the other input absent and
   confirm the combined objective remains blocked with one instruction to
   buffer both inputs.
4. Put at least one Titanium Crystal (YELLOW) or Particle Broadband (PURPLE)
   into another storage box. Within the normal panel refresh window, confirm
   the combined objective completes and reports both owned counts.
5. Restore both feeds and allow all three Cube Labs to produce. Confirm the
   separate Lab objective completes; no new substage or intermediate branch
   objective appears.
6. In one phase, let one terminal input continue feeding the Labs while its
   production falls below consumption. Confirm the concise draining risk and
   matching Next Action can appear independently, then restore production and
   confirm it clears.
7. Save one diagnostic snapshot for each phase in the both-input, producing
   state. Report any new plugin warning, exception, objective churn, selection
   change, or regression in navigation, collapse, Cube rates, typography, risk
   glyph, `DON'T PANIC`, or `Save snapshot`.

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
- The JSON is no larger than 256 KiB and names snapshot schema `2.14`.
- It contains focused selected-phase evidence and explicit coverage or
  omission diagnostics, not broad factory dumps.
- Leave the panel visible for one minute and hidden for one minute. Report any
  visible hitch, plugin exception, or new log error.

Report the tested DLL variant, DSP and mod versions, save/phase used, checks
performed, and any mismatch. Provide screenshots for presentation failures and
a diagnostic snapshot only when runtime analysis requires repository-side
audit.
