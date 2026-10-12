# Completed DSP 0.10.35.29131 receiver hotfix

## Status and authority

This bounded compatibility hotfix completed on 2026-10-12 after implementation,
automated validation and release-owner validation with both diagnostic and public
variants. It remained subordinate to [`PROJECT.md`](../../PROJECT.md), which
continued to define the maintained product contract. No active implementation
story or validation gate remained in this record.

The evidence source was the read-only DSP `0.10.35.29131` comparison retained
outside this repository. It confirmed that Ray Receiver lens calculations use the
actively consumed lens (`curCatalystId` while `catalystPoint > 0`) before falling
back to buffered lenses (`catalystId` while `catalystCount > 0`). Guide Check's
previous sampler instead combined the buffered ID with the active remaining
points, allowing a receiver to be reported as unlensed after its buffer reached
zero even though an active Graviton Lens was still being consumed.

## RECEIVER-LENS-HOTFIX-01 — Follow native active/buffered lens state

**User story:** As a player using the DYSON receiver objective, I want a Photon
Generation receiver with an actively consumed Graviton Lens to remain recognized
as lensed when its input buffer is empty, so that a valid receiver does not add
false unhealthy samples or regress the objective during the native lens
transition.

### Implemented behavior

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

### Deterministic validation completed

Focused tests proved:

1. an active Graviton Lens with remaining points and an empty buffer is lensed;
2. no active lens with a buffered Graviton Lens is lensed;
3. neither an active nor buffered lens is not lensed;
4. a non-lens item ID is never accepted as a Graviton Lens;
5. the current and historical checks use the same effective-lens rule;
6. missing reflective evidence fails softly;
7. the existing receiver-window and two-unhealthy-sample policy is unchanged.

The focused receiver suite ran before both diagnostic and public builds.
Compilation and deterministic tests remained distinct from the subsequently
accepted runtime gate.

### Runtime acceptance evidence

The release owner used DSP `0.10.35.29131` with the diagnostic DLL and a DYSON
save containing four fully warmed Ray Receivers configured for Photon Generation.

1. `DSP-Status-20261012-143409-ElPhekrah III.json` established four healthy,
   actively lensed receivers under snapshot schema 2.25 and plugin 3.2.113.
2. `DSP-Status-20261012-143811-ElPhekrah III.json` captured receiver entity
   `14710` with `currentCatalystId` 1209, two active catalyst points, zero
   buffered catalyst ID/count, effective catalyst ID 1209 and `lensedNow: true`.
   This directly proved the corrected active-lens/empty-buffer behavior.
3. The owner reproduced active expiry and restored delivery, observed the
   established continuity behavior, and found no visible regression.
4. The owner repeated the pertinent behavior with the public DLL and accepted
   both variants. No snapshot files were committed because they contain player
   save evidence.

### Completed acceptance criteria

- The smallest collector/model change implemented the native precedence without
  altering unrelated receiver policy.
- Focused deterministic checks passed.
- Diagnostic and public release builds completed with zero errors.
- Diagnostic evidence proved the active-buffer-empty case and the true no-lens
  case on DSP `0.10.35.29131`.
- The release owner accepted the runtime gate for both variants.
- Affected contracts, compatibility text and changelog were updated only after
  acceptance.
- The completed story and gate record were archived, and the live roadmap returned
  to a maintenance placeholder.

## Sequence

1. Explicit implementation authorization was received.
2. The collector and evidence correction were implemented with focused tests.
3. Both variants built and CI run 113 completed successfully from commit
   `d46a7bf50f6b2bf9590ae365f3d3f79a907ef3dd`.
4. The focused runtime gate was accepted by the release owner on 2026-10-12.
5. Compatibility documentation was reconciled, this record was archived and the
   project returned to maintenance mode.

No further obligation is carried forward from this completed record.
