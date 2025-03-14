using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace banging_code.common
{
    [DisallowMultipleExtensions]
    public sealed class Flipper : Extension 
    {
        public bool FacingRight { get; private set; }
        [Inject] private SpriteRenderer spriteRenderer;

        public Flipper(bool facingRight)
        {
            FacingRight = facingRight;
        }

        public void FlipAtDirection(float x)
        {
            if(x > 0) FlipToRight();
            else      FlipToLeft();
        }

        public void FaceToTransform(Transform transform) => FaceToPoint(transform.position);
        
        public void FaceToPoint(Vector2 position)
        {
            if (position.x > Owner.transform.position.x) FlipToRight();   
            else                                         FlipToLeft();
        }
        
        public void FlipToOpposite()
        {
            if(FacingRight) FlipToLeft();
            else            FlipToRight();
        }

        public void FlipToRight()
        {
            if (!FacingRight)
            {
                Owner.transform.localScale = new Vector3(1, 1, 1);
                FacingRight = true;
            }
        }
        public void FlipToLeft()
        {
            if (FacingRight)
            {
                Owner.transform.localScale = new Vector3(-1, 1, 1);
                FacingRight = false;
            }
        }
    }
}