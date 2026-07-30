# DSP Guide Check 1.18 - Runtime Validation

This protocol uses only Dyson Sphere Program with the mod installed through
BepInEx. It requires no coding task or other software.

GUIDE-01 changes the selected-phase objective contracts while preserving
manual navigation, native telemetry, compact snapshots, and the accepted
panel behavior. Representative checkpoints are enough; another full
playthrough is not required for this acceptance pass.

## Preparation

1. Install the v1.18 DLL in the usual BepInEx plugin location.
2. Load each available representative save and let it run for at least one
   minute before judging production objectives.
3. Select phases only with the panel controls.
4. Use `Save snapshot` only for the most informative checkpoints.

## Checkpoint A - Early and middle progression

Use one early phase (BLUE or RED) and one middle phase (YELLOW or ILS).

Expected:

- the objective list matches the readiness checklist in the source guide;
- exact rates appear only where the checklist names them;
- preparation or player-knowledge checks remain honest rather than appearing
  automatically complete;
- an idle but buffered older Cube does not become a false failure;
- no future-phase research blocks the selected phase.

Save one snapshot if an objective conclusion needs comparison with visible
game state.

## Checkpoint B - PURPLE and GREEN

Use representative PURPLE and GREEN saves.

Expected:

- PURPLE shows its eight checklist objectives, including one combined
  recheck of Blue, Red, and Yellow rather than a dashboard of every Cube rate;
- Processor, Particle Broadband, Graphene, and Carbon Nanotube objectives
  judge stable supply without inventing unsupported exact targets;
- GREEN shows its eight checklist objectives;
- the chosen endgame Green pace remains a player check rather than a hidden
  20/min or 40/min gate;
- missing preparation produces a useful pending action, not automatic
  navigation.

## Checkpoint C - Optional routes

Select WARP, DYSON, and SPHERE manually on suitable late-game saves.

Expected:

- WARP has no objective or completion gate;
- selecting WARP, DYSON, or SPHERE never changes another phase automatically;
- DYSON and SPHERE remain distinct alternatives;
- DYSON presents sail production, launches, efficiency, generation, and
  receiver research from the published checklist;
- SPHERE presents rocket production, silo launches, enclosed shell area,
  Solar Sail absorption, efficiency, and permanent generation;
- native Dyson evidence remains congruent with the Dyson editor.

Save the most informative DYSON or SPHERE snapshot.

## Checkpoint D - PHOTON and WHITE

Use a save with four Photon Generation receivers and a late-game save with
WHITE available.

Expected:

- PHOTON shows six checklist objectives: receiver health and continuity,
  48 Critical Photons/min, 48 Antimatter/min, safe returned Hydrogen, and all
  five earlier Cube lines capable of 40/min;
- the accepted 60-second receiver continuity behavior remains intact;
- WHITE shows its five checklist objectives before completion;
- after Mission Completed, WHITE reduces to `Mission Accomplished!`;
- neither PHOTON nor mission completion changes the selected phase.

Save one PHOTON snapshot and, if convenient, one completed WHITE snapshot.

## Checkpoint E - LOGISTICS

From WHITE, select LOGISTICS with the next-phase control.

Expected:

- previous returns to WHITE and next remains on LOGISTICS;
- the first three objectives report automatic refill evidence for
  Distributor/Bot, PLS/Drone, and ILS/Vessel infrastructure;
- personal construction resupply and route literacy remain explicit player
  checks;
- no generic post-game dashboard or combat guidance appears.

Save one LOGISTICS snapshot if the phase has representative production or
stock.

## Cross-contract regression check

During the checkpoints, also confirm:

- F8 never saves a snapshot;
- phase and route selections change only through player controls;
- the selected phase does not change while paused, after research, after
  completing an objective, or after Mission Completed;
- the selection persists across a normal game restart;
- navigation, collapse/expand, scrolling, footer controls, and panel layout
  behave normally;
- `Save snapshot` writes one JSON, turns green for two seconds on success,
  returns to its default style, and does not open Windows Explorer;
- `DON'T PANIC` opens the selected guide anchor, including `#logistics`;
- no noticeable new sampling hitch occurs with the panel closed or open;
- each saved JSON is no larger than 256 KiB.

## Testing handoff

Please return:

- snapshots from the most informative checkpoints;
- screenshots only for a player-facing mismatch or layout defect;
- the selected phase and any objective that disagreed with the visible state;
- whether navigation, persistence, layout, footer, or performance regressed;
- whether WARP remained gate-free and LOGISTICS behaved as a manual
  post-completion phase.

The returned snapshots will be reviewed for checklist alignment, native
evidence congruence, compactness, manual selection ownership, and clear
separation between objectives, Pending, Current Status, and player checks.
