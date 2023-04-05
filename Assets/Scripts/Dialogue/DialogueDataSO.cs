using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum DialogueType
{
    StartDialogue,
    QuestCompleteDialogue,
    QuestIncompleteDialogue,
    DefaultDialogue
}

public enum ChoiceActionType
{
    DoNothing,
}


/// <summary>
/// Dialogue is contained in Ink files and played in sequence using the player's input to skip forward or make choices where necessary.
/// </summary>

[CreateAssetMenu(fileName = "newDialogue", menuName = "Dialogues/Dialogue Data")]

public class DialogueDataSO : ScriptableObject
{
    [SerializeField] private string _dialogueName;
    [SerializeField] private TextAsset _dialogueInk = default;
    [SerializeField] private DialogueType _dialogueType = default;
    [SerializeField] private VoidEventChannelSO _endOfDialogueEvent = default;

    public string DialogueName { get { return _dialogueName; }}
    public VoidEventChannelSO EndOfDialogueEvent => _endOfDialogueEvent;
    public TextAsset DialogueInk => _dialogueInk;

    public DialogueType DialogueType { get { return _dialogueType; } set { _dialogueType = value; }}

    public void FinishDialogue()
    {
        if (EndOfDialogueEvent != null) EndOfDialogueEvent.RaiseEvent();
    }


}
