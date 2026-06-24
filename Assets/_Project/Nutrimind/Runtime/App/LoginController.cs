using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NutriMind.Runtime.App.Dto;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Coordinates the Login screen's Canvas UI, handles credentials validation,
    /// and transitions the application state through authentication and bootstrapping.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [Header("UI Fields")]
        [SerializeField] private TMP_InputField LrnInput;
        [SerializeField] private TMP_InputField PinInput;
        [SerializeField] private Button LoginButton;
        [SerializeField] private Button DemoLoginButton;
        [SerializeField] private TextMeshProUGUI ErrorText;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (LoginButton != null)
            {
                LoginButton.onClick.AddListener(OnLoginClicked);
            }

            if (DemoLoginButton != null)
            {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                DemoLoginButton.gameObject.SetActive(false);
                Destroy(DemoLoginButton.gameObject);
#else
                DemoLoginButton.onClick.AddListener(OnDemoLoginClicked);
#endif
            }
        }

        private void Start()
        {
            // Clear any error messages from the inspector template on startup
            if (ErrorText != null)
            {
                ErrorText.text = "";
            }
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            if (LoginButton != null)
            {
                LoginButton.onClick.RemoveListener(OnLoginClicked);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (DemoLoginButton != null)
            {
                DemoLoginButton.onClick.RemoveListener(OnDemoLoginClicked);
            }
#endif
        }

        private void OnLoginClicked()
        {
            string lrn = LrnInput != null ? LrnInput.text : "";
            string pin = PinInput != null ? PinInput.text : "";

            // Client-side validation: check if empty
            if (string.IsNullOrEmpty(lrn) || string.IsNullOrEmpty(pin))
            {
                if (ErrorText != null)
                {
                    ErrorText.text = "Please enter LRN and PIN.";
                }
                return;
            }

            PerformLogin(lrn, pin);
        }

        private void OnDemoLoginClicked()
        {
            const string demoLrn = "000000000001";
            const string demoPin = "1234";

            // Auto-fill inputs
            if (LrnInput != null) LrnInput.text = demoLrn;
            if (PinInput != null) PinInput.text = demoPin;

            PerformLogin(demoLrn, demoPin);
        }

        private async void PerformLogin(string lrn, string pin)
        {
            if (ErrorText != null)
            {
                ErrorText.text = "";
            }

            SetInputState(false);

            try
            {
                var root = CompositionRoot.Instance;
                var stateMachine = root.StateMachine;
                var dataProvider = root.DataProvider;

                // Transition state machine to AppState.Authenticating
                if (!stateMachine.TryTransition(AppState.Authenticating))
                {
                    Debug.LogWarning("[LoginController] State machine rejected transition to Authenticating. Current state: " + stateMachine.CurrentState);
                }

                // Call DataProvider.LoginAsync
                var loginRequest = new LoginRequestDto { Lrn = lrn, Pin = pin };
                var loginResult = await dataProvider.LoginAsync(loginRequest, _cts.Token);

                if (_cts.Token.IsCancellationRequested) return;

                if (loginResult.Success && loginResult.Data != null)
                {
                    // ApplyLoginResponse
                    root.AuthSession.ApplyLoginResponse(loginResult.Data);

                    // Transition state machine to AppState.Bootstrapping
                    if (!stateMachine.TryTransition(AppState.Bootstrapping))
                    {
                        Debug.LogWarning("[LoginController] State machine rejected transition to Bootstrapping. Current state: " + stateMachine.CurrentState);
                    }

                    // Call DataProvider.GetBootstrapAsync()
                    var bootstrapResult = await dataProvider.GetBootstrapAsync(_cts.Token);

                    if (_cts.Token.IsCancellationRequested) return;

                    if (bootstrapResult.Success)
                    {
                        // Transition state machine to AppState.MainMenu
                        if (!stateMachine.TryTransition(AppState.MainMenu))
                        {
                            Debug.LogWarning("[LoginController] State machine rejected transition to MainMenu. Current state: " + stateMachine.CurrentState);
                        }

                        // Call AppNavigation.LoadScene("MainMenu") if in play mode
                        if (Application.isPlaying)
                        {
                            AppNavigation.LoadScene("MainMenu");
                        }
                        else
                        {
                            Debug.Log("[LoginController] Mock navigation: AppNavigation.LoadScene(\"MainMenu\") bypassed in EditMode.");
                        }
                    }
                    else
                    {
                        // Bootstrap failed
                        HandleFailure(bootstrapResult.Error?.Message ?? "Failed to fetch bootstrap data. Please try again.");
                    }
                }
                else
                {
                    // Login failed
                    HandleFailure(loginResult.Error?.Message ?? "Invalid LRN or PIN. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                HandleFailure("An unexpected error occurred. Please try again.");
            }
        }

        private void HandleFailure(string errorMessage)
        {
            if (ErrorText != null)
            {
                ErrorText.text = errorMessage;
            }

            SetInputState(true);

            var root = CompositionRoot.Instance;
            if (root != null && root.StateMachine != null)
            {
                root.StateMachine.TryTransition(AppState.LoggedOut);
            }
        }

        private void SetInputState(bool enabled)
        {
            if (LrnInput != null) LrnInput.interactable = enabled;
            if (PinInput != null) PinInput.interactable = enabled;
            if (LoginButton != null) LoginButton.interactable = enabled;
            if (DemoLoginButton != null) DemoLoginButton.interactable = enabled;
        }
    }
}
