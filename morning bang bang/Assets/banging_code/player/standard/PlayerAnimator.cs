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
        private UnityArmatureInstance armatureAPI;
        [Inject] private PlayerRoot playerRoot;
        
        //data
        private string currentAnimation;
        private float currentSpeed;

        //animations
        private const string Idle = "idle";
        private const string Run = "run";
        private const string Roll = "roll";

        private PlayerAnimatorBundle playerAnimatorBundle;

        private UnityArmatureInstance side;
        private UnityArmatureInstance up;
        private UnityArmatureInstance down;
        
        public override bool EnableOnStart()
        {
            return true;
        }

        public override void ContainerStarted()
        {
            DBUnityFactory.factory.LoadDragonBonesData("animations/rat_gun_ske");
            DBUnityFactory.factory.LoadTextureAtlasData("animations/rat_gun_tex");

            side = DBUnityFactory.factory.BuildArmatureComponent("rat_gun_side", "rat_gun");
            up = DBUnityFactory.factory.BuildArmatureComponent("rat_gun_up", "rat_gun");
            down = DBUnityFactory.factory.BuildArmatureComponent("rat_gun_down", "rat_gun");
            side.transform.parent = Owner.transform;
            up.transform.parent = Owner.transform;
            down.transform.parent = Owner.transform;
            side.gameObject.SetActive(true);
            armatureAPI = side;
            up.gameObject.SetActive(false);
            down.gameObject.SetActive(false);
        }
        
        public override void Update()
        {
            if ((playerRoot.Direction == GameDirection.Left || playerRoot.Direction == GameDirection.Right) && !side.isActiveAndEnabled)
            {
                side.gameObject.SetActive(true);
                down.gameObject.SetActive(false);
                up.gameObject.SetActive(false);
                side.Animation.Play(currentAnimation);
                armatureAPI = side;
            }
            else if (playerRoot.Direction == GameDirection.Top && armatureAPI.Armature.name != "rat_gun_up")
            {
                up.gameObject.SetActive(true); 
                down.gameObject.SetActive(false);
                side.gameObject.SetActive(false);
                up.Animation.Play(currentAnimation);
                armatureAPI = up;
            }
            else if (playerRoot.Direction == GameDirection.Bottom && armatureAPI.Armature.name != "rat_gun_down")
            {
                down.gameObject.SetActive(true);                
                side.gameObject.SetActive(false);
                up.gameObject.SetActive(false);
                down.Animation.Play(currentAnimation);
                armatureAPI = down;
            }
        }
        
        private void Play(string name, float speed)
        {
            if(armatureAPI.Animation == null)return;
            if (armatureAPI.Animation.lastAnimationName == name) return;
            armatureAPI.Animation.Play(name, 0);
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