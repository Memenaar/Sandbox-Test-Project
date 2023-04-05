using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script governs an NPC's response to player interaction, and interfaces with the quest system.

public class NPCResponseController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharIdentitySO _NPCID = default;
    private DialogueDataSO _defaultDialogue;
    //[SerializeField] private QuestManagerSO _questData = default;
    [SerializeField] private GameStateSO _gameStateManager = default;

    [Header("Broadcasting On")]
    public DialogueDataChannelSO _startDialogueEvent = default;

    [Header("Listening To")]
    [SerializeField] private IntEventChannelSO _endDialogueEvent = default;

    private DialogueDataSO _currentDialogue;

    public bool isInDialogue; // Currently handled in the PlayerStateMachine by the _playerInteraction bool. Functionality should likely be migrated.

    void Awake()
    {
        if (_NPCID != null)
        {
            _defaultDialogue = _NPCID.DefaultDialogue;
            if (_defaultDialogue == null)
            {
                Debug.LogError("Couldn't find a default dialogue for the following NPC ID: " + _NPCID);
            }
        } else {}
    }

    void PlayDefaultDialogue()
    {
        if (_defaultDialogue != null)
        {
            
            _currentDialogue = _defaultDialogue;
            StartDialogue();
        }
    }

    public void InteractWithCharacter()
    {
        if (_gameStateManager.CurrentGameState == GameState.GameplayTown)
        {
            DialogueDataSO displayDialogue = null; // This line is placeholder. null value to be replaced by a check with the Quest Manager, with NPCID passed through.

            if (displayDialogue != null)
            {
                _currentDialogue = displayDialogue; // If the check with Quest Manager returns a dialogue, use that dialogue as the current dialogue.
                StartDialogue();
            } else
            {
                PlayDefaultDialogue(); // Otherwise play default dialogue
            }
        }
    }


    void StartDialogue()
    {
        _startDialogueEvent.RaiseEvent(_currentDialogue);
        //_endDialogueEvent.OnEventRaised += EndDialogue;
        //StopConversation(); // Little confused about this as it doesn't seem to work in the source project, and it's reversed? Investigate further.
        isInDialogue = true;
    }

    void EndDialogue()
    {
        //_endDialogueEvent.OnEventRaised -= EndDialogue;
        //ResumeConversation(); // Little confused about this as it doesn't seem to work in the source project, and it's reversed? Investigate further.
        isInDialogue = false;
    }


    private void StopConversation()
    {
        GameObject[] talkingTo = gameObject.GetComponent<NPC>().talkingTo; // New array of game objects called Talking To.
        if (talkingTo != null) // If not
        {
            for (int i = 0; i < talkingTo.Length; ++i)
            {
                talkingTo[i].GetComponent<NPC>().npcState = NPCState.Idle;
            }
        }
    }

    private void ResumeConversation()
    {
        GameObject[] talkingTo = GetComponent<NPC>().talkingTo;
        if (talkingTo != null)
        {
            for (int i = 0; i < talkingTo.Length; ++i)
            {
                talkingTo[i].GetComponent<NPC>().npcState = NPCState.Talk;
            }
        }
    }
}
