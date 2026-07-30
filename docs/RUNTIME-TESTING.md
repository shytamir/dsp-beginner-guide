# DSP Guide Check - Runtime Validation

This protocol uses only Dyson Sphere Program with the mod installed through
BepInEx. It requires no coding task or other software.

## Preparation

1. Install the v1.16.0 DLL in the usual BepInEx plugin location.
2. Load the late-game PHOTON save that previously showed periodic stuttering.
3. Let the game run normally for at least one minute.

## Performance

Observe the stationary game for at least one minute with the guide panel
closed, then for another minute with PHOTON open.

Expected:

- no noticeable five-second hitch;
- no new performance drop while the panel is open;
- receiver counts and Current Status continue to refresh normally.

## Receiver continuity

Use a save with four configured Photon Generation receivers.

Expected:

- deployed and Photon Generation receiver counts are correct;
- lenses, warmup, strength, and continuity preserve the accepted PHOTON
  behavior;
- continuity completes after at least 60 healthy game seconds;
- changing receiver mode or losing a lens still interrupts continuity.

## `DON'T PANIC`

Open any expanded phase panel and inspect the right footer control.

Expected:

- it reads `DON'T` above `PANIC`;
- it uses Comic Sans, a slightly larger size, and bright red text;
- it remains clear of native lower-right controls;
- clicking it opens the selected phase in the published source guide.

Also confirm navigation, collapse/expand, scrolling, `Save snapshot`, and panel
layout show no regression.

## Snapshot

From the most informative PHOTON state:

1. Click `Save snapshot`.
2. Confirm one JSON is saved and its directory opens.
3. Attach the JSON with your report.

The snapshot will be checked for:

- exporter 1.16.0 and schema 2.0;
- unchanged PHOTON objective and receiver evidence;
- `dysonAndReceivers` sampler duration no longer showing the former large
  periodic spike;
- compact research, Cube, selected-phase and collector evidence;
- no repeated raw factory, player, all-technology, all-item or normalized-state
  sections;
- explicit omission and receiver-truncation markers where applicable;
- a saved file no larger than 256 KiB.

## Report

Please report:

- whether either one-minute observation showed a five-second hitch;
- whether receiver counts and continuity remained correct;
- whether the `DON'T PANIC` appearance and link behaved as intended;
- whether any performance, layout, navigation, or control regression appeared;
- the saved JSON.

# v1.15.1 phase-persistence checkpoint

- Load a save and use the panel controls to select a phase that differs from
  the latest researched Cube.
- Leave the game paused for at least 60 seconds; confirm the selected phase
  does not change.
- Allow an autosave or create a differently named manual save; confirm the
  selected phase does not change.
- Exit DSP, relaunch it and resume the same playthrough; confirm the selected
  phase is restored.
- Complete research or a phase objective while the panel is open; confirm the
  selected phase does not change.
- Save one snapshot and confirm `guideSelection.identityVersion` is
  `creation-time-v2`, `automaticTransitionsEnabled` is `false`, and
  `persistenceState` reports a stable-key restore, migration, seed or explicit
  player update consistent with the test.

The schema 2.0 snapshot checks above are part of the next phase's test cycle;
no separate legacy-snapshot migration checkpoint is required.
