using banging_code.common.rooms;
using MothDIed;
using UnityEngine;

namespace banging_code.ai
{
    public abstract class Entity : Moody 
    {
        public bool IsSleeping { get; private set; }
        public EntityHealth Health { get; protected set; }

        [SerializeField] private GameObject deadBody;

        private void Update() => Systems.UpdateContainer();
        private void FixedUpdate() => Systems.FixedUpdateContainer();

        public virtual void Initialize()
        {
            Systems.SetOwner(this);
            Systems.StartContainer();

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
            Game.CurrentScene.Fabric.Instantiate(deadBody, transform.position);
            Destroy(gameObject);
        }

        public override void PlayerBrokenIntoRoom(BreakArg breakArg)
        {
            
        }
    }
}