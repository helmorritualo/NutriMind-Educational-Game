using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NutriMind.Runtime.App.Dto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Thin controller for the Available Quiz List Canvas panel.
    /// Loads provider data, applies subject filters, paginates rows, and preserves list state.
    /// </summary>
    public class AvailableQuizListController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Navigation")]
        [SerializeField] private QuizPortalNavigationController _navigation;

        [Header("Subject Filters")]
        [SerializeField] private Button _allFilterButton;
        [SerializeField] private Button _literaQuestFilterButton;
        [SerializeField] private Button _peHealthFilterButton;
        [SerializeField] private Button _scienceFilterButton;
        [SerializeField] private Image _allFilterImage;
        [SerializeField] private Image _literaQuestFilterImage;
        [SerializeField] private Image _peHealthFilterImage;
        [SerializeField] private Image _scienceFilterImage;
        [SerializeField] private Sprite _filterSelectedSprite;
        [SerializeField] private Sprite _filterUnselectedSprite;
        [SerializeField] private Sprite _literaQuestSelectedSprite;
        [SerializeField] private Sprite _peHealthSelectedSprite;
        [SerializeField] private Sprite _scienceSelectedSprite;
        [SerializeField] private Sprite _literaQuestUnselectedSprite;
        [SerializeField] private Sprite _peHealthUnselectedSprite;
        [SerializeField] private Sprite _scienceUnselectedSprite;

        [Header("List")]
        [SerializeField] private ScrollRect _listScroll;
        [SerializeField] private RectTransform _listContent;
        [SerializeField] private QuizListRowView _rowPrefab;

        [Header("Pagination")]
        [SerializeField] private Button _paginationLeftButton;
        [SerializeField] private Button _paginationRightButton;
        [SerializeField] private Button[] _pageNumberButtons = Array.Empty<Button>();
        [SerializeField] private TextMeshProUGUI _pageSummaryText;

        [Header("Actions")]
        [SerializeField] private Button _backToHomeButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI _loadErrorLabel;
        [SerializeField] private GameObject _loadingOverlay;

        [Header("Empty State")]
        [SerializeField] private GameObject _emptyStateRoot;
        [SerializeField] private Button _emptyRefreshButton;
        [SerializeField] private Button _emptyBackButton;

        private readonly List<QuizListRowView> _spawnedRows = new();
        private CancellationTokenSource? _cts;
        private bool _isLoading;

        public void SetPanelRoot(GameObject val) => _panelRoot = val;
        public void SetNavigation(QuizPortalNavigationController val) => _navigation = val;
        public void SetAllFilterButton(Button val) => _allFilterButton = val;
        public void SetLiteraQuestFilterButton(Button val) => _literaQuestFilterButton = val;
        public void SetPeHealthFilterButton(Button val) => _peHealthFilterButton = val;
        public void SetScienceFilterButton(Button val) => _scienceFilterButton = val;
        public void SetAllFilterImage(Image val) => _allFilterImage = val;
        public void SetLiteraQuestFilterImage(Image val) => _literaQuestFilterImage = val;
        public void SetPeHealthFilterImage(Image val) => _peHealthFilterImage = val;
        public void SetScienceFilterImage(Image val) => _scienceFilterImage = val;
        public void SetFilterSelectedSprite(Sprite val) => _filterSelectedSprite = val;
        public void SetFilterUnselectedSprite(Sprite val) => _filterUnselectedSprite = val;
        public void SetLiteraQuestSelectedSprite(Sprite val) => _literaQuestSelectedSprite = val;
        public void SetPeHealthSelectedSprite(Sprite val) => _peHealthSelectedSprite = val;
        public void SetScienceSelectedSprite(Sprite val) => _scienceSelectedSprite = val;
        public void SetLiteraQuestUnselectedSprite(Sprite val) => _literaQuestUnselectedSprite = val;
        public void SetPeHealthUnselectedSprite(Sprite val) => _peHealthUnselectedSprite = val;
        public void SetScienceUnselectedSprite(Sprite val) => _scienceUnselectedSprite = val;
        public void SetListScroll(ScrollRect val) => _listScroll = val;
        public void SetListContent(RectTransform val) => _listContent = val;
        public void SetRowPrefab(QuizListRowView val) => _rowPrefab = val;
        public void SetPaginationLeftButton(Button val) => _paginationLeftButton = val;
        public void SetPaginationRightButton(Button val) => _paginationRightButton = val;
        public void SetPageNumberButtons(Button[] val) => _pageNumberButtons = val ?? Array.Empty<Button>();
        public void SetPageSummaryText(TextMeshProUGUI val) => _pageSummaryText = val;
        public void SetBackToHomeButton(Button val) => _backToHomeButton = val;
        public void SetLoadErrorLabel(TextMeshProUGUI val) => _loadErrorLabel = val;
        public void SetLoadingOverlay(GameObject val) => _loadingOverlay = val;
        public void SetEmptyStateRoot(GameObject val) => _emptyStateRoot = val;
        public void SetEmptyRefreshButton(Button val) => _emptyRefreshButton = val;
        public void SetEmptyBackButton(Button val) => _emptyBackButton = val;

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            BindFilterButtons();
            BindPaginationButtons();
            if (_backToHomeButton != null) _backToHomeButton.onClick.AddListener(OnBackToHomeClicked);
            if (_emptyRefreshButton != null) _emptyRefreshButton.onClick.AddListener(OnEmptyRefreshClicked);
            if (_emptyBackButton != null) _emptyBackButton.onClick.AddListener(OnEmptyBackClicked);
            ApplyFilterVisuals(QuizSubjectFilters.All);
        }

        private void OnEnable()
        {
            ClearLoadError();
            StartCoroutine(InitializePanelRoutine());
        }

        private void OnDisable()
        {
            SaveScrollPosition();
            SetEmptyStateVisible(false);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            UnbindFilterButtons();
            UnbindPaginationButtons();
            if (_backToHomeButton != null) _backToHomeButton.onClick.RemoveListener(OnBackToHomeClicked);
            if (_emptyRefreshButton != null) _emptyRefreshButton.onClick.RemoveListener(OnEmptyRefreshClicked);
            if (_emptyBackButton != null) _emptyBackButton.onClick.RemoveListener(OnEmptyBackClicked);
        }

        private IEnumerator InitializePanelRoutine()
        {
            var store = EnsureStore();
            ApplyFilterVisuals(store.SelectedSubjectFilter);

            if (!store.IsLoaded || store.Quizzes.Count == 0)
            {
                yield return LoadCatalogRoutine();
            }
            else
            {
                RenderList();
            }
        }

        private IEnumerator LoadCatalogRoutine()
        {
            if (_isLoading) yield break;
            _isLoading = true;
            SetLoadingVisible(true);
            ClearLoadError();

            var root = CompositionRoot.Instance;
            if (root?.DataProvider == null)
            {
                ShowLoadError("Quiz data is not available right now.");
                SetEmptyStateVisible(false);
                SetLoadingVisible(false);
                _isLoading = false;
                yield break;
            }

            var store = EnsureStore();
            var loadTask = QuizListCatalogLoader.LoadAllAsync(root.DataProvider, store, _cts?.Token ?? CancellationToken.None);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.IsFaulted || loadTask.IsCanceled)
            {
                ShowLoadError("Could not load quizzes. Please try again.");
                SetEmptyStateVisible(false);
                SetLoadingVisible(false);
                _isLoading = false;
                yield break;
            }

            var result = loadTask.Result;
            if (result == null || !result.Success)
            {
                ShowLoadError(result?.Error?.Message ?? "Could not load quizzes. Please try again.");
                SetEmptyStateVisible(false);
                SetLoadingVisible(false);
                _isLoading = false;
                yield break;
            }

            SetLoadingVisible(false);
            _isLoading = false;
            RenderList();
        }

        public void RenderList()
        {
            var store = EnsureStore();
            var filtered = FilterQuizzes(store.Quizzes, store.SelectedSubjectFilter);
            store.CurrentPage = QuizListActionResolver.ClampPage(store.CurrentPage, filtered.Count);

            ClearRows();
            var (start, endEx) = QuizListActionResolver.GetPageSlice(store.CurrentPage, filtered.Count);
            for (int i = start; i < endEx; i++)
            {
                SpawnRow(filtered[i]);
            }

            UpdatePagination(filtered.Count);
            RestoreScrollPosition(store.ScrollPosition);
            EvaluateEmptyState(filtered.Count);
        }

        public static bool ShouldShowEmptyState(int filteredCount, bool isLoaded, bool loadErrorPresent)
            => filteredCount == 0 && isLoaded && !loadErrorPresent;

        public static List<QuizDto> FilterQuizzes(IReadOnlyList<QuizDto> quizzes, string filterKey)
        {
            var filtered = new List<QuizDto>();
            foreach (var quiz in quizzes)
            {
                if (quiz == null) continue;
                if (QuizSubjectFilters.Matches(quiz.SubjectSlug, filterKey))
                    filtered.Add(quiz);
            }

            return filtered;
        }

        private void SpawnRow(QuizDto quiz)
        {
            if (_rowPrefab == null || _listContent == null) return;

            var row = Instantiate(_rowPrefab, _listContent);
            var actionKind = QuizListActionResolver.Resolve(quiz);
            row.Bind(quiz, actionKind, OnRowActionClicked);
            _spawnedRows.Add(row);
        }

        private void OnRowActionClicked(QuizDto quiz)
        {
            var store = EnsureStore();
            store.SelectedQuizId = quiz.Id;

            var actionKind = QuizListActionResolver.Resolve(quiz);
            switch (actionKind)
            {
                case QuizListRowActionKind.Start:
                    Debug.Log($"[AvailableQuizListController] Start quiz '{quiz.Id}' (Quiz Instructions unit not built yet).");
                    break;
                case QuizListRowActionKind.View:
                    Debug.Log($"[AvailableQuizListController] View quiz '{quiz.Id}' (Locked Quiz State modal not built yet).");
                    break;
                case QuizListRowActionKind.ViewResult:
                    Debug.Log($"[AvailableQuizListController] View result for quiz '{quiz.Id}' (Quiz Result Screen not built yet).");
                    break;
            }
        }

        private void UpdatePagination(int filteredCount)
        {
            var store = EnsureStore();
            if (_pageSummaryText != null)
                _pageSummaryText.text = QuizListActionResolver.FormatPageSummary(store.CurrentPage, filteredCount);

            int totalPages = QuizListActionResolver.GetTotalPages(filteredCount);
            if (_paginationLeftButton != null)
                _paginationLeftButton.interactable = store.CurrentPage > 1;
            if (_paginationRightButton != null)
                _paginationRightButton.interactable = store.CurrentPage < totalPages;

            for (int i = 0; i < _pageNumberButtons.Length; i++)
            {
                var button = _pageNumberButtons[i];
                if (button == null) continue;

                int pageNumber = i + 1;
                bool visible = pageNumber <= totalPages;
                button.gameObject.SetActive(visible);
                button.interactable = visible && pageNumber != store.CurrentPage;

                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = pageNumber.ToString();
            }
        }

        private void OnFilterClicked(string filterKey)
        {
            var store = EnsureStore();
            if (store.SelectedSubjectFilter == filterKey) return;

            store.SelectedSubjectFilter = filterKey;
            store.CurrentPage = 1;
            ApplyFilterVisuals(filterKey);
            RenderList();
        }

        private void ApplyFilterVisuals(string activeFilter)
        {
            ApplyFilterChip(_allFilterImage, activeFilter == QuizSubjectFilters.All, _filterSelectedSprite, _filterUnselectedSprite);
            ApplyFilterChip(
                _literaQuestFilterImage,
                activeFilter == QuizSubjectFilters.LiteraQuest,
                _literaQuestSelectedSprite ?? _filterSelectedSprite,
                _literaQuestUnselectedSprite ?? _filterUnselectedSprite);
            ApplyFilterChip(
                _peHealthFilterImage,
                activeFilter == QuizSubjectFilters.PeHealth,
                _peHealthSelectedSprite ?? _filterSelectedSprite,
                _peHealthUnselectedSprite ?? _filterUnselectedSprite);
            ApplyFilterChip(
                _scienceFilterImage,
                activeFilter == QuizSubjectFilters.Science,
                _scienceSelectedSprite ?? _filterSelectedSprite,
                _scienceUnselectedSprite ?? _filterUnselectedSprite);
        }

        private static void ApplyFilterChip(Image? image, bool selected, Sprite? selectedSprite, Sprite? unselectedSprite)
        {
            if (image == null) return;
            image.sprite = selected ? selectedSprite : unselectedSprite;
        }

        private void OnPageNumberClicked(int pageNumber)
        {
            var store = EnsureStore();
            store.CurrentPage = pageNumber;
            RenderList();
        }

        private void OnPaginationLeftClicked()
        {
            var store = EnsureStore();
            if (store.CurrentPage <= 1) return;
            store.CurrentPage--;
            RenderList();
        }

        private void OnPaginationRightClicked()
        {
            var store = EnsureStore();
            var filteredCount = FilterQuizzes(store.Quizzes, store.SelectedSubjectFilter).Count;
            int totalPages = QuizListActionResolver.GetTotalPages(filteredCount);
            if (store.CurrentPage >= totalPages) return;
            store.CurrentPage++;
            RenderList();
        }

        private void OnBackToHomeClicked()
        {
            SaveScrollPosition();
            _navigation?.ShowHome();
        }

        private void EvaluateEmptyState(int filteredCount)
        {
            var store = EnsureStore();
            bool errorShown = _loadErrorLabel != null && _loadErrorLabel.gameObject.activeSelf;
            SetEmptyStateVisible(ShouldShowEmptyState(filteredCount, store.IsLoaded, errorShown));
        }

        private void SetEmptyStateVisible(bool visible)
        {
            if (_emptyStateRoot != null)
                _emptyStateRoot.SetActive(visible);
        }

        private void OnEmptyRefreshClicked()
        {
            if (_isLoading) return;
            StartCoroutine(LoadCatalogRoutine());
        }

        private void OnEmptyBackClicked()
        {
            SaveScrollPosition();
            _navigation?.ShowHome();
        }

        private void SaveScrollPosition()
        {
            if (_listScroll == null) return;
            var store = EnsureStore();
            store.ScrollPosition = _listScroll.verticalNormalizedPosition;
        }

        private void RestoreScrollPosition(float normalizedPosition)
        {
            if (_listScroll == null) return;
            _listScroll.verticalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
        }

        private void ClearRows()
        {
            foreach (var row in _spawnedRows)
            {
                if (row != null) Destroy(row.gameObject);
            }

            _spawnedRows.Clear();
        }

        private QuizAvailabilityStore EnsureStore()
        {
            var root = CompositionRoot.Instance;
            if (root?.Session == null)
                return new QuizAvailabilityStore();

            if (root.Session.QuizAvailabilityStore == null)
                root.Session.QuizAvailabilityStore = new QuizAvailabilityStore();

            return root.Session.QuizAvailabilityStore;
        }

        private void SetLoadingVisible(bool visible)
        {
            if (_loadingOverlay != null)
                _loadingOverlay.SetActive(visible);
        }

        private void ShowLoadError(string message)
        {
            if (_loadErrorLabel != null)
            {
                _loadErrorLabel.gameObject.SetActive(true);
                _loadErrorLabel.text = message;
            }
            else
            {
                Debug.LogWarning($"[AvailableQuizListController] {message}");
            }
        }

        private void ClearLoadError()
        {
            if (_loadErrorLabel != null)
            {
                _loadErrorLabel.text = string.Empty;
                _loadErrorLabel.gameObject.SetActive(false);
            }
        }

        private void BindFilterButtons()
        {
            if (_allFilterButton != null) _allFilterButton.onClick.AddListener(() => OnFilterClicked(QuizSubjectFilters.All));
            if (_literaQuestFilterButton != null) _literaQuestFilterButton.onClick.AddListener(() => OnFilterClicked(QuizSubjectFilters.LiteraQuest));
            if (_peHealthFilterButton != null) _peHealthFilterButton.onClick.AddListener(() => OnFilterClicked(QuizSubjectFilters.PeHealth));
            if (_scienceFilterButton != null) _scienceFilterButton.onClick.AddListener(() => OnFilterClicked(QuizSubjectFilters.Science));
        }

        private void UnbindFilterButtons()
        {
            if (_allFilterButton != null) _allFilterButton.onClick.RemoveAllListeners();
            if (_literaQuestFilterButton != null) _literaQuestFilterButton.onClick.RemoveAllListeners();
            if (_peHealthFilterButton != null) _peHealthFilterButton.onClick.RemoveAllListeners();
            if (_scienceFilterButton != null) _scienceFilterButton.onClick.RemoveAllListeners();
        }

        private void BindPaginationButtons()
        {
            if (_paginationLeftButton != null) _paginationLeftButton.onClick.AddListener(OnPaginationLeftClicked);
            if (_paginationRightButton != null) _paginationRightButton.onClick.AddListener(OnPaginationRightClicked);

            for (int i = 0; i < _pageNumberButtons.Length; i++)
            {
                int pageNumber = i + 1;
                var button = _pageNumberButtons[i];
                if (button != null)
                    button.onClick.AddListener(() => OnPageNumberClicked(pageNumber));
            }
        }

        private void UnbindPaginationButtons()
        {
            if (_paginationLeftButton != null) _paginationLeftButton.onClick.RemoveListener(OnPaginationLeftClicked);
            if (_paginationRightButton != null) _paginationRightButton.onClick.RemoveListener(OnPaginationRightClicked);

            foreach (var button in _pageNumberButtons)
            {
                if (button != null) button.onClick.RemoveAllListeners();
            }
        }
    }
}
