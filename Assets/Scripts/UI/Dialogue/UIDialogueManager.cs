using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dialogueText = default;
	[SerializeField] private GameObject _dialoguePanel = default;
	[SerializeField] private TextMeshProUGUI _nameTextLeft = default;
	[SerializeField] private GameObject _namePanelLeft = default;
	[SerializeField] private TextMeshProUGUI _nameTextRight = default;
	[SerializeField] private GameObject _namePanelRight = default;
	[SerializeField] private GameObject[] _leftPortraits;
	[SerializeField] private GameObject[] _rightPortraits;

	[SerializeField] private GameObject _continueIcon;
	
	[SerializeField] private UIDialogueChoicesManager _choicesManager = default;

	[Header("Listening to")]
	[SerializeField] private DialogueChoicesChannelSO _showChoicesEvent = default;

	[Header("Parameters")]
    [SerializeField] private float _typingSpeed = 0.04f;

	private bool _skipLine = false;

	private bool _canContinueToNextLine = false;

	private Coroutine _displayLineCoroutine;

	public TextMeshProUGUI DialogueText { get { return _dialogueText; } set { _dialogueText = value; }}
	public bool SkipLine { get { return _skipLine; } set { _skipLine = value; }}
	public bool CanContinueToNextLine { get { return _canContinueToNextLine; } set { _canContinueToNextLine = value; }}


	private void OnEnable()
	{
		//_showChoicesEvent.OnEventRaised += ShowChoices;
	}

	private void OnDisable()
	{
		//_showChoicesEvent.OnEventRaised -= ShowChoices;
	}

	public void SetDialogue(string dialogueLine, CharIdentitySO speaker)
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

}
