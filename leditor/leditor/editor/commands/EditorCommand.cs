using System.Reflection;
using leditor.root;

namespace leditor.actions
{
    public class EditorCommand
    {
        public readonly string Key;
        public readonly string Description;
        public readonly MethodBase Callback;
        public readonly ParameterInfo[] Args;
        public readonly Type[] ArgsAsType;

        public bool Valid { get; private set; } = false;
        
        public EditorCommand(MethodInfo commandBase)
        {
            if(commandBase.DeclaringType != typeof(EditorCommandsContainer)) return;
            if(commandBase.ReturnType != typeof(string))
            {
#if UNITY_EDITOR
                Debug.LogError($"[CONSOLE COMMAND] TRYING TO CREATE COMMAND WITHOUT STRING OUTPUT; COMMAND NAME{commandBase.Name}");
#endif
                
                return;
            }

            var keyAttribute = commandBase.GetCustomAttribute<CommandBaseAttribute>();
            
            if (keyAttribute == null)
            {
                Key = commandBase.Name;
            }
            else
            {
                Key = keyAttribute.Key;
            }

            var descriptionAttribute = commandBase.GetCustomAttribute<CommandDescription>();
            if(descriptionAttribute != null)
            {
                Description = descriptionAttribute.Description;
            }

            Callback = commandBase;

            Args = commandBase.GetParameters();
            
            List<Type> argTypes = new();
            
            foreach (ParameterInfo parameterInfo in Args)
            {
                argTypes.Add(parameterInfo.ParameterType);
            }

            ArgsAsType = argTypes.ToArray();
            
            Valid = true;
        }
        
        public string TryInvoke(object[] args)
        {
            if(!Valid) return "[!] Command invocation failed; Invalid command";
            
            if (args.Length != 0 && args[0] as string == "help")
            {
                return GetCommandPattern() + ". " + Description;
            }

            if (args.Length != Args.Length)
            {
                return "[!] Command invocation failed; Invalid arguments";
            }

            for (int index = 0; index < args.Length; index++)
            {
                object arg = args[index];
                
                if (arg.GetType() != Args[index].ParameterType)
                {
                    if(arg is Single s && Args[index].ParameterType == typeof(int))
                    {
                        args[index] = (int)s;
                        continue;
                    }
                    if(arg is int i && Args[index].ParameterType == typeof(Single))
                    {
                        args[index] = (float)i;
                        continue;
                    }
                    
                    return $"Invalid argument: {arg.GetType()}. {Args[index].ParameterType} was expected";
                }
            }
            
            return Callback.Invoke(App.LeditorInstance.Commands.Container, args) as string;
        }

        public string GetCommandPattern()
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

    public enum ActionScope
    {
        InProject,
        InTilemap
    }
}