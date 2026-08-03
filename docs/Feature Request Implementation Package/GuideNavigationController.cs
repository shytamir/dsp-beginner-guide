using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DSPGuideCheckMod.Analytics;

namespace DSPGuideCheckMod.UI
{
    /// <summary>
    /// Metadata representation for a single guide phase.
    /// </summary>
    [Serializable]
    public class GuidePhaseData
    {
        public int PhaseIndex;
        public string Title;
        public string ShortDescription;
        public bool IsCompleted;
        public List<int> TargetItemIds; // Items tracked during this phase
    }

    /// <summary>
    /// Holds UI element references for a single card inside the Quick-Jump Modal grid.
    /// </summary>
    public class QuickJumpCardHolder : MonoBehaviour
    {
        public Button CardButton;
        public Text CardText;
        public Image CardBackground;
        public int PhaseIndex;
    }

    /// <summary>
    /// Manages UI header navigation, phase selection state, the Quick-Jump Grid Modal,
    /// and dynamic uGUI ScrollRect layout updates.
    /// </summary>
    public class GuideNavigationController : MonoBehaviour
    {
        [Header("Header Controls")]
        [SerializeField] private Button prevPhaseButton;
        [SerializeField] private Button nextPhaseButton;
        [SerializeField] private Button phaseTitleButton;
        [SerializeField] private Text phaseTitleText;
        [SerializeField] private Image statusPipImage; // 4px border indicator for collapsed state

        [Header("Main Content Viewport")]
        [SerializeField] private ScrollRect mainScrollRect;
        [SerializeField] private RectTransform mainContentContainer;

        [Header("Quick-Jump Modal")]
        [SerializeField] private GameObject modalOverlayObject;
        [SerializeField] private Button modalCloseButton;
        [SerializeField] private RectTransform modalGridContent;
        [SerializeField] private GameObject phaseCardPrefab;

        // State Tracking
        private List<GuidePhaseData> _loadedPhases = new List<GuidePhaseData>();
        private List<QuickJumpCardHolder> _instantiatedCardHolders = new List<QuickJumpCardHolder>();
        private int _currentPhaseIndex = 0;
        private Action<int> _onPhaseChangedCallback;

        // Fallback color if TryParseHtmlString fails
        private static readonly Color FALLBACK_PIP_COLOR = new Color(0.0f, 1.0f, 0.8f, 1.0f); // Cyan

        #region Unity Lifecycle & Initialization

        /// <summary>
        /// Initializes the navigation system with guide phases and a phase-change listener callback.
        /// Re-initialization safe.
        /// </summary>
        public void Initialize(List<GuidePhaseData> phases, Action<int> onPhaseChanged)
        {
            _loadedPhases = phases ?? new List<GuidePhaseData>();
            _onPhaseChangedCallback = onPhaseChanged;

            // Clear previous listeners to maintain idempotency
            CleanupButtonListeners();

            // Wire up Header Buttons safely
            if (prevPhaseButton != null)
                prevPhaseButton.onClick.AddListener(() => StepPhase(-1));

            if (nextPhaseButton != null)
                nextPhaseButton.onClick.AddListener(() => StepPhase(1));

            if (phaseTitleButton != null)
                phaseTitleButton.onClick.AddListener(() => SetModalVisible(true));

            if (modalCloseButton != null)
                modalCloseButton.onClick.AddListener(() => SetModalVisible(false));

            RebuildModalGrid();
            SetPhase(0, forceRefresh: true);
        }

        private void OnDestroy()
        {
            CleanupButtonListeners();
            _instantiatedCardHolders.Clear();
        }

        /// <summary>
        /// Removes all attached listeners from serialized UI buttons to prevent memory leaks or dual clicks.
        /// </summary>
        private void CleanupButtonListeners()
        {
            prevPhaseButton?.onClick.RemoveAllListeners();
            nextPhaseButton?.onClick.RemoveAllListeners();
            phaseTitleButton?.onClick.RemoveAllListeners();
            modalCloseButton?.onClick.RemoveAllListeners();
        }

        #endregion

        #region Navigation Logic

        /// <summary>
        /// Steps backward or forward through phases relative to current index.
        /// </summary>
        public void StepPhase(int delta)
        {
            if (_loadedPhases.Count == 0) return;
            int newIndex = Mathf.Clamp(_currentPhaseIndex + delta, 0, _loadedPhases.Count - 1);
            if (newIndex != _currentPhaseIndex)
            {
                SetPhase(newIndex);
            }
        }

        /// <summary>
        /// Sets the active phase index directly, updating headers, modal highlights, and content.
        /// </summary>
        public void SetPhase(int index, bool forceRefresh = false)
        {
            if (_loadedPhases.Count == 0) return;
            
            int clampedIndex = Mathf.Clamp(index, 0, _loadedPhases.Count - 1);
            if (clampedIndex == _currentPhaseIndex && !forceRefresh) return;

            _currentPhaseIndex = clampedIndex;
            GuidePhaseData activePhase = _loadedPhases[_currentPhaseIndex];

            // 1. Update Header Title Text (Using safe '+' glyph instead of unicode grid)
            if (phaseTitleText != null)
            {
                phaseTitleText.text = $"Phase {_currentPhaseIndex + 1}: {activePhase.Title}  <color=#00FFCC>[+]</color>";
            }

            // 2. Update Header Button Interactability
            if (prevPhaseButton != null) prevPhaseButton.interactable = (_currentPhaseIndex > 0);
            if (nextPhaseButton != null) nextPhaseButton.interactable = (_currentPhaseIndex < _loadedPhases.Count - 1);

            // 3. Reset Main ScrollRect Position to Top
            if (mainScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                mainScrollRect.verticalNormalizedPosition = 1.0f;
            }

            // 4. Trigger Phase Change Callback to refresh item rows
            _onPhaseChangedCallback?.Invoke(_currentPhaseIndex);

            // 5. Close Modal if open
            SetModalVisible(false);
        }

        /// <summary>
        /// Updates the 4px border status pip color based on the worst severity rating in the active phase.
        /// Includes defensive fallback handling for color parsing.
        /// </summary>
        public void UpdateStatusPip(DiagnosticSeverity worstSeverity)
        {
            if (statusPipImage == null) return;

            string hexCode;
            switch (worstSeverity)
            {
                case DiagnosticSeverity.Starved:
                    hexCode = ProductionDiagnosticFormatter.COLOR_STARVED;
                    break;
                case DiagnosticSeverity.Draining:
                    hexCode = ProductionDiagnosticFormatter.COLOR_DRAINING;
                    break;
                case DiagnosticSeverity.Ramping:
                    hexCode = ProductionDiagnosticFormatter.COLOR_RAMPING;
                    break;
                case DiagnosticSeverity.Sufficient:
                    hexCode = ProductionDiagnosticFormatter.COLOR_SUFFICIENT;
                    break;
                default:
                    hexCode = ProductionDiagnosticFormatter.COLOR_BALANCED;
                    break;
            }

            if (ColorUtility.TryParseHtmlString(hexCode, out Color targetColor))
            {
                statusPipImage.color = targetColor;
            }
            else
            {
                Debug.LogWarning($"[GuideNavigationController] Failed to parse hex color '{hexCode}'. Applying fallback color.");
                statusPipImage.color = FALLBACK_PIP_COLOR;
            }
        }

        #endregion

        #region Quick-Jump Modal Management

        /// <summary>
        /// Opens or closes the pop-over Quick-Jump Grid Modal.
        /// Dynamically refreshes card visual states upon opening.
        /// </summary>
        public void SetModalVisible(bool visible)
        {
            if (modalOverlayObject != null)
            {
                modalOverlayObject.SetActive(visible);
                if (visible)
                {
                    RefreshModalCardStates();
                }
            }
        }

        /// <summary>
        /// Instantiates cards inside the Quick-Jump Modal grid container.
        /// Uses Immediate destruction to prevent mid-frame layout rebuild thrash.
        /// </summary>
        private void RebuildModalGrid()
        {
            if (modalGridContent == null || phaseCardPrefab == null) return;

            _instantiatedCardHolders.Clear();

            // Destroy existing children immediately to avoid delayed layout thrash
            while (modalGridContent.childCount > 0)
            {
                Transform child = modalGridContent.GetChild(0);
                child.SetParent(null); // Unparent first to unhook from layout immediately
                DestroyImmediate(child.gameObject);
            }

            for (int i = 0; i < _loadedPhases.Count; i++)
            {
                int phaseIdx = i; // Closure capture for lambda
                GameObject cardObj = Instantiate(phaseCardPrefab, modalGridContent);
                
                QuickJumpCardHolder holder = cardObj.GetComponent<QuickJumpCardHolder>();
                if (holder == null)
                {
                    holder = cardObj.AddComponent<QuickJumpCardHolder>();
                }

                // Defensively populate references regardless of whether holder existed on prefab
                holder.CardButton = cardObj.GetComponent<Button>();
                holder.CardText = cardObj.GetComponentInChildren<Text>();
                holder.CardBackground = cardObj.GetComponent<Image>();
                holder.PhaseIndex = phaseIdx;

                if (holder.CardButton != null)
                {
                    holder.CardButton.onClick.RemoveAllListeners();
                    holder.CardButton.onClick.AddListener(() => SetPhase(phaseIdx));
                }

                _instantiatedCardHolders.Add(holder);
            }

            RefreshModalCardStates();
        }

        /// <summary>
        /// Refreshes modal cards to sync both completion checkmarks and active phase highlights.
        /// Called every time the modal is opened.
        /// </summary>
        private void RefreshModalCardStates()
        {
            for (int i = 0; i < _instantiatedCardHolders.Count; i++)
            {
                QuickJumpCardHolder holder = _instantiatedCardHolders[i];
                if (holder == null || i >= _loadedPhases.Count) continue;

                GuidePhaseData phase = _loadedPhases[i];

                // 1. Refresh Dynamic Completion Checkmark
                if (holder.CardText != null)
                {
                    string statusPrefix = phase.IsCompleted ? "<color=#55FF55>[✓]</color>" : "[ ]";
                    holder.CardText.text = $"{statusPrefix} Phase {i + 1}: {phase.Title}";
                }

                // 2. Refresh Active Background Highlight
                if (holder.CardBackground != null)
                {
                    holder.CardBackground.color = (i == _currentPhaseIndex) 
                        ? new Color(0.0f, 1.0f, 0.8f, 0.35f) // Active Cyan Tint
                        : new Color(0.1f, 0.1f, 0.15f, 0.85f); // Standard Translucent Frame
                }
            }
        }

        #endregion
    }
}

/*
================================================================================
PARAMETER & FIELD MAPPING SPECIFICATION
================================================================================
| Field / Method           | Type                      | Purpose & Description |
| :----------------------- | :------------------------ | :-------------------- |
| prevPhaseButton          | UnityEngine.UI.Button     | Step backward to adjacent phase. Disabled at index 0. |
| nextPhaseButton          | UnityEngine.UI.Button     | Step forward to adjacent phase. Disabled at final index. |
| phaseTitleButton         | UnityEngine.UI.Button     | Click target on header title that opens Quick-Jump Grid Modal. |
| statusPipImage           | UnityEngine.UI.Image      | 4px border image element tinted by worst current risk severity. |
| mainScrollRect           | UnityEngine.UI.ScrollRect  | Native uGUI scroll viewer hosting expanded accordion content. |
| modalOverlayObject       | UnityEngine.GameObject    | Translucent backdrop modal overlay (starts active=false). |
| QuickJumpCardHolder      | MonoBehaviour             | Component guaranteeing references to button, text, and bg image. |
================================================================================
IMPLEMENTATION HINTS & DSP ENGINE PITFALLS
================================================================================
1. PREFAB DEFENSIVE ASSIGNMENTS:
   - Always re-assign GetComponent references inside RebuildModalGrid to ensure unassigned
     Inspector fields on custom mod prefabs do not throw NullReferenceExceptions.
2. IDEMPOTENT INITIALIZATION:
   - CleanupButtonListeners() is called in Initialize() and OnDestroy() to prevent duplicate
     listener invocation stacks when switching save files or reloading UI overlays.
3. GC-FRIENDLY MODAL REBUILDS:
   - DestroyImmediate unparents and clears layout elements inside a while loop, avoiding mid-frame
     layout thrashing with uGUI GridLayoutGroup and ContentSizeFitter.
================================================================================
*/