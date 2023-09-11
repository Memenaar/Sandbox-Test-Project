using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;
using Ink.Runtime;

public class UIDialogueManager : MonoBehaviour
{
	[SerializeField] [ReadOnly] private DialogueManager _dialogueManager;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private GameObject _continueIcon;
    [SerializeField] private GameObject _namePanelLeft;
    [SerializeField] private GameObject _namePanelRight;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _nameTextLeft;
    [SerializeField] private TextMeshProUGUI _nameTextRight;
    [SerializeField] private GameObject _leftPortraitPanel;
    [SerializeField] private GameObject _rightPortraitPanel;
    [SerializeField] private GameObject[] _leftPortraits;
    [SerializeField] private GameObject[] _rightPortraits;
	
	[Header("Choices UI")]
	[SerializeField] private GameObject[] _choices;
	private TextMeshProUGUI[] _choicesText;

    [Header("Value Holders")]
    [SerializeField] [ReadOnly] private string _activeSide;
    [SerializeField] [ReadOnly] private string _currentSpeakerText;
    [SerializeField] [ReadOnly] private TextMeshProUGUI _activeNameText;
	[SerializeField] [ReadOnly] private GameObject _activeNamePanel;
    [SerializeField] [ReadOnly] private GameObject _activePortraitPanel;
    [SerializeField] [ReadOnly] private GameObject[] _activePortraits;
	[SerializeField] [ReadOnly] private GameObject _targetPortrait;
    [SerializeField] [ReadOnly] public List<GameObject> _currentPortraits;   
	[SerializeField] [ReadOnly] private List<GameObject> _allPortraits;
    [SerializeField] [ReadOnly] private List<CharIdentitySO> _currentSpeakers;
	[SerializeField] [ReadOnly] private List<Choice> _currentChoices;
	[SerializeField] [ReadOnly] List<GameObject> inactivePortraits;
	
	[SerializeField] private UIDialogueChoicesManager _choicesManager = default;

	[Header("Listening to")]
	[SerializeField] private DialogueChoicesChannelSO _showChoicesEvent = default;
	[SerializeField] private DialogueUIChannelSO _changeDialogueUIEvent = default;

	[Header("Parameters")]
    [SerializeField] private float _typingSpeed = 0.04f;

	private bool _skipLine = false;

	private bool _canContinueToNextLine = false;

	private Coroutine _displayLineCoroutine;

	private bool _abortParse = false;

	[Header("Tag Keys")]
    private const string SIDE_TAG = "side";
    private const string LAYOUT_TAG = "layout";
    private const string POSITION_TAG = "position";
	private const string STYLE_TAG = "style";
    private const string SPEAKER_TAG = "speaker";
    private const string ANIMATION_TAG = "animation";
	private const string NAMEOVERRIDE_TAG = "name";
	private const string DISABLE_TAG = "disable";


	public TextMeshProUGUI DialogueText { get { return _dialogueText; } set { _dialogueText = value; }}
	public bool SkipLine { get { return _skipLine; } set { _skipLine = value; }}
	public bool CanContinueToNextLine { get { return _canContinueToNextLine; } set { _canContinueToNextLine = value; }}

	private void Awake()
	{
		_dialogueManager = DialogueManager.GetInstance();
		InitializeChoices();
		CollectPortraits();
	}

	private void OnEnable()
	{
		_showChoicesEvent.OnEventRaised += CollectChoices;
		_changeDialogueUIEvent.OnEventRaised += ProcessTags;
	}

	private void OnDisable()
	{
		_showChoicesEvent.OnEventRaised -= CollectChoices;
		_changeDialogueUIEvent.OnEventRaised -= ProcessTags;
	}

	private void InitializeChoices()
	{
		_choicesText = new TextMeshProUGUI[_choices.Length];
        int index = 0;
        foreach (GameObject choice in _choices)
        {
            _choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
	}

	public void CollectPortraits()
	{
		foreach (GameObject portrait in _leftPortraits)
		{
			_allPortraits.Add(portrait);
		}
		foreach (GameObject portrait in _rightPortraits)
		{
			_allPortraits.Add(portrait);
		}
	}

	public void CollectChoices(List<Choice> choices)
	{
		_currentChoices = choices;
	}

	public void SetDialogue(string dialogueLine)
	{
		if(_displayLineCoroutine != null)
		{
			StopCoroutine(_displayLineCoroutine);
		}

		// Type out the current dialogue line using the DisplayLine coroutine.
		_displayLineCoroutine = StartCoroutine(DisplayLine(dialogueLine));

	}

	public void SetError(string errorLine)
	{
		if(_displayLineCoroutine != null)
		{
			StopCoroutine(_displayLineCoroutine);
		}

		_dialogueText.text = errorLine;
		_dialogueText.maxVisibleCharacters = errorLine.Length;
		_dialogueManager.DialogueError = true;
		_abortParse = true;

	}

    private void ShowChoices()
    {
        //_choicesManager.FillChoices(choices);
        //_choicesManager.gameObject.SetActive(true);
    }

	private IEnumerator DisplayLine(string line)
	{
		_dialogueText.text = line; // Set the dialogue text to the full line
        _dialogueText.maxVisibleCharacters = 0; // Set visible characters to 0

		_continueIcon.SetActive(false);
		HideChoices();

		_canContinueToNextLine = false;

		bool isAddingRichTextTag = false;

		// Display each letter one at a time
		foreach (char letter in line.ToCharArray())
		{
			if (_skipLine) // If _skipLine has been triggered
			{
				_dialogueText.maxVisibleCharacters = line.Length; // Reveal the whole line at once
				_skipLine = false; // Reset _skipLine
				break; // Exit the loop
			}

			if (letter == '<' || isAddingRichTextTag) // Check for RTT opening char or whether RTT is in progress
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

		_continueIcon.SetActive(true);
		DisplayChoices();
		_canContinueToNextLine = true; // Allows player to continue to next line.
	}

	public void ResetAll()
	{
		ResetTags();
		ResetUI();
	}

    private void ResetUI()
    {
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
		_currentSpeakerText = "";
		_currentSpeakers.RemoveRange(0, _currentSpeakers.Count);
        _currentPortraits.RemoveRange(0, _currentPortraits.Count);
    }

	private void ProcessTags(List<string> currentTags)
	{
        // Loop through and split each tag
        foreach (string tag in currentTags)
        {
			if (_abortParse)
			{
				_abortParse = false;
				return;
			}

            // Parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
				SpitError("ERROR: Tag could not be appropriately parsed: ", tag);
                return;
            }
            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

			switch (tagKey)
			{
				case SIDE_TAG:
					if (tagValue == "left")
					{
						_activeNameText = _nameTextLeft;
						_namePanelLeft.SetActive(true);
						_namePanelRight.SetActive(false);
						_activePortraits = _leftPortraits;	
						_activePortraitPanel = _leftPortraitPanel;
					}else if (tagValue == "right")
					{
						_activeNameText = _nameTextRight;
						_namePanelRight.SetActive(true);
						_namePanelLeft.SetActive(false);
						_activePortraits = _rightPortraits;
						_activePortraitPanel = _rightPortraitPanel;
					} else SpitError("ERROR: Unrecognized SIDE_TAG value ", tagValue);
					break;
				case LAYOUT_TAG:
					_activePortraitPanel.GetComponent<Animator>().Play(tagValue);
					break;
				case POSITION_TAG:
					if (_currentPortraits.Count == 0) 
					{
						foreach (GameObject portrait in _allPortraits)
						{
							portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,0.25f);
						}	
					}
					int positionIndex = int.Parse(tagValue);
					_targetPortrait = _activePortraits[positionIndex];
					_targetPortrait.SetActive(true);
					_targetPortrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,1);
					_currentPortraits.Add(_targetPortrait);
					break;
				case STYLE_TAG:
					// Apply Style from tagValue to _activeNamePanel and _currentPortraits[i]
					break;
				case SPEAKER_TAG:
					CharIdentitySO calledChar = DialogueManager.GetInstance().Callsheet.CallTime(tagValue);
					_currentSpeakers.Add(calledChar);
					_currentSpeakerText = _currentSpeakers[0].CharName;
					_targetPortrait.GetComponent<Animator>().runtimeAnimatorController = _currentSpeakers[0].PortraitController;
					if (_currentSpeakers.Count > 1) 
					{
						int i = 1;
						foreach (CharIdentitySO character in _currentSpeakers.Skip(1))
						{
							_currentSpeakerText += " & " + _currentSpeakers[i].CharName;
							_targetPortrait.GetComponent<Animator>().runtimeAnimatorController = _currentSpeakers[i].PortraitController;
							i++;
						}
					}
					_activeNameText.text = _currentSpeakerText;
					
					break;
				case NAMEOVERRIDE_TAG:
					_activeNameText.text = tagValue;
					break;
				case ANIMATION_TAG:
					_targetPortrait.GetComponent<Animator>().Play(tagValue);
					Debug.Log(tagValue);
					break;
				case DISABLE_TAG:
					int disableIndex = int.Parse(tagValue);
					_allPortraits[disableIndex].GetComponent<Animator>().runtimeAnimatorController = null;
					_allPortraits[disableIndex].SetActive(false);
					break;
				default:
					SpitError("ERROR: Tag came in with unrecognized tagKey ", tagKey);
					break;
			}
		}
		//ChangePortraitFocus(_currentPortraits); // Only kind of a fix. Will still fuck up if only the animation changes, as an example. Should be changed so that this line only runs if one or more Speaker tags were passed.
		ResetTags();
	}

	private void ChangePortraitFocus(List<GameObject> currentPortraits)
    {
		foreach (GameObject portrait in _allPortraits)
		{
			inactivePortraits.Add(portrait);
		}

		foreach (GameObject portrait in currentPortraits)
		{
			inactivePortraits.Remove(portrait);
		}

		foreach(GameObject portrait in inactivePortraits)
		{
			//Debug.Log("Changing color of " + portrait);
			portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,0.25f);
		}
	
		foreach(GameObject portrait in currentPortraits)
		{
			portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,1);
		}
    }

	private void SpitError(string errorText, string errorCause)
	{
		SetError(errorText + errorCause);
		Debug.LogWarning(errorText + errorCause);
	}

	private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(_choices[0].gameObject);
    }



	private void HideChoices()
    {
        foreach (GameObject choiceButton in _choices)
        {
            choiceButton.SetActive(false);
        }
    }

	private void DisplayChoices()
    {
		if (_currentChoices.Count == 0 || _currentChoices == null)
		{

			return;
		}
		
        // Defensive check to ensure our UI can support the number of incoming choices.
        if (_currentChoices.Count > _choices.Length)
        {
			SpitError("ERROR: More choices were given than UI can support.", "");
            return;
        }

        int index = 0;
        // Enable and initialize choice buttons for each available response in the UI.
        foreach(Choice choice in _currentChoices)
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

}
