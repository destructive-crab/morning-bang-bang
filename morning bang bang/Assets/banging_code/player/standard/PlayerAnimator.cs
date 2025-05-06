using banging_code.common;
using banging_code.player_logic.states;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.player_logic
{
    [DisallowMultipleSystems]
    public class PlayerAnimator : MonoSystem
    {
        //dependencies
        [Inject] private Animator animator;
        [Inject] private PlayerRoot playerRoot;
        
        //data
        private string currentAnimation;
        private float currentSpeed;

        //animations
        private const string Idle = "Idle";
        private const string Run = "Run";
        private const string Roll = "Roll";

        private PlayerAnimatorBundle playerAnimatorBundle;

        public override bool EnableOnStart()
        {
            return true;
        }

        public override void ContainerStarted()
        {
            playerAnimatorBundle = Resources.Load<PlayerAnimatorBundle>(PTH.PlayerAnimatorBundle);
        }

        public override void Update()
        {
            if ((playerRoot.Direction == GameDirection.Left || playerRoot.Direction == GameDirection.Right) && animator.runtimeAnimatorController != playerAnimatorBundle.side)
            {
                animator.runtimeAnimatorController = playerAnimatorBundle.side;
                Play(currentAnimation, currentSpeed);
            }
            else if (playerRoot.Direction == GameDirection.Top && animator.runtimeAnimatorController != playerAnimatorBundle.up)
            {
                animator.runtimeAnimatorController = playerAnimatorBundle.up;
                Play(currentAnimation, currentSpeed);
            }
            else if (playerRoot.Direction == GameDirection.Bottom && animator.runtimeAnimatorController != playerAnimatorBundle.down)
            {
                animator.runtimeAnimatorController = playerAnimatorBundle.down;
                Play(currentAnimation, currentSpeed);
            }
        }

        private void Play(string name, float speed)
        {
            animator.speed = speed;
            animator.Play(name);
        }
        
        public void PlayIdle(float speed)
        {
            Play(Idle, speed);
        }

        public void PlayRun(float speed)
        {
            Play(Run, speed);
        }
        
        public void PlayRoll(float speed)
        {
            Play(Roll, speed);
        }


    }
}