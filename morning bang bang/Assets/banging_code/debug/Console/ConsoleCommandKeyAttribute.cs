using System;

namespace banging_code.debug.Console
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)] 
    public class ConsoleCommandKeyAttribute : Attribute
    {
        public readonly string Key;

        public ConsoleCommandKeyAttribute(string key)
        {
            Key = key;
        }
    }
}