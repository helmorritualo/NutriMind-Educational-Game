using UnityEngine;
using UnityEngine.SceneManagement;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Static bridge/utility for key-based scene transitions.
    /// Resolves stable keys using the pure NavigationService from CompositionRoot
    /// and performs asynchronous scene loading via UnityEngine.SceneManagement.
    /// </summary>
    public static class AppNavigation
    {
        /// <summary>
        /// Asynchronously navigates to the scene registered under the given key.
        /// </summary>
        /// <param name="sceneKey">The stable key (e.g., "Login", "MainMenu") registered in the SceneRegistry.</param>
        public static void LoadScene(string sceneKey)
        {
            if (string.IsNullOrEmpty(sceneKey))
            {
                Debug.LogError("[AppNavigation] Navigate called with null or empty scene key.");
                return;
            }

            var root = CompositionRoot.Instance;
            if (root == null)
            {
                Debug.LogError("[AppNavigation] CompositionRoot.Instance is null. Cannot perform navigation.");
                return;
            }

            var navService = root.NavigationService;
            if (navService == null)
            {
                Debug.LogError("[AppNavigation] NavigationService is null inside CompositionRoot.");
                return;
            }

            var result = navService.Navigate(sceneKey);
            if (result.IsAvailable && !string.IsNullOrEmpty(result.ScenePath))
            {
                Debug.Log($"[AppNavigation] Transitioning from '{SceneManager.GetActiveScene().name}' to '{sceneKey}' ({result.ScenePath}).");
                SceneManager.LoadSceneAsync(result.ScenePath);
            }
            else
            {
                Debug.LogError($"[AppNavigation] Cannot navigate to '{sceneKey}'. Key unregistered or path empty.");
            }
        }
    }
}
