using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// This class is used for Events to toggle the interaction UI.
/// Example: Display or hide the interaction UI via a bool and the interaction type from the enum via int
/// </summary>

[CreateAssetMenu(menuName = "Events/Interaction UI Event Channel")]
public class InteractionUIEventChannelSO : DescriptionBaseSO
{
	public UnityAction<bool, InteractionType, GameObject> OnEventRaised;

	public void RaiseEvent(bool state, InteractionType interactionType, GameObject subject)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(state, interactionType, subject);
	}
}

