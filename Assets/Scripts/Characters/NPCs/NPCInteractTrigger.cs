using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractTrigger : MonoBehaviour
{

    [SerializeField]
    public GameObject _speechBubble;
    public NPCManager _npcManager;
    public GameStateSO _gameState = default;

    void Awake()
    {
        _npcManager = GetComponentInParent<NPCManager>();
    }

    void Update()
    {
        if (_gameState.CurrentGameState == GameState.Dialogue)
        {
            _speechBubble.SetActive(false);
        } else if (_npcManager.PlayerProximity)
        {
            _speechBubble.SetActive(true);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _npcManager.PlayerProximity = true;
            _speechBubble.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _npcManager.PlayerProximity = false;
            _speechBubble.SetActive(false);
        }
    }
}
