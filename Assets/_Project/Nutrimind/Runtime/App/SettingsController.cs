using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Thin controller for the Settings scene.
    /// Manages loading, binding, modifying, and saving user settings,
    /// safe-area optimizations, and transitions.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("UI Sliders")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Slider _musicSlider;

        [Header("UI Dropdowns")]
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private TMP_Dropdown _textSizeDropdown;
        [SerializeField] private TMP_Dropdown _accessibilityDropdown;

        [Header("Controls & Transitions")]
        [SerializeField] private Button _logoutButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        [Header("Layout Optimization")]
        [SerializeField] private RectTransform _safeAreaPanel;

        private CancellationTokenSource _cts;
        private bool _isTransitioning;

        // ──────────────────────────────────────────────────────────────
        //  Public Setters for Editor Auto-Wiring (Avoids forbidden Reflection)
        // ──────────────────────────────────────────────────────────────
        public void SetVolumeSlider(Slider val) => _volumeSlider = val;
        public void SetMusicSlider(Slider val) => _musicSlider = val;
        public void SetLanguageDropdown(TMP_Dropdown val) => _languageDropdown = val;
        public void SetTextSizeDropdown(TMP_Dropdown val) => _textSizeDropdown = val;
        public void SetAccessibilityDropdown(TMP_Dropdown val) => _accessibilityDropdown = val;
        public void SetLogoutButton(Button val) => _logoutButton = val;
        public void SetSaveButton(Button val) => _saveButton = val;
        public void SetMainCanvasGroup(CanvasGroup val) => _mainCanvasGroup = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;
        public void SetSafeAreaPanel(RectTransform val) => _safeAreaPanel = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (_saveButton != null)
            {
                _saveButton.onClick.AddListener(OnSaveClicked);
            }

            if (_logoutButton != null)
            {
                _logoutButton.onClick.AddListener(OnLogoutClicked);
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
            StartCoroutine(LoadSettingsRoutine());
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            if (_saveButton != null)
            {
                _saveButton.onClick.RemoveListener(OnSaveClicked);
            }

            if (_logoutButton != null)
            {
                _logoutButton.onClick.RemoveListener(OnLogoutClicked);
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
            // Optimize raycast target on dropdown sub-labels or backgrounds if necessary
            // In the settings panel, we keep labels static (raycastTarget = false)
        }

        private IEnumerator LoadSettingsRoutine()
        {
            // Initialize with loading / default states
            SetControlsInteractable(false);

            var root = CompositionRoot.Instance;
            if (root != null)
            {
                var dataProvider = root.DataProvider;
                if (dataProvider != null)
                {
                    var task = dataProvider.GetSettingsAsync(_cts.Token);
                    yield return new WaitUntil(() => task.IsCompleted);

                    if (!_cts.Token.IsCancellationRequested && task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        var result = task.Result;
                        if (result != null && result.Success && result.Data != null)
                        {
                            ApplySettingsDtoToUi(result.Data);
                        }
                        else
                        {
                            Debug.LogWarning("[SettingsController] Failed to fetch settings. Falling back to session store or defaults.");
                            ApplyFallbackSettings(root);
                        }
                    }
                    else
                    {
                        ApplyFallbackSettings(root);
                    }
                }
                else
                {
                    ApplyFallbackSettings(root);
                }
            }
            else
            {
                ApplyFallbackDefaults();
            }

            SetControlsInteractable(true);

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

        private void ApplySettingsDtoToUi(Dto.SettingsDto data)
        {
            if (_volumeSlider != null)
            {
                _volumeSlider.value = data.SfxVolume ?? 1f;
            }

            if (_musicSlider != null)
            {
                _musicSlider.value = data.MusicVolume ?? 1f;
            }

            if (_languageDropdown != null)
            {
                string lang = data.Language ?? "en";
                _languageDropdown.value = (lang.ToLower() == "fil") ? 1 : 0;
            }

            if (_textSizeDropdown != null)
            {
                string size = data.TextSize ?? "medium";
                switch (size.ToLower())
                {
                    case "small":
                        _textSizeDropdown.value = 0;
                        break;
                    case "large":
                        _textSizeDropdown.value = 2;
                        break;
                    default:
                        _textSizeDropdown.value = 1;
                        break;
                }
            }

            if (_accessibilityDropdown != null)
            {
                bool motion = data.ReducedMotion ?? false;
                _accessibilityDropdown.value = motion ? 1 : 0;
            }
        }

        private void ApplyFallbackSettings(CompositionRoot root)
        {
            var session = root.Session;
            if (session != null && session.SettingsStore != null)
            {
                var store = session.SettingsStore;
                if (_volumeSlider != null) _volumeSlider.value = store.SfxVolume;
                if (_musicSlider != null) _musicSlider.value = store.MusicVolume;
                if (_languageDropdown != null)
                {
                    string lang = store.Language ?? "en";
                    _languageDropdown.value = (lang.ToLower() == "fil") ? 1 : 0;
                }
                if (_textSizeDropdown != null) _textSizeDropdown.value = 1; // Default Medium
                if (_accessibilityDropdown != null) _accessibilityDropdown.value = 0; // Default Off
            }
            else
            {
                ApplyFallbackDefaults();
            }
        }

        private void ApplyFallbackDefaults()
        {
            if (_volumeSlider != null) _volumeSlider.value = 1f;
            if (_musicSlider != null) _musicSlider.value = 1f;
            if (_languageDropdown != null) _languageDropdown.value = 0; // English
            if (_textSizeDropdown != null) _textSizeDropdown.value = 1; // Medium
            if (_accessibilityDropdown != null) _accessibilityDropdown.value = 0; // Off
        }

        private void SetControlsInteractable(bool state)
        {
            if (_volumeSlider != null) _volumeSlider.interactable = state;
            if (_musicSlider != null) _musicSlider.interactable = state;
            if (_languageDropdown != null) _languageDropdown.interactable = state;
            if (_textSizeDropdown != null) _textSizeDropdown.interactable = state;
            if (_accessibilityDropdown != null) _accessibilityDropdown.interactable = state;
            if (_logoutButton != null) _logoutButton.interactable = state;
            if (_saveButton != null) _saveButton.interactable = state;
        }

        private void OnSaveClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(SaveAndExitRoutine());
        }

        private IEnumerator SaveAndExitRoutine()
        {
            SetControlsInteractable(false);

            var root = CompositionRoot.Instance;
            if (root != null)
            {
                var dataProvider = root.DataProvider;
                if (dataProvider != null)
                {
                    // Construct DTO to patch
                    var dto = new Dto.SettingsDto();
                    
                    if (_volumeSlider != null) dto.SfxVolume = _volumeSlider.value;
                    if (_musicSlider != null) dto.MusicVolume = _musicSlider.value;
                    
                    if (_languageDropdown != null)
                    {
                        dto.Language = (_languageDropdown.value == 1) ? "fil" : "en";
                    }

                    if (_textSizeDropdown != null)
                    {
                        dto.TextSize = _textSizeDropdown.value == 0 ? "small" : (_textSizeDropdown.value == 2 ? "large" : "medium");
                    }

                    if (_accessibilityDropdown != null)
                    {
                        dto.ReducedMotion = (_accessibilityDropdown.value == 1);
                    }

                    // Call backend async PatchSettings
                    var task = dataProvider.PatchSettingsAsync(dto, _cts.Token);
                    yield return new WaitUntil(() => task.IsCompleted);

                    if (!_cts.Token.IsCancellationRequested && task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        var result = task.Result;
                        if (result != null && result.Success && result.Data != null)
                        {
                            // Update local store as well
                            if (root.Session != null)
                            {
                                if (root.Session.SettingsStore == null)
                                {
                                    root.Session.SettingsStore = new SettingsStore();
                                }
                                var store = root.Session.SettingsStore;
                                store.SfxVolume = dto.SfxVolume ?? 1f;
                                store.MusicVolume = dto.MusicVolume ?? 1f;
                                store.Language = dto.Language;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[SettingsController] Failed to save settings to server. Saving locally only.");
                            UpdateLocalStoreOnly(root, dto);
                        }
                    }
                    else
                    {
                        UpdateLocalStoreOnly(root, dto);
                    }
                }
            }

            // Smooth Fade Out
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

            System.GC.Collect();
            AppNavigation.LoadScene("MainMenu");
        }

        private void UpdateLocalStoreOnly(CompositionRoot root, Dto.SettingsDto dto)
        {
            if (root.Session != null)
            {
                if (root.Session.SettingsStore == null)
                {
                    root.Session.SettingsStore = new SettingsStore();
                }
                var store = root.Session.SettingsStore;
                store.SfxVolume = dto.SfxVolume ?? 1f;
                store.MusicVolume = dto.MusicVolume ?? 1f;
                store.Language = dto.Language;
            }
        }

        private void OnLogoutClicked()
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(LogoutAndExitRoutine());
        }

        private IEnumerator LogoutAndExitRoutine()
        {
            SetControlsInteractable(false);

            var root = CompositionRoot.Instance;
            if (root != null)
            {
                var dataProvider = root.DataProvider;
                if (dataProvider != null)
                {
                    var task = dataProvider.LogoutAsync(_cts.Token);
                    yield return new WaitUntil(() => task.IsCompleted);

                    // Clear local session state regardless of backend success
                    root.Session.Clear();
                    root.AuthSession.Reset();
                }
            }

            // Smooth Fade Out
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

            System.GC.Collect();
            AppNavigation.LoadScene("Login");
        }
    }
}
