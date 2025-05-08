using MothDIed.MonoSystems;

namespace banging_code.ai.pathfinding
{
    public class EntityState : MonoData
    {
        public CommonStates State;
        
        public enum CommonStates
        {
            Attack,
            Run,
            Idle,
            Sleep
        }
    }
}