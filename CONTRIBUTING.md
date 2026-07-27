# Contributing

Bug reports and focused improvements are welcome.

## Bug reports

Please include:

- DSP and DSP Guide Check versions;
- selected phase and optional route;
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
scrolling, footer controls and deliberate snapshot action still work.
