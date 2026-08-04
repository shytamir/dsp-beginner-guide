# DSP Guide Check - Guide 2.0 runtime validation

This protocol uses only Dyson Sphere Program with the mod installed through
BepInEx. It validates the current nine-phase guide 2.0 contract; no coding
tools are needed.

## Preparation

1. Install the latest `DspGuideCheck.dll` in the usual BepInEx plugin folder.
2. Load a representative save and let the game run for at least one minute
   before judging production objectives.
3. Open the panel with F8 and select phases only with Previous and Next.
4. Use `Save snapshot` only for a checkpoint whose conclusion needs audit.
5. Confirm each new snapshot names schema `2.8` and guide analysis `2.9`.

## RISK-04 native risk-signal presentation gate

Use the latest diagnostic DLL and the same mature GREEN save used for RISK-03.
Keep GREEN selected throughout. Let the game run until ten-minute history is
ready before producing the actionable states.

Capture these states:

1. **Quiet:** run the Quantum Chip line with production meeting consumption,
   or stop consumption while its accessible output storage is full. Open the
   panel and confirm no extra signal glyph appears beside the Cube-rate rail.
2. **Draining:** keep the Green Cube consumers running, then disconnect or
   pause enough upstream Quantum Chip assemblers that recent production stays
   below consumption while accessible Quantum Chips remain buffered. Wait for
   `Quantum Chips are draining faster than they are replenished`. Capture the
   expanded panel showing DSP signal 402 beside the Cube-rate rail.
3. **Collapse-proof:** without changing that factory state, collapse the panel
   body and capture the same glyph in the same rail-relative position.
4. **Starved:** empty the accessible Quantum Chip supply by removing buffered
   chips and clearing or disconnecting belts feeding the consumer logistics
   station, while Green Cube consumption demand remains established. Keep the
   upstream chip line stopped. Wait for `Quantum Chips production is starved`
   and capture the expanded panel showing the distinct DSP signal 404 glyph.

At 4K, confirm both glyphs are easy to distinguish at a glance, retain their
native colors, do not flash or animate, and do not capture clicks. Toggle F8
to confirm the glyph disappears with the panel. Switch once to an adjacent
phase and back to confirm navigation, title/Cube icons, Cube-rate text, and
layout still refresh normally. Leave the panel visible for one minute and
hidden for one minute; report any visible hitch, plugin exception, or new log
error. Snapshots are optional supporting evidence, not a human field-check
requirement.

## Navigation and persistence

Expected phase order:

```text
BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
     -> DYSON -> PHOTON -> WHITE
```

Confirm:

- Previous on BLUE stays on BLUE and Next on WHITE stays on WHITE;
- no FLIGHT, TITANIUM, SPHERE, WARP, LOGISTICS, COMPLETE, or standalone
  BOOTSTRAP panel exists;
- runtime evidence, pausing, research, objectives, and Mission Completed never
  change the selected phase;
- the selection persists across a normal game restart;
- a stored legacy selection, when available, opens on its documented retained
  replacement without exposing the removed phase.

## BLUE and RED

- BLUE combines continuous starter inputs, routine-hardware replenishment,
  Blue Cubes at 20/min, and the explicit no-hand-feeding player check.
- BLUE does not impose a fixed power objective or enumerate every healthy mall
  product.
- RED requires two configured Labs sustaining 20 Red Cubes/min.
- Refined Oil congestion may appear as one Current Status warning but never as
  a second hard RED objective.

## ILS expedition

Only one checkpoint appears at a time:

1. Before launch: required technology, carried essentials, independent power,
   and the explicit remote-outpost player check.
2. During the expedition: same-planet Titanium and Silicon production plus
   860 Titanium Ingots and 520 High-Purity Silicon in local storage.
3. During the research rush: the current research target, protected build
   reserve, two ILS towers, five Vessels, and active Titanium/Silicon routes.

Starter-system reconnaissance advice does not appear as an objective.

## YELLOW through GREEN

- YELLOW requires three configured Yellow-Cube Labs producing continuously.
- PURPLE requires three configured Purple-Cube Labs producing continuously.
- GREEN requires two configured Green-Cube Labs producing continuously plus
  visible Quantum Chip and Graviton Lens storage.
- A genuinely draining PURPLE or GREEN supporting input produces at most one
  focused Current Status finding; healthy supporting chains add no clutter.
- Soft pace bands remain in the Cube-rate column rather than becoming hard
  objective rows.

## DYSON and PHOTON

- DYSON is titled `Build the Photon swarm` and requires Solar Sail production,
  launches, active sails, and useful swarm generation.
- Antimatter is a handoff cue to PHOTON, not a DYSON objective or automatic
  navigation trigger.
- PHOTON requires four lensed, continuously supplied Ray Receivers, running
  Critical Photon and Antimatter production, and 2,000 stored Antimatter.
- Actual Photon and Antimatter rates use 48/min only as receiver-array
  reference context.
- Current Status compares receiver demand with available Dyson generation.

## WHITE

Expected objective rows:

1. Universe Matrix is researched.
2. Ten Labs sustain 40 White Cubes/min, with stored White Cubes shown.
3. Mission Completed consumes or has consumed 4,000 White Cubes.

The six feeder inputs are not repeated as objective prose. One genuinely
draining feeder may appear in Current Status. After Mission Completed, WHITE
presents `Mission Accomplished!` without navigating elsewhere.

## Panel and export regression

- F8 never saves a snapshot.
- Navigation, collapse, scrolling, footer controls, and layout behave normally.
- The Cube-rate column shows the applicable Matrix icons in Blue-to-White
  order, remains click-through and visible while collapsed, and updates rate
  text without a visible hitch.
- `Save snapshot` writes one JSON, turns green for two seconds on success, and
  does not open Windows Explorer.
- `DON'T PANIC` opens the matching retained guide anchor.
- No noticeable sampling hitch occurs with the panel closed or open.
- Each JSON is no larger than 256 KiB and names snapshot schema `2.8`.

## Migration closure regression sweep

1. Navigate from BLUE through WHITE and back to BLUE once. Confirm the exact
   nine-phase order and both endpoint behaviors.
2. Stop on ILS, close the game normally, restart the same playthrough, and
   confirm ILS remains selected.
3. Save fresh BLUE, ILS, and WHITE snapshots. Together with the accepted
   PURPLE/GREEN and DYSON/PHOTON evidence, these cover the retained early,
   expedition, Matrix, Dyson, receiver, and completion families.
4. Search the visible panels and those three JSON files by inspection: no
   removed phase may appear as the selected phase, objective contract, Current
   Status finding, or evidence route. A legacy-normalization diagnostic is
   allowed only when a legacy stored selection was actually migrated.
5. Confirm the BLUE snapshot includes starter and Blue evidence, the ILS
   snapshot includes its active checkpoint evidence, and the WHITE snapshot
   includes Cube/Antimatter evidence without broad factory dumps.

Return the three snapshots and a short pass/fail note for navigation,
persistence, removed-contract absence, controls, and performance. Screenshots
are needed only for a visible mismatch.
