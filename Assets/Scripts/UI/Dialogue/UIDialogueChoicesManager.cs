using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Ink.Runtime;
using TMPro;

public class UIDialogueChoicesManager : MonoBehaviour
{
    [SerializeField] private UIDialogueManager _uiDialogueManager;

    [Header("Broadcasting On")]
    [SerializeField] private DialogueChoiceChannelSO _onChoiceMade = default;

    [SerializeField] private GameObject[] _choiceButtons;
    private TextMeshProUGUI[] _choicesText;
    
    
    private void Awake()
    {
    }
    
    private void Start()
    {
        InitializeChoices();
    }

    public void DisplayChoices(List<Choice> currentChoices)
    {
        if (currentChoices != null)
        {
            if (currentChoices.Count > _choiceButtons.Length)
            {
                //_dialogueError = true;
                //_dialogueText.text = "Error: More choices were given than UI can support.";
                Debug.LogError("More choices were given than UI can support.");
                return;
            }

            int index = 0;

            foreach(Choice choice in currentChoices)
            {
                _choiceButtons[index].gameObject.SetActive(true);
                _choicesText[index].text = choice.text;
                index++;
            }

            for (int i = index; i < _choiceButtons.Length; i++)
            {
                _choiceButtons[i].gameObject.SetActive(false);
            }

            StartCoroutine(SelectFirstChoice());
        }
    }

    public void InitializeChoices()
    {
        _choicesText = new TextMeshProUGUI[_choiceButtons.Length];
        int index = 0;
        foreach (GameObject choice in _choiceButtons)
        {
            _choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void HideChoices()
    {
        foreach (GameObject choice in _choiceButtons)
        {
            choice.SetActive(false);
        }
    }

    private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(_choiceButtons[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        if (_uiDialogueManager.CanContinueToNextLine)
        {
            _onChoiceMade.RaiseEvent(choiceIndex);
        }
    }
}
