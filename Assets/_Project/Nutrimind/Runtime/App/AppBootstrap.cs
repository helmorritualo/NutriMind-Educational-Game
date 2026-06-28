using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Initial entry point for the NutriMind application.
    /// Constructs/warms up the composition root, registers all application scenes,
    /// and manages the initial transition into the Splash screen.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Runs before other game scripts
    public class AppBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField] private float _minimumDisplayTime = 1.0f;
        [SerializeField] private bool _loadSplashOnStart = true;

        [Header("UI References")]
        [SerializeField] private UIDocument _uiDocument;

        private Label _statusLabel;

        private void Awake()
        {
            // Dynamically build a beautifully centered loading UI inside AppShell's screen-container
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                var backgroundLayer = _uiDocument.rootVisualElement.Q<VisualElement>("background-layer");
                if (backgroundLayer != null)
                {
                    backgroundLayer.style.backgroundColor = Color.black;
                }

                var screenContainer = _uiDocument.rootVisualElement.Q<VisualElement>("screen-container");
                if (screenContainer != null)
                {
                    var centerContainer = new VisualElement();
                    centerContainer.style.alignItems = Align.Center;
                    centerContainer.style.justifyContent = Justify.Center;
                    centerContainer.style.height = Length.Percent(100);
                    centerContainer.style.width = Length.Percent(100);

                    var titleLabel = new Label("NutriMind");
                    titleLabel.style.fontSize = 36;
                    titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    titleLabel.style.color = new Color(0.298f, 0.686f, 0.314f); // Theme green: var(--nm-color-primary)
                    titleLabel.style.marginBottom = 16;
                    centerContainer.Add(titleLabel);

                    _statusLabel = new Label("Initializing...");
                    _statusLabel.style.fontSize = 18;
                    _statusLabel.style.color = new Color(0.75f, 0.75f, 0.75f); // Light gray for readability on black background
                    centerContainer.Add(_statusLabel);

                    screenContainer.Add(centerContainer);
                }
            }

            UpdateStatus("Initializing Composition Root...");

            // Access/create CompositionRoot
            var root = CompositionRoot.Instance;
            root.Initialize();
        }

        private IEnumerator Start()
        {
            float startTime = Time.time;

            UpdateStatus("Registering Application Scenes...");

            // Register scenes with the SceneRegistry
            var registry = CompositionRoot.Instance.SceneRegistry;
            RegisterScenes(registry);

            // Update state machine
            var stateMachine = CompositionRoot.Instance.StateMachine;
            if (stateMachine.CurrentState == AppState.Starting)
            {
                stateMachine.TryTransition(AppState.CheckingServer);
            }

            // Ensure minimum show time so it looks smooth and professional
            float elapsed = Time.time - startTime;
            float remaining = _minimumDisplayTime - elapsed;
            if (remaining > 0)
            {
                yield return new WaitForSeconds(remaining);
            }

            // Transition to Splash Screen
            if (_loadSplashOnStart)
            {
                UpdateStatus("Loading Splash Screen...");
                NavigateToSplash();
            }
        }

        public void RegisterScenes(SceneRegistry registry)
        {
            // Register standard NutriMind application scenes
            registry.RegisterScene("Bootstrap", "Assets/_Project/Nutrimind/Scenes/App/Bootstrap.unity");
            registry.RegisterScene("SplashScreen", "Assets/_Project/Nutrimind/Scenes/App/SplashScreen.unity");
            registry.RegisterScene("Login", "Assets/_Project/Nutrimind/Scenes/App/Login.unity");
            registry.RegisterScene("MainMenu", "Assets/_Project/Nutrimind/Scenes/App/MainMenu.unity");
            registry.RegisterScene("Profile", "Assets/_Project/Nutrimind/Scenes/App/Profile.unity");
            registry.RegisterScene("Settings", "Assets/_Project/Nutrimind/Scenes/App/Settings.unity");
            registry.RegisterScene("Worldhub", "Assets/_Project/Nutrimind/Scenes/App/Worldhub.unity");
            registry.RegisterScene("LiteraQuestTerms", "Assets/_Project/Nutrimind/Scenes/App/Literaquest Term/LiteraQuestTerms.unity");
            registry.RegisterScene("HealthQuestTerms", "Assets/_Project/Nutrimind/Scenes/App/Health Quest_Term/HealthQuestTerms.unity");
        }

        private void NavigateToSplash()
        {
            var navService = CompositionRoot.Instance.NavigationService;
            var result = navService.Navigate("SplashScreen");

            if (result.IsAvailable && !string.IsNullOrEmpty(result.ScenePath))
            {
                SceneManager.LoadSceneAsync(result.ScenePath);
            }
            else
            {
                Debug.LogError("[AppBootstrap] SplashScreen scene is not registered in SceneRegistry!");
            }
        }

        private void UpdateStatus(string status)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = status;
            }
            Debug.Log($"[Bootstrap] {status}");
        }
    }
}
