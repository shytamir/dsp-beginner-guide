# Guide 2.0 Gap Analysis and Migration Roadmap

## Source and scope

- Published guide: <https://dsp-beginner-guide.pages.dev/>
- Edition reviewed: public 2.0 release
- Published `guide-version` metadata: `1.23.0`
- Review date: 2026-08-04

This report compares the published guide with the current DSP Guide Check
contract. It is preparation only: no runtime, analysis, snapshot, navigation,
or panel code changes are part of this pass.

The retained mod scope is the guide's default critical path only:

```text
BLUE -> RED -> ILS -> YELLOW -> PURPLE -> GREEN
     -> DYSON -> PHOTON -> WHITE
```

FLIGHT and TITANIUM remain internal ILS checkpoints. WARP, SPHERE, LOGISTICS,
combat guidance, alternatives, and post-completion sandbox routes remain out
of scope.

## Executive conclusion

The new guide permits a net simplification. Its largest structural change is
the consolidation of the former standalone BOOTSTRAP phase into BLUE. Most
later readiness contracts remain close to the accepted mod behavior, so they
need a focused wording and evidence review rather than a new analysis system.

The guide's new images do not justify adding decorative art throughout the
panel. Images should be adopted where they replace an existing identity cue
or make tracked state easier to read. The first such use is the Cube-rate
column: its six colored backgrounds can be replaced by the six authorized
Matrix icons already present and mapped in the repository.

The production-risk roadmap is paused because of its blocking concern. It is
not a fallback authority for this migration and must not expand the guide 2.0
stories.

## Gap inventory

| Area | Published guide 2.0 | Current mod | Required disposition | Story |
|---|---|---|---|---|
| Phase topology | Starter factory work is part of BLUE; there is no standalone BOOTSTRAP phase. | Navigation, analysis, snapshots, persistence, and links still expose ten phases including BOOTSTRAP. | Remove BOOTSTRAP as a selectable contract, fold its useful evidence into a compact BLUE presentation, and normalize a stored BOOTSTRAP selection to BLUE. | `GUIDE2-01` |
| BLUE | Covers the starter factory, unattended Blue research, and enough grid headroom to begin RED. | Starter automation and Blue readiness are split across two panels. | Preserve the useful factory-readiness evidence without copying every mall item or imposing a fixed power target. | `GUIDE2-01` |
| RED | Two Red-Cube Labs run continuously and Refined Oil retains an outlet. | The current contract also emphasizes both refinery outputs as hard readiness text. | Align the stable objective and warning language with the local checklist; keep deadlock evidence diagnostic rather than adding rows. | `GUIDE2-02` |
| ILS | Retains preparation, expedition, return manifest, and automated Titanium/Silicon routes; starter-system reconnaissance is advice. | The three checkpoint model already matches the mission, but reconnaissance is not represented. | Keep reconnaissance out of the panel because it is neither a stable gate nor a retained optional-route feature; confirm checkpoint wording against the new guide. | `GUIDE2-02` |
| YELLOW | Three Yellow-Cube Labs run continuously. | Same core objective. | Verify terminology and remove any now-superfluous supporting text. | `GUIDE2-02` |
| PURPLE | Three Purple-Cube Labs run continuously; Processor and Particle Broadband stores identify the branch to expand. | Same broad evidence exists, with historical risk of showing every healthy rate. | Keep one stable phase objective and surface only an actionable draining branch in Current Status. | `GUIDE2-02` |
| GREEN | Two Green-Cube Labs run continuously with Quantum Chips and Graviton Lenses visibly buffered. | Same broad objective and storage evidence. | Retain the compact objective; avoid restoring branch-rate dashboard clutter. | `GUIDE2-02` |
| DYSON | Renamed around building the Photon swarm: produce and launch Solar Sails and establish useful swarm output. The move-on prose hands off toward reliable Antimatter. | Current objectives already track sail production, launches, and swarm generation under older wording. | Rename and align the phase. Treat Antimatter as a handoff cue to PHOTON, not a duplicate hard DYSON objective or automatic transition rule. | `GUIDE2-02` |
| PHOTON | Four lensed receivers, Photon-to-Antimatter conversion, a 48/min comfortable reference, and the 2,000-Antimatter midpoint. | Current focused evidence is close to this contract. | Recheck titles, reference wording, receiver demand/capacity status, and midpoint presentation only. | `GUIDE2-02` |
| WHITE | Universe Matrix, six live inputs, ten Labs at 40/min, and Mission Completed. | The panel tracks research, White production, stored White Cubes, and Mission Completed; the adjacent Cube column already represents feeder rates. | Do not add a verbose six-input objective. Use the Cube column or one exceptional status conclusion when a feeder actually fails. | `GUIDE2-02` |
| Cube targets | Blue, Red, Yellow, Purple, and Green pace bands are unchanged. | The accepted Cube-rate column already uses those thresholds but identifies Cubes with flat colored squares. | Preserve threshold logic and replace the backgrounds with authorized Matrix icons; color only the rate text. | `VIS2-01` |
| Images | Game images now carry more identity in the guide. | The panel uses text and flat color cues; the assembly embeds only the presentation font. | Adopt images only where they improve a tracked element. Embed and cache the six Matrix icons; do not create a general decorative-image framework. | `VIS2-01` |
| Superseded contracts | Nine critical-path phases and consolidated BLUE. | BOOTSTRAP-only rules, evidence routing, snapshot fields, tests, and documentation may remain after migration. | Remove only genuinely orphaned consumers, update contracts deliberately, and retain audit evidence needed by the remaining phases. | `GUIDE2-03` |
| Project state | Guide 2.0 is the new authority. | Root and management docs still call guide 1.22.2 current and RISK-02 active. | Make guide migration the immediate roadmap, mark risk work paused, and update public/runtime documentation when behavior lands. | `GUIDE2-03` |

## Image adoption plan

The authoritative asset map already binds the required item identities:

| Item | Matrix | Repository asset |
|---:|---|---|
| 6001 | Electromagnetic Matrix | `t-matrix.png` |
| 6002 | Energy Matrix | `e-matrix.png` |
| 6003 | Structure Matrix | `c-matrix.png` |
| 6004 | Information Matrix | `i-matrix.png` |
| 6005 | Gravity Matrix | `g-matrix.png` |
| 6006 | Universe Matrix | `u-matrix.png` |

All six files exist under the authorized exported-assets tree. Implementation
should embed those exact files in the plugin, decode and cache them once, and
reuse the existing 44-pixel click-through column slot. A roughly 32-pixel
icon leaves enough room for the outlined `/m` rate while remaining readable
at the current panel scale. Exact placement is accepted by an in-game
screenshot, not by code measurements alone.

The icon communicates Cube identity. Threshold state remains in the rate text:

- red only for the current phase's Cube (or the latest preceding Cube in a
  non-Cube phase) when it is below its minimum;
- orange at the initial minimum;
- white at the comfortable pace; and
- green at the later target.

Missing or unreadable embedded image data must fail softly to a compact
text-only rate tile. Loading, decoding, or resizing must not occur on the
sampling cadence or per frame.

## Immediate roadmap

### GUIDE2-01 — Consolidate BOOTSTRAP into BLUE

**Status:** Accepted. Early-save and working-Blue runtime checks passed with
correct objectives, prompt completion, and no visible regression.

**User story**

As a guide reader starting a new factory, I want the first mod phase to match
the consolidated BLUE phase so that starter automation and Blue research read
as one coherent objective rather than two panels.

**Acceptance criteria**

- The selectable sequence contains the nine retained phases and begins at
  BLUE.
- BLUE owns compact evidence for continuous starter inputs, routine hardware
  replenishment, continuous Blue-Cube production at the guide minimum, and
  unattended research.
- The panel does not enumerate every mall product or impose the guide's
  15–20 MW planning target as a fixed objective.
- A stored BOOTSTRAP selection normalizes once to BLUE; player-owned manual
  navigation and per-playthrough persistence otherwise remain unchanged.
- Initial seeding, source-guide anchors, snapshots, and deterministic tests
  recognize BLUE as the first phase.
- No runtime evidence automatically changes the selected phase.

**Validation gate**

- Test an early save before Blue production and another with a working mall
  and continuous Blue production.
- Confirm first-use seeding, legacy BOOTSTRAP normalization, navigation,
  persistence, panel compactness, and focused snapshot evidence.

### GUIDE2-02 — Realign and simplify the nine phase contracts

**Status:** Accepted. Deterministic checks pass. The focused runtime gate used
plugin 1.18.40 with analysis 2.8 and snapshot schema 2.7. Navigation, retained
objectives, deliberate omissions, RED behavior, focused PURPLE, GREEN, and
WHITE deficit findings, the corrected DYSON contract, and PHOTON receiver
status all matched the intended contract. An earlier attempt with schema 2.6
was excluded as pre-contract evidence.

**User story**

As a player moving through the critical path, I want each selected panel to
express the local guide 2.0 readiness contract and only actionable supporting
context so that the companion teaches without becoming a rate dashboard.

**Acceptance criteria**

- Titles, stable objectives, Pending actions, and Current Status are
  re-derived for BLUE through WHITE from the published 2.0 edition.
- Exact numbers are hard objectives only where the local phase contract makes
  them exact; planning targets and comfort references remain status context.
- RED keeps refinery deadlock evidence concise; PURPLE and GREEN show only an
  actionable weak or draining branch; healthy supporting chains do not add
  completed rows.
- ILS retains its three checkpoint model and omits reconnaissance advice from
  the panel.
- DYSON uses the Photon-swarm contract. Antimatter is a handoff to PHOTON, not
  a DYSON gate, duplicated objective, or navigation trigger.
- PHOTON retains four-receiver continuity, actual Photon and Antimatter rates,
  the 48/min reference, the 2,000-Antimatter midpoint, and useful
  demand-versus-generation status.
- WHITE does not repeat all six feeder inputs as prose when the Cube-rate
  column already carries them; it reports an exceptional feeder problem only
  when one is actionable.
- Optional paths and combat guidance remain absent.

**Validation gate**

- Run representative checkpoints for all nine selected phases, with focused
  before/after cases for RED deadlock, one weak PURPLE branch, one weak GREEN
  branch, DYSON-to-PHOTON handoff, and WHITE feeder failure.
- Compare panel conclusions and saved snapshots with the matching guide
  sections and native Statistics/Dyson UI evidence.

### VIS2-01 — Replace Cube-rate color tiles with Matrix icons

**User story**

As a player glancing at Cube production, I want each rate paired with its
actual Matrix icon so that Cube identity is immediate without expanding or
cluttering the guide panel.

**Acceptance criteria**

- The six strictly mapped Matrix PNGs are embedded from the authorized asset
  tree and loaded through one bounded, cached resource path.
- Each existing 44-pixel column slot displays a clear Matrix icon at an
  appropriate scale and the current `/m` rate; the old flat Cube-color
  background is removed.
- Threshold state is communicated by the outlined rate-text color, preserving
  the accepted minimum, comfortable, and later-target rules.
- Only the current or most recent applicable Cube can turn red below minimum.
- The column remains click-through, independent of expand/collapse, and does
  not allocate or decode images on the sampling cadence or per frame.
- A missing or invalid image fails softly to a readable text-only tile.
- Asset permission, attribution, packaging, and map references remain intact.

**Validation gate**

- Capture bright- and dark-background screenshots with one, three, and six
  visible Cubes.
- Verify icon identity, rate legibility, threshold transitions, alignment,
  click-through behavior, collapse independence, and absence of new hitches.

### GUIDE2-03 — Prune superseded contracts and close the migration

**User story**

As a maintainer, I want obsolete guide-1.22.2 and BOOTSTRAP-only contracts
removed after the new phase rules land so that the implementation and
diagnostics have one current authority without losing useful audit evidence.

**Acceptance criteria**

- BOOTSTRAP-only analysis, presentation, evidence routing, snapshot output,
  tests, and links are inventoried and removed only when they have no retained
  consumer.
- Snapshot and normalized-state versions change only if their serialized
  contracts actually change; selected-phase evidence remains sufficient to
  audit every retained conclusion.
- Root README, project definition, runtime test protocol, packaging README,
  source-guide anchors, and historical references distinguish current guide
  2.0 behavior from superseded releases.
- The production-risk roadmap remains documented but paused until its blocker
  is resolved; no risk behavior is folded into the guide migration.
- Release, BepInEx, snapshot, analysis, progression, and panel contracts are
  synchronized with the behavior that actually ships.

**Validation gate**

- Build with zero errors, run deterministic tests, inspect a snapshot from
  each retained phase family, and complete one manual navigation/persistence
  sweep.
- Confirm no BOOTSTRAP or guide-1.22.2 phase contract appears in current
  runtime output while historical records remain available.

## Sequence

```text
GUIDE2-01 -> GUIDE2-02 -> VIS2-01 -> GUIDE2-03
```

`VIS2-01` can be implemented after the nine-phase topology is stable. It does
not depend on the production-risk roadmap.

## Explicitly deferred

- `RISK-02` through `RISK-04` and their runtime gates, pending resolution of
  the blocking concern.
- Decorative phase art, building illustrations, recipe images, or a generic
  image-card framework.
- Optional WARP, SPHERE, or LOGISTICS panels.
- Starter-system recommendation scoring, rare-resource advice, combat
  guidance, factory grading, build planning, or automatic phase selection.
