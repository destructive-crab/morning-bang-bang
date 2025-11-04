using System;
using banging_code.common;
using DragonBonesBridge;
using MothDIed.MonoSystems;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.player_logic
{
    [DisallowMultipleSystems]
    public class PlayerAnimator : MonoSystem
    {
        //dependencies
        [Inject] private PlayerRoot playerRoot;
        
        //data
        private string currentAnimation;
        private float currentSpeed;

        //animations
        private const string Idle = "idle";
        private const string Run = "run";
        private const string Roll = "roll";

        private PlayerAnimatorBundle playerAnimatorBundle;
        private ArmatureController armature;
        private GameDirection appliedDirection;

        public override bool EnableOnStart() => true;

        public override void ContainerStarted()
        {
            armature = DBBridge.Create("rat_gun_up", "rat_gun");
            armature.transform.parent = playerRoot.transform;
            armature.transform.localPosition = Vector3.zero;
            armature.SetOrderInLayer(3);
        }

        public override void Update()
        {
            if (armature.CurrentAnimation != currentAnimation)
            {
                armature.Play(currentAnimation);
            }
            if (playerRoot.Direction == appliedDirection) return;
            switch (playerRoot.Direction)
            {
                case GameDirection.Left:
                    DBBridge.Edit("rat_gun_side", "rat_gun", armature);
                    armature.transform.localScale = new Vector3(-1, 1, 1);
                    appliedDirection = GameDirection.Left;
                    break;
                case GameDirection.Right:
                    DBBridge.Edit("rat_gun_side", "rat_gun", armature);
                    armature.transform.localScale = new Vector3(1, 1, 1);
                    appliedDirection = GameDirection.Right;
                    break;
                case GameDirection.Top:
                    DBBridge.Edit("rat_gun_up", "rat_gun", armature);
                    armature.transform.localScale = new Vector3(1, 1, 1);
                    appliedDirection = GameDirection.Top;
                    break;
                case GameDirection.Bottom:
                    DBBridge.Edit("rat_gun_down", "rat_gun", armature);
                    armature.transform.localScale = new Vector3(1, 1, 1);
                    appliedDirection = GameDirection.Bottom;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Play(string name, float speed)
        {
            currentAnimation = name;
            currentSpeed = speed;
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