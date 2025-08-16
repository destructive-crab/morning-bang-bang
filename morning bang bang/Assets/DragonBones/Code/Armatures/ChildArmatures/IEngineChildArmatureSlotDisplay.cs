namespace DragonBones
{
    public interface IEngineChildArmatureSlotDisplay 
    {
        public ChildArmatureDisplayData Data { get; }
        public IEngineArmatureDisplay ArmatureDisplay { get; }

        void Enable();
        void Disable();
    }
}