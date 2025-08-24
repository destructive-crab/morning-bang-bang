namespace DragonBones
{
    public interface IEngineChildArmature
    {
        public ChildArmature Armature { get; }
        public ChildArmatureDisplayData Data { get; }
        public IEngineArmatureRoot EngineRoot { get; }
        
        public Armature ParentArmature => Armature.Parent.Armature;
        public Slot ParentSlot => Armature.Parent;

        void DBInit(ChildArmature armature, ChildArmatureDisplayData data);
        void DBClear();
        void DBUpdate();
    }
}