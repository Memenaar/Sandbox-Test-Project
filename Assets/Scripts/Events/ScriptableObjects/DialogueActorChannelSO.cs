using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// This class is used for talk interaction events.
/// Example: start talking to an actor passed as paramater
/// </summary>

[CreateAssetMenu(menuName = "Events/Dialogue Actor Channel")]
public class DialogueActorChannelSO : DescriptionBaseSO
{
	public UnityAction<CharIdentitySO> OnEventRaised;
	
	public void RaiseEvent(CharIdentitySO charID)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(charID);
	}
}

