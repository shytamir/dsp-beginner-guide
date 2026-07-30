# Guide revision ingestion contract

## Status

- Preparation only.
- No forthcoming guide revision has been received or adopted.
- The existing guide was inspected read-only to identify every mod contract
  that must be re-evaluated when the revision arrives.
- Comparison baseline: committed DSP Practical Progression Guide `1.17.0` at
  source commit `0020f050cb46679c480008f95cba7cc676359891`
  (`index.html` blob `be472061c2ff2ae7a5f82eb802dd8eeddc2f4683`),
  inspected 2026-07-30.
- Uncommitted work in the guide repository was deliberately excluded because
  it may belong to the forthcoming revision.
- The baseline identifies the scope of derived work. It is not a claim that
  the current analyzer already implements every `1.17.0` statement.

## GUIDE-01 - Ingest the revised guide

### Source control

- Record the exact guide version, source commit or artifact, publication URL,
  and SHA-256 before analysis.
- Compare the new revision with the baseline above.
- Treat the revised guide as the authority; do not preserve an old rule merely
  because the mod already implements it.
- Keep the guide artifact read-only. Changes to the guide belong in its own
  repository and review process.

### Re-derive the product contract

- Re-read the introduction and usage model before extracting individual
  thresholds.
- Reconfirm the default route, optional routes, route exclusivity, post-Mission
  scope, and phase inventory.
- Reconfirm the player-agency contract: runtime evidence evaluates the
  player-selected phase and never navigates for the player.
- Reconfirm which advice is required, recommended, conditional, optional,
  skippable, or explicitly outside scope.
- Remove superseded derived rules instead of accumulating compatibility
  branches for obsolete guide language.

### Extract one reviewed record per phase

- Stable phase identifier, display name, order, guide anchor, and route.
- Entry cue and phase purpose.
- Exact readiness checklist.
- Hard objectives and their completion semantics.
- Reference or comfort paces that must not become hard gates.
- Pending actions suitable for the panel.
- Warnings and Current Status conclusions worth showing only when relevant.
- Optional alternatives and the evidence required before mentioning them.
- Research dependencies and whether they are entry work or next-phase work.
- Terms, Cube/Matrix nomenclature, colors, and player-facing wording.
- Evidence unavailable from runtime state and the honest fallback for it.

### Classify every guide statement

Each extracted statement must be assigned exactly one primary presentation
role:

- **Objective** - a stable item from the selected phase's readiness checklist
  that runtime evidence can evaluate honestly.
- **Pending** - an actionable unresolved step toward an objective.
- **Current Status** - a concise diagnosis, warning, bottleneck, or relevant
  optional-path observation.
- **Reference** - a soft pace or comfort target used for context, never phase
  navigation.
- **Manual/unknown** - meaningful guide advice the runtime cannot establish.
- **Omitted** - prose that belongs in the source guide rather than the panel.

Checklist membership alone does not justify a fabricated runtime result.
Unobservable checklist items remain manual/unknown or become carefully worded
pending guidance.

### Re-evaluate every derived surface

- `ManualPhaseNavigation`: phase inventory, order, route controls, initial seed,
  persistence compatibility, and guide anchors.
- `GuideGateEngine`: stable objective inventory and evidence semantics.
- `GuideAnalyzer`: thresholds, diagnoses, optional findings, priorities, and
  wording.
- `ObservedGameState` and telemetry collectors: exact evidence required by the
  revised rules, including evidence no longer needed.
- `GuidePanelModel` and `GuidePanelController`: titles, terminology, colors,
  section density, and footer links.
- `CompactSnapshotBuilder`: only evidence needed to validate implemented
  conclusions under the revised contract.
- Deterministic tests, runtime checkpoints, README, project contracts, and
  changelog.

### Evidence and telemetry matrix

For every retained objective or finding, record:

- authoritative runtime source;
- native UI surface that exposes the same fact, when one exists;
- aggregation scope and time window;
- normalized field;
- analysis consumer;
- snapshot evidence;
- missing-evidence behavior;
- runtime comparison needed for acceptance.

Prefer native aggregate statistics over reconstructing a displayed value from
lower-level histories or topology. Complete `TEL-01` before accepting revised
rules that depend on affected production or Dyson evidence.

### Review and implementation sequence

- Review the phase records and classification before changing code.
- Produce an explicit old-to-new disposition ledger: retained, changed,
  moved, removed, or new.
- Implement coherent phase or telemetry slices rather than one repository-wide
  rewrite.
- Keep objectives stable while the player remains on the selected phase.
- Validate deterministic analysis first, then compare runtime evidence with the
  native UI and exercise representative saves.
- Require a focused full-playthrough checkpoint before declaring the revised
  guide contract complete.

## Definition of ready

GUIDE-01 may begin when the revision has an immutable version or commit,
the guide author identifies it as ready for ingest, and the artifact can be
hashed and compared with the baseline.

## Definition of done

- Every phase and optional route has an approved extracted record.
- Every old derived rule has an explicit disposition.
- Every displayed objective comes from the revised phase-readiness contract.
- Soft paces, optional paths, and warnings remain distinct from objectives.
- All retained findings have an authoritative evidence mapping.
- Navigation remains entirely player-owned.
- Documentation, contracts, versions, tests, and snapshots match the adopted
  revision.
