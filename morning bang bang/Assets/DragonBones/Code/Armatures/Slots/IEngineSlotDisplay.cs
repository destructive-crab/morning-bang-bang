namespace DragonBones
{
    public interface IEngineSlotDisplay
    {
        Slot Parent { get; }
        DisplayData Data { get; }

        void Init(DisplayData data);
        
        void Enable();
        void Disable();
    }
}