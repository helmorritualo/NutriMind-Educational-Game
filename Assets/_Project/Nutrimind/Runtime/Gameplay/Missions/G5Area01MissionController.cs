using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum G5Area01MissionState
{
    NotStarted,
    NearFarmerLira,
    Dialogue,
    Questioning,
    FragmentUnlocked,
    FragmentCollected,
    AreaComplete
}

[Serializable]
public struct G5Area01MissionQuestion
{
    public string text;
    public string optionA;
    public string optionB;
    public string optionC;
    public string optionD;
    [Range(0, 3)] public int correctIndex;
    public string wrongClue;
}

public class G5Area01MissionController : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _missionTitleText;
    [SerializeField] private TextMeshProUGUI _gradeSubjectTermText;
    [SerializeField] private TextMeshProUGUI _objectiveText;
    [SerializeField] private TextMeshProUGUI _itemCounterText;
    [SerializeField] private GameObject _interactPrompt;
    [SerializeField] private Button _interactButton;
    [SerializeField] private TextMeshProUGUI _interactPromptLabel;

    [Header("Panels")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _questionPanel;
    [SerializeField] private GameObject _feedbackPanel;
    [SerializeField] private GameObject _areaCompletePanel;

    [Header("Dialogue")]
    [SerializeField] private TextMeshProUGUI _npcNameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private Button _dialogueContinueButton;

    [Header("Question")]
    [SerializeField] private TextMeshProUGUI _questionProgressText;
    [SerializeField] private TextMeshProUGUI _questionText;
    [SerializeField] private Button[] _answerButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] _answerLabels = new TextMeshProUGUI[4];

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI _feedbackText;
    [SerializeField] private Button _feedbackContinueButton;
    [SerializeField] private TextMeshProUGUI _feedbackContinueLabel;
    [SerializeField] private string _feedbackTryAgainLabel = "Try Again";

    [Header("Area Complete")]
    [SerializeField] private TextMeshProUGUI _areaCompleteTitleText;
    [SerializeField] private TextMeshProUGUI _areaCompleteMessageText;
    [SerializeField] private Button _areaCompleteContinueButton;

    [Header("World")]
    [SerializeField] private GameObject _storyMapFragment;
    [SerializeField] private GameObject _closedGate;
    [SerializeField] private GameObject _openGate;
    [SerializeField] private Collider _gateBlocker;

    [Header("MVP Content")]
    [SerializeField] private string _missionTitle = "Festival Storybook Rescue";
    [SerializeField] private string _gradeSubjectTerm = "Grade 5 • LiteraQuest • Term 1";
    [SerializeField] private string _initialObjective = "Talk to Farmer Lira";
    [SerializeField] private string _questioningObjective = "Answer Farmer Lira's story questions.";
    [SerializeField] private string _pickupObjective = "Pick up the Story Map Fragment.";
    [SerializeField] private string _areaCompleteObjective = "Area 1 complete.";
    [SerializeField] private string _initialItemCounter = "Story Map Fragment: 0/1";
    [SerializeField] private string _collectedItemCounter = "1/1";
    [SerializeField] private string _farmerLiraInteractLabel = "Talk";
    [SerializeField] private string _npcName = "Farmer Lira";
    [TextArea(2, 6)]
    [SerializeField] private string _dialogueBody =
        "These objects belong to the missing story, but I do not know which part comes first. Help me understand the story before the parade begins.";
    [SerializeField] private string _areaCompleteTitle = "Area Complete!";
    [TextArea(2, 4)]
    [SerializeField] private string _areaCompleteBody =
        "You restored the first story clue. The path to the next area is now open.";
    [SerializeField] private G5Area01MissionQuestion[] _questions =
    {
        new G5Area01MissionQuestion
        {
            text = "Mika joined the town parade and carried a flag. Who is the main character?",
            optionA = "A. The rain",
            optionB = "B. Mika",
            optionC = "C. The festival field",
            optionD = "D. The decorations",
            correctIndex = 1,
            wrongClue = "Look at the sentence again. Who is doing the action in the story?"
        },
        new G5Area01MissionQuestion
        {
            text = "Dark clouds appeared, and rain began to fall on the decorations. What is the problem?",
            optionA = "A. Mika carried a flag.",
            optionB = "B. The parade became successful.",
            optionC = "C. Rain may ruin the decorations.",
            optionD = "D. The town prepared early.",
            correctIndex = 2,
            wrongClue = "A problem is what goes wrong in the story. What danger could stop the parade?"
        },
        new G5Area01MissionQuestion
        {
            text = "What happened first?",
            optionA = "A. The rain stopped.",
            optionB = "B. The parade continued.",
            optionC = "C. Mika and her friends protected the decorations.",
            optionD = "D. The town prepared flags and decorations.",
            correctIndex = 3,
            wrongClue = "Think about the beginning of the story. What happened before the rain and before the parade continued?"
        }
    };

    public G5Area01MissionState CurrentState { get; private set; } = G5Area01MissionState.NotStarted;

    public IReadOnlyList<G5Area01MissionQuestion> Questions => _questions;

    private int _currentQuestionIndex;
    private UnityAction[] _answerButtonHandlers;

    private void Awake()
    {
        ResolveInteractPromptRefs();
        ResolveAnswerLabelRefs();
        ResolveFeedbackContinueLabelRef();
        InitializeMission();
    }

    private void Start()
    {
        WireInteractButton();
        WireDialogueContinueButton();
        WireAnswerButtons();
        WireFeedbackContinueButton();
        WireAreaCompleteContinueButton();
    }

    private void OnDestroy()
    {
        if (_interactButton != null)
        {
            _interactButton.onClick.RemoveListener(OnInteractPressed);
        }

        if (_dialogueContinueButton != null)
        {
            _dialogueContinueButton.onClick.RemoveListener(OnDialogueContinuePressed);
        }

        UnwireAnswerButtons();

        if (_feedbackContinueButton != null)
        {
            _feedbackContinueButton.onClick.RemoveListener(OnFeedbackContinuePressed);
        }

        if (_areaCompleteContinueButton != null)
        {
            _areaCompleteContinueButton.onClick.RemoveListener(OnAreaCompleteContinuePressed);
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            OnInteractPressed();
        }
    }

    [ContextMenu("Re-initialize Mission")]
    public void InitializeMission()
    {
        _currentQuestionIndex = 0;
        SetState(G5Area01MissionState.NotStarted);
        ApplyHudContent();
        ApplyDialogueContent();
        ApplyAreaCompletePanelContent();
        ApplyStartingWorldState();
        HideAllModalPanels();
        HideInteractPrompt();
    }

    public void SetState(G5Area01MissionState state)
    {
        if (CurrentState == state)
        {
            return;
        }

        G5Area01MissionState previous = CurrentState;
        CurrentState = state;
        Debug.Log($"[G5Area01Mission] State: {previous} -> {state}");
    }

    public void HideAllModalPanels()
    {
        HideDialoguePanel();
        HideQuestionPanel();
        HideFeedbackPanel();
        HideAreaCompletePanel();
    }

    public void ShowDialoguePanel()
    {
        ApplyDialogueContent();
        HideAllModalPanels();
        SetPanelActive(_dialoguePanel, true);
    }

    public void HideDialoguePanel()
    {
        SetPanelActive(_dialoguePanel, false);
    }

    public void ShowQuestionPanel()
    {
        HideAllModalPanels();
        SetPanelActive(_questionPanel, true);
    }

    public void HideQuestionPanel()
    {
        SetPanelActive(_questionPanel, false);
    }

    public void ShowFeedbackPanel()
    {
        HideAllModalPanels();
        SetPanelActive(_feedbackPanel, true);
    }

    public void HideFeedbackPanel()
    {
        SetPanelActive(_feedbackPanel, false);
    }

    public void ShowAreaCompletePanel()
    {
        HideAllModalPanels();
        SetPanelActive(_areaCompletePanel, true);
    }

    public void HideAreaCompletePanel()
    {
        SetPanelActive(_areaCompletePanel, false);
    }

    public void ShowInteractPrompt()
    {
        SetPanelActive(_interactPrompt, true);
    }

    public void HideInteractPrompt()
    {
        SetPanelActive(_interactPrompt, false);
    }

    public void EnterFarmerLiraRange()
    {
        if (CurrentState != G5Area01MissionState.NotStarted &&
            CurrentState != G5Area01MissionState.NearFarmerLira)
        {
            return;
        }

        SetState(G5Area01MissionState.NearFarmerLira);
        SetText(_interactPromptLabel, _farmerLiraInteractLabel);
        ShowInteractPrompt();
    }

    public void ExitFarmerLiraRange()
    {
        if (CurrentState != G5Area01MissionState.NearFarmerLira)
        {
            return;
        }

        SetState(G5Area01MissionState.NotStarted);
        HideInteractPrompt();
    }

    public void OnInteractPressed()
    {
        if (CurrentState != G5Area01MissionState.NearFarmerLira)
        {
            return;
        }

        HideInteractPrompt();
        ShowDialoguePanel();
        SetState(G5Area01MissionState.Dialogue);
    }

    public void OnDialogueContinuePressed()
    {
        if (CurrentState != G5Area01MissionState.Dialogue)
        {
            return;
        }

        HideDialoguePanel();
        BeginQuestionChallenge();
    }

    private void BeginQuestionChallenge()
    {
        _currentQuestionIndex = 0;
        SetText(_objectiveText, _questioningObjective);
        SetState(G5Area01MissionState.Questioning);
        PresentQuestion(_currentQuestionIndex);
        ShowQuestionPanel();
    }

    private void PresentQuestion(int index)
    {
        if (index < 0 || index >= _questions.Length)
        {
            Debug.LogWarning($"[G5Area01Mission] Question index {index} is out of range.");
            return;
        }

        G5Area01MissionQuestion question = _questions[index];
        SetText(_questionProgressText, $"Question {index + 1} of {_questions.Length}");
        SetText(_questionText, question.text);
        SetText(_answerLabels, 0, question.optionA);
        SetText(_answerLabels, 1, question.optionB);
        SetText(_answerLabels, 2, question.optionC);
        SetText(_answerLabels, 3, question.optionD);
        SetAnswerButtonsInteractable(true);
    }

    public void OnAnswerSelected(int selectedIndex)
    {
        if (CurrentState != G5Area01MissionState.Questioning)
        {
            return;
        }

        if (_feedbackPanel != null && _feedbackPanel.activeSelf)
        {
            return;
        }

        if (_currentQuestionIndex < 0 || _currentQuestionIndex >= _questions.Length)
        {
            return;
        }

        G5Area01MissionQuestion question = _questions[_currentQuestionIndex];
        if (selectedIndex == question.correctIndex)
        {
            HandleCorrectAnswer();
            return;
        }

        ShowWrongAnswerFeedback(question);
    }

    private void HandleCorrectAnswer()
    {
        _currentQuestionIndex++;

        if (_currentQuestionIndex >= _questions.Length)
        {
            CompleteQuestionChallenge();
            return;
        }

        PresentQuestion(_currentQuestionIndex);
    }

    private void ShowWrongAnswerFeedback(G5Area01MissionQuestion question)
    {
        SetText(_feedbackText, question.wrongClue);
        SetFeedbackContinueButtonLabel(_feedbackTryAgainLabel);
        SetAnswerButtonsInteractable(false);
        HideQuestionPanel();
        SetPanelActive(_feedbackPanel, true);
    }

    public void OnFeedbackContinuePressed()
    {
        if (CurrentState != G5Area01MissionState.Questioning)
        {
            return;
        }

        HideFeedbackPanel();
        PresentQuestion(_currentQuestionIndex);
        ShowQuestionPanel();
    }

    private void CompleteQuestionChallenge()
    {
        HideQuestionPanel();
        UnlockStoryMapFragment();
    }

    public void UnlockStoryMapFragment()
    {
        SetState(G5Area01MissionState.FragmentUnlocked);
        SetObjectActive(_storyMapFragment, true);
        SetText(_objectiveText, _pickupObjective);
    }

    public void CollectStoryMapFragment()
    {
        if (CurrentState != G5Area01MissionState.FragmentUnlocked)
        {
            return;
        }

        SetState(G5Area01MissionState.FragmentCollected);
        SetObjectActive(_storyMapFragment, false);
        SetText(_itemCounterText, _collectedItemCounter);
        OpenGate();
        SetText(_objectiveText, _areaCompleteObjective);
        ApplyAreaCompletePanelContent();
        ShowAreaCompletePanel();
        SetState(G5Area01MissionState.AreaComplete);
    }

    public void OnAreaCompleteContinuePressed()
    {
        if (CurrentState != G5Area01MissionState.AreaComplete)
        {
            return;
        }

        HideAreaCompletePanel();
    }

    private void OpenGate()
    {
        SetObjectActive(_closedGate, false);
        SetObjectActive(_openGate, true);

        if (_gateBlocker != null)
        {
            _gateBlocker.enabled = false;
        }
    }

    private void ResolveInteractPromptRefs()
    {
        if (_interactPrompt == null)
        {
            return;
        }

        if (_interactButton == null)
        {
            _interactButton = _interactPrompt.GetComponentInChildren<Button>(true);
        }

        if (_interactPromptLabel == null)
        {
            _interactPromptLabel = _interactPrompt.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void WireInteractButton()
    {
        ResolveInteractPromptRefs();

        if (_interactButton == null)
        {
            Debug.LogWarning("[G5Area01Mission] Interact button is not assigned under InteractPrompt.");
            return;
        }

        _interactButton.onClick.RemoveListener(OnInteractPressed);
        _interactButton.onClick.AddListener(OnInteractPressed);
    }

    private void WireDialogueContinueButton()
    {
        if (_dialogueContinueButton == null)
        {
            Debug.LogWarning("[G5Area01Mission] Dialogue continue button is not assigned.");
            return;
        }

        _dialogueContinueButton.onClick.RemoveListener(OnDialogueContinuePressed);
        _dialogueContinueButton.onClick.AddListener(OnDialogueContinuePressed);
    }

    private void WireAnswerButtons()
    {
        if (_answerButtons == null || _answerButtons.Length == 0)
        {
            Debug.LogWarning("[G5Area01Mission] Answer buttons are not assigned.");
            return;
        }

        UnwireAnswerButtons();
        _answerButtonHandlers = new UnityAction[_answerButtons.Length];

        for (int i = 0; i < _answerButtons.Length; i++)
        {
            int answerIndex = i;
            Button button = _answerButtons[i];
            if (button == null)
            {
                continue;
            }

            _answerButtonHandlers[i] = () => OnAnswerSelected(answerIndex);
            button.onClick.AddListener(_answerButtonHandlers[i]);
        }
    }

    private void UnwireAnswerButtons()
    {
        if (_answerButtons == null || _answerButtonHandlers == null)
        {
            return;
        }

        for (int i = 0; i < _answerButtons.Length && i < _answerButtonHandlers.Length; i++)
        {
            if (_answerButtons[i] == null || _answerButtonHandlers[i] == null)
            {
                continue;
            }

            _answerButtons[i].onClick.RemoveListener(_answerButtonHandlers[i]);
        }
    }

    private void WireFeedbackContinueButton()
    {
        if (_feedbackContinueButton == null)
        {
            Debug.LogWarning("[G5Area01Mission] Feedback continue button is not assigned.");
            return;
        }

        _feedbackContinueButton.onClick.RemoveListener(OnFeedbackContinuePressed);
        _feedbackContinueButton.onClick.AddListener(OnFeedbackContinuePressed);
    }

    private void WireAreaCompleteContinueButton()
    {
        if (_areaCompleteContinueButton == null)
        {
            Debug.LogWarning("[G5Area01Mission] Area complete continue button is not assigned.");
            return;
        }

        _areaCompleteContinueButton.onClick.RemoveListener(OnAreaCompleteContinuePressed);
        _areaCompleteContinueButton.onClick.AddListener(OnAreaCompleteContinuePressed);
    }

    private void ResolveFeedbackContinueLabelRef()
    {
        if (_feedbackContinueLabel != null || _feedbackContinueButton == null)
        {
            return;
        }

        Transform textChild = _feedbackContinueButton.transform.Find("Text");
        _feedbackContinueLabel = textChild != null
            ? textChild.GetComponent<TextMeshProUGUI>()
            : _feedbackContinueButton.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void SetFeedbackContinueButtonLabel(string label)
    {
        ResolveFeedbackContinueLabelRef();
        SetText(_feedbackContinueLabel, label);
    }

    private void SetAnswerButtonsInteractable(bool interactable)
    {
        if (_answerButtons == null)
        {
            return;
        }

        foreach (Button button in _answerButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    private void ResolveAnswerLabelRefs()
    {
        if (_answerButtons == null || _answerButtons.Length == 0)
        {
            return;
        }

        if (_answerLabels == null || _answerLabels.Length != _answerButtons.Length)
        {
            _answerLabels = new TextMeshProUGUI[_answerButtons.Length];
        }

        for (int i = 0; i < _answerButtons.Length; i++)
        {
            if (_answerLabels[i] != null || _answerButtons[i] == null)
            {
                continue;
            }

            Transform textChild = _answerButtons[i].transform.Find("Text");
            _answerLabels[i] = textChild != null
                ? textChild.GetComponent<TextMeshProUGUI>()
                : _answerButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void ApplyHudContent()
    {
        SetText(_missionTitleText, _missionTitle);
        SetText(_gradeSubjectTermText, _gradeSubjectTerm);
        SetText(_objectiveText, _initialObjective);
        SetText(_itemCounterText, _initialItemCounter);
    }

    private void ApplyDialogueContent()
    {
        SetText(_npcNameText, _npcName);
        SetText(_dialogueText, _dialogueBody);
    }

    private void ApplyAreaCompletePanelContent()
    {
        SetText(_areaCompleteTitleText, _areaCompleteTitle);
        SetText(_areaCompleteMessageText, _areaCompleteBody);
    }

    private void ApplyStartingWorldState()
    {
        SetObjectActive(_storyMapFragment, false);
        SetObjectActive(_closedGate, true);
        SetObjectActive(_openGate, false);

        if (_gateBlocker != null)
        {
            _gateBlocker.enabled = true;
        }
        else
        {
            Debug.LogWarning("[G5Area01Mission] _gateBlocker is not assigned; gate blocking will not work.");
        }
    }

    private static void SetText(TextMeshProUGUI label, string value)
    {
        if (label == null)
        {
            return;
        }

        label.text = value;
    }

    private static void SetText(TextMeshProUGUI[] labels, int index, string value)
    {
        if (labels == null || index < 0 || index >= labels.Length)
        {
            return;
        }

        SetText(labels[index], value);
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null)
        {
            if (active)
            {
                Debug.LogWarning("[G5Area01Mission] Tried to show a panel that is not assigned.");
            }

            return;
        }

        panel.SetActive(active);
    }

    private static void SetObjectActive(GameObject target, bool active)
    {
        if (target == null)
        {
            if (!active)
            {
                Debug.LogWarning("[G5Area01Mission] Tried to set active state on an unassigned world object.");
            }

            return;
        }

        target.SetActive(active);
    }
}
