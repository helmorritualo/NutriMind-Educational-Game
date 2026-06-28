using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Thin controller for the Subject Selection / Worldhub scene.
    /// Coordinates subject card buttons, back navigation, safe-area, fader transitions, and state changes.
    /// </summary>
    public class WorldhubController : MonoBehaviour
    {
        [Header("Subject Card Buttons")]
        [SerializeField] private Button _litButton; // LiteraQuest
        [SerializeField] private Button _heButton;  // PE & Health Quest
        [SerializeField] private Button _sciButton; // ScienceQuest (Deferred)

        [Header("Back Control")]
        [SerializeField] private Button _backButton;

        [Header("Optimization & Transitions")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        [Header("Layout Optimization")]
        [SerializeField] private RectTransform _safeAreaPanel;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        // ──────────────────────────────────────────────────────────────
        //  Public Setters for Editor Auto-Wiring (Avoids forbidden Reflection)
        // ──────────────────────────────────────────────────────────────
        public void SetLitButton(Button val) => _litButton = val;
        public void SetHeButton(Button val) => _heButton = val;
        public void SetSciButton(Button val) => _sciButton = val;
        public void SetBackButton(Button val) => _backButton = val;
        public void SetMainCanvasGroup(CanvasGroup val) => _mainCanvasGroup = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;
        public void SetSafeAreaPanel(RectTransform val) => _safeAreaPanel = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_litButton != null)
            {
                _litButton.onClick.AddListener(() => OnSubjectSelected(SubjectType.LiteraQuest));
            }

            if (_heButton != null)
            {
                _heButton.onClick.AddListener(() => OnSubjectSelected(SubjectType.HealthQuest));
            }

            if (_sciButton != null)
            {
                // ScienceQuest is deferred for this milestone. Make it non-interactable.
                _sciButton.interactable = false;
                
                // Visually dim the button to indicate it's locked/unavailable
                var img = _sciButton.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                }
            }

            if (_mainCanvasGroup != null)
            {
                _mainCanvasGroup.alpha = 0f; // Start hidden for a smooth fade-in
            }
        }

        private void Start()
        {
            // Transition the state machine to SelectingSubject if we are coming from MainMenu
            var root = CompositionRoot.Instance;
            if (root != null && root.StateMachine != null)
            {
                if (root.StateMachine.CurrentState == AppState.MainMenu)
                {
                    root.StateMachine.TryTransition(AppState.SelectingSubject);
                }
                else
                {
                    Debug.Log($"[WorldhubController] Started in state: {root.StateMachine.CurrentState}");
                }
            }

            ApplySafeArea();
            ApplyPerformanceOptimizations();

            // Smooth fade-in
            if (_mainCanvasGroup != null)
            {
                StartCoroutine(FadeInRoutine());
            }
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }

            if (_litButton != null)
            {
                _litButton.onClick.RemoveAllListeners();
            }

            if (_heButton != null)
            {
                _heButton.onClick.RemoveAllListeners();
            }
        }

        private IEnumerator FadeInRoutine()
        {
            float elapsed = 0f;
            float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _mainCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            _mainCanvasGroup.alpha = 1f;
        }

        private void ApplySafeArea()
        {
            if (_mainCanvasGroup == null) return;

            // Create a runtime safe area panel to avoid editor prefab variant lock constraints
            GameObject saObj = new GameObject("RuntimeSafeAreaPanel", typeof(RectTransform));
            saObj.transform.SetParent(_mainCanvasGroup.transform, false);

            RectTransform saRect = saObj.GetComponent<RectTransform>();
            saRect.anchorMin = Vector2.zero;
            saRect.anchorMax = Vector2.one;
            saRect.offsetMin = Vector2.zero;
            saRect.offsetMax = Vector2.zero;

            // Move all Canvas children except the background image/video and the panel itself
            int childCount = _mainCanvasGroup.transform.childCount;
            var childrenToMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < childCount; i++)
            {
                Transform child = _mainCanvasGroup.transform.GetChild(i);
                if (child != saObj.transform && !child.name.ToLower().Contains("bg") && !child.name.ToLower().Contains("background"))
                {
                    childrenToMove.Add(child);
                }
            }

            foreach (var child in childrenToMove)
            {
                child.SetParent(saRect, true);
            }

            // Apply safe area anchors to the runtime panel
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            saRect.anchorMin = anchorMin;
            saRect.anchorMax = anchorMax;
            saRect.offsetMin = Vector2.zero;
            saRect.offsetMax = Vector2.zero;

            _safeAreaPanel = saRect;
        }

        private void ApplyPerformanceOptimizations()
        {
            // Disable Raycast Target on background to optimize GraphicRaycaster sweeps
            var bgObj = GameObject.Find("bg");
            if (bgObj != null)
            {
                var bgImage = bgObj.GetComponent<Image>();
                if (bgImage != null)
                {
                    bgImage.raycastTarget = false;
                }
            }
        }

        private void OnBackClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(FadeAndLoadRoutine("MainMenu", AppState.MainMenu));
        }

        private void OnSubjectSelected(SubjectType subject)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            // Apply subject selection to AuthSession/SessionStores
            var root = CompositionRoot.Instance;
            if (root != null)
            {
                if (root.Session != null)
                {
                    root.Session.SelectedSubject = subject;
                    if (root.Session.SubjectTermStore == null)
                    {
                        root.Session.SubjectTermStore = new SubjectTermStore();
                    }
                    root.Session.SubjectTermStore.SelectedSubject = subject;
                }

                if (root.StateMachine != null)
                {
                    root.StateMachine.TrySelectSubject(subject);
                }
            }

            string termSceneKey = subject == SubjectType.LiteraQuest ? "LiteraQuestTerms" : "HealthQuestTerms";
            StartCoroutine(FadeAndLoadRoutine(termSceneKey, AppState.SelectingTerm));
        }

        private IEnumerator FadeAndLoadRoutine(string sceneKey, AppState targetState)
        {
            // Update State Machine
            var root = CompositionRoot.Instance;
            if (root != null && root.StateMachine != null)
            {
                root.StateMachine.TryTransition(targetState);
            }

            System.GC.Collect();
            AppNavigation.LoadScene(sceneKey);
            yield break;
        }
    }
}
