using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteract : MonoBehaviour
{

    #region Declarations
    // Objects & Components
    public InputReader inputReader = default; // Scriptable object that conveys input
    [SerializeField]
    private GameObject _speechBubble;
    public bool playerProximity;
    private GameObject _player;
    public Transform _navTransform;
    
    #endregion

    void Awake()
    {
        _player = GameObject.Find("Player");
        _navTransform = transform.parent.Find("Navigator").GetComponent<Transform>();
    }

    void OnEnable()
    {
        inputReader.PlayerInteractEvent += PlayerInteract;
    }
    void OnDisable()
    {
        inputReader.PlayerInteractEvent -= PlayerInteract;
    }


    // Update is called once per frame
    void Update()
    {
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
    
    private void PlayerInteract(bool interactPressed)
    {
        if (interactPressed && playerProximity)
        {
            // 1. Determine whether a. dialogue w/ char or b. other interaction
            //      if a, then 1b. Tween camera pos to refocus on other Char.
            // 3. Call Dialogue box UI
            // 4. Call and display Dialogue
            LookAtPlayer();
            Debug.Log("Sup fucker.");
        }
    }

    private void LookAtPlayer()
    {
        _navTransform.LookAt(_player.transform); 
    }
}
