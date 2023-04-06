using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "newCallsheet", menuName = "Identities/Callsheet")]

public class CallsheetSO : DescriptionBaseSO
{
    [SerializeField] private List<CharIdentitySO> _charactersList = default;
    public List<CharIdentitySO> CharactersList { get { return _charactersList; }}

    public CharIdentitySO CallTime(string charName)
    {
        CharIdentitySO calledChar = _charactersList.Find(o => o.CharID.ToString() == charName);
        return calledChar;
    }
}
