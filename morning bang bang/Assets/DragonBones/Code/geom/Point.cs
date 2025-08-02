namespace DragonBones
{
    /// <summary>
    /// - The Point object represents a location in a two-dimensional coordinate system.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class Point
    {
        /// <summary>
        /// - The horizontal coordinate.
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float x = 0.0f;
        /// <summary>
        /// - The vertical coordinate.
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float y = 0.0f;

        /// <summary>
        /// - Creates a new point. If you pass no parameters to this method, a point is created at (0,0).
        /// </summary>
        /// <param name="x">- The horizontal coordinate.</param>
        /// <param name="y">- The vertical coordinate.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Point()
        {
        }

        /// <private/>
        public void CopyFrom(Point value)
        {
            this.x = value.x;
            this.y = value.y;
        }

        /// <private/>
        public void Clear()
        {
            this.x = this.y = 0.0f;
        }
    }
}
