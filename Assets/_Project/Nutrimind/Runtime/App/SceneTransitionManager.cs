using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Persistent, dynamic manager that creates a solid background overlay at runtime
    /// and controls a smooth dark-themed fade in/out transition during asynchronous scene loading.
    /// This completely eliminates the Unity default "gray empty scene space" flash/flicker.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager s_instance;

        public static SceneTransitionManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    GameObject go = new GameObject("NutriMind-SceneTransitionManager");
                    s_instance = go.AddComponent<SceneTransitionManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Image _fadeImage;
        private bool _isTransitioning;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Create Canvas
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 99999; // Always on top of all application views

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Add GraphicRaycaster to block clicks and input actions during scene loading
            gameObject.AddComponent<GraphicRaycaster>();

            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f; // Invisible by default
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // Create solid background image
            GameObject imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(transform, false);

            _fadeImage = imageGo.AddComponent<Image>();
            // Dark professional NutriMind theme background color (#111111)
            _fadeImage.color = new Color(0.067f, 0.067f, 0.067f, 1f);
            
            RectTransform rect = _fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Initiates a smooth dark transition overlay, loads the target scene asynchronously,
        /// and fades the overlay back to transparent once loading completes.
        /// </summary>
        public void TransitionToScene(string scenePath, System.Action onComplete = null)
        {
            if (_isTransitioning) return;
            StartCoroutine(TransitionRoutine(scenePath, onComplete));
        }

        private IEnumerator TransitionRoutine(string scenePath, System.Action onComplete)
        {
            _isTransitioning = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            // 1. Fade to solid background (0.25s)
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // 2. Load the scene asynchronously
            AsyncOperation op = SceneManager.LoadSceneAsync(scenePath);
            if (op != null)
            {
                while (!op.isDone)
                {
                    yield return null;
                }
            }

            // Small delay to ensure UI Toolkit / Canvas rendering starts completely
            yield return new WaitForSeconds(0.15f);

            // 3. Fade out the overlay cleanly (0.25s)
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _isTransitioning = false;

            onComplete?.Invoke();
        }
    }
}