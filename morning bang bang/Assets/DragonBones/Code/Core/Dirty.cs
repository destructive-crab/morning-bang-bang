namespace DragonBones
{
    public class Dirty<TType>
    {
        public bool IsDirty { get; private set; }
        public bool NotDirty => !IsDirty;
        public TType V;

        public Dirty() { }
        public Dirty(TType initialValue) { Set(initialValue);}
        
        public void MarkAsDirty() => IsDirty = true;
        public void ResetDirty() => IsDirty = false;

        public void Set(TType newValue)
        {
            V = newValue;
            MarkAsDirty(); 
        }

        public TType GetAndChange()
        {
            MarkAsDirty();
            return V;
        }
    }
}