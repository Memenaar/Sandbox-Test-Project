using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum InteractionType { None = 0, PickUp, Talk };

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader = default;
    [SerializeField] private PlayerStateMachine _PSM = default;

    // Events for the different Interaction types
    [Header("Broadcasting On")]
    //[SerializeField] private ItemEventChannelSO _onObjectPickUp = default; -- Implement this later once we have items that can be picked up
    [SerializeField] private DialogueActorChannelSO _startTalking = default;
    [SerializeField] private InteractionUIEventChannelSO _toggleInteractionUI = default;

    [Header("Listening To")]
    [SerializeField] private VoidEventChannelSO _onInteractionEnded = default;

    [ReadOnly] public InteractionType currentInteractionType; // This is checked/consumed by conditions in methods below

    private LinkedList<Interaction> _potentialInteractions = new LinkedList<Interaction>(); // Stores the objects the player could potentially interact with

    private void OnEnable()
    {
        //_inputReader.PlayerInteractEvent += OnInteractionButtonPress; // Will move receipt of Player Interaction input here from PlayerStateMachine when ready
        _inputReader.PlayerInteractEvent += OnInteractionButtonPress;
        //_onInteractionEnded.OnEventRaised += OnInteractionEnd;
    }

    private void OnDisable()
    {
        //_inputReader.PlayerInteractEvent -= OnInteractionButtonPress; // Will move receipt of Player Interaction input here from PlayerStateMachine when ready\
        _inputReader.PlayerInteractEvent -= OnInteractionButtonPress;
        //_onInteractionEnded.OnEventRaised -= OnInteractionEnd;
    }

    private void Collect()
    {
        // Stub for interactions that give the player an item.
    }

    private void OnInteractionButtonPress()
    {
        if (_potentialInteractions.Count == 0)
            return;

        currentInteractionType = _potentialInteractions.First.Value.type;

        switch (_potentialInteractions.First.Value.type)
        {
            case InteractionType.Talk:
                if (_startTalking != null && _PSM.CurrentSuperState == _PSM.Factory.Grounded())
                {
                    FacePlayer(_potentialInteractions.First.Value.interactableObject);
                    _potentialInteractions.First.Value.interactableObject.GetComponent<NPCResponseController>().InteractWithCharacter();
                    //_inputReader.EnableDialogueInput();
                }
                break;

            // Original script does not implement a Case for the pickup type, instead handling it through the State Machine, then calling the Collect() function through the AnmimationClip.
            // Likely will change this.
        }
    }

	private void OnInteractionEnd()
	{
		switch (currentInteractionType)
		{
			case InteractionType.Talk:
				//We show the UI after repeatable interactions like talking, in case player wants to interact again
				RequestUpdateUI(true, _potentialInteractions.First.Value.interactableObject);
				break;
		}

		//_inputReader.EnableGameplayInput(); This might be handled elsewhere?
	}

    // This method displays the interaction UI. Need to alter so it enables the interact prompt above the subject.
    private void RequestUpdateUI(bool visible, GameObject subject)
	{
		if (visible)
			_toggleInteractionUI.RaiseEvent(true, _potentialInteractions.First.Value.type, subject);
		else
			_toggleInteractionUI.RaiseEvent(false, InteractionType.None, subject);
	}

    //Called by the Event on the trigger collider on the child GO called "InteractionDetector"
	public void OnTriggerChangeDetected(bool entered, GameObject obj)
	{
        if (entered)
			AddPotentialInteraction(obj);
		else
			RemovePotentialInteraction(obj);
	}

	private void AddPotentialInteraction(GameObject obj)
	{
		Interaction newInteraction = new Interaction(InteractionType.None, obj);

		if (obj.CompareTag("NPC"))
		{
			newInteraction.type = InteractionType.Talk;
		}
		//else if (obj.CompareTag("Pickable"))
		//{
		//	newPotentialInteraction.type = InteractionType.PickUp;
		//}

		if (newInteraction.type != InteractionType.None)
		{
			_potentialInteractions.AddFirst(newInteraction);
			RequestUpdateUI(true, obj);
		}
	}

	private void RemovePotentialInteraction(GameObject obj)
	{
		LinkedListNode<Interaction> currentNode = _potentialInteractions.First;
		while (currentNode != null)
		{
			if (currentNode.Value.interactableObject == obj)
			{
				_potentialInteractions.Remove(currentNode);
				break;
			}
			currentNode = currentNode.Next;
		}
        
		RequestUpdateUI(_potentialInteractions.Count > 0, obj);
	}

    private void FacePlayer(GameObject subject)
    {
        subject.transform.Find("Navigator").LookAt(this.transform); 
    }

}
