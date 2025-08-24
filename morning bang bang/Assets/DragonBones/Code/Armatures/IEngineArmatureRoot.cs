namespace DragonBones
{
    public interface IEngineArmatureRoot : IEventDispatcher<EventObject>
    {
        void DBInit(Armature armature);
        void DBClear();
        void DBUpdate();

         Armature Armature { get; }
         AnimationPlayer AnimationPlayer { get; }
    }
}
