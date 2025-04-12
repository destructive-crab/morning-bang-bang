using banging_code.common.rooms;

namespace banging_code.level.random_gen
{
    public abstract class Generator
    {
        public abstract Room[] Generate();

        public abstract void Clear();

        public abstract Room[] Regenerate();
    }
}