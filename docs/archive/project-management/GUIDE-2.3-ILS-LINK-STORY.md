# GUIDE23-LINK-01 - Checkpoint-aware ILS source guide link

## Status and authority

Implemented for the adopted public guide 2.3 edition on 2026-08-09. This was a
completed implementation record and carried no active work.

## User story

As a player returning to the multi-stage ILS expedition, I wanted `DON'T PANIC`
to open the source guide at the checkpoint currently shown by DSP Guide Check
so that I resume the relevant preparation, expedition, or automation guidance
instead of rereading the phase entrance.

## Acceptance criteria

- ILS preparation opens the published `#flight` stage.
- An active off-world production or haulback checkpoint opens `#titanium`.
- The ILS research, protected-reserve, and route-automation checkpoint opens
  `#ils-automate`.
- Every non-ILS phase continues to open its existing phase anchor.
- Missing or unrecognized ILS gate evidence falls back to `#ils`.
- Runtime evidence affects only the destination of the deliberate guide-link
  click; it never changes or persists the player-selected phase.
- Checkpoint resolution belongs to the presentation model. The Unity
  controller performs no runtime inference.
- The presentation-only anchor was excluded from the snapshot contract, so no
  snapshot schema or serialized panel-contract change was required.
- Focused deterministic coverage and the release build pass with zero errors.

## Implementation boundary

`GuideGateEngine` remained the sole authority for the active ILS checkpoint.
`GuidePanelModelBuilder` maps the checkpoint's existing condition IDs to the
three stable guide 2.3 anchors, and `GuidePanelController` opens the resolved
anchor. The established `#ils` phase anchor remained the soft fallback.

The same change adopted guide 2.3 in repository authority metadata. It
did not add optional-route panels, stage persistence, automatic phase
navigation, new telemetry, or a serialized field.
