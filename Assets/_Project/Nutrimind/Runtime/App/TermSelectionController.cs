using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Thin controller for the Term Selection and Quiz Filter scenes (LiteraQuestTerms and HealthQuestTerms).
    /// Dynamically binds terms data, manages card selection, back navigation, safe-areas, and smooth transitions.
    /// </summary>
    public class TermSelectionController : MonoBehaviour
    {
        [Header("Term Selection Cards")]
        [SerializeField] private GameObject _term1Card;
        [SerializeField] private GameObject _term2Card;
        [SerializeField] private GameObject _term3Card;

        [Header("Back Control")]
        [SerializeField] private Button _backButton;

        [Header("Optimization & Transitions")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;
        private readonly Dictionary<int, GameObject> _termCards = new();
        private readonly Dictionary<int, bool> _termAvailability = new();

        // Public setters for editor wiring and tests
        public void SetTermCards(GameObject t1, GameObject t2, GameObject t3)
        {
            _term1Card = t1;
            _term2Card = t2;
            _term3Card = t3;
        }

        public void SetBackButton(Button val) => _backButton = val;
        public void SetMainCanvasGroup(CanvasGroup val) => _mainCanvasGroup = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (_term1Card != null) _termCards[1] = _term1Card;
            if (_term2Card != null) _termCards[2] = _term2Card;
            if (_term3Card != null) _termCards[3] = _term3Card;

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_mainCanvasGroup != null)
            {
                _mainCanvasGroup.alpha = 0f; // Start hidden for a smooth fade-in
            }
        }

        private void Start()
        {
            ApplySafeArea();
            ApplyPerformanceOptimizations();

            // Fetch term data and bind UI
            StartCoroutine(FetchAndBindTermsRoutine());

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

            foreach (var card in _termCards.Values)
            {
                if (card != null)
                {
                    var btn = card.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.onClick.RemoveAllListeners();
                    }
                }
            }
        }

        private IEnumerator FetchAndBindTermsRoutine()
        {
            var root = CompositionRoot.Instance;
            if (root == null || root.DataProvider == null)
            {
                Debug.LogWarning("[TermSelectionController] CompositionRoot or DataProvider is null. Using offline layout.");
                yield break;
            }

            // Get selected subject slug
            string subjectSlug = "literaquest";
            if (root.Session != null && root.Session.SelectedSubject.HasValue)
            {
                subjectSlug = root.Session.SelectedSubject.Value.ToString().ToLower();
            }

            // Perform async fetch
            var task = root.DataProvider.GetTermsAsync(subjectSlug, _cts.Token);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[TermSelectionController] Failed to fetch terms data from provider due to task error.");
                yield break;
            }

            var result = task.Result;
            if (result != null && result.Success && result.Data != null)
            {
                // Map API terms to selection cards
                foreach (var termDto in result.Data)
                {
                    if (termDto.TermNumber.HasValue)
                    {
                        int termNum = termDto.TermNumber.Value;
                        bool isAvailable = termDto.IsAvailable ?? true;
                        _termAvailability[termNum] = isAvailable;

                        if (_termCards.TryGetValue(termNum, out var cardObj) && cardObj != null)
                        {
                            BindTermCard(termNum, cardObj, isAvailable);
                        }
                    }
                }
            }
            else
            {
                string errorMsg = result?.Error?.Message ?? "Unknown error";
                Debug.LogWarning($"[TermSelectionController] DataProvider returned failure for terms list: {errorMsg}. Using default/unlocked state.");
                // Default to unlocked if fetch fails in dev
                foreach (var kvp in _termCards)
                {
                    BindTermCard(kvp.Key, kvp.Value, true);
                }
            }
        }

        private void BindTermCard(int termNumber, GameObject cardObj, bool isAvailable)
        {
            // Dynamically attach Button component if not present
            var btn = cardObj.GetComponent<Button>();
            if (btn == null)
            {
                btn = cardObj.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
            }

            btn.onClick.RemoveAllListeners();

            if (isAvailable)
            {
                btn.interactable = true;
                btn.onClick.AddListener(() => OnTermSelected(termNumber));

                // Reset color multiplier to full visibility
                var img = cardObj.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white;
                }
            }
            else
            {
                btn.interactable = false;

                // Visually dim the locked card
                var img = cardObj.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }

        private void OnTermSelected(int termNumber)
        {
            if (_isTransitioning) return;

            var root = CompositionRoot.Instance;
            if (root == null) return;

            // Set current term selection in store
            if (root.Session != null)
            {
                if (root.Session.SubjectTermStore == null)
                {
                    root.Session.SubjectTermStore = new SubjectTermStore();
                }
                root.Session.SubjectTermStore.CurrentTerm = termNumber.ToString();
            }

            // Execute fader transition
            NavigateToScene("MainMenu", AppState.LoadingWorld); // Transition to LoadingWorld
        }

        private void OnBackClicked()
        {
            NavigateToScene("Worldhub", AppState.SelectingSubject);
        }

        private void NavigateToScene(string fallbackSceneKey, AppState targetState)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(FadeAndLoadRoutine(fallbackSceneKey, targetState));
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

        private IEnumerator FadeAndLoadRoutine(string fallbackSceneKey, AppState targetState)
        {
            // Smooth fade out
            if (_mainCanvasGroup != null)
            {
                float elapsed = 0f;
                float duration = 0.3f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _mainCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    yield return null;
                }
                _mainCanvasGroup.alpha = 0f;
            }

            // Update State Machine
            var root = CompositionRoot.Instance;
            if (root != null && root.StateMachine != null)
            {
                root.StateMachine.TryTransition(targetState);
            }

            System.GC.Collect();

            // Under quiz-first milestone, gameplay missions are deferred.
            // If the state is LoadingWorld, we would load the Quiz Portal / Quiz List for that term.
            // Since the Quiz Portal is built in Phase 8B, we safely fall back to MainMenu or stay in state.
            if (targetState == AppState.LoadingWorld)
            {
                // In actual milestone, Phase 8B will register "QuizPortal" scene.
                // We attempt to load "QuizPortal". If unregistered, we log gracefully and load MainMenu.
                string targetKey = "QuizPortal";
                if (root != null && root.SceneRegistry != null && root.SceneRegistry.GetScene(targetKey) != null)
                {
                    AppNavigation.LoadScene(targetKey);
                }
                else
                {
                    Debug.LogWarning($"[TermSelectionController] QuizPortal scene is not registered yet. Falling back to MainMenu.");
                    AppNavigation.LoadScene("MainMenu");
                }
            }
            else
            {
                AppNavigation.LoadScene(fallbackSceneKey);
            }
        }

        private void ApplySafeArea()
        {
            if (_mainCanvasGroup == null) return;

            // Create runtime safe area panel to avoid prefab variant locked constraints
            GameObject saObj = new GameObject("RuntimeSafeAreaPanel", typeof(RectTransform));
            saObj.transform.SetParent(_mainCanvasGroup.transform, false);

            RectTransform saRect = saObj.GetComponent<RectTransform>();
            saRect.anchorMin = Vector2.zero;
            saRect.anchorMax = Vector2.one;
            saRect.offsetMin = Vector2.zero;
            saRect.offsetMax = Vector2.zero;

            // Move Canvas children except backgrounds
            int childCount = _mainCanvasGroup.transform.childCount;
            var childrenToMove = new List<Transform>();
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

            // Apply physical Screen safe area to RectTransform
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
        }

        private void ApplyPerformanceOptimizations()
        {
            // Turn off raycast target on visual static objects to reduce overhead
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
    }
}
