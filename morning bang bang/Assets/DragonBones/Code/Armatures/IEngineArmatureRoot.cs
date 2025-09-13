namespace DragonBones
{
    public interface IEngineArmatureRoot : IEventDispatcher<EventObject>
    {
        void DBConnect(Armature armature);
        void DBClear();
        void DBUpdate();

        Armature Armature { get; }
        AnimationPlayer AnimationPlayer { get; }
    }
}
