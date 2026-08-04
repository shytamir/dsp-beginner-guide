# DSP Guide Check - Critical-path runtime validation

This protocol uses only Dyson Sphere Program with the mod installed through
BepInEx. It validates the guide v1.22.2 contract; no coding tools are needed.

The first representative guide 1.22.2 user test completed without reported
defects. This document remains the regression protocol for future changes.

## Preparation

1. Install the latest `DspGuideCheck.dll` in the usual BepInEx plugin folder.
2. Load a representative save and let the game run for at least one minute
   before judging production objectives.
3. Open the panel with F8 and select phases only with Previous and Next.
4. Use `Save snapshot` for a checkpoint whose visible conclusion needs audit.

## Navigation and persistence

Expected phase order:

```text
BOOTSTRAP -> BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
          -> DYSON -> PHOTON -> WHITE
```

Confirm:

- no FLIGHT, TITANIUM, SPHERE, WARP, LOGISTICS, or COMPLETE panel exists;
- runtime evidence, pausing, research, objectives, and Mission Completed never
  change the selected phase;
- the selection persists across a normal game restart;
- a legacy removed selection opens on its documented retained replacement.

## Checkpoint A - BOOTSTRAP through RED

Expected:

- BOOTSTRAP reports continuous starter inputs, automatic routine-building
  replenishment, and the early power grid;
- BLUE uses 20 Blue Cubes/min as its only rate objective and retains the
  explicit no-hand-feeding check;
- RED uses one combined conclusion: two Labs, 20 Red Cubes/min, and both
  refinery outputs moving;
- no future-phase research or comfort pace blocks these phases.

## Checkpoint B - ILS expedition

Exercise a save before departure and one after automatic delivery if
available. Confirm that one stable checklist covers:

- Drive Engine Lv2 and Titanium Smelting;
- trip loadout and the explicit remote-outpost player check;
- powered remote Titanium and Silicon production;
- 860 Titanium Ingots and 520 High-Purity Silicon returned;
- the finite 200-Yellow-Cube purchase;
- two ILS towers and five Logistics Vessels;
- Titanium and Silicon arriving home automatically.

No objective should claim that an outpost plan or route exists without
positive evidence.

## Checkpoint C - YELLOW through GREEN

Expected:

- YELLOW requires three configured Labs with continuous production;
- PURPLE requires three configured Labs with continuous production;
- GREEN requires two configured Labs with continuous production plus visible
  Quantum Chip and Graviton Lens storage;
- none of these phases acquires an old numeric dashboard rate gate;
- healthy supporting chains do not create completed objective clutter.

## Checkpoint D - DYSON and PHOTON

Expected:

- DYSON requires reliable Critical Photon-to-Antimatter production and
  automatic delivery to science;
- sail pace, launch duty, receiver details, generation, 48/min, and 1.655 GW
  remain diagnostic evidence rather than readiness gates;
- PHOTON requires 2,000 stored Antimatter and retains one explicit player
  check that the rising rate is sufficient for the planned WHITE run;
- a receiver or conversion failure produces at most one useful causal status.

## Checkpoint E - WHITE

Expected objective rows:

1. Universe Matrix is researched;
2. all five Matrix colors and Antimatter reach the Labs continuously;
3. ten Labs sustain 40 White Cubes/min;
4. Mission Completed consumes or has consumed 4,000 White Cubes.

After Mission Completed, WHITE presents `Mission Accomplished!` without
navigating or exposing a later phase.

## Cross-contract regression check

During the checkpoints, also confirm:

- F8 never saves a snapshot;
- navigation, collapse, scrolling, footer controls, and layout behave normally;
- `Save snapshot` writes one JSON, turns green for two seconds on success,
  and does not open Windows Explorer;
- `DON'T PANIC` opens the matching retained guide anchor;
- no noticeable new sampling hitch occurs with the panel closed or open;
- each JSON is no larger than 256 KiB and names snapshot schema 2.4.

## Testing handoff

Please return the most informative snapshots, screenshots only for visible
mismatches, and notes identifying any objective that disagreed with the game.
Also report navigation, persistence, layout, footer, or performance regressions.
