using banging_code.common.rooms;

namespace banging_code.level.random_gen
{
    public abstract class LevelMapGenerator
    {
        public abstract BasicRoomTypes GetNext(BasicRoomTypes[] map, int current, int size);
    }
}