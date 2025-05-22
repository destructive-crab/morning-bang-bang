using System;

namespace banging_code.debug.Console
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)] 
    public class ConsoleCommandDescriptionAttribute : Attribute
    {
        public readonly string Description;

        public ConsoleCommandDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}