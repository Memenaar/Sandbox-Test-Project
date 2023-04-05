using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Ink.Runtime;


public class UIManager : MonoBehaviour
{
    [Header("Scene UI")]
    [SerializeField] private UIDialogueManager _dialogueController = default;

    [Header("Gameplay")]
    [SerializeField] private GameStateSO _gameStateManager = default;
    [SerializeField] private InputReader _inputReader = default;

    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;

    [Header("Dialogue Events")]
    [SerializeField] private DialogueLineChannelSO _openUIDialogueEvent = default;
    [SerializeField] private IntEventChannelSO _closeUIDialogueEvent = default;

    [Header("Interaction Events")]
    [SerializeField] private InteractionUIEventChannelSO _setInteractionEvent = default;

    [Header("Broadcasting On")]
    [SerializeField] private LoadEventChannelSO _loadMenuEvent = default;
    [SerializeField] private VoidEventChannelSO _onInteractionEndedEvent = default;

    private void OnEnable()
    {
        //_onSceneReady.OnEventRaised += ResetUI;
        _openUIDialogueEvent.OnEventRaised += OpenUIDialogue;
        //_closeUIDialogueEvent.OnEventRaised += CloseUIDialogue;
        _setInteractionEvent.OnEventRaised += SetInteractionUI;
    }

    private void OnDisable()
    {
        //_onSceneReady.OnEventRaised -= ResetUI;
        _openUIDialogueEvent.OnEventRaised -= OpenUIDialogue;
        //_closeUIDialogueEvent.OnEventRaised -= CloseUIDialogue;
        _setInteractionEvent.OnEventRaised -= SetInteractionUI;
    }

    void OpenUIDialogue(string dialogueLine, CharIdentitySO speaker)
    {
        _dialogueController.gameObject.SetActive(true);
        // Set appropriate Speaker nameplate and text based on tags (may need to pass an extra parameter or two to this function)
        // Set appropriate Portrait and resolve status of other portraits (may need to pass an extra parameter)
        _dialogueController.SetDialogue(dialogueLine, speaker);
        // Set Interaction Pip UI to OFF here
    }

    void CloseUIDialogue(int dialogueType)
    {
        _dialogueController.gameObject.SetActive(false);
        _onInteractionEndedEvent.RaiseEvent();
    }

    void SetInteractionUI(bool isOpen, InteractionType interactionType, GameObject subject)
    {
        GameObject interactUI;
        if (subject != null)
        {
            if (subject.transform.Find("InteractUI").gameObject != null)
            {
                interactUI = subject.transform.Find("InteractUI").gameObject;
            } else interactUI = null;
        } else return;
    
        if (interactUI != null)
        {
            if (_gameStateManager.CurrentGameState == GameState.GameplayTown)
            {
                if (isOpen)
                {

                }

                interactUI.SetActive(isOpen);

            } else if (!isOpen)
            {
               interactUI.SetActive(false); 
            }
        } else 
        {
            Debug.LogError("InteractUI child object could not be found for Game Object " + subject);
        }
    }
}
