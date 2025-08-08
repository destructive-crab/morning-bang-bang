namespace DragonBones
{
    public interface IEngineChildArmatureSlotDisplay : IEngineSlotDisplay
    {
        public ChildArmatureDisplayData ChildArmatureDisplayData => Data as ChildArmatureDisplayData;
        public IEngineArmatureDisplay ArmatureDisplay { get; }
    }
}