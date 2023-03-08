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
            
            // Start is called before the first frame update
            void Start()
            {
                
            }

            // Update is called once per frame
            void Update()
            {

            }
        }
}