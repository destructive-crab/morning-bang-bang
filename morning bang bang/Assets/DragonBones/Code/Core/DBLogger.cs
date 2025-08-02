using banging_code.debug;

namespace DragonBones
{
    internal static class DBLogger
    {   
        internal static void LogMessage(object message)
        {
            LGR.PM("[DragonBones] " + message);
        }
        
        internal static void LogWarning(object message)
        {
            LGR.PW("[DragonBones] " + message);
        }

        internal static void Assert(bool condition, string message)
        {
            LGR.AM(condition, "[DragonBones] " + message);
        }
    }
}