using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.common.extensions
{
    [DisallowMultipleSystems]
    public class VelocityFlipper : MonoSystem
    {
        [Inject] private Flipper flipper;
        [Inject] private Rigidbody2D rigidbody;

        public override bool EnableOnStart()
        {
            return true;
        }

        public override void Update()
        {
            if(rigidbody.velocity.x != 0) 
            {
                flipper.FlipAtDirection(rigidbody.velocity.x);
            }
        }
    }
}