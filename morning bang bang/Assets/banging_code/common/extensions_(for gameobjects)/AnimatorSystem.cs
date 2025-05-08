using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.common.extensions__for_gameobjects_
{
    public abstract class AnimatorSystem : MonoSystem
    {
        public string Current { get; private set; }
        protected Animator animator;

        protected void Play(string animation)
        {
            if (Current == animation) return;
            
            Current = animation;
            animator.Play(animation);
        }
    }
}