namespace DragonBones
{
    public interface IEngineSlotDisplay
    {
        Slot Parent { get; }
        DisplayData Data { get; }

        void Enable();
        void Disable();
    }
}