using MothDIed;

namespace banging_code.debug
{
    public static class LGR
    {
        private static BangDebugger debugger;
        
        //means push message
        public static void PM(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            return;
#endif
            if (LGR.debugger == null && Game.TryGetDebugger(out BangDebugger debugger))
            {
                LGR.debugger = debugger;
            }
            else
            {
                return;
            }
            
            debugger.Console.InvokeCommand(true, "echo", message);
        }

        public static void PERR(string error)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(error);
            return;
#endif
            if (LGR.debugger == null && Game.TryGetDebugger(out BangDebugger debugger))
            {
                LGR.debugger = debugger;
            }
            else
            {
                return;
            }

            debugger.Console.InvokeCommand(true, "echo_error", error);
        }
        public static void PW(string warning)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(warning);
            return;
#endif
            if (LGR.debugger == null && Game.TryGetDebugger(out BangDebugger debugger))
            {
                LGR.debugger = debugger;
            }
            else
            {
                return;
            }

            debugger.Console.InvokeCommand(true, "echo_warning", warning);
        }
    }
}