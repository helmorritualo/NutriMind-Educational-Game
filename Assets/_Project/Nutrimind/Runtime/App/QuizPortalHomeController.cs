using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Thin controller for the global Quiz Portal Home Canvas panel.
    /// Binds welcome text, wires navigation actions, and manages safe-area/fade transitions.
    /// </summary>
    public class QuizPortalHomeController : MonoBehaviour
    {
        [Header("Action Buttons")]
        [SerializeField] private Button _availableQuizzesButton;
        [SerializeField] private Button _myQuizResultsButton;
        [SerializeField] private Button _backMainMenuButton;

        [Header("Dynamic Labels")]
        [SerializeField] private TextMeshProUGUI _welcomeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Navigation")]
        [SerializeField] private QuizPortalNavigationController _navigation;

        [Header("Optimization & Transitions")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        [SerializeField] private Image _backgroundImage;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        public void SetAvailableQuizzesButton(Button val) => _availableQuizzesButton = val;
        public void SetMyQuizResultsButton(Button val) => _myQuizResultsButton = val;
        public void SetBackMainMenuButton(Button val) => _backMainMenuButton = val;
        public void SetWelcomeText(TextMeshProUGUI val) => _welcomeText = val;
        public void SetDescriptionText(TextMeshProUGUI val) => _descriptionText = val;
        public void SetNavigation(QuizPortalNavigationController val) => _navigation = val;
        public void SetMainCanvasGroup(CanvasGroup val) => _mainCanvasGroup = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;
        public void SetBackgroundImage(Image val) => _backgroundImage = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (_availableQuizzesButton != null)
            {
                _availableQuizzesButton.onClick.AddListener(OnAvailableQuizzesClicked);
            }

            if (_myQuizResultsButton != null)
            {
                _myQuizResultsButton.onClick.AddListener(OnMyQuizResultsClicked);
            }

            if (_backMainMenuButton != null)
            {
                _backMainMenuButton.onClick.AddListener(OnBackMainMenuClicked);
            }

            if (_mainCanvasGroup != null)
            {
                _mainCanvasGroup.alpha = 0f;
            }
        }

        private void Start()
        {
            var root = CompositionRoot.Instance;
            if (root?.StateMachine != null)
            {
                // ponytail: InWorld maps to Quiz Portal until dedicated QuizPortal enum is added
                if (root.StateMachine.CurrentState == AppState.LoadingWorld)
                {
                    root.StateMachine.TryTransition(AppState.InWorld);
                }
            }

            ApplySafeArea();
            ApplyPerformanceOptimizations();
            BindWelcomeText();

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

            if (_availableQuizzesButton != null)
            {
                _availableQuizzesButton.onClick.RemoveListener(OnAvailableQuizzesClicked);
            }

            if (_myQuizResultsButton != null)
            {
                _myQuizResultsButton.onClick.RemoveListener(OnMyQuizResultsClicked);
            }

            if (_backMainMenuButton != null)
            {
                _backMainMenuButton.onClick.RemoveListener(OnBackMainMenuClicked);
            }
        }

        /// <summary>
        /// Binds welcome greeting from authenticated student profile.
        /// Exposed for EditMode tests.
        /// </summary>
        public void BindWelcomeText()
        {
            var root = CompositionRoot.Instance;
            string displayName = "Explorer";

            if (root?.AuthSession != null && root.AuthSession.IsAuthenticated)
            {
                displayName = root.AuthSession.StudentName ?? "Explorer";
            }

            if (_welcomeText != null)
            {
                _welcomeText.text = $"Welcome, {displayName}!";
            }

            if (_descriptionText != null && string.IsNullOrWhiteSpace(_descriptionText.text))
            {
                _descriptionText.text =
                    "Challenge yourself with fun quizzes across topics and terms. Learn, grow, and celebrate progress!";
            }
        }

        private void ApplySafeArea()
        {
            if (_mainCanvasGroup == null) return;

            var saObj = new GameObject("RuntimeSafeAreaPanel", typeof(RectTransform));
            saObj.transform.SetParent(_mainCanvasGroup.transform, false);

            var saRect = saObj.GetComponent<RectTransform>();
            saRect.anchorMin = Vector2.zero;
            saRect.anchorMax = Vector2.one;
            saRect.offsetMin = Vector2.zero;
            saRect.offsetMax = Vector2.zero;

            int childCount = _mainCanvasGroup.transform.childCount;
            var childrenToMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < childCount; i++)
            {
                Transform child = _mainCanvasGroup.transform.GetChild(i);
                if (child != saObj.transform &&
                    !child.name.ToLowerInvariant().Contains("bg") &&
                    !child.name.ToLowerInvariant().Contains("background"))
                {
                    childrenToMove.Add(child);
                }
            }

            foreach (var child in childrenToMove)
            {
                child.SetParent(saRect, true);
            }

            Rect safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

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
            if (_backgroundImage != null)
            {
                _backgroundImage.raycastTarget = false;
            }

            if (_welcomeText != null) _welcomeText.raycastTarget = false;
            if (_descriptionText != null) _descriptionText.raycastTarget = false;
        }

        private void OnAvailableQuizzesClicked()
        {
            if (_navigation != null)
            {
                _navigation.ShowAvailableQuizList();
            }
        }

        private void OnMyQuizResultsClicked()
        {
            if (_navigation != null)
            {
                _navigation.ShowQuizResults();
            }
        }

        private void OnBackMainMenuClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(FadeAndLoadRoutine("MainMenu", AppState.MainMenu));
        }

        private IEnumerator FadeInRoutine()
        {
            float elapsed = 0f;
            const float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _mainCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            _mainCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeAndLoadRoutine(string sceneKey, AppState targetState)
        {
            var root = CompositionRoot.Instance;
            if (root?.StateMachine != null)
            {
                root.StateMachine.TryTransition(targetState);
            }

            System.GC.Collect();
            AppNavigation.LoadScene(sceneKey);
            yield break;
        }
    }
}
