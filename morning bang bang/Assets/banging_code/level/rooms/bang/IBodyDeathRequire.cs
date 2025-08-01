using System;
using banging_code.health;

namespace banging_code.common.rooms
{
    public interface IBodyDeathRequire : IConnectedWithRoom<BangRoom>
    {
        bool IsDead { get; }

        HitableBody Owner { get; }
        
        event Action<IBodyDeathRequire, HitableBody> OnDie;
    }
}