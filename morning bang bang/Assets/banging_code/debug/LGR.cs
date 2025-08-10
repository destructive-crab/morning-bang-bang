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
        public static void PM(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            return;
#endif
            TryPrintInConsole(message, ECHO);
        }

        public static void PERR(string error)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(error);
            return;
#endif
            TryPrintInConsole(error, ECHO_ERROR);
        }

        public static void PW(string warning)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(warning);
            return;
#endif
            TryPrintInConsole(warning, ECHO_WARNING);
        }

        public static void AM(bool condition,  string message)
        {
            if(!condition) return;
            
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            return;
#endif
            TryPrintInConsole(message, ECHO);
        }

        private static void TryPrintInConsole(string message, string command)
        {
            if (LGR.debugger == null && Game.TryGetDebugger(out BangDebugger debugger))
            {
                LGR.debugger = debugger;
            }
            else if(LGR.debugger == null)
            {
                return;
            }

            LGR.debugger.Console.InvokeCommand(true, command, message);
        }
    }
}