namespace DragonBones
{
    public interface IEngineArmatureRoot : IEventDispatcher<EventObject>
    {
        void DBConnect(DBRegistry.DBID armatureID);
        void DBInit(Armature armature);
        void DBClear();
        void DBUpdate();

        DBRegistry.DBID ArmatureID { get; }
        Armature Armature { get; }
        AnimationPlayer AnimationPlayer { get; }
    }
}
