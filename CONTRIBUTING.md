# Contributing

Bug reports and focused improvements are welcome.

## Bug reports

Please include:

- DSP and DSP Guide Check versions;
- selected critical-path phase;
- what was expected and what appeared instead;
- a screenshot when presentation or wording is involved;
- a snapshot created with `Save snapshot` when runtime analysis is involved;
- whether the problem repeats after reloading the save.

Snapshots can contain save and planet names. Review them before publishing.

## Changes

Keep the product invariants in `docs/PROJECT.md` intact:

- phase selection belongs to the player;
- objectives remain stable within a selected phase;
- Current Status communicates conclusions rather than dumping every rate;
- optional guidance never becomes a hidden gate;
- missing evidence fails softly;
- the mod remains passive and read-only.

Build against a local DSP installation and verify that the panel, navigation,
scrolling, fixed Cube rail, `DON'T PANIC`, and the diagnostic snapshot action
still work. The current [Guide 3.0 roadmap](docs/ROADMAP.md) is accepted for
implementation; its execution policy consolidates human
validation into the final workshop. Proposed features should identify the
published-guide change, reproducible defect, or in-scope unmet player need
they address. Consult [PROJECT.md](docs/PROJECT.md) for current authority.

## Guide 3.0 candidate checks

Run `pwsh -NoProfile -File scripts/Test-Guide3Candidate.ps1` on a clean checkout.
This PowerShell 7 command builds both local-game variants, runs retained and
story fixtures, checks compact parity/size and package identity, and retains a
hashed candidate under ignored artifacts. See the README for GameRoot, SDK
access and sequence options. Hosted CI runs Test-Guide3Portable for both DLLs;
production lookup, full phase/typography contracts, collector normalization,
BepInEx config/hidden-controller and snapshot fixtures require local game
references. None substitutes for the final owner workshop.
