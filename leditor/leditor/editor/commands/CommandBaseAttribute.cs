namespace leditor.actions
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)] 
    public class CommandBaseAttribute : Attribute
    {
        public readonly string Key;
        public readonly ActionScope Scope;


        public CommandBaseAttribute(string key, ActionScope scope)
        {
            Key = key;
            Scope = scope;
        }
    }
}