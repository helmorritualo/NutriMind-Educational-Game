using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Threading.Tasks;
using NutriMind.Runtime.App;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class LoginControllerTests
    {
        private GameObject _holder;
        private LoginController _controller;
        
        private TMP_InputField _lrnInput;
        private TMP_InputField _pinInput;
        private Button _loginButton;
        private Button _demoLoginButton;
        private TextMeshProUGUI _errorText;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("LoginControllerTestHolder");
            _controller = _holder.AddComponent<LoginController>();

            // Setup mock UI elements
            var lrnGo = new GameObject("LrnInput");
            _lrnInput = lrnGo.AddComponent<TMP_InputField>();
            
            var pinGo = new GameObject("PinInput");
            _pinInput = pinGo.AddComponent<TMP_InputField>();

            var loginBtnGo = new GameObject("LoginButton");
            _loginButton = loginBtnGo.AddComponent<Button>();

            var demoBtnGo = new GameObject("DemoButton");
            _demoLoginButton = demoBtnGo.AddComponent<Button>();

            var errorGo = new GameObject("ErrorText");
            _errorText = errorGo.AddComponent<TextMeshProUGUI>();

            // Use reflection to wire serialized private fields
            SetField("LrnInput", _lrnInput);
            SetField("PinInput", _pinInput);
            SetField("LoginButton", _loginButton);
            SetField("DemoLoginButton", _demoLoginButton);
            SetField("ErrorText", _errorText);

            // Register MainMenu scene to prevent navigation errors
            var root = CompositionRoot.Instance;
            if (root.SceneRegistry.GetScene("MainMenu") == null)
            {
                root.SceneRegistry.RegisterScene("MainMenu", "Assets/_Project/Nutrimind/Scenes/App/MainMenu.unity");
            }

            // Force state machine to LoggedOut baseline
            root.StateMachine.TryTransition(AppState.LoggedOut);

            // Trigger Awake on MonoBehaviour by calling the private method
            CallMethod("Awake");
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
            if (_lrnInput != null) Object.DestroyImmediate(_lrnInput.gameObject);
            if (_pinInput != null) Object.DestroyImmediate(_pinInput.gameObject);
            if (_loginButton != null) Object.DestroyImmediate(_loginButton.gameObject);
            if (_demoLoginButton != null) Object.DestroyImmediate(_demoLoginButton.gameObject);
            if (_errorText != null) Object.DestroyImmediate(_errorText.gameObject);
        }

        [Test]
        public void OnLoginClicked_WithEmptyInputs_ShowsValidationError()
        {
            _lrnInput.text = "";
            _pinInput.text = "";

            CallMethod("OnLoginClicked");

            Assert.That(_errorText.text, Is.EqualTo("Please enter LRN and PIN."));
        }

        [Test]
        public void OnDemoLoginClicked_AutoFillsCredentials()
        {
            _lrnInput.text = "";
            _pinInput.text = "";

            CallMethod("OnDemoLoginClicked");

            Assert.That(_lrnInput.text, Is.EqualTo("000000000001"));
            Assert.That(_pinInput.text, Is.EqualTo("1234"));
        }

        [Test]
        public async Task PerformLogin_WithInvalidCredentials_ShowsErrorAndRevertsState()
        {
            _lrnInput.text = "invalid_lrn";
            _pinInput.text = "invalid_pin";

            var root = CompositionRoot.Instance;
            root.StateMachine.TryTransition(AppState.LoggedOut); // Set baseline state

            CallMethod("OnLoginClicked");

            // Allow the async void PerformLogin task to complete on the main thread
            int timeout = 0;
            while (root.StateMachine.CurrentState == AppState.Authenticating && timeout < 100)
            {
                await Task.Delay(10);
                timeout++;
            }

            Assert.That(_errorText.text, Does.Contain("That LRN or PIN doesn't match") | Does.Contain("Invalid LRN or PIN"));
            Assert.That(root.StateMachine.CurrentState, Is.EqualTo(AppState.LoggedOut));
        }

        [Test]
        public async Task PerformLogin_WithValidCredentials_TransitionsToMainMenu()
        {
            _lrnInput.text = "000000000001";
            _pinInput.text = "1234";

            var root = CompositionRoot.Instance;
            root.StateMachine.TryTransition(AppState.LoggedOut); // Set baseline state

            // Temporarily register MainMenu scene mapping so AppNavigation doesn't log error
            if (root.SceneRegistry.GetScene("MainMenu") == null)
            {
                root.SceneRegistry.RegisterScene("MainMenu", "Assets/_Project/Nutrimind/Scenes/App/MainMenu.unity");
            }

            CallMethod("OnLoginClicked");

            // Wait for transitions to finish (LoggedOut -> Authenticating -> Bootstrapping -> MainMenu)
            int timeout = 0;
            while (root.StateMachine.CurrentState != AppState.MainMenu && timeout < 100)
            {
                await Task.Delay(10);
                timeout++;
            }

            Assert.That(root.StateMachine.CurrentState, Is.EqualTo(AppState.MainMenu));
            Assert.That(root.AuthSession.IsAuthenticated, Is.True);
            Assert.That(root.AuthSession.StudentId, Is.EqualTo("demo-student-5-001"));
        }

        private void SetField(string fieldName, object value)
        {
            var field = typeof(LoginController).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(_controller, value);
            }
        }

        private void CallMethod(string methodName)
        {
            var method = typeof(LoginController).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(_controller, null);
            }
        }
    }
}
