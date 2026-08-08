# DSP Guide Check Completed Maintenance Roadmap

## Status and authority

This bounded maintenance cycle concluded after the release owner's full
critical-path playthrough and focused runtime gates. It remained subordinate
to [`PROJECT.md`](PROJECT.md), which continued to define the product contract.
No active implementation story or validation gate remained in this roadmap.

Guide-authoring observations were kept outside the mod roadmap. RED coproduct
diagnosis was also excluded because it contradicted the adopted concise RED
contract and would have required disproportionate engineering to attribute
belt, sorter, tank, depot, and logistics backpressure reliably.

| Priority | Item | Resolution | Outcome |
|---:|---|---|---|
| 1 | `WHITE-CONCISE-01` | Accepted | WHITE objectives and Mission Completed status became concise without removing stored-Cube evidence. |
| 2 | `NATIVE-TYPE-01` | Accepted | Panel text and Cube rates adopted the game's vein-label typography and retained a soft fallback. |
| 3 | `CUBE-BRANCH-01` | Accepted | YELLOW and PURPLE adopted GREEN-style terminal-input tracking; the gate also accepted the ILS starter-planet exclusion. |
| 4 | `RED-DRIVE-II-01` | Rejected | Evidence showed an unsupported Drive Engine Lv2/Lv3 conflation rather than a product defect. |
| 5 | `CUBE-TARGET-RISK-01` | Accepted | Exact BLUE, RED, and WHITE Cube goals suppressed demand-only risk while the target remained satisfied. |
| 6 | `PHOTON-CONTINUITY-01` | Accepted | Receiver continuity tolerated two unhealthy samples and failed on the third retained sample. |

## Historical story record

### WHITE-CONCISE-01

The story preserved stored White Cubes as authoritative readiness evidence but
removed repeated Matrix aliases, duplicated Cube rates, research-cost flavor
text, and unnecessary Pending instructions. WHITE retained the configured-Lab
count, stored-Cube count, and the strongest authoritative Mission Completed
state: not queued, queued, progressing, or complete.

Diagnostic evidence confirmed unqueued and active Mission Completed progress.
The release owner accepted the concise presentation without a discovered
regression, and both DLL build contracts passed.

### NATIVE-TYPE-01

The panel adopted the installed game's live vein-label Text style from
`UIRoot.instance.uiGame.veinDetail.nodePrefab.infoText`. It reused that Text
component's font, material, font style, line spacing, and attached Shadow or
Outline effects once during panel creation. A bounded loaded-node lookup
covered cases where the serialized Text was not ready, while embedded Basic
Regular remained the soft fallback.

The first visual gate exposed an overly strict lookup assumption. After that
assumption was corrected, the release owner confirmed that panel text and Cube
rates visibly matched the native presentation without layout or interaction
regressions.

### CUBE-BRANCH-01

YELLOW and PURPLE reused GREEN's established terminal-input framework instead
of introducing branch progression modeling. YELLOW tracked Diamonds and
Titanium Crystals; PURPLE tracked Processors and Particle Broadband. Each phase
retained its independent configured-Lab objective, combined positive-storage
objective, and risk eligibility for both terminal inputs and its Cube.

The accepted gate also covered the ILS hotfix that excluded the native birth
planet from expedition evidence. Starter-planet Stone-to-Silicon production
therefore no longer selected the outpost stage. Public and diagnostic builds,
focused deterministic checks, and release-owner runtime validation passed.

### RED-DRIVE-II-01

The candidate was rejected because it conflated two separate ILS requirements.
Preparation correctly checked technology `2902`, Drive Engine Lv2. The later
research-rush chain intentionally checked technology `2903`, Drive Engine Lv3,
before Interstellar Logistics System. DSP's runtime prototype name sometimes
rendered the later target as only `Drive Engine`, which made the correct Lv3
requirement look like an uncleared Lv2 requirement.

No naming or completion change was authorized. The cycle established a
same-state screenshot and diagnostic snapshot, with evidence distinguishing
technologies `2902` and `2903`, as prerequisites for accepting any future
recurrence as a new maintenance candidate.

### CUBE-TARGET-RISK-01

The story established that an exact phase goal dominated demand-driven Cube
risk. BLUE at 20/min, RED at 20/min, and WHITE at 40/min remained quiet when
current cluster production met or exceeded the goal, even when faster research
consumed stored surplus. Diagnostics preserved the unmodified rates and raw
demand deficit while identifying target satisfaction separately. Below-target
and non-exact risks retained the established analyzer behavior.

Post-warmup RED snapshots proved that 30/min production against 60/min
consumption and 21/min against 50/min remained quiet and target-satisfied,
while 15/min against 45/min became actionable and draining. The public DLL
reproduced the pertinent presentation states without regression. The shared
mechanism and deterministic phase coverage made a separate WHITE runtime
capture unnecessary for acceptance.

### PHOTON-CONTINUITY-01

The story retained the existing rolling 60-second receiver history,
approximately five-second sampling cadence, and ten-sample readiness minimum.
A ready receiver history remained sustained with zero, one, or two unhealthy
samples; the third unhealthy sample revoked sustained status. An unhealthy
sample meant the receiver was not configured for Critical Photon production,
lacked a Graviton Lens, fell below full strength, or fell below full warmup
under the existing thresholds.

Current Photon Generation configuration and lens presence remained immediate
requirements for the four-receiver objective. No recovery state, recovery
timer, or consecutive-sample model was introduced; status recovered naturally
as unhealthy samples left the retained window.

Deterministic checks passed the unready, zero-, one-, two-, three-, and
rolling-recovery boundaries in both builds. Release-owner runtime checks
confirmed that a sustained receiver interruption revoked the objective and
that restoring the receiver restored completion without a regression. A
schema-2.16 diagnostic snapshot then confirmed the retained-history integration:
all four receivers were currently configured, lensed, full-strength, and
continuous, while their ready 14-sample histories contained six to eight
unhealthy samples against the allowance of two; all four correctly remained
not sustained and the objective remained blocked.

The exact short-blip boundary was not assigned to manual runtime testing
because DSP receiver mode changes also reset strength and warmup, making a
controlled one- or two-sample interruption impractical. The deterministic
collector-policy checks covered that boundary, while runtime validation covered
the real interruption, retained-history, recovery, presentation, and logging
paths.

## Closeout

The cycle ended with five accepted stories, one evidence-based rejection, and
no active work. DSP Guide Check returned to maintenance mode pending a
meaningful guide change, reproducible defect or compatibility regression, or
an accepted in-scope feature request. This record was reconciled in historical
tense and was ready for archival without carrying an open status forward.
