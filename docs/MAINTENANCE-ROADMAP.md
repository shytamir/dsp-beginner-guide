# DSP Guide Check Maintenance Roadmap

## Status and authority

This is the active, bounded maintenance roadmap subordinate to
[`PROJECT.md`](PROJECT.md). The released guide 2.0 product remains complete;
this cycle addresses findings accepted from a full critical-path playthrough.

Work proceeds in the order below. An item becomes implementation work only
when its story or required diagnostic gate is ready. Guide-authoring changes
are deliberately excluded and remain tracked separately outside this roadmap.

| Priority | Item | State | Next gate |
|---:|---|---|---|
| 1 | `WHITE-CONCISE-01` — concise WHITE objectives and status | Accepted | Complete. |
| 2 | `NATIVE-TYPE-01` — match native game-label typography | Accepted | Complete. |
| 3 | `CUBE-BRANCH-01` — useful PURPLE and YELLOW terminal-input states | Runtime validation required | Validate the GREEN-style combined storage objective, terminal-item risks, and unchanged Lab objective. |
| 4 | `RED-DRIVE-II-01` — correct the RED-to-ILS Drive Engine II target | Validation required | Capture a diagnostic snapshot with Drive Engine II researched but still displayed or pending incorrectly. |
| 5 | `EARLY-RISK-01` — BLUE/RED Cube risk and RED refinery backpressure | Validation required | Reproduce above-threshold research drain and each blocked refinery coproduct independently. |
| 6 | `PHOTON-CONTINUITY-01` — tolerate isolated receiver blips | Policy required | Fix the allowed interruption and sustained-failure timings before implementation. |

Enhancing the production-risk analyzer beyond `EARLY-RISK-01` is not active
work. It requires a specific future feature request with a concrete player
problem and evidence source.

## WHITE-CONCISE-01 — Concise WHITE objectives and status

### User story

As a player completing WHITE, I want the panel to state the lab, stored-Cube,
and Mission Completed facts in compact language so I can understand the phase
at a glance without rereading rates, costs, aliases, or flavor text already
communicated elsewhere.

### Product decisions

- Stored White Cubes remain visible. This overrides the playthrough suggestion
  to omit storage and preserves the authoritative WHITE readiness contract.
- Use `White Cubes` in player-facing text. Do not repeat `Universe Matrix` or
  `Universe Matrices` after the phase has established the item.
- The fixed Cube-rate rail remains the rate presentation. Do not repeat the
  measured White-Cube rate in objective evidence.
- Keep the configured-lab count because it directly measures the ten-lab
  objective.
- Do not repeatedly explain the 4,000-Cube research cost.

### Required presentation

The WHITE objective set continues to represent the same three facts:

1. White Cubes are researched.
2. Ten configured Labs sustain the required production, with concise evidence
   containing the configured-Lab count and stored White-Cube count.
3. Mission Completed is pending, queued, progressing, or complete.

Use compact state language equivalent to:

- `White Cubes researched`
- `7/10 labs configured; 1,240 White Cubes stored`
- `Mission Completed not queued`
- `Mission Completed queued`
- `Mission Completed 37% done`
- `Mission Completed complete`

Until Mission Completed is complete, its single Pending instruction is:
`Complete Mission Completed research.` Do not mention 4,000 White Cubes there.

### Evidence and failure behavior

- Preserve the existing authoritative research, configured-recipe, production,
  and owned-storage evidence.
- Obtain active Mission Completed progress from an authoritative game research
  field. Do not infer progress from Cube consumption or inventory deltas.
- If exact progress is unavailable, fail softly to the strongest known state
  (`not queued` or `queued`) without displaying a fabricated percentage.
- This story changes presentation, not the 40/min readiness threshold, the
  ten-Lab requirement, storage collection, phase selection, or completion
  semantics.

### Acceptance criteria

- WHITE uses `White Cubes` consistently and contains no repeated parenthetical
  Matrix alias.
- The lab evidence shows configured Labs and stored White Cubes but does not
  duplicate the Cube rate.
- Mission Completed shows the most precise authoritative compact state
  available.
- Pending contains only the concise Mission Completed action while incomplete.
- Completed objectives, risk rows, Next Actions, navigation, collapse, the
  Cube-rate rail, and both public and diagnostic builds remain functional.
- Deterministic model checks cover unresearched White Cubes, incomplete Labs,
  unqueued Mission Completed, queued research, active progress when available,
  and completion.
- Runtime validation captures the unqueued, queued or active, and completed
  presentations; a diagnostic snapshot audits the underlying evidence.

### Implementation status

Accepted after diagnostic evidence confirmed authoritative unqueued and active
Mission Completed progress. The release owner accepted the concise WHITE
presentation without a discovered regression; both DLL build contracts pass.

## NATIVE-TYPE-01 — Match native game-label typography

### User story

As a player using the guide panel, especially at 4K, I want its text and Cube
rates to use the same compact, heavy, strongly outlined presentation as DSP's
world-space labels so the companion reads as part of the game UI instead of a
visually separate overlay.

### Native reference

The visual references are DSP's Dark Fog element names and the vein labels
shown when vein-distribution detail is enabled. The desired presentation is
shorter and heavier than the current Basic Regular text, with a thicker,
darker outline.

### Scope

- Apply the native presentation to the collapsible panel's headings, objective
  and status text, navigation labels, and the collapse-proof Cube-rate text.
- Preserve the published-guide phase icons, Matrix icons, phase colors, risk
  glyphs, hierarchy, wrapping, alignment, and interaction behavior.
- `DON'T PANIC` retains its intentionally separate bright-red Comic Sans
  treatment.
- Reuse the installed game's runtime font, material, or equivalent native UI
  styling. Do not copy or redistribute game assets.
- If the expected runtime resource is unavailable or renamed, fail softly to
  the current embedded Basic Regular presentation and log at most one concise
  diagnostic warning.

### Acceptance criteria

- Runtime inspection identifies and documents the exact game font/material
  source and outline settings used for the chosen native reference.
- Panel text and Cube rates visibly match the weight, proportions, and dark
  outline of that reference at 4K.
- Text remains legible at 1080p and 4K without clipping, new overlap, unstable
  wrapping, or an excessive increase in panel height.
- Collapsed and expanded layouts remain correct as the visible Cube count
  changes.
- The risk glyph, Cube column, `DON'T PANIC`, navigation, scrolling, and the
  diagnostic-only snapshot control retain their positions and interaction
  behavior.
- Font/material lookup and reuse do not add per-frame searches, allocations,
  or copied game assets to the repository or package.
- Both public and diagnostic builds compile cleanly and pass focused runtime
  screenshots at 1080p and 4K.

### Implementation status

Accepted after release-owner visual validation; both DLL build contracts pass.
Runtime assembly inspection confirmed the source path
`UIRoot.instance.uiGame.veinDetail.nodePrefab.infoText`; the panel reuses
that live Text component's font, material, font style, line spacing, and
attached Shadow or Outline effects once at creation. A failed first gate proved
the original Outline requirement was too strict; the corrected lookup also
checks loaded vein-detail nodes once when the serialized Text is not ready.
Embedded Basic Regular remains the soft fallback, and diagnostics expose the
resolved resource and settings. After correcting the first gate's fallback,
the release owner confirmed the native presentation visibly matches the game.

## Remaining ordered validation work

### CUBE-BRANCH-01

#### User story

As a player entering YELLOW or PURPLE, I want the panel to confirm that both
terminal Cube ingredients are visibly buffered, just as GREEN does, so the
phase shows the useful convergence state without exposing or modeling each
internal production branch.

#### Product decisions

- Reuse GREEN's established framework: one stable combined storage objective,
  one Pending action while either terminal input is absent, and independent
  production-risk monitoring for the two terminal inputs and the Cube.
- YELLOW tracks Diamonds and Titanium Crystals. PURPLE tracks Processors and
  Particle Broadband.
- Visible storage means the existing normalized owned-item evidence and the
  same positive-count rule used by GREEN.
- Do not introduce substages, partial branch objectives, branch-completion
  state, topology inference, or intermediate-component tracking. In
  particular, Carbon Nanotubes are not a PURPLE terminal input.
- Preserve each phase's existing three-configured-Lab continuous-production
  objective as a separate hard condition.

#### Acceptance criteria

- Each phase exposes exactly one combined terminal-input objective with both
  owned counts in its evidence.
- The objective remains blocked when neither or only one terminal input is
  owned, becomes ready when both counts are positive, and supplies one concise
  instruction to buffer both inputs while blocked.
- Draining or starved terminal inputs and Cubes remain independently eligible
  for the bounded Current Status and Next Actions risk presentation.
- Compact snapshots contain the two terminal inputs and Cube for the selected
  phase; PURPLE no longer routes Carbon Nanotube evidence.
- GREEN behavior is unchanged, objectives remain stable, and no evidence can
  change player-owned phase selection.
- Public and diagnostic DLLs build and pass the focused deterministic contract
  checks before runtime validation.

#### Implementation status

Implemented for the runtime gate. Deterministic checks cover empty, one-input,
and both-input storage states for YELLOW, PURPLE, and unchanged GREEN behavior;
the diagnostic artifact still requires in-game acceptance.

### RED-DRIVE-II-01

Validate the Drive Engine II target that carries the player from RED into ILS.
With ILS selected and Drive Engine II visibly researched, capture the incorrect
label, completion state, and Pending row plus one diagnostic snapshot. Use that
evidence to locate whether the defect is the technology ID, unlocked-tech
collection, level-aware naming, or stale presentation.

### EARLY-RISK-01

For BLUE and RED, capture deliberate research consumption above the guide's
accepted Cube pace while production also remains above that pace. Establish
whether a draining warning is useful, should be delayed, or should be
suppressed until the buffer becomes materially threatened. Separately block
RED's Refined Oil and Hydrogen outputs one at a time and prove that each real
deadlock receives concise actionable guidance while healthy coproduct flow
remains quiet.

### PHOTON-CONTINUITY-01

Define a bounded grace rule for an isolated unhealthy receiver sample and a
separate duration that represents a sustained failure. Validate healthy,
single-blip recovery, repeated interruption, and sustained loss so completed
continuity does not flicker while real receiver failure still revokes it in a
reasonable time.
