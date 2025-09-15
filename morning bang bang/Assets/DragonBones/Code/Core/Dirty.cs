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
            if ((V == null && newValue == null) || (V != null && V.Equals(newValue)))
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
            return o == (object)V;
        }
    }
}