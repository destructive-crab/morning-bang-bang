using System;
using UnityEngine;

namespace banging_code
{
    [RequireComponent(typeof(Collider2D))]
    public class Trigger : MonoBehaviour
    {
        public event Action<Collider2D> OnEnter;
        public event Action<Collider2D> OnExit;

#if UNITY_EDITOR
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
#endif

        protected void OnTriggerEnter2D(Collider2D other)
        {
            OnEnter?.Invoke(other); 
        }

        protected void OnTriggerExit2D(Collider2D other)
        {
            OnExit?.Invoke(other);
        }
    }
}