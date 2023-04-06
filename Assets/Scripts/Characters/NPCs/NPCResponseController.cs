using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script governs an NPC's response to player interaction, and interfaces with the quest system.

public class NPCResponseController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] [ReadOnly] private NPCManager _NPCManager = default;
    //[SerializeField] private QuestManagerSO _questData = default;
    [SerializeField] private GameStateSO _gameStateManager = default;
    [SerializeField] [ReadOnly] private DialogueDataSO _defaultDialogue;
    [SerializeField] [ReadOnly] private DialogueDataSO _currentDialogue;

    [Header("Broadcasting On")]
    public DialogueDataChannelSO _startDialogueEvent = default;

    [Header("Listening To")]
    [SerializeField] private IntEventChannelSO _endDialogueEvent = default;

    [Header("Parameters")]
    public bool isInDialogue; // Consumed by NPC State Machine

    void Awake()
    {
        _NPCManager = gameObject.GetComponent<NPCManager>();

        if (_NPCManager.NPCID != null)
        {
            _defaultDialogue = _NPCManager.NPCID.DefaultDialogue; // Set Default Dialogue to the DD contained in the assigned CharIdentitySO
            if (_defaultDialogue == null) // If null, throw error
            {
                Debug.LogError("Couldn't find a default dialogue for the following NPC ID: " + _NPCManager.NPCID);
            }
        }
    }

    void PlayDefaultDialogue()
    {
        if (_defaultDialogue != null)
        {
            _currentDialogue = _defaultDialogue; // Assign default dialogue to current dialogue
            StartDialogue();
        }
    }

    // Called by the Player Character's Interaction Manager script.
    public void InteractWithCharacter()
    {
        if (_gameStateManager.CurrentGameState == GameState.GameplayTown) // This line needs an update once GameplayDungeon state is implemented.
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
        _endDialogueEvent.OnEventRaised += EndDialogue;
        //StopConversation(); // If NPC is in conversation with a 2nd NPC, abort that conversation to talk to Player.
        isInDialogue = true;
    }

    void EndDialogue(int dialogueType)
    {
        _endDialogueEvent.OnEventRaised -= EndDialogue;
        //ResumeConversation(); // If the NPC was in conversation with a 2nd NPC, resume that conversation once done talking to Player.
        isInDialogue = false;
    }


    private void StopConversation()
    {
        GameObject[] talkingTo = gameObject.GetComponent<NPC>().talkingTo;
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
        GameObject[] talkingTo = GetComponent<NPC>().talkingTo; // Copy an array called TalkingTo from the NPC script, and recreate it here.
        if (talkingTo != null)
        {
            for (int i = 0; i < talkingTo.Length; ++i) 
            {
                talkingTo[i].GetComponent<NPC>().npcState = NPCState.Talk;
            }
        }
    }
}
