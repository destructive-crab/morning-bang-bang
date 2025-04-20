using System;
using UnityEngine;

namespace banging_code
{
    [RequireComponent(typeof(Collider2D))]
    public class Trigger<TComponent> : MonoBehaviour
        where TComponent : class
    {
        public Collider2D TriggerCollider()
        {
            if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
            return triggerCollider;
        }
        protected Collider2D triggerCollider;
        
        public event Action<TComponent> OnEnter;
        protected void OnEnterInvocation(TComponent other) => OnEnter?.Invoke(other);
        public event Action<TComponent> OnExit;
        protected void OnExitInvocation(TComponent other) => OnExit?.Invoke(other);

#if UNITY_EDITOR
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
#endif
        private void Awake()
        {
            TriggerCollider().isTrigger = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out TComponent target))
            {
                OnEnterInvocation(target);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out TComponent target))
            {
                OnExitInvocation(target);
            }
        }
    }
}