using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteController {
        public class AnimationManager : MonoBehaviour
        {
            [Header("Animations")]
            protected List<AnimationClip> idleAnimations = new List<AnimationClip>();
            protected List<AnimationClip> walkAnimations = new List<AnimationClip>();
            protected List<AnimationClip> runAnimations = new List<AnimationClip>();
            protected List<AnimationClip> jumpAnimations = new List<AnimationClip>();

            [Header("Components")]
            protected SpriteRenderer spriteRenderer;
            protected SpriteManager spriteManager;
            protected PlayerStateMachine charMovement;
            protected DirectionTracker directionTracker;
            public Animator animator;
            private string currentState;

            void Awake()
            {
                spriteManager = GetComponent<SpriteManager>();
                spriteRenderer = GetComponent<SpriteRenderer>();
                charMovement = GetComponentInParent<PlayerStateMachine>();
                animator = GetComponent<Animator>();
                directionTracker = transform.parent.Find("Navigator").GetComponent<DirectionTracker>();
            }
            // Start is called before the first frame update
            void Start()
            {
                
            }

            // Update is called once per frame
            void Update()
            {
                // Change this script so that it reacts to the state machine rather than to pseudo-states like isGrounded.
             /*   if (charMovement.CharController.isGrounded)
                {
                    if (charMovement._slideState == SlideState.Slide)
                    {
                        string animName = "Ivy_Slide_" + directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                        Debug.Log(animName);

                    } else if (charMovement.velocity != Vector3.zero)
                    {
                        if (charMovement.velocity.magnitude <= charMovement.WalkMax)
                        {
                            string animName = "Ivy_Walk_" + directionTracker.direction.ToString();
                            ChangeAnimationState(animName);
                        } else
                        {
                            string animName = "Ivy_Run_" + directionTracker.direction.ToString();
                            ChangeAnimationState(animName);
                        }

                    } else
                    {
                        string animName = "Ivy_Idle_" + directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                    }
                } else if (charMovement._slideState == SlideState.WallSlide)
                {
                        string animName = "Ivy_WallSlide_Left";
                        ChangeAnimationState(animName);
                } else if (charMovement._jumpState != JumpState.None || (charMovement.CurrentSubState == charMovement.Factory.Fall() && charMovement.ySpeed <= -5f))
                {
                        string animName = "Ivy_Jump_" + directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                } else
                {
                        string animName = "Ivy_Idle_" + directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                } */
            }

            void ChangeAnimationState(string newState)
            {
                if (currentState == newState) return;

                animator.Play(newState);

                currentState = newState;
            }
        }
}