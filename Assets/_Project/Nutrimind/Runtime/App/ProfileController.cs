using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Thin controller for the Profile scene.
    /// Manages fetching student profile details, binding data to Canvas labels,
    /// safe-area optimizations, and transitions.
    /// </summary>
    public class ProfileController : MonoBehaviour
    {
        [Header("UI Text Fields")]
        [SerializeField] private TextMeshProUGUI _studentNameText;
        [SerializeField] private TextMeshProUGUI _gradeSectionText;
        [SerializeField] private TextMeshProUGUI _lrnText;
        [SerializeField] private TextMeshProUGUI _roomText;
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _badgesText;

        [Header("Controls & Transitions")]
        [SerializeField] private Button _backButton;
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        [SerializeField] private VideoPlayer _bgVideoPlayer;

        [Header("Layout Optimization")]
        [SerializeField] private RectTransform _safeAreaPanel;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        // ──────────────────────────────────────────────────────────────
        //  Public Setters for Editor Auto-Wiring (Avoids forbidden Reflection)
        // ──────────────────────────────────────────────────────────────
        public void SetStudentNameText(TextMeshProUGUI val) => _studentNameText = val;
        public void SetGradeSectionText(TextMeshProUGUI val) => _gradeSectionText = val;
        public void SetLrnText(TextMeshProUGUI val) => _lrnText = val;
        public void SetRoomText(TextMeshProUGUI val) => _roomText = val;
        public void SetLevelNumberText(TextMeshProUGUI val) => _levelNumberText = val;
        public void SetCoinsText(TextMeshProUGUI val) => _coinsText = val;
        public void SetBadgesText(TextMeshProUGUI val) => _badgesText = val;
        public void SetBackButton(Button val) => _backButton = val;
        public void SetMainCanvasGroup(CanvasGroup val) => _mainCanvasGroup = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;
        public void SetBgVideoPlayer(VideoPlayer val) => _bgVideoPlayer = val;
        public void SetSafeAreaPanel(RectTransform val) => _safeAreaPanel = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

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
            StartCoroutine(LoadProfileRoutine());
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

            // Move all Canvas children except the background video/image and the panel itself
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
            // Uncheck Raycast Target on labels to optimize GraphicRaycaster sweeps
            if (_studentNameText != null) _studentNameText.raycastTarget = false;
            if (_gradeSectionText != null) _gradeSectionText.raycastTarget = false;
            if (_lrnText != null) _lrnText.raycastTarget = false;
            if (_roomText != null) _roomText.raycastTarget = false;
            if (_levelNumberText != null) _levelNumberText.raycastTarget = false;
            if (_coinsText != null) _coinsText.raycastTarget = false;
            if (_badgesText != null) _badgesText.raycastTarget = false;

            // Pre-warm VideoPlayer if it exists and hasn't played yet
            if (_bgVideoPlayer != null && !_bgVideoPlayer.isPrepared)
            {
                _bgVideoPlayer.Prepare();
            }
        }

        private IEnumerator LoadProfileRoutine()
        {
            // Set dynamic text labels to loading indicator
            SetTextValues("Loading...", "Loading...", "Loading...", "Loading...", "..", "...", "...");

            // Async load bootstrap data
            var root = CompositionRoot.Instance;
            if (root != null)
            {
                var dataProvider = root.DataProvider;
                if (dataProvider != null)
                {
                    var task = dataProvider.GetBootstrapAsync(_cts.Token);
                    yield return new WaitUntil(() => task.IsCompleted);

                    if (!_cts.Token.IsCancellationRequested && task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        var result = task.Result;
                        if (result != null && result.Success && result.Data != null)
                        {
                            var data = result.Data;
                            string name = data.Student?.Name ?? root.AuthSession.StudentName ?? "Explorer";
                            string lrn = data.Student?.LrnMasked ?? root.AuthSession.LrnMasked ?? "••••••••••••";
                            string section = data.Classroom?.Section ?? "A";
                            string roomName = data.Classroom?.Name ?? $"Section {section}";
                            int level = data.Student?.GradeLevel ?? root.AuthSession.GradeLevel ?? 5;
                            int coins = data.ProgressSummary?.Coins ?? 0;
                            int completedQuizzes = data.ProgressSummary?.TotalQuizzesCompleted ?? 0;
                            int totalQuizzes = data.ProgressSummary?.TotalQuizzesAvailable ?? 12;

                            SetTextValues(
                                name,
                                $"Grade {level} - Section {section}",
                                lrn,
                                roomName,
                                level.ToString(),
                                coins.ToString("N0"),
                                $"{completedQuizzes} / {totalQuizzes}"
                            );
                        }
                        else
                        {
                            Debug.LogWarning("[ProfileController] Failed to fetch bootstrap. Falling back to cached session data.");
                            ApplyFallbackData(root);
                        }
                    }
                    else
                    {
                        ApplyFallbackData(root);
                    }
                }
                else
                {
                    ApplyFallbackData(root);
                }
            }
            else
            {
                SetTextValues("Offline Explorer", "Grade 5", "••••••••••••", "Offline Mode", "5", "0", "0 / 12");
            }

            // Smooth Fade In
            if (_mainCanvasGroup != null)
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
        }

        private void ApplyFallbackData(CompositionRoot root)
        {
            var auth = root.AuthSession;
            string name = auth.StudentName ?? "Explorer";
            string lrn = auth.LrnMasked ?? "••••••••••••";
            int level = auth.GradeLevel ?? 5;

            SetTextValues(
                name,
                $"Grade {level} - Section A",
                lrn,
                $"Grade {level} Section A",
                level.ToString(),
                "0",
                "0 / 12"
            );
        }

        private void SetTextValues(string name, string gradeSection, string lrn, string room, string lvl, string coins, string badges)
        {
            if (_studentNameText != null) _studentNameText.text = name;
            if (_gradeSectionText != null) _gradeSectionText.text = gradeSection;
            if (_lrnText != null) _lrnText.text = lrn;
            if (_roomText != null) _roomText.text = room;
            if (_levelNumberText != null) _levelNumberText.text = lvl;
            if (_coinsText != null) _coinsText.text = coins;
            if (_badgesText != null) _badgesText.text = badges;
        }

        private void OnBackClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(FadeAndExitRoutine());
        }

        private IEnumerator FadeAndExitRoutine()
        {
            if (_bgVideoPlayer != null && _bgVideoPlayer.isPlaying)
            {
                _bgVideoPlayer.Stop();
                Debug.Log("[ProfileController] Stopped background VideoPlayer to optimize transition resources.");
            }

            System.GC.Collect();
            AppNavigation.LoadScene("MainMenu");
            yield break;
        }
    }
}
