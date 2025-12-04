namespace leditor.root;

[AttributeUsage(AttributeTargets.Field)]
public class XYAxisStartAttribute : Attribute
{
    public readonly string LabelName;

    public XYAxisStartAttribute(string labelName)
    {
        LabelName = labelName;
    }
}