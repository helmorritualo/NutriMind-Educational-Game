using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NutriMind.Runtime.App;

namespace NutriMind.Editor
{
    /// <summary>
    /// One-shot setup for QuizPortalScene layout, Canvas config, and controller wiring.
    /// </summary>
    public static class QuizPortalSceneSetupEditor
    {
        private const string ScenePath = "Assets/_Project/Nutrimind/Scenes/App/QuizPortalScene.unity";
        private const string AvailableQuizzesSheet = "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/AvailableQuizzes.png";
        private const string QuizStatesSheet = "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/QuizStates.png";
        private const string AvailableQuizListDesignRef = "docs/design-reference-quiz-system/AvailableQuizList.png";
        private const string BackButtonSpritePath = "Assets/_Project/Nutrimind/Art/Sprite/QuizAssets/QuizPortal/main-menu-button.png";

        // ponytail: widths tuned against docs/design-reference-quiz-system/AvailableQuizList.png
        private static readonly float[] QuizListColumnWidths = { 52f, 250f, 115f, 88f, 80f, 58f, 118f, 108f };
        private const float QuizListRowPaddingLeft = 8f;
        private const float QuizListRowColumnSpacing = 6f;
        private static readonly string[] QuizListHeaderNames =
        {
            "QuizTitleText",
            "SubjectTitleText",
            "TermTitleText",
            "GradeTitleText",
            "ItemsTitleText",
            "StatusTitleText",
            "ActionsTitleText"
        };
        private static readonly string[] QuizListHeaderSprites =
        {
            "AvailableQuizzes_18",
            "AvailableQuizzes_19",
            "AvailableQuizzes_20",
            "AvailableQuizzes_21",
            "AvailableQuizzes_22",
            "AvailableQuizzes_23",
            "AvailableQuizzes_24"
        };

        [MenuItem("NutriMind/Quiz Portal/Unlock Quiz List Row For Manual Edit")]
        public static void UnlockQuizListRowForManualEditMenu()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var row = GameObject.Find("QuizPortalCanvas/AvailableQuizListPanel/QuizListScrollArea/Viewport/Content/QuizListRow");
            if (row == null)
            {
                Debug.LogWarning("[QuizPortalSceneSetup] QuizListRow not found. Run Add Quiz List Row Template To Scene first.");
                return;
            }

            UnlockQuizListRowForManualEdit(row.transform);
            EditorUtility.SetDirty(row);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[QuizPortalSceneSetup] QuizListRow layout groups disabled — you can drag children freely.");
        }

        [MenuItem("NutriMind/Quiz Portal/Add Quiz List Row Template To Scene")]
        public static void AddQuizListRowTemplateToScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var listPanel = FindSceneObject(scene, "AvailableQuizListPanel");
            if (listPanel == null)
            {
                Debug.LogError("[QuizPortalSceneSetup] AvailableQuizListPanel not found.");
                return;
            }

            bool wasActive = listPanel.activeSelf;
            if (!wasActive) listPanel.SetActive(true);

            EnsureQuizListRowSceneTemplate(listPanel.transform);

            if (!wasActive) listPanel.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[QuizPortalSceneSetup] QuizListRow template added under QuizListScrollArea/Viewport/Content.");
        }

        [MenuItem("NutriMind/Quiz Portal/Setup Quiz Portal Scene")]
        public static void SetupQuizPortalScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var canvasGo = GameObject.Find("QuizPortalCanvas");
            if (canvasGo == null)
            {
                Debug.LogError("[QuizPortalSceneSetup] QuizPortalCanvas not found.");
                return;
            }

            var canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.localScale = Vector3.one;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            var canvasGroup = canvasGo.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvasGo.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            var raycaster = canvasGo.GetComponent<GraphicRaycaster>();

            var background = GameObject.Find("BackgroundImage")?.GetComponent<Image>();
            if (background != null)
            {
                var bgRect = background.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                bgRect.localScale = Vector3.one;
                background.raycastTarget = false;
            }

            var homePanel = GameObject.Find("QuizPortalHomePanel");
            if (homePanel != null)
            {
                var homeRect = homePanel.GetComponent<RectTransform>();
                homeRect.anchorMin = new Vector2(0.15f, 0.12f);
                homeRect.anchorMax = new Vector2(0.85f, 0.88f);
                homeRect.offsetMin = Vector2.zero;
                homeRect.offsetMax = Vector2.zero;
                homeRect.localScale = Vector3.one;

                NormalizeChildScales(homePanel.transform);
                LayoutHomePanelChildren(homePanel.transform);
            }

            var welcomeGo = GameObject.Find("WelcomText") ?? GameObject.Find("WelcomeText");
            if (welcomeGo != null)
            {
                welcomeGo.name = "WelcomeText";
            }

            var descriptionGo = GameObject.Find("DescriptionText");
            if (descriptionGo == null && homePanel != null)
            {
                descriptionGo = CreateDescriptionText(homePanel.transform);
            }

            var rootGo = GameObject.Find("QuizPortalRoot");
            if (rootGo == null)
            {
                rootGo = new GameObject("QuizPortalRoot");
            }

            var navigation = rootGo.GetComponent<QuizPortalNavigationController>();
            if (navigation == null)
            {
                navigation = rootGo.AddComponent<QuizPortalNavigationController>();
            }

            var homeController = rootGo.GetComponent<QuizPortalHomeController>();
            if (homeController == null)
            {
                homeController = rootGo.AddComponent<QuizPortalHomeController>();
            }

            var availableButton = GameObject.Find("AvailableQuzzesButton")?.GetComponent<Button>();
            var resultsButton = GameObject.Find("MyQuizResultsButton")?.GetComponent<Button>();
            var backButton = GameObject.Find("BackMainMenuButton")?.GetComponent<Button>();
            var welcomeText = GameObject.Find("WelcomeText")?.GetComponent<TextMeshProUGUI>();
            var descriptionText = GameObject.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();

            navigation.SetHomePanelRoot(homePanel);
            homeController.SetAvailableQuizzesButton(availableButton);
            homeController.SetMyQuizResultsButton(resultsButton);
            homeController.SetBackMainMenuButton(backButton);
            homeController.SetWelcomeText(welcomeText);
            homeController.SetDescriptionText(descriptionText);
            homeController.SetNavigation(navigation);
            homeController.SetMainCanvasGroup(canvasGroup);
            homeController.SetGraphicRaycaster(raycaster);
            homeController.SetBackgroundImage(background);

            var listPanel = FindSceneObject(scene, "AvailableQuizListPanel");
            if (listPanel != null)
            {
                SetupAvailableQuizListPanel(listPanel, navigation, homeController);
            }
            else
            {
                Debug.LogWarning("[QuizPortalSceneSetup] AvailableQuizListPanel not found.");
            }

            ApplySerializedProperties(navigation);
            ApplySerializedProperties(homeController);

            EditorUtility.SetDirty(rootGo);
            EditorUtility.SetDirty(canvasGo);
            if (homePanel != null) EditorUtility.SetDirty(homePanel);
            if (listPanel != null) EditorUtility.SetDirty(listPanel);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EnsureBuildSettingsEntry();

            Debug.Log("[QuizPortalSceneSetup] Quiz Portal scene setup complete.");
        }

        private static void SetupAvailableQuizListPanel(
            GameObject listPanel,
            QuizPortalNavigationController navigation,
            QuizPortalHomeController homeController)
        {
            bool wasActive = listPanel.activeSelf;
            if (!wasActive) listPanel.SetActive(true);

            var listRect = listPanel.GetComponent<RectTransform>();
            if (listRect != null) listRect.localScale = Vector3.one;

            navigation.SetAvailableQuizListRoot(listPanel);

            var listController = listPanel.GetComponent<AvailableQuizListController>();
            if (listController == null)
            {
                listController = listPanel.AddComponent<AvailableQuizListController>();
            }

            var scrollArea = EnsureScrollArea(listPanel.transform);
            var backToHome = EnsureBackToHomeButton(listPanel.transform);
            var loadError = EnsureLoadErrorLabel(listPanel.transform);
            SetUiLayer(scrollArea);
            SetUiLayer(backToHome.gameObject);
            SetUiLayer(loadError.gameObject);
            PlaceScrollAreaBelowHeader(listPanel.transform, scrollArea.transform);
            LayoutAvailableQuizListPanel(listPanel.transform);
            EnsureQuizListRowSceneTemplate(listPanel.transform);

            listController.SetPanelRoot(listPanel);
            listController.SetNavigation(navigation);
            listController.SetAllFilterButton(FindDescendantComponent<Button>(listPanel.transform, "AllFilterButton"));
            listController.SetLiteraQuestFilterButton(FindDescendantComponent<Button>(listPanel.transform, "LiteraQuestFilterButton"));
            listController.SetPeHealthFilterButton(FindDescendantComponent<Button>(listPanel.transform, "PE/HealthFilterButton"));
            listController.SetScienceFilterButton(FindDescendantComponent<Button>(listPanel.transform, "ScienceFilterButton"));
            listController.SetAllFilterImage(FindDescendantComponent<Image>(listPanel.transform, "AllFilterButton"));
            listController.SetLiteraQuestFilterImage(FindDescendantComponent<Image>(listPanel.transform, "LiteraQuestFilterButton"));
            listController.SetPeHealthFilterImage(FindDescendantComponent<Image>(listPanel.transform, "PE/HealthFilterButton"));
            listController.SetScienceFilterImage(FindDescendantComponent<Image>(listPanel.transform, "ScienceFilterButton"));
            listController.SetFilterSelectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_2"));
            listController.SetFilterUnselectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_6"));
            listController.SetLiteraQuestSelectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_3"));
            listController.SetPeHealthSelectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_4"));
            listController.SetScienceSelectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_5"));
            listController.SetLiteraQuestUnselectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_7"));
            listController.SetPeHealthUnselectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_8"));
            listController.SetScienceUnselectedSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_9"));
            listController.SetListScroll(scrollArea.GetComponent<ScrollRect>());
            listController.SetListContent(scrollArea.transform.Find("Viewport/Content") as RectTransform);
            listController.SetRowPrefab(null);
            listController.SetPaginationLeftButton(FindDescendantComponent<Button>(listPanel.transform, "PanginationLeftButton"));
            listController.SetPaginationRightButton(FindDescendantComponent<Button>(listPanel.transform, "PanginationRightButton"));
            listController.SetPageNumberButtons(new[]
            {
                FindDescendantComponent<Button>(listPanel.transform, "OneButton"),
                FindDescendantComponent<Button>(listPanel.transform, "TwoButton"),
                FindDescendantComponent<Button>(listPanel.transform, "ThreeButton"),
                FindDescendantComponent<Button>(listPanel.transform, "FourButton")
            });
            listController.SetPageSummaryText(FindDescendantComponent<TextMeshProUGUI>(listPanel.transform, "NumberOfQuizzesPerPaginationText"));
            listController.SetBackToHomeButton(backToHome);
            listController.SetLoadErrorLabel(loadError);

            var emptyPanel = FindSceneObject(listPanel.scene, "EmptyQuizStatePanel");
            if (emptyPanel != null)
            {
                emptyPanel.SetActive(false);
                listController.SetEmptyStateRoot(emptyPanel);
                listController.SetEmptyRefreshButton(FindDescendantComponent<Button>(emptyPanel.transform, "RefeshButton"));
                listController.SetEmptyBackButton(FindDescendantComponent<Button>(emptyPanel.transform, "BackButton"));
                SetUiLayer(emptyPanel);
            }

            EditorUtility.SetDirty(listController);

            ApplySerializedProperties(listController);

            // ponytail: home controller keeps navigation ref; list uses same navigation instance
            homeController.SetNavigation(navigation);

            listPanel.SetActive(false);
        }

        private static void LayoutAvailableQuizListPanel(Transform listPanel)
        {
            LayoutFilterButtons(listPanel.Find("FilterButtons"));

            // ponytail: once the table grid (TableHeaderText + QuizListRow) has been hand-aligned
            // to docs/design-reference-quiz-system/AvailableQuizList.png, re-running setup must not
            // reset header/scroll anchors and break the column alignment.
            var content = listPanel.Find("QuizListScrollArea/Viewport/Content");
            var existingRow = content != null ? content.Find("QuizListRow") : null;
            if (existingRow != null && existingRow.childCount > 0)
                return;

            LayoutTableHeaders(listPanel.Find("TableHeaderText"));
            LayoutScrollArea(listPanel.Find("QuizListScrollArea"));
        }

        private static void LayoutFilterButtons(Transform? filterRoot)
        {
            if (filterRoot == null) return;

            var rect = filterRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, 0.76f);
            rect.anchorMax = new Vector2(0.94f, 0.82f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            var layout = filterRoot.GetComponent<HorizontalLayoutGroup>() ?? filterRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            string[] buttonNames = { "AllFilterButton", "LiteraQuestFilterButton", "PE/HealthFilterButton", "ScienceFilterButton" };
            float[] buttonWidths = { 120f, 165f, 155f, 135f };
            string[] selectedSprites = { "AvailableQuizzes_2", "AvailableQuizzes_3", "AvailableQuizzes_4", "AvailableQuizzes_5" };
            string[] unselectedSprites = { "AvailableQuizzes_6", "AvailableQuizzes_7", "AvailableQuizzes_8", "AvailableQuizzes_9" };

            for (int i = 0; i < buttonNames.Length; i++)
            {
                var button = filterRoot.Find(buttonNames[i]);
                if (button == null) continue;

                button.localScale = Vector3.one;
                var buttonRect = button.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.sizeDelta = new Vector2(buttonWidths[i], 36f);

                var layoutElement = button.GetComponent<LayoutElement>() ?? button.gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = buttonWidths[i];
                layoutElement.preferredHeight = 36f;

                var image = button.GetComponent<Image>();
                if (image == null) continue;
                image.preserveAspect = true;
                image.raycastTarget = true;
                image.sprite = LoadSprite(AvailableQuizzesSheet, i == 0 ? selectedSprites[i] : unselectedSprites[i]);
            }
        }

        private static void LayoutTableHeaders(Transform? headerRoot)
        {
            if (headerRoot == null) return;

            var rect = headerRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, 0.68f);
            rect.anchorMax = new Vector2(0.94f, 0.74f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            var layout = headerRoot.GetComponent<HorizontalLayoutGroup>() ?? headerRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8 + (int)QuizListColumnWidths[0] + 6, 8, 0, 0);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < QuizListHeaderNames.Length; i++)
            {
                var header = headerRoot.Find(QuizListHeaderNames[i]);
                if (header == null) continue;

                header.localScale = Vector3.one;
                var headerRect = header.GetComponent<RectTransform>();
                headerRect.anchorMin = new Vector2(0.5f, 0.5f);
                headerRect.anchorMax = new Vector2(0.5f, 0.5f);
                headerRect.sizeDelta = new Vector2(QuizListColumnWidths[i], 40f);

                var layoutElement = header.GetComponent<LayoutElement>() ?? header.gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = QuizListHeaderNames[i] == "QuizTitleText"
                    ? QuizListColumnWidths[1]
                    : QuizListColumnWidths[i + 1];
                layoutElement.preferredHeight = 40f;

                var image = header.GetComponent<Image>();
                if (image == null) continue;
                image.sprite = LoadSprite(AvailableQuizzesSheet, QuizListHeaderSprites[i]);
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
        }

        private static void LayoutScrollArea(Transform? scrollArea)
        {
            if (scrollArea == null) return;

            var rect = scrollArea.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, 0.18f);
            rect.anchorMax = new Vector2(0.94f, 0.66f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            var content = scrollArea.Find("Viewport/Content") as RectTransform;
            if (content == null) return;

            var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null)
            {
                verticalLayout.padding = new RectOffset(8, 8, 4, 4);
                verticalLayout.spacing = 6f;
                verticalLayout.childAlignment = TextAnchor.UpperLeft;
                // ponytail: template rows are hand-aligned; don't drive child RectTransforms from Content
                verticalLayout.childControlWidth = false;
                verticalLayout.childControlHeight = false;
                verticalLayout.childForceExpandWidth = false;
                verticalLayout.childForceExpandHeight = false;
            }

            var layoutElement = content.GetComponent<LayoutElement>() ?? content.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 900f;
        }

        private static void ApplySerializedProperties(Component component)
        {
            var serialized = new SerializedObject(component);
            serialized.Update();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject? FindSceneObject(Scene scene, string objectName)
        {
            if (!scene.IsValid()) return null;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == objectName)
                        return transform.gameObject;
                }
            }

            return null;
        }

        private static T? FindDescendantComponent<T>(Transform root, string objectName) where T : Component
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name != objectName) continue;
                var component = transform.GetComponent<T>();
                if (component != null) return component;
            }

            return null;
        }

        private static T? FindChildComponent<T>(Transform root, string path) where T : Component
        {
            var target = root.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static void SetUiLayer(GameObject go)
        {
            if (go == null) return;
            foreach (var transform in go.GetComponentsInChildren<Transform>(true))
            {
                transform.gameObject.layer = 5;
            }
        }

        private static void PlaceScrollAreaBelowHeader(Transform listPanel, Transform scrollArea)
        {
            var header = listPanel.Find("TableHeaderText");
            if (header == null) return;
            int targetIndex = header.GetSiblingIndex() + 1;
            scrollArea.SetSiblingIndex(targetIndex);
        }

        private static GameObject EnsureScrollArea(Transform listPanel)
        {
            var existing = listPanel.Find("QuizListScrollArea");
            if (existing != null) return existing.gameObject;

            var scrollGo = new GameObject("QuizListScrollArea", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(listPanel, false);

            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.08f, 0.22f);
            scrollRect.anchorMax = new Vector2(0.92f, 0.62f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scrollRect.localScale = Vector3.one;

            var scrollImage = scrollGo.GetComponent<Image>();
            scrollImage.color = new Color(1f, 1f, 1f, 0.01f);
            scrollImage.raycastTarget = true;
            scrollGo.layer = 5;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportGo.GetComponent<Image>().raycastTarget = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4f;

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            return scrollGo;
        }

        private static Button EnsureBackToHomeButton(Transform listPanel)
        {
            var existing = listPanel.Find("BackToQuizPortalHomeButton");
            if (existing != null) return existing.GetComponent<Button>();

            var go = new GameObject("BackToQuizPortalHomeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(listPanel, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -24f);
            rect.sizeDelta = new Vector2(180f, 48f);

            var image = go.GetComponent<Image>();
            var backSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackButtonSpritePath);
            if (backSprite != null) image.sprite = backSprite;
            image.type = Image.Type.Sliced;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "Back";
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            return go.GetComponent<Button>();
        }

        private static TextMeshProUGUI EnsureLoadErrorLabel(Transform listPanel)
        {
            var existing = listPanel.Find("QuizListLoadErrorText");
            if (existing != null) return existing.GetComponent<TextMeshProUGUI>();

            var go = new GameObject("QuizListLoadErrorText", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(listPanel, false);
            go.SetActive(false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.16f);
            rect.anchorMax = new Vector2(0.5f, 0.16f);
            rect.sizeDelta = new Vector2(640f, 40f);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 16f;
            tmp.color = new Color(0.7f, 0.1f, 0.1f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static QuizListRowView? EnsureQuizListRowSceneTemplate(Transform listPanel)
        {
            var content = listPanel.Find("QuizListScrollArea/Viewport/Content");
            if (content == null)
            {
                Debug.LogWarning("[QuizPortalSceneSetup] Quiz list Content not found.");
                return null;
            }

            var existing = content.Find("QuizListRow");
            if (existing != null && existing.childCount > 0)
            {
                // ponytail: preserve the hand-aligned row template; rebuilding would wipe the
                // column grid aligned with TableHeaderText.
                return existing.GetComponent<QuizListRowView>();
            }

            var rowGo = BuildQuizListRow(content);
            rowGo.name = "QuizListRow";
            ApplyQuizListRowSampleText(rowGo.transform);
            ApplyQuizListRowVisuals(rowGo.transform);
            SetUiLayer(rowGo);
            EditorUtility.SetDirty(rowGo);
            Debug.Log($"[QuizPortalSceneSetup] QuizListRow template created from {AvailableQuizListDesignRef}.");
            return rowGo.GetComponent<QuizListRowView>();
        }

        private static void ApplyQuizListRowSampleText(Transform rowRoot)
        {
            SetRowText(rowRoot.Find("TitleCell"), "TitleText", "Build a Better Paragraph");
            SetRowText(rowRoot.Find("TitleCell"), "TitleSubtitle", "Build sentences with the right words");
            SetRowText(rowRoot, "SubjectText", "LiteraQuest");
            SetRowText(rowRoot, "TermText", "Term 1");
            SetRowText(rowRoot, "GradeText", "Grade 3");
            SetRowText(rowRoot, "ItemsText", "20");
            SetRowText(rowRoot.Find("StatusBadge"), "StatusLabel", string.Empty);
            SetRowText(rowRoot.Find("ActionButton"), "ActionLabel", string.Empty);
        }

        private static void ApplyQuizListRowVisuals(Transform rowRoot)
        {
            var icon = rowRoot.Find("SubjectIcon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_11");
                icon.preserveAspect = true;
                icon.enabled = icon.sprite != null;
            }

            var statusBadge = rowRoot.Find("StatusBadge")?.GetComponent<Image>();
            if (statusBadge != null)
            {
                statusBadge.sprite = LoadSprite(QuizStatesSheet, "QuizStates_2");
                statusBadge.enabled = statusBadge.sprite != null;
            }

            var statusLabel = rowRoot.Find("StatusBadge/StatusLabel");
            if (statusLabel != null) statusLabel.gameObject.SetActive(false);

            var actionImage = rowRoot.Find("ActionButton")?.GetComponent<Image>();
            if (actionImage != null)
            {
                actionImage.sprite = LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_16");
                actionImage.preserveAspect = true;
            }

            var actionLabel = rowRoot.Find("ActionButton/ActionLabel");
            if (actionLabel != null) actionLabel.gameObject.SetActive(false);

            var subjectText = rowRoot.Find("SubjectText")?.GetComponent<TextMeshProUGUI>();
            if (subjectText != null)
                subjectText.color = new Color(0.08f, 0.18f, 0.42f, 1f);
        }

        private static void SetRowText(Transform? root, string childName, string value)
        {
            if (root == null) return;
            var label = root.Find(childName)?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = value;
        }

        private static GameObject BuildQuizListRow(Transform parent)
        {
            var rowGo = new GameObject("QuizListRow", typeof(RectTransform), typeof(LayoutElement), typeof(QuizListRowView));
            rowGo.transform.SetParent(parent, false);
            rowGo.layer = 5;

            var rowRect = rowGo.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.sizeDelta = new Vector2(0f, 68f);
            rowRect.anchoredPosition = Vector2.zero;

            var layoutElement = rowGo.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;
            layoutElement.minHeight = 68f;
            layoutElement.preferredHeight = 68f;

            CreateSubjectIcon(rowGo.transform, "SubjectIcon", QuizListColumnWidths[0], 48f);

            var titleCell = CreateTitleCell(rowGo.transform, QuizListColumnWidths[1]);
            var titleText = titleCell.Find("TitleText")!.GetComponent<TextMeshProUGUI>();
            var titleSubtitle = titleCell.Find("TitleSubtitle")!.GetComponent<TextMeshProUGUI>();

            var rowView = rowGo.GetComponent<QuizListRowView>();
            rowView.SetTitleText(titleText);
            rowView.SetSubjectText(CreateRowText(rowGo.transform, "SubjectText", QuizListColumnWidths[2], 16, FontStyles.Bold));
            rowView.SetTermText(CreateRowText(rowGo.transform, "TermText", QuizListColumnWidths[3], 16, FontStyles.Normal));
            rowView.SetGradeText(CreateRowText(rowGo.transform, "GradeText", QuizListColumnWidths[4], 16, FontStyles.Normal));
            rowView.SetItemsText(CreateRowText(rowGo.transform, "ItemsText", QuizListColumnWidths[5], 16, FontStyles.Normal));

            var statusRoot = CreateRowImage(rowGo.transform, "StatusBadge", QuizListColumnWidths[6], 44f);
            rowView.SetStatusBadgeImage(statusRoot);
            rowView.SetStatusText(CreateRowText(statusRoot.transform, "StatusLabel", QuizListColumnWidths[6] - 16f, 14, FontStyles.Italic));

            var actionRoot = CreateActionButton(rowGo.transform, "ActionButton", QuizListColumnWidths[7], 48f);
            rowView.SetActionButton(actionRoot.GetComponent<Button>());
            rowView.SetActionButtonImage(actionRoot.GetComponent<Image>());
            rowView.SetActionLabel(CreateRowText(actionRoot.transform, "ActionLabel", QuizListColumnWidths[7] - 16f, 16, FontStyles.Bold));

            rowView.SetAvailableBadgeSprite(LoadSprite(QuizStatesSheet, "QuizStates_2"));
            rowView.SetLockedBadgeSprite(LoadSprite(QuizStatesSheet, "QuizStates_3"));
            rowView.SetCompletedBadgeSprite(LoadSprite(QuizStatesSheet, "QuizStates_2"));
            rowView.SetStartButtonSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_16"));
            rowView.SetViewButtonSprite(LoadSprite(AvailableQuizzesSheet, "AvailableQuizzes_17"));

            titleText.color = new Color(0.08f, 0.18f, 0.42f, 1f);
            titleSubtitle.color = new Color(0.35f, 0.38f, 0.45f, 1f);

            ApplyQuizListRowVisuals(rowGo.transform);
            UnlockQuizListRowForManualEdit(rowGo.transform);
            ApplySerializedProperties(rowView);
            return rowGo;
        }

        private static Image CreateSubjectIcon(Transform parent, string name, float width, float height)
        {
            var image = CreateRowImage(parent, name, width, height);
            image.preserveAspect = true;
            return image;
        }

        private static Transform CreateTitleCell(Transform parent, float width)
        {
            var go = new GameObject("TitleCell", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.layer = 5;

            CreateRowText(go.transform, "TitleText", width, 18, FontStyles.Bold);
            CreateRowText(go.transform, "TitleSubtitle", width, 13, FontStyles.Italic);
            return go.transform;
        }

        private static void ApplyQuizListRowLayout(Transform rowRoot)
        {
            ApplyRowTextLayout(rowRoot.Find("TitleCell/TitleText"), QuizListColumnWidths[1], 18, FontStyles.Bold);
            ApplyRowTextLayout(rowRoot.Find("TitleCell/TitleSubtitle"), QuizListColumnWidths[1], 13, FontStyles.Italic);
            ApplyRowTextLayout(rowRoot.Find("SubjectText"), QuizListColumnWidths[2], 16, FontStyles.Bold);
            ApplyRowTextLayout(rowRoot.Find("TermText"), QuizListColumnWidths[3], 16, FontStyles.Normal);
            ApplyRowTextLayout(rowRoot.Find("GradeText"), QuizListColumnWidths[4], 16, FontStyles.Normal);
            ApplyRowTextLayout(rowRoot.Find("ItemsText"), QuizListColumnWidths[5], 16, FontStyles.Normal);
            ApplyQuizListRowVisuals(rowRoot);
            UnlockQuizListRowForManualEdit(rowRoot);
        }

        /// <summary>
        /// Scene-template rows are hand-aligned before prefabbing; layout groups lock RectTransform fields.
        /// </summary>
        private static void UnlockQuizListRowForManualEdit(Transform rowRoot)
        {
            DestroyLayoutComponent<HorizontalLayoutGroup>(rowRoot);

            var titleCell = rowRoot.Find("TitleCell");
            if (titleCell != null)
                DestroyLayoutComponent<VerticalLayoutGroup>(titleCell);

            DestroyLayoutElementsOnDescendants(rowRoot);

            var rowLayout = rowRoot.GetComponent<LayoutElement>() ?? rowRoot.gameObject.AddComponent<LayoutElement>();
            rowLayout.ignoreLayout = true;
            rowLayout.minHeight = 68f;
            rowLayout.preferredHeight = 68f;

            var content = rowRoot.parent;
            if (content != null)
            {
                var contentLayout = content.GetComponent<VerticalLayoutGroup>();
                if (contentLayout != null)
                {
                    contentLayout.childControlWidth = false;
                    contentLayout.childControlHeight = false;
                    contentLayout.childForceExpandWidth = false;
                }
            }

            ApplyManualQuizListRowPositions(rowRoot);
        }

        private static void DestroyLayoutComponent<T>(Transform target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component != null)
                Object.DestroyImmediate(component);
        }

        private static void DestroyLayoutElementsOnDescendants(Transform root)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                DestroyLayoutComponent<LayoutElement>(child);
                DestroyLayoutElementsOnDescendants(child);
            }
        }

        private static void ApplyManualQuizListRowPositions(Transform rowRoot)
        {
            float x = QuizListRowPaddingLeft;

            PlaceManualChild(rowRoot.Find("SubjectIcon"), x, QuizListColumnWidths[0], 48f);
            x += QuizListColumnWidths[0] + QuizListRowColumnSpacing;

            var titleCell = rowRoot.Find("TitleCell");
            PlaceManualChild(titleCell, x, QuizListColumnWidths[1], 56f);
            PlaceManualChild(titleCell?.Find("TitleText"), 0f, QuizListColumnWidths[1], 24f, localY: 10f);
            PlaceManualChild(titleCell?.Find("TitleSubtitle"), 0f, QuizListColumnWidths[1], 18f, localY: -12f);
            x += QuizListColumnWidths[1] + QuizListRowColumnSpacing;

            PlaceManualChild(rowRoot.Find("SubjectText"), x, QuizListColumnWidths[2], 24f);
            x += QuizListColumnWidths[2] + QuizListRowColumnSpacing;
            PlaceManualChild(rowRoot.Find("TermText"), x, QuizListColumnWidths[3], 24f);
            x += QuizListColumnWidths[3] + QuizListRowColumnSpacing;
            PlaceManualChild(rowRoot.Find("GradeText"), x, QuizListColumnWidths[4], 24f);
            x += QuizListColumnWidths[4] + QuizListRowColumnSpacing;
            PlaceManualChild(rowRoot.Find("ItemsText"), x, QuizListColumnWidths[5], 24f);
            x += QuizListColumnWidths[5] + QuizListRowColumnSpacing;

            var statusBadge = rowRoot.Find("StatusBadge");
            PlaceManualChild(statusBadge, x, QuizListColumnWidths[6], 44f);
            PlaceManualChild(statusBadge?.Find("StatusLabel"), 8f, QuizListColumnWidths[6] - 16f, 20f);
            x += QuizListColumnWidths[6] + QuizListRowColumnSpacing;

            var actionButton = rowRoot.Find("ActionButton");
            PlaceManualChild(actionButton, x, QuizListColumnWidths[7], 48f);
            PlaceManualChild(actionButton?.Find("ActionLabel"), 8f, QuizListColumnWidths[7] - 16f, 20f);
        }

        private static void PlaceManualChild(Transform? target, float x, float width, float height, float localY = 0f)
        {
            if (target == null) return;

            var rect = target.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(x, localY);
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;
        }

        private static void ApplyRowTextLayout(Transform? target, float width, float fontSize, FontStyles style)
        {
            if (target == null) return;

            var tmp = target.GetComponent<TextMeshProUGUI>();
            if (tmp == null) return;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static TextMeshProUGUI CreateRowText(Transform parent, string name, float width, float fontSize, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = new Color(0.12f, 0.16f, 0.28f, 1f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Image CreateRowImage(Transform parent, string name, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static GameObject CreateActionButton(Transform parent, string name, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.preserveAspect = true;
            return go;
        }

        private static Sprite LoadSprite(string sheetPath, string spriteName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == spriteName)
                    return sprite;
            }

            return null;
        }

        private static void NormalizeChildScales(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                child.localScale = Vector3.one;
            }
        }

        private static void LayoutHomePanelChildren(Transform homePanel)
        {
            SetRect(homePanel.Find("QuizPortalLabelBox"), 0.5f, 0.92f, 280f, 48f);
            SetRect(homePanel.Find("Icon"), 0.28f, 0.48f, 220f, 220f);
            SetRect(homePanel.Find("WelcomeText"), 0.68f, 0.72f, 420f, 44f);
            SetRect(homePanel.Find("DescriptionText"), 0.68f, 0.58f, 420f, 80f);
            SetRect(homePanel.Find("AvailableQuzzesButton"), 0.68f, 0.42f, 360f, 56f);
            SetRect(homePanel.Find("MyQuizResultsButton"), 0.68f, 0.30f, 360f, 56f);
            SetRect(homePanel.Find("BackMainMenuButton"), 0.68f, 0.18f, 360f, 56f);
        }

        private static void SetRect(Transform target, float anchorX, float anchorY, float width, float height)
        {
            if (target == null) return;

            var rect = target.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorX, anchorY);
            rect.anchorMax = new Vector2(anchorX, anchorY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static GameObject CreateDescriptionText(Transform parent)
        {
            var go = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = "Challenge yourself with fun quizzes across topics and terms. Learn, grow, and celebrate progress!";
            tmp.fontSize = 14f;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;

            return go;
        }

        private static void EnsureBuildSettingsEntry()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var entry in scenes)
            {
                if (entry.path == ScenePath)
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
