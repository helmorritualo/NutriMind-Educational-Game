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

            // Start configuration check in parallel
            StartServerCheck();
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
                AppNavigation.LoadScene("Login");
            }
            else if (_serverCheckCompleted && !_serverCheckSuccessful)
            {
                // Wait for the player to resolve the error (e.g. click Retry)
                Debug.Log("[SplashController] Server check failed or still in error. Blocking transition.");
            }
        }
    }
}