namespace DragonBones
{
    /// <summary>
    /// - A Rectangle object is an area defined by its position, as indicated by its top-left corner point (x, y) and by its
    /// width and its height.<br/>
    /// The x, y, width, and height properties of the Rectangle class are independent of each other; changing the value of
    /// one property has no effect on the others. However, the right and bottom properties are integrally related to those
    /// four properties. For example, if you change the value of the right property, the value of the width property changes;
    /// if you change the bottom property, the value of the height property changes.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class Rectangle
    {
        /// <summary>
        /// - The x coordinate of the top-left corner of the rectangle.
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float x;
        /// <summary>
        /// - The y coordinate of the top-left corner of the rectangle.
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float y;
        /// <summary>
        /// - The width of the rectangle, in pixels.
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float width;

        /// <summary>
        /// - The height of the rectangle, in pixels.
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>zh_CN</language>
        public float height;

        /// <private/>
        public Rectangle()
        {
        }

        /// <private/>
        public void CopyFrom(Rectangle value)
        {
            this.x = value.x;
            this.y = value.y;
            this.width = value.width;
            this.height = value.height;
        }

        /// <private/>
        public void Clear()
        {
            this.x = this.y = 0.0f;
            this.width = this.height = 0.0f;
        }
    }
}
