namespace DragonBones
{
    public class ChildArmature : Armature
    {
        public bool IsActive = false;
        
        public ChildArmatureDisplayData DisplayData { get; private set; }
        public Slot Parent { get; private set; }

        public void InitializeChildArmature(ChildArmatureDisplayData data, Slot parent)
        {
            DisplayData = data;
            Parent = parent;
        }

        public override void OnReleased()
        {
            base.OnReleased();

            Parent = null;
            DisplayData = null;
        }
    }
}