using System;

namespace DragonBones
{
    /// <summary>
    /// - 2D Transform.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class DBTransform
    {
        /// <private/>
        public static readonly float PI = 3.141593f;
        /// <private/>
        public static readonly float PI_D = PI * 2.0f;
        /// <private/>
        public static readonly float PI_H = PI / 2.0f;
        /// <private/>
        public static readonly float PI_Q = PI / 4.0f;
        /// <private/>
        public static readonly float RAD_DEG = 180.0f / PI;
        /// <private/>
        public static readonly float DEG_RAD = PI / 180.0f;

        /// <private/>
        public static float NormalizeRadian(float value)
        {
            value = (value + PI) % (PI * 2.0f);

           
            value += value > 0.0f ? -PI : PI;

            return value;
        }

        /// <summary>
        /// - Horizontal translate.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public float x = 0.0f;
        /// <summary>
        /// - Vertical translate.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public float y = 0.0f;
        /// <summary>
        /// - Skew. (In radians)
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public float skew = 0.0f;
        /// <summary>
        /// - rotation. (In radians)
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public float rotation = 0.0f;
        /// <summary>
        /// - Horizontal Scaling.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public float scaleX = 1.0f;
        /// <summary>
        /// - Vertical scaling.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public float scaleY = 1.0f;

        /// <private/>
        public DBTransform()
        {
            
        }

        public override string ToString()
        {
            return "[object dragonBones.Transform] x:" + this.x + " y:" + this.y + " skew:" + this.skew* 180.0 / PI + " rotation:" + this.rotation* 180.0 / PI + " scaleX:" + this.scaleX + " scaleY:" + this.scaleY;
        }

        /// <private/>
        public DBTransform CopyFrom(DBTransform value)
        {
            this.x = value.x;
            this.y = value.y;
            this.skew = value.skew;
            this.rotation = value.rotation;
            this.scaleX = value.scaleX;
            this.scaleY = value.scaleY;

            return this;
        }

        /// <private/>
        public DBTransform Identity()
        {
            this.x = this.y = 0.0f;
            this.skew = this.rotation = 0.0f;
            this.scaleX = this.scaleY = 1.0f;

            return this;
        }

        /// <private/>
        public DBTransform Add(DBTransform value)
        {
            this.x += value.x;
            this.y += value.y;
            this.skew += value.skew;
            this.rotation += value.rotation;
            this.scaleX *= value.scaleX;
            this.scaleY *= value.scaleY;

            return this;
        }

        /// <private/>
        public DBTransform Minus(DBTransform value)
        {
            this.x -= value.x;
            this.y -= value.y;
            this.skew -= value.skew;
            this.rotation -= value.rotation;
            this.scaleX /= value.scaleX;
            this.scaleY /= value.scaleY;

            return this;
        }

        /// <private/>
        public DBTransform FromMatrix(DBMatrix dbMatrix)
        {
            var backupScaleX = this.scaleX;
            var backupScaleY = this.scaleY;

            this.x = dbMatrix.tx;
            this.y = dbMatrix.ty;

            var skewX = (float)Math.Atan(-dbMatrix.c / dbMatrix.d);
            this.rotation = (float)Math.Atan(dbMatrix.b / dbMatrix.a);

            if(float.IsNaN(skewX))
            {
                skewX = 0.0f;
            }

            if(float.IsNaN(this.rotation))
            {
                this.rotation = 0.0f; 
            }

            this.scaleX = (float)((this.rotation > -PI_Q && this.rotation < PI_Q) ? dbMatrix.a / Math.Cos(this.rotation) : dbMatrix.b / Math.Sin(this.rotation));
            this.scaleY = (float)((skewX > -PI_Q && skewX < PI_Q) ? dbMatrix.d / Math.Cos(skewX) : -dbMatrix.c / Math.Sin(skewX));

            if (backupScaleX >= 0.0f && this.scaleX < 0.0f)
            {
                this.scaleX = -this.scaleX;
                this.rotation = this.rotation - PI;
            }

            if (backupScaleY >= 0.0f && this.scaleY < 0.0f)
            {
                this.scaleY = -this.scaleY;
                skewX = skewX - PI;
            }

            this.skew = skewX - this.rotation;

            return this;
        }

        /// <private/>
        public DBTransform ToMatrix(DBMatrix dbMatrix)
        {
            if(this.rotation == 0.0f)
            {
                dbMatrix.a = 1.0f;
                dbMatrix.b = 0.0f;
            }
            else
            {
                dbMatrix.a = (float)Math.Cos(this.rotation);
                dbMatrix.b = (float)Math.Sin(this.rotation);
            }

            if(this.skew == 0.0f)
            {
                dbMatrix.c = -dbMatrix.b;
                dbMatrix.d = dbMatrix.a;
            }
            else
            {
                dbMatrix.c = -(float)Math.Sin(this.skew + this.rotation);
                dbMatrix.d = (float)Math.Cos(this.skew + this.rotation);
            }

            if(this.scaleX != 1.0f)
            {
                dbMatrix.a *= this.scaleX;
                dbMatrix.b *= this.scaleX;
            }

            if(this.scaleY != 1.0f)
            {
                dbMatrix.c *= this.scaleY;
                dbMatrix.d *= this.scaleY;
            }

            dbMatrix.tx = this.x;
            dbMatrix.ty = this.y;

            return this;
        }
    }
}
