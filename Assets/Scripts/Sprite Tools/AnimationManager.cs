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
            public SpriteRenderer _spriteRenderer;
            public SpriteManager _spriteManager;
            public PlayerStateMachine _psm;
            public DirectionTracker _directionTracker;
            public Animator animator;
            public PlayerStateFactory _factory;
            public PlayerBaseState _currentSuperState;
            private string currentState;
        

            void Awake()
            {
                _spriteManager = GetComponent<SpriteManager>();
                _spriteRenderer = GetComponent<SpriteRenderer>();
                _psm = GetComponentInParent<PlayerStateMachine>();
                animator = GetComponent<Animator>();
                _directionTracker = transform.parent.Find("Navigator").GetComponent<DirectionTracker>();
            }

            // Update is called once per frame
            void Update()
            {
                // Change this script so that it reacts to the state machine rather than to pseudo-states like isGrounded.
                if (_psm.CurrentSuperState == _psm.Factory.Grounded())
                {
                    if (_psm.CurrentSubState == _psm.Factory.Slide())
                    {
                        string animName = _psm._activeChar + "_Slide_" + _directionTracker.direction.ToString();
                        ChangeAnimationState(animName);

                    } else if (_psm.CurrentSubState == _psm.Factory.HardLanding())
                    {
                        string animName = _psm._activeChar + "_Slide_" + _directionTracker.direction.ToString();
                        ChangeAnimationState(animName);

                    } else if (_psm.Velocity != Vector3.zero)
                    {
                        if (_psm.CurrentSubState == _psm.Factory.Walk())
                        {
                            string animName = _psm._activeChar + "_Walk_" + _directionTracker.direction.ToString();
                            ChangeAnimationState(animName);
                        } else if (_psm.CurrentSubState == _psm.Factory.Run())
                        {
                            string animName = _psm._activeChar + "_Run_" + _directionTracker.direction.ToString();
                            ChangeAnimationState(animName);
                        }

                    } else
                    {
                        string animName = _psm._activeChar + "_Idle_" + _directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                    }
                } else if (_psm.CurrentSuperState == _psm.Factory.Airborne())
                {
                    if (_psm.CurrentSubState == _psm.Factory.Jump() || _psm.CurrentSubState == _psm.Factory.WallJump() || (_psm.CurrentSubState == _psm.Factory.Fall() && Mathf.Abs(_psm.YSpeed) <= _psm.HardLandingThreshold))
                    {
                        string animName = _psm._activeChar + "_Jump_" + _directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                    }
                } else if (_psm.CurrentSuperState == _psm.Factory.Wall())
                {
                    if (_psm.CurrentSubState == _psm.Factory.WallSlide())
                    {
                        string animName = _psm._activeChar + "_WallSlide_Left";
                        ChangeAnimationState(animName);
                        //Debug.Log("Animation Name: " + animName);
                    }
                } else
                {
                        string animName = _psm._activeChar + "_Idle_" + _directionTracker.direction.ToString();
                        ChangeAnimationState(animName);
                }
            }

            void ChangeAnimationState(string newState)
            {
                if (currentState == newState) return;

                animator.Play(newState);

                currentState = newState;
            }
        }
}