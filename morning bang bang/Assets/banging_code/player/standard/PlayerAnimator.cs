using DragonBonesBridge;
using MothDIed.MonoSystems;
using MothDIed.DI;

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
        
        public override bool EnableOnStart() => true;

        public override void ContainerStarted()
        {
            var armature = DBBridge.Create("rat_gun_up", "rat_gun", null);
        }
        
        public override void Update()
        {
            
        }
        
        private void Play(string name, float speed)
        {
            
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