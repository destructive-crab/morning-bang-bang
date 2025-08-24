namespace DragonBones
{
    public sealed class UnityChildArmature : DBObject, IEngineChildArmature
    {
        public ChildArmature Armature { get; private set; }
        public ChildArmatureDisplayData Data { get; private set; }
        public IEngineArmatureRoot EngineRoot { get; private set; }

        public UnityArmatureRoot UnityRoot { get; private set; }
        public UnitySlot UnityParentSlot { get; private set; }
        
        public Armature ParentArmature => Armature.Parent.Armature;
        public Slot ParentSlot => Armature.Parent;

        public void DBInit(ChildArmature armature, ChildArmatureDisplayData childArmatureData)
        {
            Armature = armature;
            Data = childArmatureData;

            EngineRoot = armature.Parent.Armature.Root;
            UnityRoot = EngineRoot as UnityArmatureRoot;
            UnityParentSlot = ParentSlot as UnitySlot;
        }

        public void DBClear() { }
        public void DBUpdate() { }
        public override void OnReleased() => DBClear();
    }
}