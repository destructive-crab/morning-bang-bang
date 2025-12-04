namespace leditor.actions
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)] 
    public class CommandDescription : Attribute
    {
        public readonly string Description;

        public CommandDescription(string description)
        {
            Description = description;
        }
    }
}