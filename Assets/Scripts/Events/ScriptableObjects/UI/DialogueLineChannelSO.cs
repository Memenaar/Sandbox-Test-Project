using UnityEngine;
using UnityEngine.Events;
using Ink.Runtime;

[CreateAssetMenu(menuName = "Events/UI/Dialogue Line Channel")]
public class DialogueLineChannelSO : DescriptionBaseSO
{
	public UnityAction<string, CharIdentitySO> OnEventRaised;

	public void RaiseEvent(string line, CharIdentitySO charID)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(line, charID);
	}
}
