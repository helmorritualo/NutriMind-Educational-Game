using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NutriMind.Editor
{
    /// <summary>
    /// One-shot setup for LiteraQuest Mission 1 Canvas UI (1920x1080).
    /// </summary>
    public static class LiteraQuestMission1CanvasSetupEditor
    {
        private const string ScenePath =
            "Assets/_Project/Nutrimind/Scenes/App/Literaquest Term/LiteraQuest_Term1_Mission1.unity";

        private const string QuizInstructionsSheet =
            "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/QuizInstructions.png";
        private const string QuizStatesSheet =
            "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/QuizStates.png";
        private const string MultipleChoiceSheet =
            "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/MultipleChoice.png";
        private const string QuizResultsSheet =
            "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/QuizResults.png";

        private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private static readonly Color Navy = new Color(0.122f, 0.161f, 0.282f, 1f);
        private static readonly Color Body = new Color(0.180f, 0.227f, 0.333f, 1f);
        private static readonly Color Gold = new Color(0.722f, 0.525f, 0.043f, 1f);
        private static readonly Color Muted = new Color(0.353f, 0.392f, 0.471f, 1f);

        private static readonly Vector2 TopLeftPivot = new Vector2(0f, 1f);
        private static readonly Vector2 TopCenterPivot = new Vector2(0.5f, 1f);
        private static readonly Vector2 CenterPivot = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 BottomRightPivot = new Vector2(1f, 0f);
        private static readonly Vector2 BottomCenterPivot = new Vector2(0.5f, 0f);

        [MenuItem("NutriMind/LiteraQuest Mission 1/Setup Mission Canvas UI")]
        public static void SetupMissionCanvasUi()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var existingCanvas = GameObject.Find("Canvas");
            if (existingCanvas != null)
            {
                Object.DestroyImmediate(existingCanvas);
            }

            var existingEventSystem = GameObject.Find("EventSystem");
            if (existingEventSystem != null)
            {
                Object.DestroyImmediate(existingEventSystem);
            }

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            if (font == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] LiberationSans SDF font not found.");
                return;
            }

            var canvasGo = CreateCanvasRoot();
            EnsureEventSystem(canvasGo.transform);

            BuildHudPanel(canvasGo.transform, font);
            BuildDialoguePanel(canvasGo.transform, font);
            BuildQuestionPanel(canvasGo.transform, font);
            BuildFeedbackPanel(canvasGo.transform, font);
            BuildAreaCompletePanel(canvasGo.transform, font);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[LiteraQuestMission1CanvasSetup] Mission Canvas UI created and scene saved.");
        }

        [MenuItem("NutriMind/LiteraQuest Mission 1/Preview Mission UI Panels")]
        public static void PreviewMissionUiPanels()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            string[] panels = { "HUDPanel", "DialoguePanel", "QuestionPanel", "FeedbackPanel", "AreaCompletePanel" };
            foreach (var panelName in panels)
            {
                var panel = GameObject.Find("Canvas/" + panelName);
                if (panel != null)
                {
                    panel.SetActive(panelName == "QuestionPanel");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = GameObject.Find("Canvas/QuestionPanel");
            Debug.Log("[LiteraQuestMission1CanvasSetup] Previewing QuestionPanel in Scene/Game view.");
        }

        private static GameObject CreateCanvasRoot()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            StretchFull(go.GetComponent<RectTransform>());
            return go;
        }

        private static void EnsureEventSystem(Transform canvas)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            go.transform.SetParent(canvas, false);
            go.layer = LayerMask.NameToLayer("UI");
        }

        private static void BuildHudPanel(Transform canvas, TMP_FontAsset font)
        {
            var hud = CreateUiObject(canvas, "HUDPanel", typeof(Image));
            StretchFull(hud.GetComponent<RectTransform>());
            var hudImage = hud.GetComponent<Image>();
            hudImage.color = new Color(1f, 1f, 1f, 0f);
            hudImage.raycastTarget = false;

            var missionTitle = CreateTmp(hud.transform, "MissionTitleText", font, 32, FontStyles.Bold, Navy,
                TextAlignmentOptions.Left, "Festival Storybook Rescue",
                new Vector2(0f, 1f), new Vector2(0f, 1f), TopLeftPivot,
                new Vector2(56f, -28f), new Vector2(860f, 44f), wrap: false, autoSizeMax: 36f);
            missionTitle.GetComponent<TextMeshProUGUI>().outlineWidth = 0.2f;
            missionTitle.GetComponent<TextMeshProUGUI>().outlineColor = new Color32(255, 245, 220, 255);

            CreateTmp(hud.transform, "ObjectiveText", font, 22, FontStyles.Normal, Body,
                TextAlignmentOptions.Left, "Talk to Farmer Lira.",
                new Vector2(0f, 1f), new Vector2(0f, 1f), TopLeftPivot,
                new Vector2(56f, -78f), new Vector2(980f, 44f), wrap: true);

            var objective = hud.transform.Find("ObjectiveText")?.GetComponent<TextMeshProUGUI>();
            if (objective != null)
            {
                objective.outlineWidth = 0.15f;
                objective.outlineColor = new Color32(255, 245, 220, 220);
            }

            CreateTmp(hud.transform, "ItemCounterText", font, 24, FontStyles.Bold, Navy,
                TextAlignmentOptions.Right, "Story Map Fragment: 0/1",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-56f, -42f), new Vector2(360f, 40f), wrap: false);

            var interact = CreateUiObject(hud.transform, "InteractPrompt", typeof(Image));
            ApplySprite(interact.GetComponent<Image>(), LoadSprite(QuizStatesSheet, "QuizStates_2"));
            interact.GetComponent<Image>().raycastTarget = true;
            SetRect(interact, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), BottomCenterPivot,
                new Vector2(0f, 84f), new Vector2(360f, 76f));

            var interactLabelGo = CreateTmp(interact.transform, "Text", font, 22, FontStyles.Bold, Color.white,
                TextAlignmentOptions.Center, "Press to Interact",
                Vector2.zero, Vector2.one, CenterPivot, Vector2.zero, Vector2.zero, wrap: false);
            StretchFull(interactLabelGo.GetComponent<RectTransform>());
            interact.SetActive(false);
        }

        private static void BuildDialoguePanel(Transform canvas, TMP_FontAsset font)
        {
            const float w = 1180f;
            const float h = 340f;
            var root = CreateCenteredPanel(canvas, "DialoguePanel", w, h,
                LoadSprite(QuizInstructionsSheet, "QuizInstructions_1"),
                new Vector2(0.5f, 0f), new Vector2(0f, 96f));

            CreateTmp(root.transform, "NPCNameText", font, 28, FontStyles.Bold, Gold,
                TextAlignmentOptions.Left, "Farmer Lira",
                new Vector2(0f, 1f), new Vector2(0f, 1f), TopLeftPivot,
                new Vector2(72f, -36f), new Vector2(w - 144f, 40f), wrap: false);

            CreateTmp(root.transform, "DialogueText", font, 24, FontStyles.Normal, Body,
                TextAlignmentOptions.TopLeft,
                "The Festival Storybook was damaged by the wind. Help us restore the missing story pieces before the celebration begins.",
                new Vector2(0f, 1f), new Vector2(0f, 1f), TopLeftPivot,
                new Vector2(72f, -88f), new Vector2(w - 144f, 150f), wrap: true, extraPadding: true);

            CreateContinueButton(root.transform, "ContinueButton",
                new Vector2(1f, 0f), new Vector2(1f, 0f), BottomRightPivot,
                new Vector2(-72f, 36f), new Vector2(260f, 68f));

            root.SetActive(false);
        }

        private static void BuildQuestionPanel(Transform canvas, TMP_FontAsset font)
        {
            const float w = 1180f;
            const float h = 884f;
            var root = CreateCenteredPanel(canvas, "QuestionPanel", w, h,
                LoadSprite(MultipleChoiceSheet, "MultipleChoice_0"), CenterPivot, Vector2.zero);

            CreateTmp(root.transform, "QuestionProgressText", font, 20, FontStyles.Bold, Muted,
                TextAlignmentOptions.Center, "Question 1 of 3",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), TopCenterPivot,
                new Vector2(0f, -42f), new Vector2(420f, 36f), wrap: false);

            CreateTmp(root.transform, "QuestionText", font, 26, FontStyles.Normal, Navy,
                TextAlignmentOptions.TopLeft,
                "Mika joined the town parade and carried a flag. Who is the main character?",
                new Vector2(0f, 1f), new Vector2(0f, 1f), TopLeftPivot,
                new Vector2(96f, -118f), new Vector2(w - 192f, 150f), wrap: true, extraPadding: true);

            var answerSprite = LoadSprite(MultipleChoiceSheet, "MultipleChoice_5");
            if (answerSprite == null)
            {
                answerSprite = LoadSprite(MultipleChoiceSheet, "MultipleChoice_4");
            }
            string[] answerNames = { "AnswerButtonA", "AnswerButtonB", "AnswerButtonC", "AnswerButtonD" };
            string[] answerPlaceholders =
            {
                "A. The rain",
                "B. Mika",
                "C. The festival field",
                "D. The decorations"
            };
            float[] answerTop = { 322f, 436f, 550f, 648f };

            for (int i = 0; i < answerNames.Length; i++)
            {
                CreateAnswerButton(root.transform, answerNames[i], answerSprite, font, answerPlaceholders[i],
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), TopCenterPivot,
                    new Vector2(0f, -answerTop[i]), new Vector2(880f, 88f));
            }

            root.transform.Find("QuestionText")?.SetAsFirstSibling();
            root.transform.Find("QuestionProgressText")?.SetAsLastSibling();

            root.SetActive(false);
        }

        private static void BuildFeedbackPanel(Transform canvas, TMP_FontAsset font)
        {
            const float w = 720f;
            const float h = 620f;
            var root = CreateCenteredPanel(canvas, "FeedbackPanel", w, h,
                LoadSprite(QuizStatesSheet, "QuizStates_0"), CenterPivot, Vector2.zero);

            CreateTmp(root.transform, "FeedbackText", font, 24, FontStyles.Normal, Body,
                TextAlignmentOptions.Center,
                "Look for the person who does the main action in the sentence.",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), CenterPivot,
                new Vector2(0f, 36f), new Vector2(w - 96f, 260f), wrap: true, extraPadding: true);

            CreateContinueButton(root.transform, "ContinueButton",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), BottomCenterPivot,
                new Vector2(0f, 52f), new Vector2(260f, 68f));

            root.SetActive(false);
        }

        private static void BuildAreaCompletePanel(Transform canvas, TMP_FontAsset font)
        {
            const float w = 820f;
            const float h = 560f;
            var root = CreateCenteredPanel(canvas, "AreaCompletePanel", w, h,
                LoadSprite(QuizResultsSheet, "QuizResults_0"), CenterPivot, Vector2.zero);

            CreateTmp(root.transform, "CompleteTitleText", font, 36, FontStyles.Bold, Navy,
                TextAlignmentOptions.Center, "Parade Meadow Complete",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), TopCenterPivot,
                new Vector2(0f, -72f), new Vector2(w - 80f, 52f), wrap: false, autoSizeMax: 40f);

            CreateTmp(root.transform, "CompleteMessageText", font, 22, FontStyles.Normal, Body,
                TextAlignmentOptions.Center,
                "Story Map Fragment collected. Sign Repair Barn is locked for later.",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), TopCenterPivot,
                new Vector2(0f, -148f), new Vector2(w - 96f, 180f), wrap: true, extraPadding: true);

            CreateContinueButton(root.transform, "ContinueButton",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), BottomCenterPivot,
                new Vector2(0f, 52f), new Vector2(260f, 68f));

            root.SetActive(false);
        }

        private static GameObject CreateCenteredPanel(Transform canvas, string name, float width, float height,
            Sprite frameSprite, Vector2 anchor, Vector2 anchoredPosition)
        {
            var panel = CreateUiObject(canvas, name, typeof(Image));
            ApplySprite(panel.GetComponent<Image>(), frameSprite);
            panel.GetComponent<Image>().raycastTarget = true;
            SetRect(panel, anchor, anchor, CenterPivot, anchoredPosition, new Vector2(width, height));
            panel.SetActive(false);
            return panel;
        }

        private static void CreateContinueButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = CreateUiObject(parent, name, typeof(Image), typeof(Button));
            ApplySprite(go.GetComponent<Image>(), LoadSprite(QuizStatesSheet, "QuizStates_2"));
            go.GetComponent<Button>().targetGraphic = go.GetComponent<Image>();
            SetRect(go, anchorMin, anchorMax, pivot, pos, size);
        }

        private static void CreateAnswerButton(Transform parent, string name, Sprite sprite, TMP_FontAsset font,
            string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = CreateUiObject(parent, name, typeof(Image), typeof(Button));
            var image = go.GetComponent<Image>();
            if (sprite != null)
            {
                ApplySprite(image, sprite);
            }
            else
            {
                image.color = new Color(1f, 1f, 1f, 0.01f);
            }

            go.GetComponent<Button>().targetGraphic = image;
            SetRect(go, anchorMin, anchorMax, pivot, pos, size);

            var textGo = CreateUiObject(go.transform, "Text", typeof(TextMeshProUGUI));
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = label;
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Normal;
            tmp.color = Navy;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            tmp.margin = new Vector4(72f, 0f, 24f, 0f);
            StretchFull(textGo.GetComponent<RectTransform>());
            textGo.GetComponent<RectTransform>().offsetMin = new Vector2(72f, 4f);
            textGo.GetComponent<RectTransform>().offsetMax = new Vector2(-16f, -4f);
        }

        private static GameObject CreateUiObject(Transform parent, string name, params System.Type[] components)
        {
            var go = new GameObject(name, components);
            if (!go.TryGetComponent<RectTransform>(out _))
            {
                go.AddComponent<RectTransform>();
            }

            go.transform.SetParent(parent, false);
            go.layer = LayerMask.NameToLayer("UI");
            return go;
        }

        private static GameObject CreateTmp(Transform parent, string name, TMP_FontAsset font, float fontSize,
            FontStyles style, Color color, TextAlignmentOptions alignment, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size,
            bool wrap, float autoSizeMax = 0f, bool extraPadding = false)
        {
            var go = CreateUiObject(parent, name, typeof(TextMeshProUGUI));
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = wrap;
            tmp.overflowMode = wrap ? TextOverflowModes.Overflow : TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            if (extraPadding)
            {
                tmp.margin = new Vector4(6f, 6f, 6f, 6f);
            }

            if (autoSizeMax > 0f)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = fontSize;
                tmp.fontSizeMax = autoSizeMax;
            }

            SetRect(go, anchorMin, anchorMax, pivot, pos, size);
            return go;
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.color = Color.white;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 pos, Vector2 size)
        {
            SetRect(go.GetComponent<RectTransform>(), anchorMin, anchorMax, pivot, pos, size);
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 pos, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static Sprite LoadSprite(string sheetPath, string spriteName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == spriteName)
                {
                    return sprite;
                }
            }

            return null;
        }
    }
}
