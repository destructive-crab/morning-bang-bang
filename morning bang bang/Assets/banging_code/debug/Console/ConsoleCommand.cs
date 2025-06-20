using System;
using System.Collections.Generic;
using System.Reflection;
using MothDIed;
using UnityEngine;

namespace banging_code.debug.Console
{
    public class ConsoleCommand
    {
        public readonly string Key;
        public readonly string Description;
        public readonly MethodBase Callback;
        public readonly ParameterInfo[] Args;
        public readonly Type[] ArgsAsType;

        public bool Valid { get; private set; } = false;
        
        public ConsoleCommand(MethodInfo commandBase)
        {
            if(commandBase.DeclaringType != typeof(ConsoleCommandsContainer)) return;
            if(commandBase.ReturnType != typeof(string))
            {
                Debug.Log($"TRYING TO ADD COMMAND WITHOUT STRING OUTPUT; COMMAND NAME{commandBase.Name}");
                return;
            }

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

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                
                if (arg.GetType() != Args[i].ParameterType)
                {
                    return $"Invalid argument: {arg.GetType()}. {Args[i].ParameterType} was expected";
                }
            }
            
            return Callback.Invoke(Game.GetDebugger().Console.Container, args) as string;
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
}