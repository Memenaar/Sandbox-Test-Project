using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCManager : MonoBehaviour
{

    #region Declarations
    // Objects & Components
    [SerializeField] private InputReader inputReader; // Scriptable object that conveys input
    [SerializeField]
    private CharIdentitySO _npcID;
    private bool _playerProximity;
    private GameObject _player;
    public Transform _navTransform;
    public Transform _playerTransform;
    public CharacterController _charController;
    public Transform _billboard;
    public Transform _interactUI;
    
    #endregion

    public bool PlayerProximity { get { return _playerProximity; } set { _playerProximity = value; }}

    void Awake()
    {
        _player = GameObject.Find("Player");
        ChangeCharSize();
    }

    void OnEnable()
    {
        //inputReader.PlayerInteractEvent += PlayerInteract;
    }
    void OnDisable()
    {
        //inputReader.PlayerInteractEvent -= PlayerInteract;
    }


    // Update is called once per frame
    void Update()
    {
    }
    
    protected void ChangeCharSize() // Change size of Character Controller to match the assigned character identity.
    {
        _charController.center = new Vector3(0, _npcID.CenterY, 0);
        _charController.radius = _npcID.Radius;
        _charController.height = _npcID.Height;
        _billboard.localPosition = new Vector3(_billboard.localPosition.x, _npcID.CenterY + 0.1f, _billboard.localPosition.z);
        _interactUI.localPosition = new Vector3(_interactUI.localPosition.x, _npcID.Height + 1f, _interactUI.localPosition.z);
    }

    private void PlayerInteract(bool interactPressed)
    {
        if (interactPressed && _playerProximity)
        {
            // 1. Determine whether a. dialogue w/ char or b. other interaction
            //      if a, then 1b. Tween camera pos to refocus on other Char.
            // 3. Call Dialogue box UI
            // 4. Call and display Dialogue
            DialogueManager.GetInstance().EnterDialogueMode(_npcID.DialogueInk);
        }
    }

}
