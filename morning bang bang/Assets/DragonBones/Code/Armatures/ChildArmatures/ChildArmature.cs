namespace DragonBones
{
    public class ChildArmature : Armature
    {
        public DBRegistry.DBID SlotID;
        public DBRegistry.DBID DisplayID;
        
        public Slot Parent { get; internal set; }

        public void InitializeChildArmature(DBRegistry.DBID slotID, DBRegistry.DBID displayID, Slot parent)
        {
            SlotID = slotID;
            DisplayID = displayID;
            
            Parent = parent;
        }

        public override void OnReleased()
        {
            base.OnReleased();

            Parent = null;
        }
    }
}