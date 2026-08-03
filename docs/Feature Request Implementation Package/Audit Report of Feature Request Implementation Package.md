# Audit Report: Feature Request Implementation Package
### Hallucinated API Surface, Internal Documentation Conflict, and Remediation Plan

**Scope of audit:** `ProductionRiskAnalyzer.cs`, `ProductionDiagnosticFormatter.cs`, `PhaseTelemetryEvaluator.cs`, `GuideNavigationController.cs`, and the accompanying feature-request markdown.

**Method:** Every game-API symbol referenced in these files (classes, fields, methods) was checked against the real `Assembly-CSharp.dll` for the installed game build, via metadata inspection (`dnfile`/`monodis`) and, where the metadata alone was ambiguous, direct IL disassembly of the methods that populate the relevant data (`dncil`). Where static analysis left a question open, the finding was flagged as a hypothesis requiring empirical verification against the live in-game panel — and one such hypothesis was subsequently corrected based on that verification (see Finding 5).

---

## Executive Summary

The package splits cleanly into two categories:

- **UI layer (`GuideNavigationController.cs`) and the math core (`ProductionRiskAnalyzer.cs`'s actual algorithm, `ProductionDiagnosticFormatter.cs`'s branching logic):** sound. No hallucinated symbols; the risk-scoring logic is a faithful, workable implementation of the approach we designed together.
- **The data-wiring layer (`PhaseTelemetryEvaluator.cs`) and the doc comments describing where its inputs come from:** built on a class, a field, and a method that **do not exist** anywhere in the actual game assembly. This code will not compile against the real game DLL. The doc comments in two other files echo the same invented names, which is how the fabrication spread across files without anyone having to notice it twice.

None of this affects the parts of the package that were already validated elsewhere in this project — `StationStore.itemId/count/max`, `ItemProto.StackSize`, `StorageComponent.size`/`GetItemCount`, and the `LDB.items.Select(id)` pattern are all real and can stay exactly as written.

---

## Finding 1 — Hallucinated class: `FactoryStatData`

**Where it appears:** `PhaseTelemetryEvaluator.cs`, `EvaluateActivePhaseHealth()`:
```csharp
FactoryStatData statData = GameMain.history?.GetPlanetFactoryStatData(factory.planetId);
```

**Verification:** No type named `FactoryStatData` exists anywhere in the assembly's TypeDef table. The real class that plays this role is `FactoryProductionStat` — a per-planet container holding a `productPool` array of `ProductStat` entries.

**Why it's plausible-sounding but wrong:** the name is a reasonable guess at what such a class *would* be called, but it isn't what the game actually calls it. This is a common failure mode when an API surface is inferred from a description rather than checked against the binary — the invented name is grammatically and semantically sensible, which is exactly what makes it dangerous to leave unverified.

---

## Finding 2 — Hallucinated field: `GameMain.history`

**Where it appears:** same line as Finding 1.

**Verification:** `GameMain` has no field named `history`. The real path to statistics data is `GameMain.data.statistics`, a `GameData.statistics` field of type `GameStatData`.

---

## Finding 3 — Hallucinated method: `GetPlanetFactoryStatData(int planetId)`

**Where it appears:** same line as Findings 1–2.

**Verification:** No method of this name exists on any type in the assembly. There is no single-call lookup from a planet ID to its stats container. The real access path requires walking `GameData.statistics` (`GameStatData`) → `.production` (`ProductionStatistics`) → `.factoryStatPool[factoryIndex]` (`FactoryProductionStat`) — indexed by **factory index**, not planet ID, which is itself a detail the invented method signature obscured.

---

## Finding 4 — Hallucinated field: `itemRegisterPool`

**Where it appears:** `PhaseTelemetryEvaluator.cs`:
```csharp
if (statData == null || statData.itemRegisterPool == null) { return; }
...
var itemRegister = statData.itemRegisterPool[itemId];
```

Also referenced in doc comments in `ProductionRiskAnalyzer.cs` (top-of-file XML comments) and `ProductionDiagnosticFormatter.cs` (per-parameter XML comments on `Evaluate()`).

**Verification:** No field of this name exists anywhere. The real per-item container is `FactoryProductionStat.productPool`, an array of `ProductStat`, indexed indirectly through a second field, `FactoryProductionStat.productIndices[itemId]`, which returns the actual array position (item IDs in DSP are non-contiguous, so this indirection is necessary and is itself confirmed real).

---

## Finding 5 — Hallucinated array shape: `pRegister[0/1/2]`, `cRegister[0/1]`

**Where it appears:** `PhaseTelemetryEvaluator.cs`:
```csharp
float p1  = itemRegister.pRegister[0];
float p10 = itemRegister.pRegister[1];
float p60 = itemRegister.pRegister[2];
float c1  = itemRegister.cRegister[0];
float c10 = itemRegister.cRegister[1];
```

**Verification:** No such fields exist. The real per-item type is `ProductStat`, and it has no flat 3-slot rate array at all. What it actually has is:

```csharp
int32[] count;    // ring-buffer sample counts
int32[] cursor;   // ring-buffer write position
int64[] total;    // the field that actually matters here
int32 itemId;
float32 refProductSpeed;
float32 refConsumeSpeed;
int64 storageCount;
int64 importStorageCount;
int64 exportStorageCount;
```

`total[]` is a 14-slot array, laid out as two 7-wide blocks: indices 0–6 for production, 7–13 for consumption, where within each block indices 0–5 are progressively coarser rolling windows and index 6 (13 for consumption) is a lifetime cumulative counter. This was confirmed by disassembling the two methods that write into it — `AddProductionToTotalArray` writes to `total[6]`, `AddConsumptionToTotalArray` writes to `total[13]`, and `GameTick`/`ComputeTheMiddleLevel` write the six rolling levels at `total[0..5]` / `total[7..12]`.

**A correction to note here:** static analysis of the tick moduli in `GameTick` (which gates each level's rollup at 1, 6, 60, 360, 3,600, and 36,000-tick intervals) initially suggested index 4 was the one-minute window and index 1 was a much shorter one. **This was checked against the live panel and shown to be wrong — index 1 is confirmed empirically as the correct one-minute figure.** The tick-cadence-to-window-duration mapping inside `ComputeTheMiddleLevel` is evidently not the simple one-to-one correspondence the modulus values implied (there's additional windowing inside that method that the moduli alone don't capture). This is exactly the situation the empirical-check recommendation exists for, and it's worth trusting the live-panel comparison over the static trace whenever they disagree.

**Practical implication:** the ten-minute figure `ProductionRiskAnalyzer` needs (`p10`/`c10`) has *not* been located with confidence yet. Do not assume a specific index for it from the tick-cadence math — the same reasoning that predicted index 4 for the one-minute case was wrong. Use the identical empirical method that confirmed index 1: pick a candidate index (a reasonable next guess is 2, since 1 was already ruled correct for one minute and simple ordinal adjacency is at least testable), watch the value against the panel's 10-minute display, and iterate through the remaining unclaimed indices (0, 2, 3, 4, 5 for production; 7, 9, 10, 11, 12 for consumption) until one matches.

---

## Finding 6 — Internal documentation conflict on rate normalization

This one is independent of the hallucination above, and would remain a real hazard even after the fictional API is replaced with real fields.

**Location A — `ProductionRiskAnalyzer.cs`, top-of-file XML doc comments:**
```
/// Source: FactoryStatData.itemRegisterPool[itemId].p10 (or index 1 count / 10.0f)
/// Source: FactoryStatData.itemRegisterPool[itemId].p60 (or index 2 count / 60.0f)
```
This asserts that `p10`/`p60` need to be *derived by dividing a raw count* by 10 or 60 to reach items-per-minute.

**Location B — `ProductionDiagnosticFormatter.cs`, per-parameter XML doc comments on `Evaluate()`:**
```
/// <param name="p10">... Source: FactoryStatData.itemRegisterPool[itemId].pRegister[1] / 10.0f</param>
/// <param name="p60">... Source: FactoryStatData.itemRegisterPool[itemId].pRegister[2] / 60.0f</param>
```
Same convention: divide-before-use.

**Location C — `PhaseTelemetryEvaluator.cs`, implementation-hints block at the bottom of the file:**
```
1. RAW REGISTER RATE ACCURACY:
   - DSP's itemRegisterPool automatically averages pRegister and cRegister slots to items-per-minute.
     Do NOT divide pRegister[1] by 10 or pRegister[2] by 60, as this corrupts baseline calculations.
```
The opposite convention: the values are already normalized, and dividing them again is explicitly called out as a bug.

**Which one the actual code follows:** `PhaseTelemetryEvaluator.EvaluateActivePhaseHealth()` reads `pRegister[0]`, `[1]`, `[2]` with no division at all — following Location C's convention, not A or B. This is also the only internally *consistent* choice: `ProductionRiskAnalyzer`'s `ESTABLISHED_LINE_THRESHOLD = 1.0f` check (`historicalBaseline < ESTABLISHED_LINE_THRESHOLD`) is calibrated to a value of "1 item/min," which only makes sense if `p10`/`p60` are already-normalized rates. If they were raw, undivided window counts as Locations A and B describe, that threshold would trip almost immediately for nearly every established line, since a raw 10-minute item count is routinely far greater than 1.

**Net effect:** three files disagree with each other about the unit convention of the same values, and the doc comments in two of them (`ProductionRiskAnalyzer.cs`, `ProductionDiagnosticFormatter.cs`) actively contradict the convention the real logic depends on. Anyone extending this package who trusts the doc comments over the implementation would silently reintroduce a scaling bug.

---

## Remediation Plan

### Step 1 — Replace the data-access path in `PhaseTelemetryEvaluator.cs`

Delete the `FactoryStatData`/`GameMain.history`/`GetPlanetFactoryStatData`/`itemRegisterPool` chain entirely. Reuse the access pattern already validated in `ProductionTelemetry.cs`, which reaches the real data via:

```
GameMain.data.statistics                      // GameStatData
  .production                                  // ProductionStatistics
    .factoryStatPool[factoryIndex]              // FactoryProductionStat
      .productPool[productIndices[itemId]]      // ProductStat  (guard: poolIndex > 0, and stat.itemId == itemId)
        .total[index]                           // int64 — the real number
```

Rather than maintaining two separate implementations of this walk, consider having `PhaseTelemetryEvaluator` consume `ProductionTelemetry`'s already-sampled, already-validated output directly instead of re-deriving its own copy of the traversal logic. One correct implementation of a non-trivial lookup is safer than two.

### Step 2 — Fix the index constants

Confirmed correct so far:
```csharp
private const int ProductionPeriodIndex  = 1;  // empirically confirmed: one-minute production
private const int ConsumptionPeriodIndex = 8;  // by symmetry: one-minute consumption
private const int LifetimeProductionIndex  = 6;   // confirmed via IL: AddProductionToTotalArray writes here
private const int LifetimeConsumptionIndex = 13;  // confirmed via IL: AddConsumptionToTotalArray writes here
```

Needed but not yet confirmed — the ten-minute pair `ProductionRiskAnalyzer` requires as `p10`/`c10`:
```csharp
private const int TenMinuteProductionIndex  = ???;  // candidates: 0, 2, 3, 4, 5 — verify against live panel
private const int TenMinuteConsumptionIndex = ???;  // candidates: 7, 9, 10, 11, 12 — verify against live panel
```

Verification method: open the in-game statistics panel, select the 10-minute view for a known item, read each untested index for that item's `ProductStat.total[]`, and match against the displayed figure — the same method that already confirmed index 1.

### Step 3 — Decide, once, whether `total[]` values need a unit conversion

`total[]` holds raw accumulated counts over whatever window that slot represents — it is not guaranteed to already be "items per minute" the way the old (fictional) `pRegister` was assumed to be. Once the real one-minute and ten-minute indices are both confirmed, check whether the raw total needs dividing by the window length (in minutes) to reach items/minute, or whether the game already stores it pre-normalized. Whichever answer is correct, write it in exactly one place — a single comment on the read call in `PhaseTelemetryEvaluator`, not restated (and potentially contradicted) in three different files' doc comments.

### Step 4 — Correct the doc comments in `ProductionRiskAnalyzer.cs` and `ProductionDiagnosticFormatter.cs`

Remove the `FactoryStatData.itemRegisterPool[...].pRegister[n] / n.0f` language from both files' XML comments. Replace with a single-sentence contract at the top of `ProductionRiskAnalyzer.cs`:

```csharp
/// <remarks>
/// All p1/p10/p60/c1/c10 parameters are already-normalized items-per-minute rates.
/// Callers are responsible for any conversion from raw ProductStat.total[] window sums
/// before calling this method. See PhaseTelemetryEvaluator for the canonical read path.
/// </remarks>
```

This makes the unit convention a single-source-of-truth statement rather than three independent (and, as shown, drifting) claims.

### Step 5 — Take advantage of two real fields the hallucinated version was missing

While replacing the data path, consider whether `ProductStat.refProductSpeed` / `refConsumeSpeed` (confirmed real fields — a designed/reference throughput, independent of recent history) can simplify or replace part of `ProductionRiskAnalyzer`'s `systemicDecay` calculation, which currently has to approximate a baseline from `p10`/`p60` history. Also, `ProductStat.storageCount` / `importStorageCount` / `exportStorageCount` are real, plain fields requiring no ring-buffer math at all — these can likely replace the more involved `GetLocalInventoryBuffers` chest-scanning logic in `PhaseTelemetryEvaluator`, or at least serve as a cross-check for it.

### Step 6 — No action needed

`GuideNavigationController.cs` touches no game-API surface (pure Unity UI) and requires no changes from this audit. The core branching logic in `ProductionRiskAnalyzer.cs` and `ProductionDiagnosticFormatter.cs` (the actual risk math and severity-tier selection) is sound as designed and only needs its *inputs* fixed, not its logic.

---

## Verification Checklist

Before considering this package fixed:

- [ ] `PhaseTelemetryEvaluator.cs` compiles against the real `Assembly-CSharp.dll` with zero references to `FactoryStatData`, `GameMain.history`, `GetPlanetFactoryStatData`, `itemRegisterPool`, `pRegister`, or `cRegister`.
- [ ] The ten-minute index pair has been empirically confirmed against the live panel (not assumed from tick-cadence math).
- [ ] A single, explicit decision has been made and documented about whether `total[]` values require a window-to-minutes conversion.
- [ ] `ProductionRiskAnalyzer.cs` and `ProductionDiagnosticFormatter.cs` doc comments no longer reference the fictional API or the division convention that contradicts the working code.
- [ ] `ESTABLISHED_LINE_THRESHOLD = 1.0f` still behaves sensibly once real values are flowing in (i.e., confirm it isn't tripping on every line or never tripping at all — a quick sanity check against a known idle vs. known active production line).

---

## Appendix — Previously Identified Design-Level Issues (Not Hallucinations, Included for Completeness)

Two issues in `ProductionRiskAnalyzer.cs`'s actual math were flagged earlier in this project and are restated briefly here since a "complete report" on this package would be incomplete without them, even though they're orthogonal to the hallucination/conflict findings above:

1. **Chronic shortfalls score as ~0 risk forever.** `systemicDecay` measures change relative to the line's *own* history (`1 - p1/historicalBaseline`). A line that has always been undersized, with no recent change, produces `rawDrop ≈ 0` and therefore `finalRisk ≈ 0`, regardless of how large the standing deficit or how empty the buffers are. Worth deciding whether this is acceptable (the analyzer only flags *deteriorating* lines, not *chronically insufficient* ones) or whether `deficitDivergence` should be allowed to contribute risk on its own when `historicalBaseline` has been flat for a long duration.
2. **Inconsistent buffer-depletion units.** `localDepletion` (chest storage) is capacity-percentage-based; the ILS/PLS dampener is runway-seconds-based. A large chest sitting at a low fill percentage can represent a long runway but currently reads as "nearly depleted." Recommend converting `localDepletion` to the same runway-seconds basis used for ILS stock, so both buffer sources are judged on comparable terms.