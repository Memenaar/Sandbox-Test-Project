using UnityEngine;

/// <summary>
/// Base class for ScriptableObjects that hold a PC, NPC, Tulpa, or Enemy Identity.
/// </summary>
public class IdentityBaseSO : SerializableScriptableObject
{
	public IdentityType _idType; // Determines ID Type
}
