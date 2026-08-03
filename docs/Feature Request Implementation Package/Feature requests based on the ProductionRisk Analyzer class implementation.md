# Feature requests based on the ProductionRisk Analyzer class implementation

## 1. Translating Risk Math into Intuitive Player Diagnostics

Reporting a raw float like `0.87` means nothing to a player. The goal of the expanded drawer is to translate $S \times D \times B$ into an immediate **Root Cause** and **Call to Action**.

### Deconstructing the Risk Score for the Player

Instead of exposing the math, we evaluate which term in your function is driving the high score and assign a human-readable **Diagnostic Driver**:

| Primary Risk Driver | What the Math Means | Player-Facing Status | Actionable Hint |
| --- | --- | --- | --- |
| **High $S$, High $B$** | Production collapsed ($P_1 \ll P_{10}$) & Buffers empty | **🔴 STARVED** | *"Upstream supply stalled. Input materials are missing."* |
| **High $D$, Moderate $B$** | Consumption exceeds production ($C_1 > P_1$) & Storage draining | **🟡 DRAINING** | *"Production deficit. Local buffers are cushioning demand."* |
| **Startup Fallback** | New line ($P_{10} \approx 0$), high demand ($C_1 \gg P_1$) | **🟠 RAMPING** | *"Production ramping up. Output is below demand target."* |
| **High $B$ alone** | Storage empty, but $C_1 \le P_1$ | **🟢 SUFFICIENT** | *"Demand satisfied, but no storage buffer exists."* |
| **Circuit Breaker Active** | $C_1 \le P_1$ or backpressure | **🟢 BALANCED** | *"Production meets or exceeds current demand."* |

---

### Accordion Drawer UI Mockup

When an item row is collapsed, it shows a clean summary line. Clicking the row expands a thin detail panel underneath using data already captured by `ComputeShortageRisk`:

```text
======================================================================
[!] Iron Ingot : 120 / 600 per min                  [🔴 STARVED]  (v)
======================================================================
│  
│  ▶ Status: Upstream Supply Collapse
│  ▶ Primary Issue: Production dropped by 80% relative to 10m average.
│  
│  • Local Planet Rate : 120 / min  (Global Total: 1,200 / min)
│  • Buffer Runway     : 0s remaining (Chests & ILS are empty)
│  
│  [Hint: Check raw Iron Ore supply or power grid on this planet.]
======================================================================

```

### Color Badge Thresholds

* **Cyan / White (`Risk < 0.3`):** Normal operation.
* **Amber / Yellow (`0.3 <= Risk < 0.7`):** Buffers actively draining or mild deficit.
* **Pulsing Red (`Risk >= 0.7`):** Active starvation or production line halt.

---

## 2. Phase Navigation & Layout Modernization

To eliminate endless arrow-clicking without taking up permanent screen real estate, we combine a compact header with a pop-over **Quick-Jump Grid Modal**.

### Header Layout

```text
  [ < ]   Phase 4: Planetary Logistics  [ ⊞ ]   [ > ]

```

* **`<` / `>` Buttons:** Step to immediately adjacent phases.
* **Center Title / Grid Icon `[ ⊞ ]`:** Clicking opens a compact pop-over window directly over the panel.

### Quick-Jump Modal (The Grid)

When clicked, a semi-transparent modal overlay opens displaying all guide phases as compact visual cards:

```text
┌─────────────────────────────────────────────────────────┐
│                    SELECT GUIDE PHASE               [X] │
├─────────────────────────────────────────────────────────┤
│  [✓] Phase 1: Basic Automation                          │
│  [✓] Phase 2: Hydrocarbon Processing                    │
│  [✓] Phase 3: Planetary Grid Setup                      │
│  [★] Phase 4: Planetary Logistics         <ACTIVE>      │
│  [ ] Phase 5: Interstellar Expansion                    │
│  [ ] Phase 6: Dyson Swarm Componentry                   │
└─────────────────────────────────────────────────────────┘

```

* **Clicking any card:** Instantly switches the active phase and closes the modal.
* **Scalability:** Works effortlessly whether your guide has 5 or 50 phases.

### Layout Container (Unity `ScrollRect`)

* Replace manual up/down text buttons with a standard uGUI `ScrollRect` container featuring a masked viewport.
* Attach a `ContentSizeFitter` and `VerticalLayoutGroup` to the content container. When accordion drawers expand or collapse, Unity automatically handles smooth scrolling and layout recalculation with **zero custom offset math**.

---

## 3. Collapsed State Health Indicator (Minimalist Approach)

To avoid screen clutter and unnecessary computation when the main panel is collapsed, we leverage the existing hanging **Cube Rate Column**.

### Implementation: The "Status Pip"

1. **Zero Extra Calculations:** During your existing 1–2 second risk evaluation tick, save a single reference: `Color worstPhaseColor`.
* If any objective in the current phase has `Risk >= 0.7`, `worstPhaseColor = Red`.
* Else if any objective has `Risk >= 0.3`, `worstPhaseColor = Yellow`.
* Else `worstPhaseColor = Cyan`.


2. **Visual Anchor:** Add a tiny 4px accent bar or thin border highlight to the top or side of the hanging cube rate monitor frame.
3. **Behavior:** When the main panel collapses, the cube rate column stays visible as usual, but its subtle border/pip is tinted with `worstPhaseColor`.

```text
Collapsed View:
┌──┐
│  │ ◄── 4px Border Glow (Tinted Red if worst current phase objective >= 0.7)
│🟪│ [Matrix Cube Rate 1]
│🟦│ [Matrix Cube Rate 2]
└──┘

```

* **Result:** The player immediately notices if their currently tracked phase is in trouble without needing the text panel taking up screen space!

---

## Implementation Summary Matrix

| Feature | Primary Purpose | Performance Overhead | uGUI Elements Used |
| --- | --- | --- | --- |
| **Diagnostic Accordion** | Human-readable root causes | Zero (Uses cached risk stats) | `Button`, `VerticalLayoutGroup`, `Text` |
| **Quick-Jump Modal** | Fast phase navigation | Zero (Only acts on click) | `CanvasGroup`, `GridLayoutGroup`, `Button` |
| **ScrollRect Viewport** | Native mousewheel scrolling | Negligible (uGUI standard) | `ScrollRect`, `Mask`, `Scrollbar` |
| **Collapsed Health Pip** | Passive bottleneck awareness | Zero (Single color assignment) | `Image` (Color tint) |
