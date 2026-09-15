# Guide 3.0 owner acceptance workshop — GC3-13

**State:** Not started. This is the only human validation story.
**Candidate:** `2.2.98.e89f73e`, source
`e89f73e65570caf9599f5837377ed8056951fc52`. The verified local binaries, source,
hashes and reports are in `artifacts/guide3/candidate-e89f73e-98/`;
`candidate.json` is the binary identity record. Do not use a preflight folder.
**Authority:** [Accepted roadmap](../ROADMAP.md) and [execution record](GUIDE3-EXECUTION.md).

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
