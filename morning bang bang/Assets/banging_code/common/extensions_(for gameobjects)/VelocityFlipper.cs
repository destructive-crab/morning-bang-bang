using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace banging_code.common.extensions
{
    [DisallowMultipleExtensions]
    public class VelocityFlipper : Extension
    {
        [Inject] private Flipper flipper;
        [Inject] private Rigidbody2D rigidbody;
       
        public override void Update()
        {
            if(rigidbody.velocity.x != 0) 
            {
                flipper.FlipAtDirection(rigidbody.velocity.x);
            }
        }
    }
}