# DSP Guide Check - Project Definition

## Purpose

DSP Guide Check is the on-demand runtime companion to the DSP Practical
Progression Guide. When asked with F8, it recalls the phase selected for the
current save, measures that phase's stable objectives, and adds concise useful
status.

The player asks; the instrument answers. It is not an autopilot, factory score,
build designer, or unsolicited warning system.

## Product invariants

- The player owns phase and optional-route selection.
- Runtime evidence evaluates the selection but never changes it.
- Objectives remain stable while a phase is selected.
- Required objectives, reference paces, and optional choices remain distinct.
- Current Status communicates one useful conclusion rather than every rate.
- The panel is hidden by default and never alerts by itself.
- F8 never saves; `Save snapshot` is the deliberate forensic export.
- Collection, normalization, analysis, panel modeling, and UI remain separate.
- Missing evidence fails softly.
- Combat remains outside scope.

## Current architecture

```text
Per-save selected phase
        +
Normalized runtime state
        |
        v
Evaluate exactly that phase
        |
        v
Stable objectives + one useful status
        |
        v
On-demand panel
```

Dyson state, receiver continuity, production, logistics traffic, and power are
sampled on staggered frames. The open panel refreshes in staged passes.
Snapshot creation is a deliberate player action.

## PHO-01 delivered and accepted

PHOTON is a player-selected phase. It neither selects itself nor advances to
WHITE automatically.

GUIDE-01 now expresses its six published readiness objectives:

1. four continuously lensed receivers at full strength;
2. continuous receiving across the array;
3. Critical Photon production at 48/min;
4. Antimatter production at 48/min;
5. returned Hydrogen that cannot block the collider;
6. all five earlier Cube lines able to reach 40/min.

### Continuity semantics

The receiver sampler keeps 65 game seconds and requires at least 60 game
seconds plus ten samples before continuity can pass. Every retained sample for
every currently configured Photon Generation receiver must have:

- Photon Generation mode;
- a Graviton Lens;
- full Receiver Strength;
- full warmup/Continuous Receiving.

One good frame is insufficient. A failure remains visible until it ages out of
the rolling window. Normal warmup is presented as progress.

### Receiver evidence

The sampler records per receiver:

- planet and entity identity;
- mode and lens state;
- current and minimum warmup;
- current and minimum strength;
- requested Dyson power from the game's own receiver request method;
- supplied power;
- current Critical Photon output derived from supplied power and product heat;
- sustained-health result.

The normalized model retains each receiver and the aggregate array totals.
Long-window item rates continue to come from DSP's cumulative production
statistics.

### Current Status policy

PHOTON suppresses generic late-game findings and emits one causal status. The
priority order is setup, receiver configuration, lens continuity, exposure or
warmup, production-window readiness, credible returned-Hydrogen pressure,
soft Dyson-power pressure, Critical Photons, Antimatter, and an older Cube
genuinely failing under WHITE demand.

Idle Cube production is not itself failure. An older Cube warning requires
active WHITE demand, a real net deficit, weak current output, and less than ten
minutes of observed reserve.

The 1.655 GW figure remains a status explanation only. It does not participate
in objective completion or navigation.

## Contracts

| Contract | Version |
|---|---:|
| Exporter | 1.18.2 |
| Snapshot schema | 2.1 |
| Normalized state | 1.5 |
| Guide selection | 1.4 |
| Guide analysis | 2.4 |
| Progression | 2.4 |
| Panel | 1.6 |

## Verification performed

PHO-01 passed both the implementation matrix and runtime acceptance:

- the complete acceptance matrix from zero receivers through sustained health;
- the actual rolling sampler's healthy, interrupted, and recovered histories;
- separate Critical Photon and Antimatter shortfalls;
- soft power remaining outside the objective contract;
- credible-only returned-Hydrogen warnings;
- idle buffered and genuinely failing older-Cube cases;
- normalized receiver-evidence round-trip;
- manual PHOTON-to-WHITE navigation;
- release compilation against the installed DSP and BepInEx assemblies with
  no warnings;
- four runtime PHOTON snapshots spanning receiver warmup, interrupted
  continuity, and sustained healthy continuity;
- no false positives or functional regressions reported.

## v1.15.1 phase persistence repair

Phase selection is keyed by the playthrough creation time and stable galaxy
descriptor rather than the mutable save name. Autosaves, renamed save slots,
pauses and game restarts therefore continue to bind the same player-owned
selection. The currently loaded legacy key is migrated once when available.

Guide-selection diagnostics report the identity version and whether the
selection was restored, migrated, seeded or changed by the player. Research
still seeds a phase only when no valid selection exists for that playthrough.

The high-level snapshot-contract follow-up is recorded in
`docs/SNAPSHOT-REDESIGN.md`.

## v1.16.0 compact snapshot contract

`Save snapshot` still evaluates the same normalized state used by the live
panel, but schema 2.0 serializes only diagnostic conclusions and the evidence
needed to validate implemented functions. It does not serialize the raw
factory model or duplicate normalized state inside analysis and panel
structures.

Every snapshot contains:

- synchronized plugin, assembly, exporter and schema provenance;
- static game tick and derived total playtime;
- selected phase, route, stable identity and persistence provenance;
- objective and Current Status conclusions;
- research totals and lifetime, stock and rolling Cube figures;
- selected-phase item and logistics evidence;
- compact power and collector-health summaries;
- focused DYSON, SPHERE or PHOTON evidence when relevant;
- explicit omission and truncation markers.

Receiver detail is capped at eight rows and marked when more are omitted. The
serialized snapshot is rejected rather than written if it exceeds 256 KiB.
The schema 2.0 runtime checkpoint is intentionally folded into the next
phase's test cycle.

## GUIDE-01 - Published guide adoption

GUIDE-01 is implemented in v1.18.0 from published guide version 1.1. The
complete immutable source record, phase-by-phase readiness contract,
old-to-new disposition, evidence matrix, and runtime acceptance requirements
are maintained in `docs/GUIDE-REVISION-INGESTION.md`.

The implementation preserves player-owned navigation and treats each
selected phase's readiness checklist as the stable objective inventory.
Reference paces become completion requirements only when the checklist names
them. WARP has no completion gate, DYSON and SPHERE remain alternatives, and
LOGISTICS is a new manually selected post-completion phase.

### TEL-01 - Native aggregate telemetry alignment

Implemented in v1.17.0 and corrected in v1.17.1. Production rates now use the native one-minute
Statistics Panel aggregates for a bounded item watch set. Lifetime counters
are separate and limited to Cube totals. Dyson generation, sail population,
structure and cell progress use native Dyson system and node aggregates;
launch devices and Ray Receivers remain dedicated collectors.

Normalized state 1.5 and snapshot schema 2.1 record source, scope, period,
coverage and concise collector failure evidence. The user-run native
Statistics, Dyson, and PHOTON comparisons confirmed the corrected telemetry;
GUIDE-01 reuses that accepted evidence contract.

## v1.15.0 public release

The receiver sampler previously discovered Ray Receivers by reflecting across
every entity in every factory. Runtime diagnostics showed this pass taking
roughly 67-139 ms every five seconds in the tested late-game saves.

v1.15.0 discovers receivers directly in each factory's power-generator pool,
using the game's gamma-mode flag. The collection remains immediate and covers
receivers in both power and photon modes, but no longer scales with total
factory entity count. The acceptance harness contains an explicit tripwire
that fails if receiver sampling accesses the broad entity pool.

The footer's guide link is presented as the two-line bright-red `DON'T PANIC`
control in Comic Sans. It retains the selected-phase guide URL behavior.

The `Save snapshot` footer control reports only whether the file write
succeeded. It turns green for two seconds after success or red after failure,
then returns to its native default style. It does not open Windows Explorer.

Runtime validation reduced the receiver collection pass from 67-139 ms before
the change to about 1.5-1.7 ms in the captured frames, with a recorded maximum
of 7.2 ms. A faint periodic hitch can still be perceived on the tested
late-game save, but it is substantially reduced.

## v1.18.1 panel usability

Panel contract 1.6 outlines all text for contrast while retaining the
minimal, background-free native Goal presentation. Phase controls have
transparent hit areas and switch between one fixed normal scale and one fixed
hover scale, so repeated pointer entry cannot compound their size. The active
DYSON/SPHERE choice uses a colored text outline instead of a filled
background.

Non-interactive panel graphics and the bounded viewport do not receive pointer
raycasts. Only explicit controls capture clicks.

## v1.18.2 logistics navigation hotfix

WHITE retains the Next control so the player can enter the post-completion
LOGISTICS phase manually. LOGISTICS is the final phase and therefore hides
Next. No automatic phase selection or progression behavior is introduced.

## Roadmap status

1. NAV-01 - accepted.
2. ARC-01 - accepted.
3. END-01 - accepted.
4. SPH-01 - accepted.
5. PHO-01 - accepted.
6. TEL-01 - accepted after the native lookup hotfix.
7. GUIDE-01 - implemented; representative runtime acceptance pending.

The v1.15.1 persistence repair and v1.16.0 compact snapshot contract retain
their deferred full-playthrough checkpoint. GUIDE-01 is the next focused
runtime test cycle.
