using System;
using NutriMind.Runtime.App.Dto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Canvas row binding for one quiz entry in Available Quiz List.
    /// </summary>
    public class QuizListRowView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subjectText;
        [SerializeField] private TextMeshProUGUI _termText;
        [SerializeField] private TextMeshProUGUI _gradeText;
        [SerializeField] private TextMeshProUGUI _itemsText;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Image _statusBadgeImage;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionLabel;
        [SerializeField] private Image _actionButtonImage;

        [Header("Status Badge Sprites")]
        [SerializeField] private Sprite _availableBadgeSprite;
        [SerializeField] private Sprite _lockedBadgeSprite;
        [SerializeField] private Sprite _completedBadgeSprite;

        [Header("Action Button Sprites")]
        [SerializeField] private Sprite _startButtonSprite;
        [SerializeField] private Sprite _viewButtonSprite;

        private QuizDto _boundQuiz;
        private Action<QuizDto>? _onActionClicked;

        public void SetTitleText(TextMeshProUGUI val) => _titleText = val;
        public void SetSubjectText(TextMeshProUGUI val) => _subjectText = val;
        public void SetTermText(TextMeshProUGUI val) => _termText = val;
        public void SetGradeText(TextMeshProUGUI val) => _gradeText = val;
        public void SetItemsText(TextMeshProUGUI val) => _itemsText = val;
        public void SetStatusText(TextMeshProUGUI val) => _statusText = val;
        public void SetStatusBadgeImage(Image val) => _statusBadgeImage = val;
        public void SetActionButton(Button val) => _actionButton = val;
        public void SetActionLabel(TextMeshProUGUI val) => _actionLabel = val;
        public void SetActionButtonImage(Image val) => _actionButtonImage = val;
        public void SetAvailableBadgeSprite(Sprite val) => _availableBadgeSprite = val;
        public void SetLockedBadgeSprite(Sprite val) => _lockedBadgeSprite = val;
        public void SetCompletedBadgeSprite(Sprite val) => _completedBadgeSprite = val;
        public void SetStartButtonSprite(Sprite val) => _startButtonSprite = val;
        public void SetViewButtonSprite(Sprite val) => _viewButtonSprite = val;

        private void Awake()
        {
            if (_actionButton != null)
                _actionButton.onClick.AddListener(HandleActionClicked);
        }

        private void OnDestroy()
        {
            if (_actionButton != null)
                _actionButton.onClick.RemoveListener(HandleActionClicked);
        }

        public void Bind(QuizDto quiz, QuizListRowActionKind actionKind, Action<QuizDto> onActionClicked)
        {
            _boundQuiz = quiz;
            _onActionClicked = onActionClicked;

            if (_titleText != null) _titleText.text = quiz.Title ?? "—";
            if (_subjectText != null) _subjectText.text = QuizListActionResolver.FormatSubjectLabel(quiz.SubjectSlug);
            if (_termText != null) _termText.text = quiz.TermNumber.HasValue ? $"Term {quiz.TermNumber}" : "—";
            if (_gradeText != null) _gradeText.text = quiz.GradeLevel.HasValue ? $"Grade {quiz.GradeLevel}" : "—";
            if (_itemsText != null) _itemsText.text = quiz.TotalItems.HasValue ? quiz.TotalItems.Value.ToString() : "—";

            string statusLabel = QuizListActionResolver.FormatStatusLabel(quiz.State);
            if (_statusText != null) _statusText.text = statusLabel;

            if (_statusBadgeImage != null)
            {
                _statusBadgeImage.sprite = ResolveStatusSprite(quiz.State);
                _statusBadgeImage.enabled = _statusBadgeImage.sprite != null;
            }

            string actionLabel = QuizListActionResolver.GetActionLabel(actionKind);
            if (_actionLabel != null) _actionLabel.text = actionLabel;

            if (_actionButton != null)
            {
                _actionButton.interactable = actionKind != QuizListRowActionKind.Disabled;
            }

            if (_actionButtonImage != null)
            {
                _actionButtonImage.sprite = actionKind == QuizListRowActionKind.Start
                    ? _startButtonSprite
                    : _viewButtonSprite;
            }
        }

        private Sprite ResolveStatusSprite(string? state)
        {
            if (QuizListActionResolver.IsAvailableStatus(state) || string.Equals(state, "completed", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(state, "completed", StringComparison.OrdinalIgnoreCase))
                    return _completedBadgeSprite != null ? _completedBadgeSprite : _availableBadgeSprite;
                return _availableBadgeSprite;
            }

            if (QuizListActionResolver.IsLockedStatus(state))
                return _lockedBadgeSprite;

            return _availableBadgeSprite;
        }

        private void HandleActionClicked()
        {
            if (_boundQuiz != null)
                _onActionClicked?.Invoke(_boundQuiz);
        }
    }
}
