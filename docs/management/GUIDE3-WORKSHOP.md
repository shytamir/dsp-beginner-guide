# Guide 3.0 owner acceptance workshop — GC3-13

**State:** Open; B1 blocks further acceptance testing until owner retest.
**Replacement candidate:** local sequence `2.2.99`, under
`artifacts/guide3/workshop-blocker-01/`. `candidate.json` and `BUILD-INFO.txt`
record its exact clean source revision, release label and DLL/package hashes.
Use its `public/DspGuideCheck.dll` or `packages/DSPGuideCheck-2.2.99.zip` for
ordinary testing; the diagnostic variant remains available separately.
The rejected `2.2.98.e89f73e` candidate is retained only as historical evidence.
Do not install a preflight build or both DLL variants together.
**Authority:** [Accepted roadmap](../ROADMAP.md) and [execution record](GUIDE3-EXECUTION.md).

## B1 — periodic panel slowdown

**Owner report:** A mature factory drops from roughly 55 FPS to 22 FPS every
15 seconds for nearly two seconds. ExpertMode is false, the panel is visible,
and WHITE is selected. The previously installed version had no noticeable
periodic dip. This blocks the playthrough, not merely acceptance closeout.

**Correction:** Reuse each native station's slot ownership and native research
prerequisites, observe receipts only in selected ILS Automation, omit the unused
full-state diagnostic export during panel analysis, and reuse already-read
availability fields. No new measurement, telemetry cadence or diagnostic system.
The [execution record](GUIDE3-EXECUTION.md#gc3-13-b1--periodic-panel-slowdown)
contains the reasons and technical checks.

**Owner retest:** With DSP closed, replace the previous mod DLL with the public
DLL from the replacement candidate. Restart DSP with ExpertMode false and load
the same mature save. Select WHITE and show the overlay across several normal
refreshes. Confirm whether the periodic dip is gone and the bar/panel still work.
No timing logs or snapshot export are required for this retest. If acceptable,
continue with the Expert-mode case first, then the planned playthrough.

**Disposition:** Correction supplied; owner retest pending. B1 and G5 stay open
until the owner confirms the result. Technical checks do not establish in-game
performance or acceptance.

## Before the workshop

1. Record source revision, package version and both DLL SHA-256 values from
   `candidate.json`. Verify the installed DLL hash against the selected variant.
2. Use DSP through BepInEx. Keep diagnostic and public DLLs separate; do not
   install both. Begin in normal mode (`ExpertMode = false`) after restart.
3. Use suitable existing or purpose-built saves. Technical validation used only
   synthetic fixtures; no playthrough, Unity layout or performance claim follows
   from those results. Retain owner screenshots and diagnostic snapshots locally.

## Acceptance cases

| Case | Owner observation required |
|---|---|
| ILS selection | Choose I/II/III, correct an initial suggestion, leave/reenter ILS, reload and rename/autosave. Selected phase/stage and guide anchors persist; cargo/research never advances them. |
| Departure and Haulback | Confirm useful research/equipment advice, finished outpost smelting, partial/full loading, transit, partial/full unloading and spent cargo. Ore, home Silicon and outpost storage cannot pretend to be aboard cargo. |
| Automation | Enter early, queue research, assemble part/all of two towers and five Vessels, and configure home Demand/outpost Supply with five home Vessels. Pending respects prerequisites and does not replenish consumed components. |
| Home delivery | Confirm waiting status before new home inputs, one-item and both-item receipts, quiet intervals, reload and changed policies. Unrelated planets/fleets and internal drone traffic cannot complete home imports. Evidence corroborates planet-level receipt, not a specific sending ship. |
| YELLOW/PURPLE | Three supplied Labs can pass with no separate input stores. Stopping feed reveals actual production/risk issues; GREEN's stores remain required. |
| DYSON bridge | Swarm work leads to the bridge link, ordered research, receivers and Photon Materialization. Check the required Hydrogen outlet and automatic science-district delivery yourself; global totals cannot complete this player check. |
| PHOTON | Exercise warming, each missing input, multiple deficits, recovery and two-minute sustained 40/min for five colored Cubes plus Antimatter. Reserve requires 2,000 stationary Antimatter. Current bar colors may recover before history completes. WHITE keeps its existing goals. |
| Normal interface | In both variants, inspect 1080p/4K, navigation, ILS selector, source links, collapse, scrolling, typography, pointer behavior and save reload. Only diagnostic normal mode has Save snapshot; F8 never exports. |
| Expert interface | Enable ExpertMode and restart each variant. F8 reveals only the Cube-rate bar and working DON'T PANIC. Former panel area passes pointer input; no title/body, arrows, collapse, stages, risk glyph or export control exists. Check refresh/reload, rates/anchors and 1080p/4K. Disable and restart to recover normal controls and stored selection. |
| Stability and utility | Inspect new logs, visible performance and missing-evidence behavior. Judge usefulness against R1-R7, including the cost of manual ILS stages and the two-minute policy. |

## Decision record — owner completes at the workshop

- Source / package / DLL hashes:
- Saves and display resolutions exercised:
- Accepted cases and evidence paths:
- In-scope defects requiring repair:
- Known limits accepted or rejected:
- **Owner decision: pending** (accept / reject / revise with bounded defects).

Do not close GC3-13 without an explicit owner decision. Repair accepted-scope
failures and rerun affected technical checks before repeating those cases.
Publication, release/tag creation and roadmap archiving require separate owner
instructions. Nothing in this checklist starts an interactive game session.
