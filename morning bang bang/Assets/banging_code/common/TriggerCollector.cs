using System.Collections.Generic;
using System.Linq;

namespace banging_code.common
{
    public class TriggerCollector<TComponent> : Trigger<TComponent>
        where TComponent : class
    {
        public TComponent[] Current => CurrentEntered.ToArray();
        protected List<TComponent> CurrentEntered = new();

        public TComponent LastEntered => CurrentEntered.Last();
        public TComponent LastExited { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            OnEnter += CurrentEntered.Add;
            OnExit += other =>
            {
                CurrentEntered.Remove(other);
                LastExited = other;
            };
        }
    }
}