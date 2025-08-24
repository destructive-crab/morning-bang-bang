using banging_code.common;
using banging_code.player_logic.states;
using DragonBones;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.player_logic
{
    [DisallowMultipleSystems]
    public class PlayerAnimator : MonoSystem
    {
        //dependencies
        private UnityArmatureRoot armatureAPI;
        [Inject] private PlayerRoot playerRoot;
        
        //data
        private string currentAnimation;
        private float currentSpeed;

        //animations
        private const string Idle = "idle";
        private const string Run = "run";
        private const string Roll = "roll";

        private PlayerAnimatorBundle playerAnimatorBundle;

        private UnityArmatureRoot side;
        private UnityArmatureRoot up;
        private UnityArmatureRoot down;
        
        public override bool EnableOnStart()
        {
            return true;
        }

        public override void ContainerStarted()
        {
            DB.UnityDataLoader.LoadDragonBonesData("animations/rat_gun_ske");
            DB.UnityDataLoader.LoadTextureAtlasData("animations/rat_gun_tex");

            side = DB.Factory.UnityCreateArmature("rat_gun_side", "rat_gun");
            armatureAPI = side;
//            up = DBInitial.UnityFactory.BuildArmatureComponent("rat_gun_up", "rat_gun");
//            down = DBInitial.UnityFactory.BuildArmatureComponent("rat_gun_down", "rat_gun");
//            
//            side.transform.parent = Owner.transform;
//            up.transform.parent = Owner.transform;
//            down.transform.parent = Owner.transform;
//            side.gameObject.SetActive(true);
//            
//            up.gameObject.SetActive(false);
//            down.gameObject.SetActive(false);
        }
        
        public override void Update()
        {
            if ((playerRoot.Direction == GameDirection.Left || playerRoot.Direction == GameDirection.Right) && armatureAPI.Armature.Name != "rat_gun_side")
            {
//                side.gameObject.SetActive(true);
//                down.gameObject.SetActive(false);
//                up.gameObject.SetActive(false);
//                side.AnimationPlayer.Play(currentAnimation);
//                engineArmatureAPI = side; 
                DB.Factory.UnityCreateArmature("rat_gun_side", "rat_gun", side);
            }
            else if (playerRoot.Direction == GameDirection.Top && armatureAPI.Armature.Name != "rat_gun_up")
            {
 //               up.gameObject.SetActive(true); 
 //               down.gameObject.SetActive(false);
 //               side.gameObject.SetActive(false);
 //               up.AnimationPlayer.Play(currentAnimation);
 //               engineArmatureAPI = up;
                DB.Factory.UnityCreateArmature("rat_gun_up", "rat_gun",side);
            }
            else if (playerRoot.Direction == GameDirection.Bottom && armatureAPI.Armature.Name != "rat_gun_down")
            {
  //              down.gameObject.SetActive(true);                
  //              side.gameObject.SetActive(false);
  //              up.gameObject.SetActive(false);
  //              down.AnimationPlayer.Play(currentAnimation);
  //              engineArmatureAPI = down; 
                DB.Factory.UnityCreateArmature("rat_gun_down", "rat_gun", side);
            }
        }
        
        private void Play(string name, float speed)
        {
            if(armatureAPI.AnimationPlayer == null) return;
            if (armatureAPI.AnimationPlayer.lastAnimationName == name) return;
            armatureAPI.AnimationPlayer.Play(name, 0);
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