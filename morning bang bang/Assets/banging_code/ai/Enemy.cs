using banging_code.common;
using banging_code.common.rooms;
using MothDIed;
using UnityEngine;

namespace banging_code.ai
{
    public abstract class Enemy : Moody 
    {
        public bool IsSleeping { get; private set; }
        public EntityHealth Health { get; protected set; }

        [SerializeField] private GameObject deadBody;

        private ID id;
        public override ID EntityID
        {
            get
            {
                if (id == null) id = new ID(GetEntityPrefix(), true);
                return id;
            }
        }

        protected virtual string GetEntityPrefix() => "enemy";

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

        protected void Die(Enemy enemy)
        {
            Game.SceneSwitcher.CurrentScene.Fabric.Instantiate(deadBody, transform.position);
            Destroy(gameObject);
        }

        public override void PlayerBrokenIntoRoom(BreakArg breakArg)
        {
            
        }
    }
}