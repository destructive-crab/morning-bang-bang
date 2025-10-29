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
#if UNITY_EDITOR
                Debug.LogError($"[CONSOLE COMMAND] TRYING TO CREATE COMMAND WITHOUT STRING OUTPUT; COMMAND NAME{commandBase.Name}");
#endif
                
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
            
            return Callback.Invoke(Game.G<BangDebugger>().Console.Container, args) as string;
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