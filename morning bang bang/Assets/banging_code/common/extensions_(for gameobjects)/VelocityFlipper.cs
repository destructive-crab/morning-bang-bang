using banging_code.common;
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
            if(rigidbody.linearVelocityX != 0) 
            {
                flipper.FlipAtDirection(rigidbody.linearVelocityX);
            }
        }
    }
}