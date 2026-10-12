# DSP 0.10.35.29131 receiver hotfix roadmap

## Status and authority

This roadmap replaces the maintenance placeholder with one bounded compatibility
hotfix. Implementation and automated validation are complete; the owner runtime
gate is open. The published baseline, source release line and last
owner-confirmed game compatibility remain unchanged until their separate gates
are completed.

The evidence source is the read-only DSP `0.10.35.29131` comparison retained
outside this repository. It confirms that Ray Receiver lens calculations use the
actively consumed lens (`curCatalystId` while `catalystPoint > 0`) before falling
back to buffered lenses (`catalystId` while `catalystCount > 0`). Guide Check's
current sampler instead combines the buffered ID with the active remaining
points. A receiver can therefore be reported as unlensed after its buffer reaches
zero even though an active Graviton Lens is still being consumed.

## RECEIVER-LENS-HOTFIX-01 — Follow native active/buffered lens state

**User story:** As a player using the DYSON receiver objective, I want a Photon
Generation receiver with an actively consumed Graviton Lens to remain recognized
as lensed when its input buffer is empty, so that a valid receiver does not add
false unhealthy samples or regress the objective during the native lens
transition.

### Required behavior

- Keep identifying Ray Receivers through the dedicated generator pool and the
  native `gamma` marker.
- Determine the effective lens using DSP's native precedence:
  1. use `curCatalystId` while `catalystPoint > 0`;
  2. otherwise use `catalystId` while `catalystCount > 0`;
  3. otherwise report no lens.
- Apply that effective state consistently to current-lens and rolling-history
  continuity evaluation.
- Preserve the existing Photon Generation, warmup, strength, sample-window and
  two-unhealthy-sample policies.
- Continue to fail softly if a reflected member is missing or renamed.
- Retain enough diagnostic snapshot evidence to distinguish the active-lens,
  buffered-lens and no-lens cases. If this changes the exported contract, update
  the normalized/exported model deliberately and bump the snapshot schema.

### Exclusions

- No redesign of receiver continuity, recovery or objective policy.
- No changes to phase ownership, navigation, objective wording or panel layout.
- No changes to Dyson power-demand calculations beyond consuming the corrected
  native method result already returned by DSP.
- No adoption of DSP Goal logic or coupling to `Assembly-CSharp.dll` at compile
  time.
- No unrelated DSP `0.10.35.29131` compatibility, UI, Dashboard, blueprint,
  terrain, clock or localization work.
- No version promotion, tagging, packaging or publication in this story.

### Deterministic validation

Focused tests must prove:

1. an active Graviton Lens with remaining points and an empty buffer is lensed;
2. no active lens with a buffered Graviton Lens is lensed;
3. neither an active nor buffered lens is not lensed;
4. a non-lens item ID is never accepted as a Graviton Lens;
5. the current and historical checks use the same effective-lens rule;
6. missing reflective evidence fails softly;
7. the existing receiver-window and two-unhealthy-sample policy is unchanged.

Run the focused receiver suite first, then build both diagnostic and public
variants. Compilation and deterministic tests do not close the runtime gate.

### Runtime acceptance gate

Use DSP `0.10.35.29131` with the diagnostic DLL and a DYSON save containing four
fully warmed Ray Receivers configured for Photon Generation.

1. Establish four healthy, lensed receivers and confirm the objective is ready.
2. On one receiver, stop new lens delivery and let its buffered lens count reach
   zero while its active lens still has remaining points.
3. During that interval, save a diagnostic snapshot. The receiver must remain
   currently lensed and must not gain an unhealthy sample solely because the
   buffer is empty. The snapshot must expose the active-versus-buffered evidence
   used for that conclusion.
4. Let the active lens expire with no buffered replacement. Confirm that the
   receiver then becomes unlensed and the established continuity policy responds
   normally.
5. Restore lens supply and confirm normal recovery, clean logs and no regression
   in receiver requested-power, supplied-power or Critical Photon evidence.

Repeat the pertinent visible behavior with the public DLL. The release owner
must accept the gate before compatibility documentation can name
`0.10.35.29131`.

### Definition of done

- The smallest collector/model change implements the native precedence without
  altering unrelated receiver policy.
- Focused deterministic checks pass.
- Diagnostic and public release builds complete with zero errors.
- Diagnostic evidence proves the active-buffer-empty case and the true no-lens
  case on DSP `0.10.35.29131`.
- The release owner accepts the runtime gate for both variants.
- Affected contracts, compatibility text and changelog are updated only after
  acceptance.
- The completed story and gate record are archived, and the live roadmap returns
  to a maintenance placeholder.

## Sequence

1. ~~Receive explicit implementation authorization.~~ Completed.
2. ~~Implement the collector and evidence correction with focused tests.~~
   Completed.
3. ~~Build both variants and push only when requested.~~ Completed.
4. Run and accept the focused runtime gate.
5. Reconcile compatibility documentation, archive this roadmap and return the
   project to maintenance mode.

No other active epic, story or validation gate is represented here.
