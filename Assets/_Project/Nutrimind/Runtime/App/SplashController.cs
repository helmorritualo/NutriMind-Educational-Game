using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        private AsyncOperation _loginLoadOp;
        private bool _aspectRatioApplied;

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

            if (_rawImage != null)
            {
                _rawImage.raycastTarget = false;
                _rawImage.enabled = false;
            }

            if (_videoPlayer == null)
            {
                _videoPlayer = GetComponent<VideoPlayer>();
            }

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

            BeginLoginPreload();

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

            StartCoroutine(SafetyTimeoutRoutine());
            StartServerCheck();
        }

        private void BeginLoginPreload()
        {
            var root = CompositionRoot.Instance;
            if (root?.NavigationService == null) return;

            var navResult = root.NavigationService.Navigate("Login");
            if (!navResult.IsAvailable || string.IsNullOrEmpty(navResult.ScenePath)) return;

            Debug.Log($"[SplashController] Pre-loading Login scene in background: {navResult.ScenePath}");
            _loginLoadOp = SceneManager.LoadSceneAsync(navResult.ScenePath);
            if (_loginLoadOp != null)
            {
                _loginLoadOp.allowSceneActivation = false;
            }
        }

        private System.Collections.IEnumerator SafetyTimeoutRoutine()
        {
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

            ApplyVideoAspectRatio(vp);
            vp.Play();
        }

        private void ApplyVideoAspectRatio(VideoPlayer vp)
        {
            if (_aspectRatioApplied || _aspectRatioFitter == null || vp.texture == null) return;
            if (vp.texture.height <= 0) return;

            _aspectRatioFitter.aspectRatio = (float)vp.texture.width / vp.texture.height;
            _aspectRatioApplied = true;
        }

        private void Update()
        {
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

                if (stateMachine.CurrentState == AppState.Starting)
                {
                    stateMachine.TryTransition(AppState.CheckingServer);
                }

                var configResult = await provider.GetConfigAsync(ct);

                if (ct.IsCancellationRequested) return;

                if (configResult.Success && configResult.Data != null)
                {
                    var config = configResult.Data;

                    if (config.MaintenanceMode == true)
                    {
                        stateMachine.TryTransition(AppState.MaintenanceBlocked);
                        ShowError("The game is currently under maintenance.\nPlease try again later.");
                        return;
                    }

                    stateMachine.TryTransition(AppState.LoggedOut);
                    _serverCheckSuccessful = true;
                    _serverCheckCompleted = true;

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
            if (_serverCheckCompleted && _serverCheckSuccessful)
            {
                Debug.Log("[SplashController] Transitioning to Login screen.");
                StartCoroutine(FadeAndLoadRoutine());
            }
            else if (_serverCheckCompleted && !_serverCheckSuccessful)
            {
                Debug.Log("[SplashController] Server check failed or still in error. Blocking transition.");
            }
        }

        private System.Collections.IEnumerator FadeAndLoadRoutine()
        {
            if (_loginLoadOp == null)
            {
                BeginLoginPreload();
            }

            if (_loginLoadOp != null)
            {
                while (_loginLoadOp.progress < 0.9f)
                {
                    yield return null;
                }
                Debug.Log("[SplashController] Login scene is fully loaded and ready. Starting fade out.");
            }

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

            WarmupTmpFont();

            if (_loginLoadOp != null)
            {
                _loginLoadOp.allowSceneActivation = true;
            }
            else
            {
                AppNavigation.LoadScene("Login");
            }
        }

        private static void WarmupTmpFont()
        {
            try
            {
                var settings = TMP_Settings.defaultFontAsset;
                if (settings != null)
                {
                    settings.HasCharacter('A');
                    Debug.Log("[SplashController] Pre-warmed default TMP Font Asset: " + settings.name);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[SplashController] Pre-warming TMP Font Asset failed: " + ex.Message);
            }
        }
    }
}
