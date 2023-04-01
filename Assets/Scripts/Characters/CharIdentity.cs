using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "CharacterIdentity", menuName = "Identities/Character")]

public class CharIdentity : IdentityBaseSO
{
    #region Declarations
    [Header("Objects & Components")] // Variables containing objects and components
    public Character _charName;
    public RuntimeAnimatorController _animationController;
    public SpriteLibraryAsset _spriteLibrary;
    [SerializeField] private TextAsset _dialogueInk;

    [Header("Collider Size")]
    [SerializeField] private Vector3 _center;
    [SerializeField] private float _radius;
    [SerializeField] private float _height;

    [Header("Movement Constants")] // Constants governing movement
    public float walkmax;
    public float walkaccel;
    public float walkdrag;
    public float walkjump;
    public const float walklerp = 0.2f;
    public float runmax;
    public float runaccel;
    public float rundrag;
    public float runjump;
    public const float runlerp = 0.06f;
    public float speedtuner;
    #endregion

    #region Getters & Setters
    
    // Objects & Components
    public Character CharName { get { return _charName; } }
    public RuntimeAnimatorController AnimationController { get { return _animationController; } }
    public SpriteLibraryAsset SpriteLibrary { get { return _spriteLibrary; } }
    public TextAsset DialogueInk { get { return _dialogueInk; }}

    // Collider Size
    public float CenterY { get { return _center.y; }}
    public float Radius { get { return _radius; }}
    public float Height { get { return _height; }}

    // MovementConstants
    public float WalkMax { get { return walkmax; }}
    public float WalkAccel { get { return walkaccel;}}
    public float WalkDrag { get { return walkdrag; }}
    public float WalkJump { get { return walkjump; }}
    public float WalkLerp { get { return walklerp; }}
    public float RunMax { get { return runmax; }}
    public float RunAccel { get { return runaccel; }}
    public float RunDrag { get { return rundrag; }}
    public float RunJump { get { return runjump; }}
    public float RunLerp { get { return runlerp; }}
    public float SpeedTuner { get { return speedtuner; }} 
    #endregion
    
}
