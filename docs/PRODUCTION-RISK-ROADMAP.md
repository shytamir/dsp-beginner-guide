# Production Risk Analyzer Roadmap

## Decision

The feature-package source files are not an implementation baseline. Their
runtime access, buffer model, navigation layer, and UI structure do not match
this repository's verified APIs or architecture.

The underlying feature remains valuable and is adopted as a product goal:

- compare recent production with a longer native baseline;
- distinguish a real supply collapse from startup, pulsed production, and
  output backpressure;
- account for accessible supply runway where its scope is provable;
- translate the result into one useful player-facing diagnosis; and
- expose the selected phase's worst actionable risk through a quiet visual
  accent that works while the text panel is collapsed.

The implementation must extend the existing collector, normalized state,
analyzer, panel-model, and UI layers. It must not introduce a second factory
scanner, phase controller, navigation surface, or dashboard.

## Adopted mathematical contract

For an evaluated item, all rates are normalized to items per minute before
they reach the risk engine:

```text
baseline = max(R10, epsilon)
dropRaw = max(0, 1 - R1 / baseline)
drop = dropRaw * dropRaw
effectiveProduction = R1 + imports
effectiveDemand = C1 + exports
runwayMinutes = accessibleStock / max(effectiveDemand, epsilon)
thinness = clamp(1 - runwayMinutes / runwayFloorMinutes, 0, 1)

risk = 0                     when history is not ready
risk = 0                     when authoritative backpressure is present
risk = drop * thinness       otherwise
```

The score measures **deterioration with little remaining runway**. It does not
by itself prove that a stable line meets a guide target. Standing demand
deficits and exact guide readiness targets remain separate interpreter inputs,
so a chronically undersized line cannot disappear merely because its recent
and historical rates are equally low.

Two refinements are mandatory:

- `R10 < epsilon` is not proof of startup. A genuine zero rate may represent
  an idle, saturated, disconnected, or absent line. Warm-up suppression must
  use explicit history-coverage evidence.
- A high fill ratio suppresses risk only when both the stock scope and maximum
  capacity are authoritative for that item. Broad planet inventory totals and
  guessed mixed-container capacity cannot establish backpressure.

## Verified native windows

The existing Statistics Panel path remains the single collector authority:

| Window | Production | Consumption | Normalization |
|---|---:|---:|---:|
| 1 minute | `ProductStat.total[1]` | `ProductStat.total[8]` | already a one-minute count |
| 10 minutes | `ProductStat.total[2]` | `ProductStat.total[9]` | divide by 10 |
| 1 hour | `ProductStat.total[3]` | `ProductStat.total[10]` | divide by 60 |

Import and export terms may enter the formula only after an equivalent native
rate and matching scope have been verified. Until then they remain unavailable
and are omitted rather than approximated from inventory movement.

## Immediate roadmap

### RISK-01 - Native multi-window evidence

**User story:** As the risk analyzer, I need trustworthy one-minute and
ten-minute production and consumption evidence with explicit coverage state,
so a recent slowdown can be compared with a real baseline without mistaking
missing history for zero production.

Scope:

- Extend the existing bounded `ProductionTelemetry` traversal; do not add a
  second factory scan or a new `MonoBehaviour` sampler.
- Normalize the verified ten-minute totals to items per minute in the
  collector and record their window provenance.
- Model one-minute and ten-minute production and consumption independently as
  available, unavailable, or not ready.
- Establish history readiness from a verified native coverage signal or a
  minimal per-item observation-age record. Never infer readiness from the
  magnitude of `R10`.
- Retain the current entire-cluster scope unless a consumer explicitly and
  honestly requests another scope.
- Export compact diagnostic evidence sufficient to compare native Statistics
  Panel values with analyzer inputs.

Acceptance:

- The 1-minute and 10-minute values match their respective Statistics Panel
  views after normalization.
- A legitimate zero is preserved as zero; unavailable and warming evidence
  remain distinct.
- Existing one-minute consumers, sampling cadence, bounded watch set, and
  performance behavior do not regress.

### RISK-02 - Accessible runway and backpressure evidence

**User story:** As the risk analyzer, I need a conservative measure of stock
that can actually cushion current demand, so idle production with a useful
buffer is not reported as starvation and a full output buffer is recognized
as backpressure only when the runtime proves it.

Scope:

- Define eligible buffer sources and their scope before collecting them.
- Use authoritative item-configured logistics slots for current/max saturation
  where available.
- Count other storage toward runway only when the item and accessibility are
  known; do not invent an item-specific maximum for mixed containers.
- Keep broad `ProductStat.storageCount` as corroborating planet inventory at
  most; never label it chest stock, accessible runway, or capacity.
- Compute runway in minutes against effective demand using consistent units.
- Represent backpressure as proven, not proven, or unknown. Unknown never
  suppresses risk.
- Verify native import/export rate fields and scope separately before adding
  them to effective production or demand.

Acceptance:

- Full configured output storage can suppress a false shortage finding.
- Empty or draining accessible buffers expose a genuine shortfall.
- Mixed storage, remote stock, belt contents, and unrelated machine buffers do
  not silently inflate capacity or runway.
- Snapshot evidence identifies which buffer sources contributed to a result.

### RISK-03 - Deterministic risk engine and interpreter

**User story:** As a player, I want the mod to distinguish warming, balanced,
backpressured, draining, and starved production, so I receive a useful cause
and next action rather than a raw score or a false alarm.

Scope:

- Implement the adopted formula as a pure C# 7.3 analysis component with no
  Unity or game-runtime dependencies.
- Keep the continuous score, its component terms, and the interpreted state
  separate.
- Evaluate standing demand deficit and exact guide targets alongside the
  deterioration score; do not multiply them into invisibility.
- Use conservative tolerance and state hysteresis so pulsed recipes and small
  rate noise do not cause presentation flicker.
- Return `Unknown` when required evidence is unavailable and `Warming` only
  when coverage evidence supports it.
- Translate only the strongest actionable selected-phase finding into concise
  player language. Do not add an item dashboard or generic troubleshooting
  checklist.
- Add deterministic tests for startup, backpressure, pulsed output, stable
  sufficiency, chronic deficit, draining buffers, and actual starvation.

Acceptance:

- The construction/startup scenarios in the supplied math reference produce
  the intended suppression or dampening.
- A stable but chronically undersized line is reported as a deficit when
  demand or an exact guide target proves one.
- The same normalized evidence always produces the same score and diagnosis.
- Current Status still presents at most one actionable conclusion.

### RISK-04 - Quiet phase-health presentation

**User story:** As a player who has requested the guide panel, I want a subtle
  phase-health signal that remains visible when its text is collapsed, so I
  can notice an actionable production risk without turning the mod into an
  unsolicited HUD or opening a diagnostic dashboard.

Scope:

- Add one small, click-through accent to the existing Cube-rate column or its
  anchor; do not introduce a modal, accordion, new navigation control, or
  obstructive background.
- Derive its color from the cached worst actionable result for the selected
  phase: quiet/neutral, amber, or red.
- Show the accent only while the player-requested panel session is present,
  including its collapsed state. F8-hidden means fully hidden.
- Do not pulse, flash, animate continuously, capture input, or change phase.
- In the expanded panel, let the existing Current Status line carry the
  interpreter's cause and next action.
- Include the displayed severity, selected finding, score terms, and evidence
  readiness in the deliberate compact snapshot.

Acceptance:

- Collapsing the text panel retains the same cached phase-health color without
  recomputing or moving the Cube-rate column.
- Expanding it presents one matching diagnosis, not a list of every evaluated
  item.
- Warming, unknown, and backpressured states do not render as critical.
- Panel click-through, navigation, Cube thresholds, layout, snapshots, and
  performance do not regress.

## Delivery order and gates

Implement in order: `RISK-01` -> `RISK-02` -> `RISK-03` -> `RISK-04`.

- `RISK-01` requires a user checkpoint comparing 1-minute and 10-minute native
  rates before buffer or scoring work begins.
- `RISK-02` requires snapshots from full, draining, empty, remote-only, and
  mixed-storage cases.
- `RISK-03` requires deterministic tests before an in-game diagnostic pass.
- `RISK-04` requires expanded and collapsed screenshots plus a performance
  check with the panel visible and hidden.

No story authorizes automatic phase changes, unsolicited alerts, combat
guidance, broad factory scans, or adoption of the discarded package files.
