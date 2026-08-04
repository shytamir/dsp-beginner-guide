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
- expose the selected phase's worst actionable risk through a quiet,
  game-native signal glyph on the fixed Cube-rate rail.

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

`RISK-01` through `RISK-04` are accepted. `RISK-05` is implemented and awaits
its focused runtime presentation gate.

### RISK-01 - Native multi-window evidence

**Status:** Accepted. The GREEN checkpoint preserved a blocked Quantum Chip
line as a legitimate zero, matched the active Graviton Lens line to the native
one-minute and normalized ten-minute views, and kept the first warming snapshot
distinct from the later ready snapshot without a performance regression.

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

**Status:** Accepted. Sequential GREEN-phase checkpoints on Comae Berenices III
proved a full authoritative Local Supply slot as backpressured, preserved
finite runway while the slot drained, exposed zero runway when empty, and did
not count 427 Quantum Chips held in a mixed depot as accessible stock. The
remote-only configuration remains a focused regression checkpoint if remote
logic changes; it is not a blocker for the guide's locally supplied component
flows.

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

Implemented policy:

- Eligible runway is limited to item-configured logistics slots set to local
  Supply and is evaluated per planet.
- Runway uses the same planet's native one-minute consumption evidence.
- Backpressure is proven only when every eligible contributor in that scope
  is full; otherwise it is not proven, or unknown when no eligible source
  exists.
- Remote-only slots, non-supply slots, and tank aggregates are excluded with
  an explicit reason in snapshot evidence.
- Import and export rates are not used because their runtime meaning and scope
  have not been independently verified.

Acceptance:

- Full configured output storage can suppress a false shortage finding.
- Empty or draining accessible buffers expose a genuine shortfall.
- Mixed storage, remote stock, belt contents, and unrelated machine buffers do
  not silently inflate capacity or runway.
- Snapshot evidence identifies which buffer sources contributed to a result.

### RISK-03 - Deterministic risk engine and interpreter

**Status:** Accepted. Sequential GREEN-phase snapshots on Comae Berenices III
confirmed quiet warming and proven backpressure, an amber draining Quantum
Chip line, and red starvation at zero accessible runway while Current Status
remained capped at one actionable conclusion. The release owner directly
observed the warning clear within 15 seconds of restored production and
accepted balanced recovery without a snapshot after an independent supporting
line subsequently failed.

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
- Retain deterministic actionable selected-phase results for a bounded
  presentation consumer while preserving one strongest forensic selection.
  Do not add an item dashboard or generic troubleshooting checklist.
- Add deterministic tests for startup, backpressure, pulsed output, stable
  sufficiency, chronic deficit, draining buffers, and actual starvation.

Implemented policy:

- Buffered items combine planet-local one- and ten-minute native rates only
  with the matching planet's accessible runway and backpressure evidence.
- Unbuffered items and exact phase targets use the entire-cluster native
  scope; an exact target remains separate from the deterioration score.
- A five-percent or 0.5-item/minute deadband suppresses insignificant rate
  noise, while native one-minute windows smooth pulsed recipes.
- Unknown, warming, backpressured, and balanced results are quiet. Draining
  and starved results are actionable and deterministically ordered for the
  bounded presentation contract.
- Compact diagnostics retain the selected state, severity, score, baseline,
  drop, thinness, deficit flags, runway, and backpressure status.

Acceptance:

- The construction/startup scenarios in the supplied math reference produce
  the intended suppression or dampening.
- A stable but chronically undersized line is reported as a deficit when
  demand or an exact guide target proves one.
- The same normalized evidence always produces the same score and diagnosis.
- Actionable results remain deterministic and separately distinguish urgent
  draining from critical starvation.

### RISK-04 - Native risk-signal presentation

**Status:** Accepted. The focused in-game gate confirmed quiet omission,
distinct draining and starved glyphs, fixed placement with the panel body
collapsed, 4K legibility, click-through behavior, normal F8 hiding, intact
navigation and layout, and no visible performance or log regression.

**User story:** As a player who has opened Guide Check, I want the strongest
  actionable production risk represented by a small, distinct game-native
  signal icon on the panel's fixed Cube-rate column, so I can recognize a
  developing shortage or stopped supply at a glance without decoding another
  color scale.

Scope:

- Add one 28-pixel, click-through glyph beside the existing Cube-rate column;
  do not introduce a modal, accordion, new navigation control, obstructive
  background, or severity-color scale.
- Map the analyzer-selected `draining` state to DSP signal 402 and `starved`
  to the distinct DSP signal 404. Preserve each embedded icon's native color.
- Render no glyph for unknown, warming, backpressured, balanced, or otherwise
  non-actionable states.
- Read only the analyzer's already-selected result. The panel model and UI do
  not rescore findings or choose a different priority.
- Keep the glyph fixed on the collapse-proof Cube-rate rail. Show it only
  while the player-requested panel session is present; F8-hidden means hidden.
- Do not pulse, flash, animate continuously, capture input, or change phase.
- Load, cache, and dispose the embedded resources with the existing panel
  icons. Missing or undecodable resources fail softly by omitting the glyph.
- Defer finding prioritization and richer presentation in the panel body to a
  later story.

Acceptance:

- Draining and starved display different, immediately recognizable native
  glyphs at a legible 4K scale; they are not color variants of one mark.
- Collapsing the panel body retains the same glyph in the same rail-relative
  position, without recomputing the selected risk.
- Quiet states display no glyph, and hiding the panel hides it completely.
- The indicator is fixed, non-interactive, and non-animated.
- Existing title and Cube icons, rate text, navigation, layout, snapshots,
  click-through behavior, and performance do not regress.

### RISK-05 - Bounded interpreted risk presentation

**Status:** Implemented; awaiting the focused in-game presentation gate.

**User story:** As a player responding to production trouble, I want a small,
stable list of plain-language conditions and immediate actions, so I can see
what needs attention without reading forensic statistics or watching the list
churn as rates fluctuate.

Scope:

- Present at most three actionable production risks in Current Status.
- Use only the established player terms: `<item> draining - check soon` and
  `<item> starved - expect stoppage`. Do not render rates, baselines, scores,
  scope terminology, or forensic evidence in those rows.
- Put the matching recommendation in a distinct Next Actions section:
  `Increase <item> production` or `Restart <item> production`.
- Rank initial candidates by starved before draining, then shortest
  trustworthy net-depletion time, then the phase's declared item order.
- Preserve displayed membership and same-severity order while each item stays
  actionable. A same-severity newcomer cannot displace an incumbent merely
  because its estimate changes. A new starved item may immediately displace
  the lowest displayed draining item. Phase changes and a newly opened panel
  start a fresh selection.
- Derive a trustworthy depletion estimate only from authoritative accessible
  stock divided by the positive net deficit (`consumption - production`). If
  a displayed risk names a tracked objective, append one short buffer estimate
  there; omit the estimate when its inputs are unavailable or it is empty.
- Keep detailed rates, scores, scope, runway, and evidence in the deliberate
  snapshot rather than normal panel prose.
- Keep existing non-production findings eligible only for unused slots within
  the same three-row Current Status bound.

Acceptance:

- One to three simultaneous risks produce the same number of compact Current
  Status rows and paired Next Actions without evidence-detail text.
- A fourth same-severity candidate neither replaces nor reorders three active
  incumbents across refreshes.
- A new starved candidate is promoted ahead of draining rows and displaces at
  most the lowest urgent incumbent when the list is full.
- Clearing an incumbent frees its slot; changing phase or reopening the panel
  recomputes a fresh deterministic selection.
- A tracked draining objective shows a concise net-depletion estimate only
  when authoritative local stock and rates support it.
- Objective identity/order, navigation, glyph behavior, collapse, scrolling,
  click-through, snapshots, layout, and performance do not regress.

## Delivery order and gates

Implement in order: `RISK-01` -> `RISK-02` -> `RISK-03` -> `RISK-04` ->
`RISK-05`.

- `RISK-01` requires a user checkpoint comparing 1-minute and 10-minute native
  rates before buffer or scoring work begins.
- `RISK-02` passed with full, draining, empty, and mixed-storage snapshots.
  Remote-only exclusion remains a focused regression checkpoint if remote
  logic changes.
- `RISK-03` passed deterministic tests and its focused in-game diagnostic
  gate, including the release-owner-approved direct observation of balanced
  recovery.
- `RISK-04` passed its quiet, draining, starved, collapsed-body, 4K,
  visible/hidden performance, interaction, navigation, layout, and log checks.
- `RISK-05` requires compact one-, three-, and four-candidate presentation,
  critical promotion, recovery, phase/session reset, trustworthy buffer-note,
  interaction, layout, performance, log, and diagnostic snapshot checks.

No story authorizes automatic phase changes, unsolicited alerts, combat
guidance, broad factory scans, or adoption of the discarded package files.
