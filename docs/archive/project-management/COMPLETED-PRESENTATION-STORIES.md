# Completed Presentation Stories

These accepted stories are retained as historical project-management records.
The current panel contract is authoritative in
[`docs/PROJECT.md`](../../PROJECT.md).

## TITLE-PRESENTATION-01 - Match the published guide's phase titles

**Final status:** Accepted on public package 2.0.52.

As a player moving between the guide and Guide Check, I wanted the panel's
phase tag to use the same icon, bracketed name, and color as the published
guide so each phase was immediately recognizable.

The accepted gate covered all nine colors and icon resources, readable title
wrapping, soft text-only fallback, cached non-interactive icons, and unchanged
navigation, collapse, scrolling, rate-column, and build-variant behavior.
BLUE, ILS, DYSON, and PHOTON screenshots supplied the focused runtime evidence.

## PANIC-01 - Keep the source-guide control clear and available

**Final status:** Accepted after diagnostic and public runtime validation.

As a player using Guide Check on either a short or collapsed panel, I wanted
the small `DON'T PANIC` source-guide control to remain available without
colliding with Cube rates so I could open the selected guide phase from every
panel state.

The accepted implementation moved the control to the fixed Cube-rate and risk
rail, immediately below the last displayed Cube with matching right edges.
The release owner confirmed both DLL variants, short and collapsed panels,
dynamic Cube-count repositioning, diagnostic snapshot-control independence,
and clean logs.
