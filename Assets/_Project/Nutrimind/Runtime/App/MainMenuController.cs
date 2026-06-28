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
    /// Thin controller for the Main Interface / Main Menu scene.
    /// Manages student profile dynamic bindings, button clicks, transitions,
    /// and implements UI/Canvas performance optimizations.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Profile & Display")]
        [SerializeField] private TextMeshProUGUI _studentNameText;
        [SerializeField] private TextMeshProUGUI _gradeText;
        [SerializeField] private TextMeshProUGUI _sectionText;

        [Header("Interactive Buttons")]
        [SerializeField] private Button _quizButton;          // Big "QUIZ" assessment button
        [SerializeField] private Button _playButton;          // Big "CLASSIC" play/assessment button (openworld gameplay)
        [SerializeField] private Button _profilePanelButton;  // Entire Top-Left profile panel is a button
        [SerializeField] private Button _settingsButton;      // Consolidated settings button
        [SerializeField] private Button _worldHubButton;      // World hub transition button

        [Header("Milestone Controls")]
        [SerializeField] private GameObject _worldHubLockedBadge; // Optional padlock/badge

        [Header("Optimization & Transitions")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;
        [SerializeField] private VideoPlayer _bgVideoPlayer;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        // ──────────────────────────────────────────────────────────────
        //  Public Setters for Editor Auto-Wiring (Avoids forbidden Reflection)
        // ──────────────────────────────────────────────────────────────
        public void SetStudentNameText(TextMeshProUGUI val) => _studentNameText = val;
        public void SetGradeText(TextMeshProUGUI val) => _gradeText = val;
        public void SetSectionText(TextMeshProUGUI val) => _sectionText = val;
        public void SetQuizButton(Button val) => _quizButton = val;
        public void SetPlayButton(Button val) => _playButton = val;
        public void SetProfilePanelButton(Button val) => _profilePanelButton = val;
        public void SetSettingsButton(Button val) => _settingsButton = val;
        public void SetWorldHubButton(Button val) => _worldHubButton = val;
        public void SetWorldHubLockedBadge(GameObject val) => _worldHubLockedBadge = val;
        public void SetMainCanvasGroup(CanvasGroup val) => _mainCanvasGroup = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;
        public void SetBgVideoPlayer(VideoPlayer val) => _bgVideoPlayer = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            // Set up click listeners
            if (_quizButton != null)
            {
                _quizButton.onClick.AddListener(OnQuizClicked);
            }

            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_profilePanelButton != null)
            {
                _profilePanelButton.onClick.AddListener(OnProfileClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_worldHubButton != null)
            {
                _worldHubButton.onClick.AddListener(OnWorldHubClicked);
            }

            // Ensure transition elements are properly prepared
            if (_mainCanvasGroup != null)
            {
                _mainCanvasGroup.alpha = 1f;
            }
        }

        private void Start()
        {
            ApplyPerformanceOptimizations();
            BindStudentProfile();
            ConfigureMilestoneScope();
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            if (_quizButton != null)
            {
                _quizButton.onClick.RemoveListener(OnQuizClicked);
            }

            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayClicked);
            }

            if (_profilePanelButton != null)
            {
                _profilePanelButton.onClick.RemoveListener(OnProfileClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (_worldHubButton != null)
            {
                _worldHubButton.onClick.RemoveListener(OnWorldHubClicked);
            }
        }

        /// <summary>
        /// Retrieves the student info from the AuthSession and displays it on the UI labels.
        /// </summary>
        private void BindStudentProfile()
        {
            var root = CompositionRoot.Instance;
            if (root == null) return;

            var auth = root.AuthSession;
            if (auth != null && auth.IsAuthenticated)
            {
                if (_studentNameText != null)
                {
                    _studentNameText.text = auth.StudentName ?? "Student Explorer";
                }

                if (_gradeText != null)
                {
                    _gradeText.text = auth.GradeLevel.HasValue ? $"Grade {auth.GradeLevel.Value}" : "Grade --";
                }

                // If a section is hardcoded or configured, we preserve or update it cleanly
                if (_sectionText != null && string.IsNullOrEmpty(_sectionText.text))
                {
                    _sectionText.text = "Section A"; // Default placeholder if empty
                }
            }
            else
            {
                Debug.LogWarning("[MainMenuController] AuthSession is not authenticated. Using static scene fallbacks.");
            }
        }

        /// <summary>
        /// Exposes only the Quiz Portal / Assessment Room and padlocks or disables the Adventure modes.
        /// </summary>
        private void ConfigureMilestoneScope()
        {
            // For this milestone, the openworld playButton is disabled/hidden
            if (_playButton != null)
            {
                _playButton.gameObject.SetActive(false);
            }

            // The World Hub button (world hub icon) and Quiz button are active
            if (_worldHubButton != null)
            {
                _worldHubButton.gameObject.SetActive(true);
            }

            if (_quizButton != null)
            {
                _quizButton.gameObject.SetActive(true);
            }

            if (_worldHubLockedBadge != null)
            {
                _worldHubLockedBadge.SetActive(false); // Make sure the badge is disabled as world hub is active
            }
        }

        /// <summary>
        /// Applies standard UI and scene rendering performance optimizations.
        /// </summary>
        private void ApplyPerformanceOptimizations()
        {
            // Uncheck Raycast Target on static labels to reduce GraphicRaycaster burden
            if (_studentNameText != null) _studentNameText.raycastTarget = false;
            if (_gradeText != null) _gradeText.raycastTarget = false;
            if (_sectionText != null) _sectionText.raycastTarget = false;

            // Pre-warm VideoPlayer if it exists and hasn't played yet
            if (_bgVideoPlayer != null && !_bgVideoPlayer.isPrepared)
            {
                _bgVideoPlayer.Prepare();
            }
        }

        private void OnQuizClicked()
        {
            // Quiz button loads the Assessment Room / Quiz Portal (registered as "Worldhub")
            NavigateToScene("Worldhub");
        }

        private void OnPlayClicked()
        {
            // Play is for future openworld gameplay
            Debug.Log("[MainMenuController] Play button clicked (deferred openworld gameplay).");
        }

        private void OnWorldHubClicked()
        {
            // World Hub button loads the Assessment Room / Quiz Portal (registered as "Worldhub")
            NavigateToScene("Worldhub");
        }

        private void OnProfileClicked()
        {
            NavigateToScene("Profile");
        }

        private void OnSettingsClicked()
        {
            NavigateToScene("Settings");
        }

        /// <summary>
        /// Transition handler that fades out the CanvasGroup, stops background processes,
        /// and navigates to the target scene cleanly.
        /// </summary>
        private void NavigateToScene(string sceneKey)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            // 1. Disable raycaster during transition to avoid double click and multi-clicks
            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(FadeAndLoadRoutine(sceneKey));
        }

        private IEnumerator FadeAndLoadRoutine(string sceneKey)
        {
            // 2. Stop bg video rendering to free memory & GPU cycles immediately
            if (_bgVideoPlayer != null && _bgVideoPlayer.isPlaying)
            {
                _bgVideoPlayer.Stop();
                Debug.Log("[MainMenuController] Stopped background VideoPlayer to optimize transition resources.");
            }

            // 4. Perform garbage collection sweeps on transition frame
            System.GC.Collect();

            // 5. Load target scene asynchronously
            Debug.Log($"[MainMenuController] Smooth transition loading scene key: '{sceneKey}'");
            AppNavigation.LoadScene(sceneKey);
            yield break;
        }
    }
}

