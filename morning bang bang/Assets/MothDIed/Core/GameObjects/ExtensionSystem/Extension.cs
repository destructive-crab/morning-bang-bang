using System;

namespace MothDIed.ExtensionSystem
{
    public abstract class Extension : IDisposable
    {
        public DepressedBehaviour Owner { get; private set; }
        public bool Enabled { get; private set; }

        public void Initialize(DepressedBehaviour owner)
        {
            Owner = owner;
        }

        public virtual void StartExtension() { }
        public virtual void Dispose() { }

        public virtual void Enable()
        {
            Enabled = true;
        }

        public virtual void Disable()
        {
            Enabled = false;
        }
        
        public virtual void Update() {}
        public virtual void FixedUpdate() {}
        
    }
}