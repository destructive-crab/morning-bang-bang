using banging_code.common;

namespace banging_code.ai.targeting
{
    public class EntityAttackRange : TriggerCollector<TargetToEntities>
    {
        public TargetToEntities Best
        {
            get
            {
                if (Current.Length == 0) return null;
                
                return Current[0];
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            OnEnter += Sort;
            OnExit += Sort;
        }

        private void Sort(TargetToEntities obj)
        {
            CurrentEntered.Sort();
        }
    }
}