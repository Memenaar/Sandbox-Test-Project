using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteract : MonoBehaviour
{

    public bool playerProximity;
    private PlayerActions _playerActions;
    private GameObject _player;
    public Transform _navTransform;
    
    [SerializeField]
    private GameObject _speechBubble;

    void Awake()
    {
        _player = GameObject.Find("Player");
        _playerActions = new PlayerActions();
        _navTransform = transform.parent.Find("Navigator").GetComponent<Transform>();
    }

    void OnEnable()
    {
        _playerActions.WorldGameplay.Enable();
    }
    void OnDisable()
    {
        _playerActions.WorldGameplay.Disable();
    }


    // Update is called once per frame
    void Update()
    {
        if (_playerActions.WorldGameplay.Interact.triggered && playerProximity)
        {
            _player.transform.Find("Navigator").LookAt(_navTransform.transform);
            // Insert dialogue call here
            PlayerInteract();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerProximity = true;
            _speechBubble.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerProximity = false;
            _speechBubble.SetActive(false);
        }
    }
    
    private void PlayerInteract()
    {
        // 1. Determine whether a. dialogue w/ char or b. other interaction
        //      if a, then 1b. Tween camera pos to refocus on other Char.
        // 3. Call Dialogue box UI
        // 4. Call and display Dialogue
        LookAtPlayer();
        Debug.Log("Sup fucker.");
    }

    private void LookAtPlayer()
    {
        _navTransform.LookAt(_player.transform); 
    }
}
