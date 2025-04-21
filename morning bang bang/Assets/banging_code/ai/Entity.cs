using MothDIed;

namespace banging_code.ai
{
    public abstract class Entity : DepressedBehaviour
    {
        public bool IsSleeping { get; private set; }

        private void Update() => Extensions.UpdateContainer();
        private void FixedUpdate() => Extensions.FixedUpdateContainer();

        public virtual void Initialize()
        {
            Extensions.SetOwner(this);
            Extensions.StartContainer();
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
    }
}