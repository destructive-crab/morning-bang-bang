namespace DragonBones
{
    public class ChildArmature : Armature
    {
        public Slot Parent { get; internal set; }

        public void InitializeChildArmature(Slot parent)
        {
            Parent = parent;
        }

        public override void OnReleased()
        {
            base.OnReleased();

            Parent = null;
        }
    }
}