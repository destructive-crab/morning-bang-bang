using banging_code.common.rooms;
using UnityEngine;

namespace banging_code.ai
{
    public abstract class Entity : Moody 
    {
        public bool IsSleeping { get; private set; }
        public EntityHealth Health { get; protected set; }

        [SerializeField] private GameObject deadBody;

        private void Update() => Extensions.UpdateContainer();
        private void FixedUpdate() => Extensions.FixedUpdateContainer();

        public virtual void Initialize()
        {
            Extensions.SetOwner(this);
            Extensions.StartContainer();

            Health = GetComponent<EntityHealth>();
            Health.OnDie += Die;
        }

        public virtual void GoSleep()
        {
            IsSleeping = true;
        }

        public virtual void WakeUp()
        {
            IsSleeping = false;
        }

        public abstract void Tick();

        protected void Die(Entity entity)
        {
            GameObject.Instantiate(deadBody);
            Destroy(gameObject);
        }

        public override void PlayerBrokenIntoRoom(BreakArg breakArg)
        {
            
        }
    }
}