# GUIDE-01 - Adopted guide authority

> Historical baseline: this document records the v1.18 implementation against
> guide v1.1. Future guide-derived work is governed by
> `GUIDE-1.22.2-GAP-ANALYSIS.md`.

## Source

- Status: adopted for DSP Guide Check 1.18.0.
- Published guide: <https://dsp-beginner-guide.pages.dev/>
- Guide version: 1.1.
- Source repository commit:
  `1bc9e0e4a198dbd2691f2164ae0dbf44a4ad8698`.
- `index.html` Git blob:
  `c5e5e3ca375f26f68e1881ccd9a247dde2d2fb8b`.
- `index.html` SHA-256:
  `A5FE2CDEAFFE8F1627D591E0A7A20F5E4EFC1BA04261306906098EB2792BCEDD`.
- Inspected read-only on 2026-07-30.

The prepared comparison baseline
`0020f050cb46679c480008f95cba7cc676359891` is not present in the current
guide repository history, so a mechanical baseline diff was unavailable.
GUIDE-01 therefore re-read and re-derived the complete published contract
rather than assuming unchanged statements.

## Product contract

- The player selects and changes phases; runtime evidence never navigates.
- Objectives are the selected phase's published readiness checklist.
- Pending rows are unresolved actions toward those objectives.
- Reference and comfort paces are context, except when the readiness checklist
  explicitly names the pace.
- Warnings appear only when relevant; optional routes never become hidden
  requirements.
- Unprovable checklist items remain explicit player checks.
- WARP is an optional reference route with no completion gate.
- DYSON and SPHERE are player-selected alternatives leading to PHOTON.
- WHITE ends the main route at Mission Accomplished.
- LOGISTICS is the new, manually selected post-completion phase.
- Combat remains outside scope.

## Adopted phase records

The phase identifier is also its published guide anchor.

### BOOTSTRAP

- Purpose: stop handcrafting the factory.
- Objectives: continuous Iron and Copper; self-fed basic smelting; automated
  Magnetic Coils and Circuit Boards; available Belts and Sorters; ordinary
  machinery without repeated handcrafting; power headroom.
- Evidence: native production, stock, native power, and a player check for
  routine machinery.
- Reference: prepare for 20 Blue Cubes/min later.

### BLUE

- Purpose: build the first continuous matrix line.
- Objectives: continuous Blue Cubes; self-fed research; 20/min; oil-expansion
  power room; basic component supply no longer consumes every metal batch.
- Evidence: native production, stock, native power, and a player check for
  direct lab feeding.
- Reference: 40/min is comfortable.

### RED

- Purpose: solve oil and prepare for flight.
- Objectives: continuous Red Cubes; 10/min; neither Refined Oil nor Hydrogen
  can jam refining; self-fed Energetic Graphite; staged Steel and Foundation;
  Blue keeps pace.
- Evidence: native production, tank fill, stock, and demand-aware Cube
  capability.
- Reference: 20/min is comfortable.

### FLIGHT

- Purpose: reach another planet safely.
- Objectives: Drive Engine Lv2 and Mecha Core Lv2; Titanium Smelting; PLS
  researched or ready; Particle Trap, Processor, and Reinforced Thruster
  researched or queued; comfortable travel energy; understood destination
  and power plan.
- Evidence: research and queue state plus two explicit player checks.

### TITANIUM

- Purpose: establish a useful off-world Titanium source.
- Objectives: remote extraction; source smelting; serious first haul; direct
  Silicon established or planned; outpost power and any needed defense plan.
- Evidence: planet-scoped native production, owned stock, and player checks.
- References: 810-860 Ingots; 60/min source smelting minimum, 120 comfortable.

### YELLOW

- Purpose: make the finite ILS research batch.
- Objectives: continuous Yellow Cubes; 7.5/min; 200 reserved or demonstrably
  spent; Titanium Alloy; ILS complete or finishing; most non-Yellow ILS
  hardware ready.
- Evidence: native production, stock, research/queue state, and station stock.
- Reference: 15/min is comfortable.

### ILS

- Purpose: end manual interplanetary hauling.
- Objectives: automated Titanium; automated or sustainable Silicon; routine
  hauling ended; charging does not destabilize power; useful local PLS;
  research queue points to Information Matrix.
- Evidence: station policy and traffic, planet-scoped production, native
  power, and research queue.

### PURPLE

- Purpose: build the first truly wide production tier.
- Objectives: continuous Purple Cubes; 12/min; Processors keep pace; stable
  Particle Broadband, Graphene, and Carbon Nanotubes; both branches complete
  or progressing; older Blue, Red, and Yellow rechecked.
- Evidence: native production, reserves and consumption, and research queue.
- Reference: 24/min is comfortable. Supporting item stability does not invent
  an exact rate not stated by the readiness checklist.

### WARP

- Purpose: optional interstellar shortcuts and rare-resource scouting.
- Objectives: none; the guide explicitly defines no WARP completion gate.
- Evidence: capability may inform Current Status, never phase completion.

### GREEN

- Purpose: make warpers routine and prepare Dyson industry.
- Objectives: continuous Green Cubes; 10/min; stable Quantum Chips and Strange
  Matter; deliberate Hydrogen and Deuterium routes; cheap 8:1 Warpers; scaling
  toward endgame pace; Dyson and photon preparation underway.
- Evidence: native production, stock, recipe configuration, and deployed
  Dyson infrastructure.
- References: 20/min comfortable and roughly 40/min endgame pace. Scaling
  remains an explicit player check because the chosen endgame pace is not a
  single universal completion threshold.

### DYSON

- Purpose: build the minimum useful Dyson swarm.
- Objectives: recalculated Solar Sail replacement production; matching
  long-run launches; known Ray Transmission Efficiency target; live swarm
  generation target; Ray Receiver research.
- Evidence: native one-minute production/consumption, native Dyson aggregate,
  research, and a player check for the chosen efficiency calculation.
- Reference baseline: 511 sails/min and 1.655 GW at the guide's baseline
  efficiency. The target must be recalculated when the guide says so.

### SPHERE

- Purpose: build permanent structure and shell cells.
- Objectives: 5 Small Carrier Rockets/min; one silo sustaining 5 launches/min;
  an enclosed shell area; at least 15 Solar Sails/min for absorption; known
  efficiency target; live permanent generation target.
- Evidence: native production/consumption, dedicated silo state, and native
  Dyson editor aggregates.
- Route: deliberate alternative to DYSON, never automatically selected.

### PHOTON

- Purpose: run the critical-photon receiver array.
- Objectives: four continuously lensed receivers; full Continuous Receiving
  and Receiver Strength; 48 Critical Photons/min; 48 Antimatter/min from one
  collider; returned Hydrogen cannot block; all five Cube lines can reach
  40/min.
- Evidence: dedicated receiver continuity, native production, tank fill, and
  demand-aware Cube capability.

### WHITE

- Purpose: sustain Universe Matrix production and complete the mission.
- Objectives: Universe Matrix research; all five Cube colors at the chosen
  pace; matching Antimatter; continuous White Cubes; Mission Completed
  consuming or having consumed 4,000 White Cubes.
- Evidence: research and native production. The current default selected pace
  is 40/min.
- Endpoint: after Mission Completed the concise objective is
  `Mission Accomplished!`; navigation still remains manual.

### LOGISTICS

- Purpose: automate the infrastructure that moves everything.
- Objectives: automatic Distributor/Bot refill; PLS/Drone refill; ILS/Vessel
  refill; automatic personal construction resupply; ability to trace
  Local/Remote provider and receiver routes.
- Evidence: native production and stock for the first three; explicit player
  checks for personal configuration and route understanding.
- References: 1 Distributor/min, 5 Bots/min, 0.5 PLS/min, 5 Drones/min,
  0.25 ILS/min, and 2 Vessels/min.

## Old-to-new disposition

- Retained: player-owned navigation, per-save persistence, WARP optionality,
  DYSON/SPHERE route choice, production-window honesty, PHOTON continuity,
  compact snapshots, and concise Current Status.
- Changed: every main-phase objective inventory now follows the published
  readiness checklist; exact rate gates exist only where that checklist gives
  an exact pace.
- Moved: comfort/reference paces and causal warnings remain outside objective
  completion unless the readiness checklist explicitly names them.
- Removed: future-phase research gates, invented supporting-item rate gates,
  automatic-route implications, and WARP completion objectives.
- New: LOGISTICS phase, manual/unknown checklist rows, demand-aware Cube
  capability, explicit DYSON efficiency player checks, and the revised SPHERE
  and post-completion contracts.

## Evidence matrix

| Evidence | Runtime authority | Consumer | Missing behavior |
|---|---|---|---|
| Item production/consumption | DSP one-minute entire-cluster Statistics aggregate | phase objectives and compact snapshot | unknown until window ready |
| Remote production | DSP factory-scoped Statistics aggregate | TITANIUM and route checks | blocked/unknown with honest action |
| Owned stock | focused inventory/storage totals | availability and reserve checks | zero stock, never inferred production |
| Research and queue | live tech state | readiness objectives | incomplete/unqueued |
| Logistics routes | station policies plus traffic aggregate | ILS | active route not found |
| Power | rolling native power aggregate | phase power objectives | unknown until sampled |
| Dyson generation/construction | native Dyson system/editor aggregates | DYSON/SPHERE | unavailable/unknown |
| Launch devices | dedicated ejector/silo pools plus item consumption | DYSON/SPHERE | not found |
| Receiver health | dedicated gamma receiver pool and rolling 60-second window | PHOTON | unknown/watch until ready |
| Player intent/knowledge | none | manual checklist rows | explicit player check |

## Runtime acceptance

The release build validates deterministic wiring and contract serialization.
In-game acceptance still requires representative early, middle, late, route,
PHOTON, WHITE, and LOGISTICS checkpoints using
`docs/RUNTIME-TESTING.md`. Runtime validation must confirm that objectives
remain stable and that no selected phase changes without player input.
