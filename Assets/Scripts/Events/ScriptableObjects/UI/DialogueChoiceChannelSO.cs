using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/UI/Dialogue Choice Channel")]
public class DialogueChoiceChannelSO : ScriptableObject
{
	/*public UnityAction<Choice> OnEventRaised; // Likely needs to pass the choice parameter from current Ink story instead
	public void RaiseEvent(Choice choice)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(choice);
	}*/
}
