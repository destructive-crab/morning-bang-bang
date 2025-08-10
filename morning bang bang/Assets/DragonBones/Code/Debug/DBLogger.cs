using System.Collections.Generic;
using System.Linq;
using banging_code.debug;
using DragonBones.Code.Debug;

namespace DragonBones
{
    internal static class DBLogger
    {
        public static ArmatureBuildLog BLog;
        private static List<ArmatureBuildLog> Logs = new();

        private const string CHILD_BUILD_LOG_MARK = "CHILD_BUILD_LOG_HERE:";
        public static ArmatureBuildLog StartNewArmatureBuildLog(string name)
        {
            Logs.Add(new ArmatureBuildLog(name));
            if (BLog != null)
            {
                Logs.Last().Parent = BLog;
                BLog.AddEntry(CHILD_BUILD_LOG_MARK, name);
            }
            BLog = Logs.Last();               
            return BLog;
        }

        public static bool FinishArmatureBuildLog()
        {
            BLog.FinishLog();
            BLog = BLog.Parent;
            return true;
        }

        public static void PrintBuildLog(string name)
        {
            string resultLog = "";
            foreach (ArmatureBuildLog log in Logs)
            {
                if (log.name == name)
                {

                    ArmatureBuildLog prevLog = null;
                    var currentBuildLog = log;
                    do
                    {
                        if (resultLog != "")
                        {
                            resultLog = currentBuildLog.ToString()
                                .Replace(CHILD_BUILD_LOG_MARK + prevLog.name, resultLog);
                        }
                        else
                        {
                            resultLog = currentBuildLog.ToString();
                        }

                        prevLog = currentBuildLog;
                        currentBuildLog = log.Parent;
                        
                    } while (currentBuildLog != null);
                }
            }

            LogMessage(resultLog);
        }
        
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