using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteract : MonoBehaviour
{

    public bool playerProximity;
    private GameObject _player;
    public Transform _navTransform;
    
    [SerializeField]
    private GameObject _speechBubble;

    void Awake()
    {
        _player = GameObject.Find("Player");
        _navTransform = transform.parent.Find("Navigator").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerProximity)
        {
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