using UnityEngine;
using UnityEngine.Events;
using Ink.Runtime;

[CreateAssetMenu(menuName = "Events/UI/Dialogue Choice Channel")]
public class DialogueChoiceChannelSO : ScriptableObject
{
	public UnityAction<int> OnEventRaised; // Likely needs to pass the choice parameter from current Ink story instead
	public void RaiseEvent(int choiceIndex)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(choiceIndex);
	}
}
