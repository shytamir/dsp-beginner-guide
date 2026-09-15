# Guide 3.0 execution record

**Authority:** [PROJECT.md](../PROJECT.md) and the accepted [roadmap](../ROADMAP.md).
**Authorization:** 2026-09-15, implement GC3-01 through GC3-12 in order, record
decisions and push to main after each completed story. Stop at GC3-13 for human
validation. Technical completion is not owner runtime acceptance or publication.

## Current state

| Stories / gate | State |
|---|---|
| GC3-01 | Technically complete; pushed as `522a5fc` |
| GC3-02 | Technically complete |
| GC3-03 | Next |
| GC3-04 through GC3-12 | Pending in dependency order |
| G1 through G4 | Pending |
| GC3-13 / G5 | Reserved for owner workshop; not started |

## GC3-01 — Select and retain the ILS stage

**Outcome:** `nav3` persists the stage and suggestion/manual origin alongside
the existing phase and playthrough identity. I/II/III controls dispatch through
the same navigation callback, remain inside the collapsible body and immediately
rebuild the selected-stage model. The fixed guide link reads the stored stage.

**Decisions:** Reuse BepInEx selection persistence, the existing button factory
and normalized evidence. Seed only on first ILS entry with an uninitialized
stage; missing birth identity cannot prove an outpost. Invalid stage data does
not discard a valid phase. No history latch, runtime event hook or automatic
advancement was added. The analysis/gate input now explicitly includes stage.
Existing phase-only fixture calls specify Stage I; ILS fixtures choose their
stage deliberately rather than expecting automatic inference.

**Contracts:** selection 1.7 (`nav3`), analysis 3.4, progression 3.2, panel 2.9,
snapshot 2.17. Normalized runtime evidence and release numbering are unchanged.

**Validation:** Diagnostic compilation via `build.cmd`: zero warnings/errors;
its four retained suites passed (the phase suite was rerun after updating all
reflection calls for the added input; receiver suite then run separately).
`Test-IlsStages.ps1` and `Test-SnapshotControlVariant.ps1` passed. Public build
via `dotnet build ... -p:IncludeSnapshotControl=false -o artifacts/guide3/public`
also had zero warnings/errors; all four retained suites, ILS-stage suite and
variant verification passed against that DLL. Fixtures cover migration,
invalid values, one-time seeds, R1-R3 evidence changes, command eligibility,
leaving/reentering ILS, independent selections, stable objective IDs and anchors.

**Reservation:** Actual button placement, pointer behavior and save-reload
interaction will be checked at GC3-13. Cargo and research rules are intentionally
unchanged here and remain the next stories. G1 is not yet complete.

## GC3-02 — Distinguish outpost production, aboard cargo and cargo at home

**Outcome:** Stable Haulback objectives report outpost smelting, return cargo
and cargo at home separately. Only 1106/1105 production qualifies. Outpost
storage is loading context; at home, package and stationary stock are summed
once. Missing evidence produces unknown status without a restocking action.

**Decisions:** Reused the existing grid/pool collection pass and native
Statistics Panel fields. Added collection availability, per-ID research
availability and location presence at normalization. Known current outpost wins;
otherwise the lowest eligible planet ID wins. Spent cargo cannot prove past
hauling: the player can select Automation after completing that trip. Equipment
presence does not claim enough fuel or safe construction space.

**Contracts:** normalized state 2.4, analysis 3.5, progression 3.3, snapshot
2.18. Stage ownership, panel contract and native sampling cadence are unchanged.

**Validation:** Diagnostic and public builds completed with zero warnings and
errors. All four retained suites and both variant checks passed; the phase
suite's normalized-version expectation was updated for 2.4. `Test-IlsCargo.ps1`
includes the retained stage fixtures plus cargo/loading/unloading/transit,
finished-versus-ore, deterministic outpost, spent stock and unavailable/zero
cases. `Test-IlsEvidenceCollection.ps1` additionally exercises real collector
methods with synthetic grids/pools and normalization with missing/valid-empty
inputs. It passed against both DLLs using local game references. The public
command remains the GC3-01 command with the new collection suite included.

**Reservation:** No new game session or save was used. Runtime presentation and
utility remain reserved for GC3-13; G1 awaits the research story.
