using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using NutriMind.Runtime.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// View Controller for the Loading/Transition scene.
    /// Manages UI Toolkit bindings, responsive safe area layout, minimum visual duration,
    /// dynamic quiz preloading via IGameDataProvider, and error-retry fallback screens.
    /// </summary>
    public class LoadingTransitionController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _minimumDisplayTime = 1.5f;

        [Header("UI Document")]
        [SerializeField] private UIDocument _uiDocument;

        private Label _statusLabel;
        private VisualElement _safeAreaWrapper;
        private VisualElement _overlayInstance;
        private VisualElement _errorContainer;
        private Label _errorLabel;
        private Button _retryButton;
        private Button _menuButton;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                var rootEl = _uiDocument.rootVisualElement;
                _safeAreaWrapper = rootEl.Q<VisualElement>("safe-area-wrapper");
                _overlayInstance = rootEl.Q<VisualElement>("overlay-instance");

                // Retrieve elements nested inside LoadingOverlay
                if (_overlayInstance != null)
                {
                    _statusLabel = _overlayInstance.Q<Label>("loading-message");
                }
                else
                {
                    _statusLabel = rootEl.Q<Label>("loading-message");
                }

                _errorContainer = rootEl.Q<VisualElement>("error-container");
                _errorLabel = rootEl.Q<Label>("error-label");
                _retryButton = rootEl.Q<Button>("retry-button");
                _menuButton = rootEl.Q<Button>("menu-button");

                // Bind failure controls
                if (_retryButton != null)
                {
                    _retryButton.clicked += OnRetryClicked;
                }
                if (_menuButton != null)
                {
                    _menuButton.clicked += OnMenuClicked;
                }

                // Register layout change event for responsive safe area scaling on Android landscape
                rootEl.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
            else
            {
                Debug.LogWarning("[LoadingTransitionController] UIDocument is missing or has no rootVisualElement.");
            }
        }

        private void Start()
        {
            ApplySafeArea();
            StartPreloading();
        }

        private void OnDestroy()
        {
            if (_retryButton != null) _retryButton.clicked -= OnRetryClicked;
            if (_menuButton != null) _menuButton.clicked -= OnMenuClicked;

            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ApplySafeArea();
        }

        /// <summary>
        /// Recalculates and applies safe area padding relative to physical Screen coordinates
        /// using UI Toolkit scaled panel coordinates.
        /// </summary>
        private void ApplySafeArea()
        {
            if (_safeAreaWrapper == null || _uiDocument == null || _uiDocument.rootVisualElement == null || _uiDocument.rootVisualElement.panel == null) return;

            var panelSize = _uiDocument.rootVisualElement.panel.visualTree.layout.size;
            Vector2 panelResolution = new Vector2(
                float.IsNaN(panelSize.x) || panelSize.x <= 0f ? Screen.width : panelSize.x,
                float.IsNaN(panelSize.y) || panelSize.y <= 0f ? Screen.height : panelSize.y
            );

            Rect safeArea = Screen.safeArea;
            Vector2 screenResolution = new Vector2(Screen.width, Screen.height);

            RectOffset padding = NutriMindSafeAreaUtility.GetPanelPadding(safeArea, screenResolution, panelResolution);

            _safeAreaWrapper.style.paddingLeft = padding.left;
            _safeAreaWrapper.style.paddingRight = padding.right;
            _safeAreaWrapper.style.paddingTop = padding.top;
            _safeAreaWrapper.style.paddingBottom = padding.bottom;
        }

        private void StartPreloading()
        {
            if (_errorContainer != null) _errorContainer.style.display = DisplayStyle.None;
            if (_overlayInstance != null) _overlayInstance.style.display = DisplayStyle.Flex;

            UpdateStatus("Preparing loading content...");
            StartCoroutine(PreloadRoutine());
        }

        private IEnumerator PreloadRoutine()
        {
            float startTime = Time.time;

            var root = CompositionRoot.Instance;
            if (root == null || root.DataProvider == null)
            {
                Debug.LogWarning("[LoadingTransitionController] CompositionRoot or DataProvider is null. Simulating transition.");
                yield return new WaitForSeconds(_minimumDisplayTime);
                NavigateDestination();
                yield break;
            }

            // Map selected subject and term to standard API query slugs
            string subjectSlug = "literaquest";
            int termNumber = 1;

            if (root.Session != null)
            {
                if (root.Session.SelectedSubject.HasValue)
                {
                    subjectSlug = root.Session.SelectedSubject.Value.ToString().ToLower();
                }

                if (root.Session.SubjectTermStore != null && !string.IsNullOrEmpty(root.Session.SubjectTermStore.CurrentTerm))
                {
                    int.TryParse(root.Session.SubjectTermStore.CurrentTerm, out termNumber);
                }
            }

            UpdateStatus($"Loading {root.Session?.SelectedSubject?.ToString() ?? "Subject"} Term {termNumber} Quizzes...");

            // Trigger and track async preloading Task
            var preloadTask = root.DataProvider.GetQuizzesAsync(subjectSlug, termNumber, _cts.Token);
            yield return new WaitUntil(() => preloadTask.IsCompleted);

            if (preloadTask.IsFaulted || preloadTask.IsCanceled)
            {
                HandlePreloadError("Preloading task was interrupted or failed.");
                yield break;
            }

            var result = preloadTask.Result;
            if (result == null || !result.Success)
            {
                string errorMsg = result?.Error?.Message ?? "Network connection lost. Please try again.";
                HandlePreloadError(errorMsg);
                yield break;
            }

            UpdateStatus("Quizzes synced successfully!");

            // Enforce minimum display time to ensure high-fidelity smooth transition
            float elapsed = Time.time - startTime;
            float remaining = _minimumDisplayTime - elapsed;
            if (remaining > 0f)
            {
                yield return new WaitForSeconds(remaining);
            }

            NavigateDestination();
        }

        private void HandlePreloadError(string errorMessage)
        {
            UpdateStatus("");
            if (_overlayInstance != null) _overlayInstance.style.display = DisplayStyle.None;
            if (_errorContainer != null) _errorContainer.style.display = DisplayStyle.Flex;

            if (_errorLabel != null)
            {
                _errorLabel.text = errorMessage;
            }

            // Also report diagnostics securely to SafeErrorService
            var root = CompositionRoot.Instance;
            if (root != null && root.SafeErrorService != null)
            {
                root.SafeErrorService.LogSafe($"[PRELOAD_FAILURE] {errorMessage}");
            }
        }

        private void NavigateDestination()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            var root = CompositionRoot.Instance;
            if (root != null && root.StateMachine != null)
            {
                // In world / In Quiz Portal state transition
                root.StateMachine.TryTransition(AppState.InWorld);
            }

            System.GC.Collect();

            // Resolve target QuizPortal scene. Fallback to MainMenu if QuizPortal is unregistered.
            string targetKey = "QuizPortal";
            if (root != null && root.SceneRegistry != null && root.SceneRegistry.GetScene(targetKey) != null)
            {
                AppNavigation.LoadScene(targetKey);
            }
            else
            {
                Debug.LogWarning("[LoadingTransitionController] QuizPortal scene is not registered yet. Transitioning back to MainMenu.");
                AppNavigation.LoadScene("MainMenu");
            }
        }

        private void OnRetryClicked()
        {
            StartPreloading();
        }

        private void OnMenuClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            var root = CompositionRoot.Instance;
            if (root != null && root.StateMachine != null)
            {
                root.StateMachine.TryTransition(AppState.MainMenu);
            }

            AppNavigation.LoadScene("MainMenu");
        }

        private void UpdateStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log($"[LoadingTransition] {message}");
            }
        }
    }
}
