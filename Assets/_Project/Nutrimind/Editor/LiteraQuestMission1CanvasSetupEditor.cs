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
                UnityEngine.Object.DestroyImmediate(existingCanvas);
            }

            var existingEventSystem = GameObject.Find("EventSystem");
            if (existingEventSystem != null)
            {
                UnityEngine.Object.DestroyImmediate(existingEventSystem);
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

        [MenuItem("NutriMind/LiteraQuest Mission 1/Wire G5 Area01 Mission Controller")]
        public static void WireG5Area01MissionController()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Transform missionArea = FindRootTransform("MissionArea01");
            if (missionArea == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] MissionArea01 not found.");
                return;
            }

            Transform gateRoot = FindDescendantTransform(missionArea, "GateToNextArea");
            if (gateRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] GateToNextArea not found.");
                return;
            }

            EnsureGateBlocker(gateRoot);

            Transform systemsRoot = FindRootTransform("Systems");
            if (systemsRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] Systems not found.");
                return;
            }

            Transform controllerRoot = FindDescendantTransform(systemsRoot, "G5Area01MissionController");
            if (controllerRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] G5Area01MissionController not found under Systems.");
                return;
            }

            // ponytail: G5Area01MissionController lives in Assembly-CSharp; NutriMind.Editor asmdef cannot reference it directly.
            Component controller = GetOrAddMissionController(controllerRoot.gameObject);
            if (controller == null)
            {
                return;
            }

            Transform canvas = FindRootTransform("Canvas");
            if (canvas == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] Canvas not found.");
                return;
            }

            var serialized = new SerializedObject(controller);
            SetObjectRef(serialized, "_storyMapFragment", FindDescendantTransform(missionArea, "StoryMapFragment"));
            SetObjectRef(serialized, "_closedGate", FindDescendantTransform(gateRoot, "ClosedGate"));
            SetObjectRef(serialized, "_openGate", FindDescendantTransform(gateRoot, "OpenGate"));
            SetColliderRef(serialized, "_gateBlocker", FindDescendantTransform(gateRoot, "GateBlocker"));

            Transform hud = FindDescendantTransform(canvas, "HUDPanel");
            Transform interactPrompt = FindDescendantTransform(hud, "InteractPrompt");
            SetTmpRef(serialized, "_missionTitleText", FindDescendantComponent<TextMeshProUGUI>(hud, "MissionTitleText"));
            SetTmpRef(serialized, "_gradeSubjectTermText", null);
            SetTmpRef(serialized, "_objectiveText", FindDescendantComponent<TextMeshProUGUI>(hud, "ObjectiveText"));
            SetTmpRef(serialized, "_itemCounterText", FindDescendantComponent<TextMeshProUGUI>(hud, "ItemCounterText"));
            SetObjectRef(serialized, "_interactPrompt", interactPrompt);
            SetButtonRef(serialized, "_interactButton", FindDescendantComponent<Button>(interactPrompt, "InteractButton"));
            SetTmpRef(serialized, "_interactPromptLabel", FindDescendantComponent<TextMeshProUGUI>(interactPrompt, "InteractText"));

            Transform playerRoot = FindRootTransform("Player");
            if (playerRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] Player not found.");
                return;
            }

            EnsurePlayerInteractionTrigger(playerRoot);
            EnsureFarmerLiraInteractionTarget(missionArea, controller);
            EnsureStoryMapFragmentPickup(missionArea, controller);

            Transform dialoguePanel = FindDescendantTransform(canvas, "DialoguePanel");
            SetObjectRef(serialized, "_dialoguePanel", dialoguePanel);
            SetTmpRef(serialized, "_npcNameText", FindDescendantComponent<TextMeshProUGUI>(dialoguePanel, "NPCNameText"));
            SetTmpRef(serialized, "_dialogueText", FindDescendantComponent<TextMeshProUGUI>(dialoguePanel, "DialogueText"));
            SetButtonRef(serialized, "_dialogueContinueButton", FindDescendantComponent<Button>(dialoguePanel, "ContinueButton"));

            Transform questionPanel = FindDescendantTransform(canvas, "QuestionPanel");
            SetObjectRef(serialized, "_questionPanel", questionPanel);
            SetTmpRef(serialized, "_questionProgressText", FindDescendantComponent<TextMeshProUGUI>(questionPanel, "QuestionProgressText"));
            SetTmpRef(serialized, "_questionText", FindDescendantComponent<TextMeshProUGUI>(questionPanel, "QuestionText"));
            SetButtonArrayRefs(serialized, "_answerButtons", questionPanel,
                "AnswerButtonA", "AnswerButtonB", "AnswerButtonC", "AnswerButtonD");
            SetTmpArrayRefs(serialized, "_answerLabels", questionPanel,
                "AnswerButtonA", "AnswerButtonB", "AnswerButtonC", "AnswerButtonD");

            Transform feedbackPanel = FindDescendantTransform(canvas, "FeedbackPanel");
            SetObjectRef(serialized, "_feedbackPanel", feedbackPanel);
            SetTmpRef(serialized, "_feedbackText", FindDescendantComponent<TextMeshProUGUI>(feedbackPanel, "FeedbackText"));
            SetButtonRef(serialized, "_feedbackContinueButton", FindDescendantComponent<Button>(feedbackPanel, "ContinueButton"));

            Transform areaCompletePanel = FindDescendantTransform(canvas, "AreaCompletePanel");
            SetObjectRef(serialized, "_areaCompletePanel", areaCompletePanel);
            SetTmpRef(serialized, "_areaCompleteTitleText", FindDescendantComponent<TextMeshProUGUI>(areaCompletePanel, "CompleteTitleText"));
            SetTmpRef(serialized, "_areaCompleteMessageText", FindDescendantComponent<TextMeshProUGUI>(areaCompletePanel, "CompleteMessageText"));
            SetButtonRef(serialized, "_areaCompleteContinueButton", FindDescendantComponent<Button>(areaCompletePanel, "ContinueButton"));

            SetStringProperty(serialized, "_pickupObjective", "Pick up the Story Map Fragment.");
            SetStringProperty(serialized, "_areaCompleteObjective", "Area 1 complete.");
            SetStringProperty(serialized, "_collectedItemCounter", "1/1");
            SetStringProperty(serialized, "_areaCompleteTitle", "Area Complete!");
            SetStringProperty(serialized, "_areaCompleteBody",
                "You restored the first story clue. The path to the next area is now open.");

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[LiteraQuestMission1CanvasSetup] G5Area01MissionController wired and scene saved. " +
                      "Farmer Lira interaction + PlayerInteractionTrigger ready. " +
                      "_gradeSubjectTermText left unassigned (no HUD TMP in scene yet).");
        }

        private static void EnsurePlayerInteractionTrigger(Transform playerRoot)
        {
            Transform markerRoot = FindDescendantTransform(playerRoot, "PlayerInteractionTrigger");
            if (markerRoot == null)
            {
                var marker = new GameObject("PlayerInteractionTrigger");
                marker.transform.SetParent(playerRoot, false);
                marker.transform.localPosition = Vector3.zero;
                marker.transform.localRotation = Quaternion.identity;
                marker.transform.localScale = Vector3.one;
                markerRoot = marker.transform;
            }

            GetOrAddAssemblyComponent(markerRoot.gameObject, "PlayerInteractionTrigger");
        }

        private static void EnsureFarmerLiraInteractionTarget(Transform missionArea, Component missionController)
        {
            Transform farmerRoot = FindDescendantTransform(missionArea, "FarmerLira_NPC");
            if (farmerRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] FarmerLira_NPC not found under MissionArea01.");
                return;
            }

            Transform markerRoot = FindDescendantTransform(farmerRoot, "NPC_InteractionMarker");
            if (markerRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] NPC_InteractionMarker not found under FarmerLira_NPC.");
                return;
            }

            Transform npcModel = FindDescendantTransform(farmerRoot, "NPC_Model");
            if (npcModel != null)
            {
                markerRoot.localPosition = npcModel.localPosition;
            }

            SphereCollider sphere = markerRoot.GetComponent<SphereCollider>();
            if (sphere == null)
            {
                sphere = markerRoot.gameObject.AddComponent<SphereCollider>();
            }

            sphere.isTrigger = true;
            sphere.radius = 2f;
            sphere.center = Vector3.zero;

            Component target = GetOrAddAssemblyComponent(markerRoot.gameObject, "G5Area01InteractionTarget");
            if (target == null)
            {
                return;
            }

            var serialized = new SerializedObject(target);
            serialized.FindProperty("_mission").objectReferenceValue = missionController;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureStoryMapFragmentPickup(Transform missionArea, Component missionController)
        {
            Transform fragmentRoot = FindDescendantTransform(missionArea, "StoryMapFragment");
            if (fragmentRoot == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] StoryMapFragment not found under MissionArea01.");
                return;
            }

            SphereCollider pickupTrigger = fragmentRoot.GetComponent<SphereCollider>();
            if (pickupTrigger == null)
            {
                pickupTrigger = fragmentRoot.gameObject.AddComponent<SphereCollider>();
            }

            pickupTrigger.isTrigger = true;
            pickupTrigger.radius = 2f;
            pickupTrigger.center = Vector3.zero;

            Component target = GetOrAddAssemblyComponent(fragmentRoot.gameObject, "G5Area01InteractionTarget");
            if (target == null)
            {
                return;
            }

            var serialized = new SerializedObject(target);
            serialized.FindProperty("_mission").objectReferenceValue = missionController;
            serialized.FindProperty("_kind").enumValueIndex = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Component GetOrAddAssemblyComponent(GameObject host, string typeName)
        {
            System.Type componentType = System.Type.GetType(typeName + ", Assembly-CSharp");
            if (componentType == null)
            {
                Debug.LogError($"[LiteraQuestMission1CanvasSetup] {typeName} type not found. Wait for script compile and retry.");
                return null;
            }

            Component existing = host.GetComponent(componentType);
            if (existing == null)
            {
                existing = host.AddComponent(componentType);
            }

            return existing;
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

        private static Component GetOrAddMissionController(GameObject host)
        {
            System.Type controllerType = System.Type.GetType("G5Area01MissionController, Assembly-CSharp");
            if (controllerType == null)
            {
                Debug.LogError("[LiteraQuestMission1CanvasSetup] G5Area01MissionController type not found. Wait for script compile and retry.");
                return null;
            }

            Component existing = host.GetComponent(controllerType);
            if (existing == null)
            {
                existing = host.AddComponent(controllerType);
            }

            return existing;
        }

        private static Transform FindRootTransform(string objectName)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name.Trim() == objectName)
                {
                    return root.transform;
                }
            }

            return null;
        }

        private static Transform FindDescendantTransform(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name.Trim() == objectName)
                {
                    return transform;
                }
            }

            return null;
        }

        private static T FindDescendantComponent<T>(Transform root, string objectName) where T : Component
        {
            var target = FindDescendantTransform(root, objectName);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static void EnsureGateBlocker(Transform gateRoot)
        {
            var existing = FindDescendantTransform(gateRoot, "GateBlocker");
            if (existing != null)
            {
                return;
            }

            var blocker = new GameObject("GateBlocker");
            blocker.transform.SetParent(gateRoot, false);
            blocker.transform.localPosition = new Vector3(-1131.5577f, 7f, 1349.6f);
            blocker.transform.localRotation = Quaternion.identity;
            blocker.transform.localScale = Vector3.one;

            var box = blocker.AddComponent<BoxCollider>();
            box.isTrigger = false;
            box.center = Vector3.zero;
            box.size = new Vector3(8f, 6f, 2f);
        }

        private static void SetObjectRef(SerializedObject serialized, string propertyName, Transform target)
        {
            serialized.FindProperty(propertyName).objectReferenceValue = target != null ? target.gameObject : null;
        }

        private static void SetColliderRef(SerializedObject serialized, string propertyName, Transform target)
        {
            serialized.FindProperty(propertyName).objectReferenceValue =
                target != null ? target.GetComponent<Collider>() : null;
        }

        private static void SetTmpRef(SerializedObject serialized, string propertyName, TextMeshProUGUI target)
        {
            serialized.FindProperty(propertyName).objectReferenceValue = target;
        }

        private static void SetButtonRef(SerializedObject serialized, string propertyName, Button target)
        {
            serialized.FindProperty(propertyName).objectReferenceValue = target;
        }

        private static void SetStringProperty(SerializedObject serialized, string propertyName, string value)
        {
            serialized.FindProperty(propertyName).stringValue = value;
        }

        private static void SetButtonArrayRefs(SerializedObject serialized, string propertyName, Transform root,
            params string[] buttonNames)
        {
            var array = serialized.FindProperty(propertyName);
            array.arraySize = buttonNames.Length;
            for (int i = 0; i < buttonNames.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue =
                    FindDescendantComponent<Button>(root, buttonNames[i]);
            }
        }

        private static void SetTmpArrayRefs(SerializedObject serialized, string propertyName, Transform root,
            params string[] buttonNames)
        {
            var array = serialized.FindProperty(propertyName);
            array.arraySize = buttonNames.Length;
            for (int i = 0; i < buttonNames.Length; i++)
            {
                var button = FindDescendantTransform(root, buttonNames[i]);
                array.GetArrayElementAtIndex(i).objectReferenceValue =
                    button != null ? button.Find("Text")?.GetComponent<TextMeshProUGUI>() : null;
            }
        }
    }
}
