namespace DragonBones
{
    public class Dirty<TType>
    {
        public bool IsDirty { get; private set; }
        public bool NotDirty => !IsDirty;
        
        public TType V { get; private set; }
        public TType pV { get; private set; }

        public Dirty() { }
        public Dirty(TType initialValue) { Set(initialValue);}
        
        public void MarkAsDirty() => IsDirty = true;
        public void ResetDirty() => IsDirty = false;

        public void Set(TType newValue)
        {
            if (newValue.Equals(V))
            {
                return;
            }

            pV = V;
            V = newValue;
            
            MarkAsDirty(); 
        }

        public TType GetAndChange()
        {
            MarkAsDirty();
            return V;
        }

        public bool Eq(object o)
        {
            return V.Equals(o);
        }
    }
}