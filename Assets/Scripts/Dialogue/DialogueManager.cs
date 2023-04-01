using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader = default; // Scriptable object that conveys input
    [SerializeField] private GameStateSO _gameState = default; // Tracks game state

    [Header("Parameters")]
    [SerializeField] private float _typingSpeed = 0.04f;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _continueIcon;
    [SerializeField] private GameObject _namePanelLeft;
    [SerializeField] private GameObject _namePanelRight;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _nameTextLeft;
    [SerializeField] private TextMeshProUGUI _nameTextRight;
    [SerializeField] private GameObject[] _portraits;
    public GameObject _currentPortrait;   
    private TextMeshProUGUI _activeNameText;
    public string _activeSide;
    Dictionary<string, GameObject> portraitSelection;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] _choices;
    private TextMeshProUGUI[] _choicesText;

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
    private const string PORTRAIT_TAG = "portrait";
    private const string SPEAKER_TAG = "speaker";
    private const string ANIMATION_TAG = "animation";


    public static DialogueManager GetInstance()
    {
        return _instance;
    }

    #region Monobehaviour Methods

    private void Awake()
    {
        InitializeDM();
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

        portraitSelection = new Dictionary<string, GameObject>() // Define the dictionary used to select current portrait.
        {
            {"PortraitL1", _portraits[0]},
            {"PortraitL2", _portraits[1]},
            {"PortraitL3", _portraits[2]},
            {"PortraitR1", _portraits[3]},
            {"PortraitR2", _portraits[4]},
            {"PortraitR3", _portraits[5]}
        };
    }
    #endregion

    #region Game State Methods
    public void EnterDialogueMode(TextAsset inkJson)
    {
        ResetAll();
        _currentStory = new Story(inkJson.text);
        _dialogueIsPlaying = true;
        _dialoguePanel.SetActive(true);
        SwitchGameState();
        // Change game state to dialogue/menu in order to disable character control & switch control scheme?
        DialogueContinue();
    }

    private void ExitDialogueMode()
    {
        _dialogueIsPlaying = false;
        _dialoguePanel.SetActive(false);
        _dialogueText.text = "";
        SwitchGameState();
        // Change game state in order to re-enable character control & switch control scheme?
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
        foreach(GameObject portrait in _portraits)
        {
            portrait.SetActive(false);
        }
    }

    private void ResetTags()
    {
        _nameTextLeft.text = "";
        _nameTextRight.text = "";
        _currentPortrait = _portraits[0];
        _activeSide = "R";
        _activeNameText = _nameTextRight;
    }
    #endregion

    private void DialogueContinue()
    {
        if (_currentStory.canContinue)
        {

            if (_displayLineCoroutine != null)
            {
                StopCoroutine(_displayLineCoroutine);
            }

            // Type out the current dialogue line using the DisplayLine coroutine
            _displayLineCoroutine = StartCoroutine(DisplayLine(_currentStory.Continue()));

            // Handle Tags
            HandleTags(_currentStory.currentTags);

        } else
        {
            ExitDialogueMode();
        }        
    }

    #region Tag Parsing
    private void HandleTags(List<string> currentTags)
    {
        // Loop through each tag and handle accordingly
        foreach (string tag in currentTags)
        {
            // Parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                _dialogueError = true;
                _dialogueText.text = "Tag could not be appropriately parsed: " + tag.ToString();
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
                return;
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            // Handle the tag
            switch (tagKey)
            {
                case SIDE_TAG:
                    if (tagValue == "L")
                    {
                        _activeSide = "L";
                        _activeNameText = _nameTextLeft;
                        _namePanelLeft.SetActive(true);
                        _namePanelRight.SetActive(false);
                    } else if (tagValue == "R")
                    {
                        _activeSide = "R";
                        _activeNameText = _nameTextRight;
                        _namePanelRight.SetActive(true);
                        _namePanelLeft.SetActive(false);
                    }
                    break;
                case PORTRAIT_TAG:
                    _currentPortrait = portraitSelection["Portrait" + _activeSide + tagValue];
                    _currentPortrait.SetActive(true);
                    ChangePortraitFocus(_currentPortrait);
                    break;
                case SPEAKER_TAG:
                    _activeNameText.text = tagValue;
                    break;
                case ANIMATION_TAG:
                    _currentPortrait.SetActive(true);
                    _currentPortrait.GetComponent<Animator>().Play(tagValue);
                    break;
                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }
    #endregion

    private IEnumerator DisplayLine(string line)
    {
        _dialogueText.text = line; // Set the dialogue text to the full line
        _dialogueText.maxVisibleCharacters = 0; // Set visible characters to 0
        
        _continueIcon.SetActive(false); // Hide continue icon while text is typing
        HideChoices();

        _canContinueToNextLine = false; // Prevents player from continuing to next line before text is fully displayed.

        bool isAddingRichTextTag = false;

        // Display each letter one at a time
        foreach (char letter in line.ToCharArray())
        {
            if (_skipLine) // If _skipLine has been triggered
            {
                _dialogueText.maxVisibleCharacters = line.Length; // Reveal the whole line at once
                _skipLine = false; // Reset _skipLine
                break; // Exit the foreach loop
            }

            if (letter == '<' || isAddingRichTextTag) // Check for RTT opening character or isAddingRichTextTag variable
            {
                isAddingRichTextTag = true;
                if (letter == '>') // Check for tag closing
                {
                    isAddingRichTextTag = false;
                }
            } else // If not adding rich text, add the next letter and wait _typingSpeed
            {
                _dialogueText.maxVisibleCharacters++;
                yield return new WaitForSeconds(_typingSpeed);
            }
        }

        _continueIcon.SetActive(true); // Display continue icon when text is done typing
        DisplayChoices(); // Display choices, if any, that respond to this dialogue line.
        _canContinueToNextLine = true; // Allows player to continue to next line
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = _currentStory.currentChoices;

        // Defensive check to ensure our UI can support the number of incoming choices.
        if (currentChoices.Count > _choices.Length)
        {
            _dialogueError = true;
            _dialogueText.text = "Error: More choices were given than UI can support.";
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

    private void ChangePortraitFocus(GameObject currentPortrait)
    {
        int index = 0;
        foreach(GameObject portrait in _portraits)
        {
            if(portrait.activeSelf)
                {
                    Debug.Log(portrait + " is active.");
                    if (portrait == _currentPortrait)
                    {
                        Debug.Log(portrait + " is current portrait");
                        portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,1);
                    } else
                    {
                        Debug.Log(portrait + " is NOT current portrait");
                        portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,0.25f);
                    }
                }
            index++;
        }
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
        if (_canContinueToNextLine)
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
            _skipLine = true;
        }
    }
    #endregion
}