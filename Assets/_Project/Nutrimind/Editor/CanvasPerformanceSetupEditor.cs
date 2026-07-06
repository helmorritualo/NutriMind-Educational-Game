using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NutriMind.Editor
{
    public static class CanvasPerformanceSetupEditor
    {
        private const string QuizAssetsPath = "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets";
        private const string MainMenuArtPath = "Assets/_Project/Nutrimind/Art/Provided/mainmenu";
        private const string LoginArtPath = "Assets/_Project/Nutrimind/Art/Provided/login";
        private const string SkydenBaseColorPath =
            "Assets/_Project/Nutrimind/ThirdParty/Skyden_Games/Low Poly Environment/Textures/Colors BaseColor.png";

        [MenuItem("NutriMind/Performance/Disable Decorative Raycasts In Active Scene")]
        public static void DisableDecorativeRaycastsInActiveScene()
        {
            int changed = 0;
            foreach (var graphic in Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (graphic == null || !graphic.raycastTarget) continue;
                if (IsInteractiveGraphic(graphic.gameObject)) continue;
                graphic.raycastTarget = false;
                changed++;
                EditorUtility.SetDirty(graphic);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[CanvasPerformanceSetup] Disabled raycastTarget on {changed} decorative graphics.");
        }

        [MenuItem("NutriMind/Performance/Apply Android UI Texture Overrides")]
        public static void ApplyAndroidTextureOverrides()
        {
            int changed = 0;
            changed += SetAndroidMaxSize(QuizAssetsPath, 1024);
            changed += SetAndroidMaxSize(MainMenuArtPath, 1024);
            changed += SetAndroidMaxSize(LoginArtPath, 1024);
            if (SetAndroidMaxSizeForAsset(SkydenBaseColorPath, 1024)) changed++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CanvasPerformanceSetup] Updated Android maxTextureSize on {changed} texture importers.");
        }

        private static bool IsInteractiveGraphic(GameObject go)
        {
            if (go.GetComponent<Button>() != null) return true;
            if (go.GetComponent<Toggle>() != null) return true;
            if (go.GetComponent<Slider>() != null) return true;
            if (go.GetComponent<Scrollbar>() != null) return true;
            if (go.GetComponent<ScrollRect>() != null) return true;
            if (go.GetComponent<InputField>() != null) return true;
            if (go.GetComponent<TMP_InputField>() != null) return true;
            if (go.GetComponent<Dropdown>() != null) return true;
            return false;
        }

        private static int SetAndroidMaxSize(string folder, int maxSize)
        {
            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (SetAndroidMaxSizeForAsset(path, maxSize)) count++;
            }
            return count;
        }

        private static bool SetAndroidMaxSizeForAsset(string path, int maxSize)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            var android = importer.GetPlatformTextureSettings("Android");
            if (android.maxTextureSize == maxSize && android.overridden) return false;

            android.overridden = true;
            android.maxTextureSize = maxSize;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
            return true;
        }
    }
}
