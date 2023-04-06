using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/UI/Dialogue UI Channel")]
public class DialogueUIChannelSO : DescriptionBaseSO
{
	public UnityAction<List<string>> OnEventRaised;

	public void RaiseEvent(List<string> currentTags)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(currentTags);
	}
}
