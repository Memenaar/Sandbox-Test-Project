using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

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
	

    [Header("Value Holders")]
    [SerializeField] [ReadOnly] private string _activeSide;
    [SerializeField] [ReadOnly] private string _currentSpeakerText;
    [SerializeField] [ReadOnly] private TextMeshProUGUI _activeNameText;
	[SerializeField] [ReadOnly] private GameObject _activeNamePanel;
    [SerializeField] [ReadOnly] private GameObject _activePortraitPanel;
    [SerializeField] [ReadOnly] private GameObject[] _activePortraits;
    [SerializeField] [ReadOnly] public List<GameObject> _currentPortraits;   
	[SerializeField] [ReadOnly] private List<GameObject> _allPortraits;
    [SerializeField] [ReadOnly] private List<CharIdentitySO> _currentSpeakers;
	
	[SerializeField] private UIDialogueChoicesManager _choicesManager = default;

	[Header("Listening to")]
	[SerializeField] private DialogueChoicesChannelSO _showChoicesEvent = default;
	[SerializeField] private DialogueUIChannelSO _changeDialogueUIEvent = default;

	[Header("Parameters")]
    [SerializeField] private float _typingSpeed = 0.04f;

	private bool _skipLine = false;

	private bool _canContinueToNextLine = false;

	private Coroutine _displayLineCoroutine;

	[Header("Tag Keys")]
    private const string SIDE_TAG = "side";
    private const string LAYOUT_TAG = "layout";
    private const string POSITION_TAG = "position";
	private const string STYLE_TAG = "style";
    private const string SPEAKER_TAG = "speaker";
    private const string ANIMATION_TAG = "animation";


	public TextMeshProUGUI DialogueText { get { return _dialogueText; } set { _dialogueText = value; }}
	public bool SkipLine { get { return _skipLine; } set { _skipLine = value; }}
	public bool CanContinueToNextLine { get { return _canContinueToNextLine; } set { _canContinueToNextLine = value; }}

	private void Awake()
	{
		_dialogueManager = DialogueManager.GetInstance();
		CollectPortraits();
	}

	private void OnEnable()
	{
		//_showChoicesEvent.OnEventRaised += ShowChoices;
		_changeDialogueUIEvent.OnEventRaised += ProcessTags;
	}

	private void OnDisable()
	{
		//_showChoicesEvent.OnEventRaised -= ShowChoices;
		_changeDialogueUIEvent.OnEventRaised -= ProcessTags;
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

	public void SetDialogue(string dialogueLine)
	{
		_choicesManager.gameObject.SetActive(false); // Does this belong here? Or should it go elsewhere?

		if(_displayLineCoroutine != null)
		{
			StopCoroutine(_displayLineCoroutine);
		}

		// Type out the current dialogue line using the DisplayLine coroutine.
		_displayLineCoroutine = StartCoroutine(DisplayLine(dialogueLine));

	}

    private void ShowChoices()
    {
        //_choicesManager.FillChoices(choices);
        //_choicesManager.gameObject.SetActive(true);
    }

    private void HideChoices()
    {
        _choicesManager.gameObject.SetActive(false);
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
		// DisplayChoices(); Display choices, if any, that respond to this dialogue line.
		_canContinueToNextLine = true; // Allows player to continue to next line.
	}

	private void ProcessTags(List<string> currentTags)
	{
		int i = 0;
        // Loop through and split each tag
        foreach (string tag in currentTags)
        {
            // Parse the tag
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
				SpitError("Tag could not be appropriately parsed: ", tag);
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
					}
					break;
				case LAYOUT_TAG:
					_activePortraitPanel.GetComponent<Animator>().Play(tagValue);
					break;
				case POSITION_TAG:
					int positionIndex = int.Parse(tagValue);
					_currentPortraits.Insert(i, _activePortraits[positionIndex]);
					_currentPortraits[i].SetActive(true);
					break;
				case STYLE_TAG:
					// Apply Style from tagValue to _activeNamePanel and _currentPortraits[i]
					break;
				case SPEAKER_TAG:
					CharIdentitySO calledChar = DialogueManager.GetInstance().Callsheet.CallTime(tagValue);
					_currentSpeakers.Add(calledChar);
					if (i == 0) _currentSpeakerText = _currentSpeakers[i].CharName;
					else _currentSpeakerText += " & " + _currentSpeakers[i].CharName; 
					_activeNameText.text = _currentSpeakerText;
					break;
				case ANIMATION_TAG:
					_currentPortraits[i].GetComponent<Animator>().runtimeAnimatorController = _currentSpeakers[i].PortraitController;
					_currentPortraits[i].GetComponent<Animator>().Play(tagValue);
					i++;
					break;
				default:
					SpitError("Tag came in with unrecognized tagKey ", tagKey);
					break;
			}
		}

		ChangePortraitFocus(_currentPortraits);
	}

	private void ChangePortraitFocus(List<GameObject> currentPortraits)
    {
		List<GameObject> inactivePortraits = _allPortraits;

		foreach (GameObject portrait in currentPortraits)
		{
			inactivePortraits.Remove(portrait);
		}

		foreach(GameObject portrait in inactivePortraits)
		{
			Debug.Log(portrait + " is NOT current portrait");
			portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,0.25f);
		}
	
		foreach(GameObject portrait in currentPortraits)
		{
			Debug.Log(portrait + " is current portrait");
			portrait.GetComponent<Image>().color = Color.HSVToRGB(0,0,1);
		}
    }

	private void SpitError(string errorText, string errorCause)
	{
		_dialogueManager.DialogueError = true;
		_dialogueText.text = errorText + errorCause;
		Debug.LogWarning(errorText + errorCause);
	}

}
