using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/UI/Dialogue Line Channel")]
public class DialogueLineChannelSO : DescriptionBaseSO
{
	public UnityAction<string> OnEventRaised;

	public void RaiseEvent(string line)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(line);
	}
}
