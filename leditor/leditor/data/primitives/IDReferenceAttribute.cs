namespace leditor.root;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class IDReferenceAttribute : Attribute
{
    public Type ReferenceTo;

    public IDReferenceAttribute(Type referenceTo)
    {
        ReferenceTo = referenceTo;
    }
}