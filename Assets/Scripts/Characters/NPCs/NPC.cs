using System.Collections;
using UnityEngine;

public enum NPCState
{
    Idle = 0,
    Walk,
    Talk
};

public class NPC : MonoBehaviour
{
    public NPCState npcState;
    public GameObject[] talkingTo;

    public void Awake()
    {

    }
}
