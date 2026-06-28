using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

namespace NutriMind.Runtime.App
{
    public class SplashController : MonoBehaviour
    {
        [Header("Video Setup")]
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;

        [Header("UI Controls")]
        [SerializeField] private GameObject _errorOverlay;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        private CancellationTokenSource _cts;
        private bool _serverCheckCompleted;
        private bool _serverCheckSuccessful;
        private bool _videoFinished;

        private void Awake()
        {
            _cts = new CancellationTokenSource();

            if (_errorOverlay != null)
            {
                _errorOverlay.SetActive(false);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(OnRetryClicked);
            }

            // Initially hide the RawImage to prevent a "flash" of white or uninitialized texture
            if (_rawImage != null)
            {
                _rawImage.enabled = false;
            }

            if (_videoPlayer == null)
            {
                _videoPlayer = GetComponent<VideoPlayer>();
            }

            // Clear the Render Texture to avoid showing a stale frame from the previous run
            if (_videoPlayer != null && _videoPlayer.targetTexture != null)
            {
                ClearRenderTexture(_videoPlayer.targetTexture);
            }
        }

        private void ClearRenderTexture(RenderTexture rt)
        {
            if (rt == null) return;
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = active;
        }

        private void Start()
        {
            if (_videoPlayer == null)
            {
                _videoPlayer = GetComponent<VideoPlayer>();
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached += OnVideoFinished;
                _videoPlayer.errorReceived += OnVideoError;
                _videoPlayer.prepareCompleted += OnVideoPrepared;
                
                Debug.Log("[SplashController] Starting asynchronous VideoPlayer preparation...");
                _videoPlayer.Prepare();
            }
            else
            {
                _videoFinished = true;
            }

            // Start safety timeout to prevent getting stuck if VideoPlayer fails to prepare or fire loopPointReached
            StartCoroutine(SafetyTimeoutRoutine());

            // Start configuration check in parallel
            StartServerCheck();
        }

        private System.Collections.IEnumerator SafetyTimeoutRoutine()
        {
            // Safety timeout is slightly longer than the video length (e.g., 6 seconds)
            yield return new WaitForSeconds(6f);
            if (!_videoFinished)
            {
                Debug.LogWarning("[SplashController] Safety timeout reached before video finished. Bypassing video player.");
                _videoFinished = true;
                if (_videoPlayer != null && _videoPlayer.isPlaying)
                {
                    _videoPlayer.Stop();
                }
                TryExitSplash();
            }
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            Debug.Log("[SplashController] VideoPlayer preparation complete. Beginning playback.");
            if (_rawImage != null)
            {
                _rawImage.texture = vp.texture;
                _rawImage.enabled = true;
            }
            vp.Play();
        }

        private void Update()
        {
            // Dynamically fit RawImage aspect ratio to prevent stretching
            if (_videoPlayer != null && _videoPlayer.isPlaying && _aspectRatioFitter != null && _videoPlayer.texture != null)
            {
                float videoAspect = (float)_videoPlayer.texture.width / _videoPlayer.texture.height;
                if (Mathf.Abs(_aspectRatioFitter.aspectRatio - videoAspect) > 0.01f)
                {
                    _aspectRatioFitter.aspectRatio = videoAspect;
                }
            }

            // Detect skip trigger (mouse click or touch tap anywhere)
            if (!_videoFinished && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
            {
                Debug.Log("[SplashController] Skip triggered by user click/tap.");
                _videoFinished = true;
                if (_videoPlayer != null && _videoPlayer.isPlaying)
                {
                    _videoPlayer.Stop();
                }
                TryExitSplash();
            }
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= OnVideoFinished;
                _videoPlayer.errorReceived -= OnVideoError;
                _videoPlayer.prepareCompleted -= OnVideoPrepared;
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(OnRetryClicked);
            }
        }

        private void StartServerCheck()
        {
            _serverCheckCompleted = false;
            _serverCheckSuccessful = false;

            if (_errorOverlay != null)
            {
                _errorOverlay.SetActive(false);
            }

            // Perform config check async
            RunServerCheckAsync(_cts.Token);
        }

        private async void RunServerCheckAsync(CancellationToken ct)
        {
            try
            {
                var root = CompositionRoot.Instance;
                if (root == null)
                {
                    ShowError("System initialization error. Please restart the game.");
                    return;
                }

                var stateMachine = root.StateMachine;
                var provider = root.DataProvider;

                if (provider == null)
                {
                    ShowError("Data provider not initialized.");
                    return;
                }

                // If state machine is still in Starting, transition to CheckingServer
                if (stateMachine.CurrentState == AppState.Starting)
                {
                    stateMachine.TryTransition(AppState.CheckingServer);
                }

                var configResult = await provider.GetConfigAsync(ct);

                if (ct.IsCancellationRequested) return;

                if (configResult.Success && configResult.Data != null)
                {
                    var config = configResult.Data;

                    // 1. Check Maintenance Mode
                    if (config.MaintenanceMode == true)
                    {
                        stateMachine.TryTransition(AppState.MaintenanceBlocked);
                        ShowError("The game is currently under maintenance.\nPlease try again later.");
                        return;
                    }

                    // 2. Check Client Version compatibility if needed
                    // (We can extend this if a specific version check is requested)

                    // 3. Success! Transition state machine to LoggedOut
                    stateMachine.TryTransition(AppState.LoggedOut);
                    _serverCheckSuccessful = true;
                    _serverCheckCompleted = true;

                    // If video was already finished, exit now
                    if (_videoFinished)
                    {
                        TryExitSplash();
                    }
                }
                else
                {
                    string errorMsg = "Cannot connect to server. Please check your internet connection.";
                    if (configResult.Error != null && !string.IsNullOrEmpty(configResult.Error.Message))
                    {
                        errorMsg = configResult.Error.Message;
                    }
                    ShowError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ShowError("An unexpected error occurred during server check.");
            }
        }

        private void ShowError(string message)
        {
            _serverCheckCompleted = true;
            _serverCheckSuccessful = false;

            if (_videoPlayer != null && _videoPlayer.isPlaying)
            {
                _videoPlayer.Pause();
            }

            if (_errorOverlay != null)
            {
                _errorOverlay.SetActive(true);
            }

            if (_errorText != null)
            {
                _errorText.text = message;
            }
        }

        private void OnRetryClicked()
        {
            if (_videoPlayer != null)
            {
                _videoPlayer.Play();
            }

            StartServerCheck();
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[SplashController] Splash video playback completed.");
            _videoFinished = true;
            TryExitSplash();
        }

        private void OnVideoError(VideoPlayer vp, string message)
        {
            Debug.LogWarning($"[SplashController] Video playback error: {message}. Skipping video player block.");
            _videoFinished = true;
            TryExitSplash();
        }

        private void TryExitSplash()
        {
            // Only transition if the server check completed successfully
            if (_serverCheckCompleted && _serverCheckSuccessful)
            {
                Debug.Log("[SplashController] Transitioning to Login screen.");
                StartCoroutine(FadeAndLoadRoutine());
            }
            else if (_serverCheckCompleted && !_serverCheckSuccessful)
            {
                // Wait for the player to resolve the error (e.g. click Retry)
                Debug.Log("[SplashController] Server check failed or still in error. Blocking transition.");
            }
        }

        private System.Collections.IEnumerator FadeAndLoadRoutine()
        {
            // 1. Resolve path for Login scene and begin asynchronous preloading in the background
            var root = CompositionRoot.Instance;
            string scenePath = null;
            if (root != null && root.NavigationService != null)
            {
                var navResult = root.NavigationService.Navigate("Login");
                if (navResult.IsAvailable)
                {
                    scenePath = navResult.ScenePath;
                }
            }

            AsyncOperation op = null;
            if (!string.IsNullOrEmpty(scenePath))
            {
                Debug.Log($"[SplashController] Pre-loading Login scene asynchronously in background: {scenePath}");
                op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);
                if (op != null)
                {
                    op.allowSceneActivation = false;
                }
            }

            // 2. Keep the Splash screen fully visible while the Login scene is loading in the background
            if (op != null)
            {
                while (op.progress < 0.9f)
                {
                    yield return null;
                }
                Debug.Log("[SplashController] Login scene is fully loaded and ready. Starting fade out.");
            }

            // 3. Smooth Fade Out (0.5s)
            if (_canvasGroup != null)
            {
                float elapsed = 0f;
                float duration = 0.5f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                    yield return null;
                }
                _canvasGroup.alpha = 0f;
            }

            // 4. Pre-warm TMPro font loading to avoid frame drops on rendering the input text inside Login scene
            try
            {
                var settings = TMP_Settings.defaultFontAsset;
                if (settings != null)
                {
                    Debug.Log("[SplashController] Pre-warmed default TMP Font Asset: " + settings.name);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SplashController] Pre-warming TMP Font Asset failed: " + ex.Message);
            }

            // 5. Defer major GC call to the exact transition frame
            System.GC.Collect();
            Debug.Log("[SplashController] Triggered garbage collection transition sweep.");

            // 6. Activate the pre-loaded Login scene instantly!
            if (op != null)
            {
                op.allowSceneActivation = true;
            }
            else
            {
                // Fallback standard load if scene preloading wasn't started
                AppNavigation.LoadScene("Login");
            }
        }
    }
}