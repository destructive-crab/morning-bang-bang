using System.IO;
using System.Runtime.CompilerServices;
using DragonBones;
using MothDIed;

namespace banging_code.debug
{
    public static class LGR
    {
        private static BangDebugger debugger;

        private const string ECHO = "echo";
        private const string ECHO_ERROR = "echo_error";
        private const string ECHO_WARNING = "echo_warning";
        
        //means push message
        public static void PM(string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            return;
#endif
            TryPrintInConsole(message, ECHO, file, member, line);
        }

        public static void PERR(string error,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(error);
            return;
#endif
            TryPrintInConsole(error, ECHO_ERROR, file, member, line);
        }

        public static void PW(string warning,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(warning);
            return;
#endif
            TryPrintInConsole(warning, ECHO_WARNING, file, member, line);
        }

        public static void AM(bool condition,  string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            if(!condition) return;
            
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            return;
#endif
            TryPrintInConsole(message, ECHO, file, member, line);
        }

        private static void TryPrintInConsole(string message, string command, string file = "", string member = "", int line = 0)
        {
            if (LGR.debugger == null && Game.TryGetDebugger(out BangDebugger debugger))
            {
                LGR.debugger = debugger;
            }
            else if(LGR.debugger == null)
            {
                return;
            }

            message = Path.GetFileName(file) + $": {member}({line}): " + message;

            LGR.debugger.Console.InvokeCommand(true, command, message);
        }

        public static void PARRAY(object[] array,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            string output = "[";
            foreach (object o in array)
            {
                output += o.ToString() + "; ";
            }

            output += "]";
            
            PM(output, file, member, line);
        }
    }
}