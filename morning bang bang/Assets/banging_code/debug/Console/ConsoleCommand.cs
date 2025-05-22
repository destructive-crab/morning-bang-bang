using System.Reflection;

namespace banging_code.debug.Console
{
    public class ConsoleCommand
    {
        public readonly string Key;
        public readonly string Description;
        public readonly MethodBase Callback;
        public readonly ParameterInfo[] Args;

        public bool Valid { get; private set; } = false;
        
        public ConsoleCommand(MethodInfo commandBase)
        {
            if(commandBase.DeclaringType != typeof(ConsoleCommandsContainer)) return;
            
            var keyAttribute = commandBase.GetCustomAttribute<ConsoleCommandKeyAttribute>();
            
            if (keyAttribute == null)
            {
                Key = commandBase.Name;
            }
            else
            {
                Key = keyAttribute.Key;
            }

            var descriptionAttribute = commandBase.GetCustomAttribute<ConsoleCommandDescriptionAttribute>();
            if(descriptionAttribute != null)
            {
                Description = descriptionAttribute.Description;
            }

            Callback = commandBase;

            Args = commandBase.GetParameters();
            Valid = true;
        }
        
        public void TryInvoke(object[] args)
        {
            if(!Valid) return;
            
            if (args.Length != 0 && args[0] as string == "help")
            {
                BangingConsole.Instance.PushMessage(GetCommandPattern() + ". " + Description);
                return;
            }

            if (args.Length != Args.Length)
            {
                BangingConsole.Instance.PushError($"Invalid arguments count");
                return;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                
                if (arg.GetType() != Args[i].ParameterType)
                {
                    BangingConsole.Instance.PushError($"Invalid argument: {arg.GetType()}. {Args[i].ParameterType} was expected");
                    return;
                }
            }
            
            Callback.Invoke(BangingConsole.Instance.Container, args);
        }

        private string GetCommandPattern()
        {
            string args = "";

            if (Args.Length != 0)
            {
                args += "[";
                for (var i = 0; i < Args.Length; i++)
                {
                    var arg = Args[i];
    
                    if (i > 0)
                    {
                        args += ";";
                    }
                    
                    args += arg.ParameterType;
                    args += " ";
                    args += arg.Name;
                }
                args += "]";
            }
            
            return Key + " " + args;
        }
    }
}