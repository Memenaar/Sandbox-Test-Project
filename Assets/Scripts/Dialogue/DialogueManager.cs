using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Ink.Runtime;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private CallsheetSO _callsheet = default; // Scriptable object containing a list of all characters' CharIdentitySOs
    [SerializeField] private InputReader _inputReader = default; // Scriptable object that conveys input
    [SerializeField] private GameStateSO _gameState = default; // Tracks game state

    [Header("Dialogue UI")]
    [SerializeField] private UIDialogueManager _uiDialogueManager;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _continueIcon;
    [SerializeField] private GameObject _namePanelLeft;
    [SerializeField] private GameObject _namePanelRight;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _nameTextLeft;
    [SerializeField] private TextMeshProUGUI _nameTextRight;
    [SerializeField] private GameObject[] _leftPortraits;
    [SerializeField] private GameObject _leftPortraitPanel;
    [SerializeField] private GameObject _rightPortraitPanel;
    [SerializeField] private GameObject[] _rightPortraits;

    [Header("Value Holders")]
    [SerializeField] [ReadOnly] private string _activeSide;
    [SerializeField] [ReadOnly] private string _currentSpeakerText;
    [SerializeField] [ReadOnly] private TextMeshProUGUI _activeNameText;
    [SerializeField] [ReadOnly] private GameObject _activePortraitSide;
    [SerializeField] [ReadOnly] private GameObject[] _activePortraits;
    [SerializeField] [ReadOnly] public List<GameObject> _currentPortraits;   
    [SerializeField] [ReadOnly] private List<CharIdentitySO> _currentSpeakers;


    [Header("Choices UI")]
    [SerializeField] private GameObject[] _choices;
    private TextMeshProUGUI[] _choicesText;

	[Header("Broadcasting On")]
	[SerializeField] private DialogueLineChannelSO _openDialogueUIEvent = default;
    [SerializeField] private DialogueUIChannelSO _changeDialogueUIEvent = default;
	[SerializeField] private DialogueChoicesChannelSO _showChoicesUIEvent = default;
	[SerializeField] private IntEventChannelSO _endDialogueWithTypeEvent = default;
	//[SerializeField] private VoidEventChannelSO _continueWithStep = default;
	//[SerializeField] private VoidEventChannelSO _playIncompleteDialogue = default;
	//[SerializeField] private VoidEventChannelSO _makeWinningChoice = default;
	//[SerializeField] private VoidEventChannelSO _makeLosingChoice = default;

    [Header("Listening To")]
	[SerializeField] private DialogueDataChannelSO _startDialogue = default;
	[SerializeField] private DialogueChoiceChannelSO _makeDialogueChoiceEvent = default;

    private DialogueDataSO _currentDialogue;
    private Story _currentStory;

    private bool _skipLine = false;

    public bool _dialogueIsPlaying {get; private set; }

    private static DialogueManager _instance;

    private GameObject _player;

    private bool _dialogueError = false;

    private Coroutine _displayLineCoroutine;

    private bool _canContinueToNextLine = false;

    [Header("Tag Keys")]
    private const string SIDE_TAG = "side";
    private const string LAYOUT_TAG = "layout";
    private const string POSITION_TAG = "position";
    private const string SPEAKER_TAG = "speaker";
    private const string ANIMATION_TAG = "animation";

    public CallsheetSO Callsheet { get { return _callsheet; }}
    public bool DialogueError { get { return _dialogueError; } set { _dialogueError = value; }}

    public static DialogueManager GetInstance()
    {
        return _instance;
    }

    #region Monobehaviour Methods

    private void Awake()
    {
        InitializeDM();
        _startDialogue.OnEventRaised += ProcessDialogueData; // When an event is raised on the channel assigned to StartDialogue
    }
    
    private void Start()
    {
        
        _dialogueIsPlaying = false;
        ResetAll();

        _choicesText = new TextMeshProUGUI[_choices.Length];
        int index = 0;
        foreach (GameObject choice in _choices)
        {
            _choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }
    #endregion

    public void ProcessDialogueData(DialogueDataSO dialogueDataSO)
    {

        SwitchGameState();
        _currentDialogue = dialogueDataSO;
        _currentStory = new Story(dialogueDataSO.DialogueInk.text);
        _dialogueIsPlaying = true;
        
        DialogueContinue();

    }

    public void DisplayDialogueLine(string dialogueLine)
    {
        _openDialogueUIEvent.RaiseEvent(dialogueLine);
    }

    #region Initialization
    public void InitializeDM()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Attempted to instantiate more than one dialogue manager in the scene.");
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
        }

        _player = GameObject.Find("Player");

    }
    #endregion

    #region Game State Methods
    private void ExitDialogueMode()
    {
        _dialogueIsPlaying = false;
        //_dialoguePanel.SetActive(false);
        _uiDialogueManager.DialogueText.text = "";
        SwitchGameState();
        
    }

    private void SwitchGameState()
    {
        if (_gameState.CurrentGameState != GameState.Dialogue)
        {
            _inputReader.EnableDialogueInput();
            _inputReader.DialogueContinueEvent += ProcessInput;
            _gameState.UpdateGameState(GameState.Dialogue);
            _player.GetComponent<PlayerStateMachine>().PlayerInteraction = true;
        } else
        {
            _inputReader.EnableTownInput();
            _inputReader.DialogueContinueEvent -= ProcessInput;
            _gameState.ResetToPreviousGameState();
            _player.GetComponent<PlayerStateMachine>().PlayerInteraction = false;
        }
    }
    #endregion

    #region ResetMethods
    private void ResetAll()
    {
        _dialoguePanel.SetActive(false);
        _namePanelLeft.SetActive(false);
        _namePanelRight.SetActive(false);
        foreach(GameObject portrait in _leftPortraits)
        {
            portrait.SetActive(false);
        }
        foreach(GameObject portrait in _rightPortraits)
        {
            portrait.SetActive(false);
        }
    }

    private void ResetTags()
    {
        _nameTextLeft.text = "";
        _nameTextRight.text = "";
        _currentPortraits = null;
        _activeSide = "R";
        _activeNameText = _nameTextRight;
    }
    #endregion

    private void DialogueContinue()
    {
        if (_currentStory.canContinue)
        {
            DisplayDialogueLine(_currentStory.Continue());
            _changeDialogueUIEvent.RaiseEvent(_currentStory.currentTags); // Passes currentTags to UI Dialogue Manager for processing

        } else
        {
            ExitDialogueMode();
        }        
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = _currentStory.currentChoices;

        // Defensive check to ensure our UI can support the number of incoming choices.
        if (currentChoices.Count > _choices.Length)
        {
            _dialogueError = true;
            _uiDialogueManager.DialogueText.text = "Error: More choices were given than UI can support.";
            Debug.LogError("More choices were given than UI can support.");
            return;
        }

        int index = 0;
        // Enable and initialize choice buttons for each available response in the UI.
        foreach(Choice choice in currentChoices)
        {
            _choices[index].gameObject.SetActive(true);
            _choicesText[index].text = choice.text;
            index++;
        }
        // Go through the remaining choice buttons and make sure they're hidden.
        for (int i = index; i < _choices.Length; i++)
        {
            _choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private void HideChoices()
    {
        foreach (GameObject choiceButton in _choices)
        {
            choiceButton.SetActive(false);
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(_choices[0].gameObject);
    }

    #region Input Methods
    public void MakeChoice(int choiceIndex)
    {
        if (_canContinueToNextLine)
        {
            _currentStory.ChooseChoiceIndex(choiceIndex);
            DialogueContinue();
        }
    }

    public void ProcessInput()
    {
        if (_uiDialogueManager.CanContinueToNextLine)
        {
            if (_currentStory.currentChoices.Count == 0) // If there are no choices in response to the current dialogue, call the DialogueContinue() method.
            {
                DialogueContinue();
            } else if (_dialogueError)
            {
                _dialogueError = false;
                ExitDialogueMode();
            }
            // Otherwise, the MakeChoice() method will be called by the selected button.
        } else
        {
            _uiDialogueManager.SkipLine = true;
        }
    }
    #endregion
}
