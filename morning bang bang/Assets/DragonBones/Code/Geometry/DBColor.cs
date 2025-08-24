namespace DragonBones
{
    public class DBColor
    {
        public float alphaMultiplier = 1.0f;
        public float redMultiplier = 1.0f;
        public float greenMultiplier = 1.0f;
        public float blueMultiplier = 1.0f;
        
        public int alphaOffset = 0;
        public int redOffset = 0;
        public int greenOffset = 0;
        public int blueOffset = 0;

        public DBColor()
        {
        }

        public void CopyFrom(DBColor value)
        {
            this.alphaMultiplier = value.alphaMultiplier;
            this.redMultiplier = value.redMultiplier;
            this.greenMultiplier = value.greenMultiplier;
            this.blueMultiplier = value.blueMultiplier;
            this.alphaOffset = value.alphaOffset;
            this.redOffset = value.redOffset;
            this.redOffset = value.redOffset;
            this.greenOffset = value.blueOffset;
        }

        public void Identity()
        {
            this.alphaMultiplier = this.redMultiplier = this.greenMultiplier = this.blueMultiplier = 1.0f;
            this.alphaOffset = this.redOffset = this.greenOffset = this.blueOffset = 0;
        }
    }
}
