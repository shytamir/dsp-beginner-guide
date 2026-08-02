# Guide v1.22.2 critical-path gap analysis

## Source and authority

- Published guide: <https://dsp-beginner-guide.pages.dev/>
- Published `guide-version` metadata: `1.22.2`
- Inspected read-only: 2026-08-02
- Scope: the default industrial route from BOOTSTRAP through WHITE

This report supersedes `GUIDE-REVISION-INGESTION.md` for future guide-derived
work. The older document remains the implementation record for the v1.18
baseline.

## Adopted scope

The panel phase sequence becomes:

```text
BOOTSTRAP -> BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
          -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM are now checkpoints inside ILS rather than independent
phases. SPHERE, WARP, and LOGISTICS are outside the mod's new critical-path
scope. They must not have navigation controls, panels, objectives, findings,
or snapshot phase contracts.

These interpretation rules keep the panel aligned with the guide without
turning it into a rate dashboard:

- the player still owns phase selection; evidence never navigates;
- the phase-local `Ready to move on when` list is the objective authority;
- the ILS mission manifest, `Before flying home`, and `Done when` checkpoints
  jointly define its stable objective inventory;
- a numerical pace is a hard objective only when the phase-local readiness
  text repeats it;
- dashboard and one-screen-summary rates otherwise remain reference context;
- supporting production appears only as a concise Current Status warning when
  a real shortage is found;
- optional paths described elsewhere in the guide do not become hidden
  requirements or mod phases.

## Phase checklist gaps

| Retained phase | Published phase-local readiness contract | Current gap and required disposition |
|---|---|---|
| BOOTSTRAP | Inputs arrive continuously; routine mall hardware replenishes automatically; the grid provides roughly 5-10 MW for the mall, Labs, and oil preparation. | Keep the replenishment and power evidence, but replace generic handcrafting/player-check rows with the three published conclusions. |
| BLUE | Blue Cubes run continuously at 20/min or better; research is not hand-fed; roughly 5-10 MW is available for Blue and the coming oil district. | Keep 20/min as the only Blue rate objective. Treat 40/min and component comfort as reference/status, not completion requirements. |
| RED | Two Labs sustain 20 Red Cubes/min while Hydrogen and Refined Oil both keep leaving the Refineries. | Replace the old 10/min gate and remove flight preparation, staged materials, and older-cube checks from the objective list. |
| ILS | Prepare the trip; establish powered remote Titanium and Silicon smelting; return with 860 Titanium Ingots and 520 High-Purity Silicon; complete the finite 200-Yellow-Cube research purchase; deploy two ILS towers and five Vessels; make both resources arrive home without Icarus. | The current framework spreads this one mission across FLIGHT, TITANIUM, YELLOW, and ILS, then places sustainable YELLOW before ILS. Consolidate the expedition and retire those duplicate boundaries. |
| YELLOW | Three Yellow-Cube Labs produce continuously. | Replace the finite-batch/ILS-unlock gate. Keep imported Titanium, chemistry balance, and the 22.5/min dashboard pace as status or reference only. |
| PURPLE | Three Purple-Cube Labs produce continuously. | Remove 12/min and all supporting-item/branch/older-cube rows from hard objectives. Report a detected endpoint or older-cube shortage only in Current Status. |
| GREEN | Two Green-Cube Labs produce continuously; Quantum Chips and Graviton Lenses each have visible storage. | Remove the 10/min, Warper, Deuterium, Strange Matter, and Dyson-preparation gates. Those supplies may produce one actionable warning when genuinely deficient. |
| DYSON | Critical Photons become Antimatter reliably, and Antimatter reaches the science district without hand-carrying. | Solar Sail pace, Ejector duty, generation, receiver research, and the former 1.655 GW target are supporting diagnostics, not completion objectives. Remove the SPHERE alternative from the panel contract. |
| PHOTON | At least 2,000 Antimatter is stored and the player is satisfied that the rising production trend can support WHITE. | Make stored Antimatter the measurable objective and leave sufficiency as an explicit player check. Receiver continuity, lens supply, Critical Photon conversion, and Hydrogen outlet health become focused status/pending evidence rather than extra completion gates. |
| WHITE | Universe Matrix is researched; all six inputs reach the Labs continuously; ten Labs sustain 40 White Cubes/min; Mission Completed consumes or has consumed 4,000 White Cubes. | Replace the generic selected-pace model with these four explicit readiness rows. Preserve the concise Mission Accomplished endpoint without introducing a later phase. |

## Gaps outside the objective inventory

- Navigation and persistence still recognize five phases that are now outside
  scope and retain a DYSON/SPHERE route choice.
- Analyzer phase metadata and panel links still expose the old phase order.
- Current Status still contains optional-route opportunities and rate findings
  whose only consumers are removed phases or old checklist rows.
- The telemetry watch set and snapshot schema still carry evidence for
  FLIGHT, TITANIUM, SPHERE, WARP, and LOGISTICS contracts.
- The acceptance harness and runtime checklist still validate the v1.18 phase
  inventory.

## Immediate roadmap

Implementation status: SCOPE-01, ILS-02, OBJ-02, LATE-01, and PRUNE-01 were
delivered together in the guide 1.22.2 critical-path migration. The acceptance
criteria below now define the focused runtime test pass.

### SCOPE-01 - Restrict navigation to the critical path

**User story:** As a guide reader, I can navigate only the ten default-route
phases, in the same order as the published guide.

Scope:

- replace the navigation inventory and analyzer phase metadata with the ten
  retained phase IDs;
- remove FLIGHT, TITANIUM, SPHERE, WARP, and LOGISTICS controls, route-choice
  state, panel models, and source-guide anchors;
- keep navigation manual and persistence per save;
- normalize stored removed selections once: FLIGHT/TITANIUM to ILS, SPHERE to
  DYSON, WARP to GREEN, and LOGISTICS to WHITE.

Acceptance:

- Previous/Next traverses only the published critical-path order;
- no removed phase can be selected, serialized, or rendered;
- loading an old removed selection lands on the stated retained phase;
- runtime evidence never changes the selected phase.

### ILS-02 - Consolidate the interplanetary expedition

**User story:** As a player working through ILS, I see one stable mission
checklist that follows preparation, the remote outpost, the finite research
purchase, and the first automatic routes.

Scope:

- replace the old FLIGHT/TITANIUM/finite-YELLOW/ILS objective fragments with
  one ILS evaluation;
- retain runtime evidence for prerequisite research, remote production,
  returned stock, ILS/Vessel availability, station policies, traffic, and
  power;
- use explicit player checks where destination knowledge, defense, or future
  station placement cannot be proven.

Acceptance:

- the ILS objective list remains stable from preparation through automatic
  delivery;
- 860 Titanium Ingots, 520 High-Purity Silicon, the 200-Yellow-Cube purchase,
  two ILS towers, five Vessels, and both active resource routes are reported
  against their matching guide checkpoints;
- no objective claims that a plan or route exists without positive evidence;
- completing ILS does not select YELLOW automatically.

### OBJ-02 - Align the simple phase checklists

**User story:** As a player in BOOTSTRAP, BLUE, RED, YELLOW, PURPLE, or GREEN,
I see only the phase-local readiness checks and concise action needed to meet
them.

Scope:

- rebuild those six objective inventories from the gap table above;
- keep exact hard rate thresholds only for BLUE at 20/min and RED at 20/min;
- evaluate continuous production, required Lab counts, endpoint storage, and
  phase power only where the published readiness text requires them;
- move comfort paces and supporting-chain health to Current Status, emitting
  them only when actionable.

Acceptance:

- objective labels and completion rules trace directly to the published
  phase-local readiness text;
- YELLOW, PURPLE, and GREEN do not acquire numeric rate gates from dashboard
  or one-screen reference tables;
- healthy supporting chains do not create completed clutter;
- Pending contains only unresolved actions for visible objectives or one
  presently actionable shortage.

### LATE-01 - Realign DYSON, PHOTON, and WHITE

**User story:** As a player on the default late-game route, I receive a small
set of trustworthy readiness conclusions while detailed native telemetry is
used only to explain a real problem.

Scope:

- make reliable Antimatter delivery the DYSON readiness contract;
- make 2,000 stored Antimatter plus one explicit sufficiency check the PHOTON
  readiness contract;
- implement the four published WHITE readiness rows exactly;
- retain focused Dyson, receiver, production, storage, and research evidence
  as diagnostics and Pending support.

Acceptance:

- Solar Sail, launch-duty, generation, 48/min, and 1.655 GW references cannot
  independently complete or block a phase;
- receiver or conversion faults yield one causal Current Status conclusion;
- WHITE alone retains a 40/min late-game Cube objective;
- Mission Completed changes WHITE to Mission Accomplished without navigating.

### PRUNE-01 - Remove obsolete evidence and contracts

**User story:** As a maintainer, I can validate the critical-path companion
without carrying dead optional-route rules, telemetry, or snapshot payload.

Scope:

- remove gate, analyzer, panel, test, and documentation code used only by the
  five retired phases or optional-route findings;
- reduce the Statistics watch set and normalized/exported evidence to actual
  consumers after SCOPE-01 through LATE-01;
- preserve native production, ILS route/traffic, power, Dyson, receiver, and
  research evidence required by retained objectives;
- revise snapshot and runtime-test contracts, bumping contract/schema versions
  only where serialized behavior actually changes.

Acceptance:

- no removed phase ID or optional-route finding remains in player-facing or
  snapshot contracts;
- every retained telemetry field has a documented objective, status, or
  diagnostic consumer;
- compact snapshots still contain enough evidence to audit every visible
  conclusion;
- the deterministic harness and release build pass against the ten-phase
  inventory.

## Delivery order

Implement in this order:

1. SCOPE-01
2. ILS-02
3. OBJ-02
4. LATE-01
5. PRUNE-01

PRUNE-01 deliberately follows the objective work so telemetry is removed only
after the retained consumers are known.
