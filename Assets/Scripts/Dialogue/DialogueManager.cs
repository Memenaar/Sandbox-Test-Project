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

    public bool _dialogueError = false;

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
        _makeDialogueChoiceEvent.OnEventRaised += ProcessChoice;
    }
    
    private void Start()
    {
        
        _dialogueIsPlaying = false;

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
        //raise the special event for end of dialogue if any 
		//_currentDialogue.FinishDialogue();
		
        SwitchGameState();

		//raise end dialogue event 
		if (_endDialogueWithTypeEvent != null)
		    _endDialogueWithTypeEvent.RaiseEvent((int)_currentDialogue.DialogueType);

        _dialogueIsPlaying = false;
        _currentDialogue = null;
        _currentStory = null;
        
        _uiDialogueManager.ResetAll();
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

    private void DialogueContinue()
    {
        if (_currentStory.canContinue && !_dialogueError)
        {
            DisplayDialogueLine(_currentStory.Continue());
            _changeDialogueUIEvent.RaiseEvent(_currentStory.currentTags); // Passes currentTags to UI Dialogue Manager for processing
            _showChoicesUIEvent.RaiseEvent(_currentStory.currentChoices); // Passes currentChoices to the UI Dialogue Manager to away display
        } else
        {
            ExitDialogueMode();
        }        
    }

    #region Input Methods

    public void ProcessInput()
    {   
        if (_dialogueError) {_dialogueError = false; ExitDialogueMode();}
        else{
            if (_uiDialogueManager.CanContinueToNextLine)
            {
                if (_currentStory.currentChoices.Count == 0) // If there are no choices in response to the current dialogue, call the DialogueContinue() method.
                {
                    DialogueContinue();
                }
                // Otherwise, the MakeChoice() method will be called by the selected button.
            } else
            {
                _uiDialogueManager.SkipLine = true;
            }
        }
    }

    public void ProcessChoice(int choiceIndex)
    {
        _currentStory.ChooseChoiceIndex(choiceIndex);
        DialogueContinue();
    }
    #endregion
}
