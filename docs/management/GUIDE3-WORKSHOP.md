# Guide 3.0 owner acceptance workshop — GC3-13

**State:** Full playthrough pending. The owner accepted B1 performance at roughly
a 7 FPS cost and confirmed the ExpertMode flag works. Expert fixed-WHITE is a
separate follow-up and does not block playthrough testing.
**Accepted performance / full-playthrough candidate:** `2.2.101.5cc8f33`, under
`artifacts/guide3/workshop-blocker-01-refresh/`. `candidate.json` and `BUILD-INFO.txt`
record its exact clean source revision, release label and DLL/package hashes.
Use its `public/DspGuideCheck.dll` or `packages/DSPGuideCheck-2.2.101.zip` for
ordinary testing; the diagnostic variant remains available separately.
**Expert fixed-WHITE candidate:** local sequence `2.2.102`, under
`artifacts/guide3/workshop-expert-white/`, with its own source/identity manifest.
Its in-game counter/anchor check remains pending; replacing the playthrough
candidate is not required by this correction.
The rejected `e89f73e` and `ff71d06` candidates are historical evidence only.
Do not install a preflight build or both DLL variants together.
**Authority:** [Accepted roadmap](../ROADMAP.md) and [execution record](GUIDE3-EXECUTION.md).

## B1 — periodic panel slowdown

**Owner report:** A mature factory drops from roughly 55 FPS to 22 FPS every
15 seconds for nearly two seconds. ExpertMode is false, the panel is visible,
and WHITE is selected. The first correction, installed as `2.2.100.ff71d06`,
did not improve it. Hiding the overlay stops the dip. A fresh comparison with
public `2.1.83.22a5998` also showed a smaller, approximately 19 FPS drop only
while its panel was visible. This supersedes the initial report that the public
version had no dip. This blocks the playthrough, not merely acceptance closeout.

**First correction (failed runtime retest):** Reuse each native station's slot ownership and native research
prerequisites, observe receipts only in selected ILS Automation, omit the unused
full-state diagnostic export during panel analysis, and reuse already-read
availability fields. No new measurement, telemetry cadence or diagnostic system.
The [execution record](GUIDE3-EXECUTION.md#gc3-13-b1--periodic-panel-slowdown)
contains the reasons and technical checks.

**Follow-up:** The owner included the inherited roughly 19 FPS hit in this build.
Stop building unused player/factory diagnostics, storage-container details,
entity counts and station-stock aggregates for panel inputs. Use native stock
counts for WHITE/Antimatter, native research flags and native Dyson power.
Keep construction/launcher details in deliberate exports; bound recipe reads to
native cursors and omit assemblers outside DYSON. Both published and
candidate binaries performed the discarded work; static comparison does not
establish how much of either slowdown it caused. No new instrumentation or
diagnostic export is part of this correction.

**Owner disposition (2026-09-16):** The remaining hit is roughly 7 FPS and is
acceptable. B1 is closed by that explicit owner acceptance. The ExpertMode flag
works; fix its effective phase to WHITE so every Cube counter is displayed.
The owner will use the accepted candidate for the full playthrough regardless
of that fix. GC3-13/G5 stay open; the playthrough and final acceptance have not
been completed.

## Expert follow-up — fixed WHITE

Use WHITE for Expert collection, analysis, all six Cube counters and the guide
anchor. Do not seed or overwrite normal-mode phase/ILS-stage preferences.
On the corrected build, check all six counters with an earlier phase saved,
then disable ExpertMode and restart to confirm normal selection returns.
This check is pending and does not block the accepted candidate's playthrough.

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
| Expert interface | The owner confirmed the flag works. On the corrected build, F8 reveals all six Cube counters and DON'T PANIC using WHITE, regardless of saved normal phase. Former panel area passes pointer input; no title/body, arrows, collapse, stages, risk glyph or export control exists. Check refresh/reload and 1080p/4K. Disable and restart to recover normal controls and stored selection. |
| Stability and utility | Inspect new logs, visible performance and missing-evidence behavior. Judge usefulness against R1-R7, including the cost of manual ILS stages and the two-minute policy. |

## Decision record — owner completes at the workshop

- Source / package / DLL hashes:
- Saves and display resolutions exercised:
- Accepted cases and evidence paths: owner reports acceptable performance and working ExpertMode flag; source `5cc8f33`, retained candidate above.
- In-scope defects requiring repair: Expert fixed-WHITE implemented; corrected-build in-game confirmation pending.
- Known limits accepted or rejected: remaining roughly 7 FPS hit accepted by owner.
- **Owner decision: pending** (accept / reject / revise with bounded defects).

Do not close GC3-13 without an explicit owner decision. Repair accepted-scope
failures and rerun affected technical checks before repeating those cases.
Publication, release/tag creation and roadmap archiving require separate owner
instructions. Nothing in this checklist starts an interactive game session.
