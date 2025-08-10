namespace DragonBones
{
    /// <internal/>
    /// <private/>
    public class CanvasData : DBObject
    {
        public bool hasBackground;
        public int color;
        public float x;
        public float y;
        public float width;
        public float height;

        public override void OnReleased()
        {
            this.hasBackground = false;
            this.color = 0x000000;
            this.x = 0.0f;
            this.y = 0.0f;
            this.width = 0.0f;
            this.height = 0.0f;
        }
    }
}
