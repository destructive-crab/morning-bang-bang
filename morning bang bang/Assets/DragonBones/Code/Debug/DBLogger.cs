using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DragonBones.Code.Debug;

namespace DragonBones
{
    internal static class DBLogger
    {
        public static ArmatureBuildLog BLog;
        private static List<ArmatureBuildLog> Logs = new();

        private const string PREFIX = "[DB] ";
        private static List<string> Log;
        
        private const string CHILD_BUILD_LOG_MARK = "CHILD_BUILD_LOG_HERE:";
        public static ArmatureBuildLog StartNewArmatureBuildLog(string name)
        {
            Logs.Add(new ArmatureBuildLog(name));
            if (BLog != null)
            {
                Logs.Last().Parent = BLog;
                BLog.Children.Add(Logs.Last());
                BLog.AddEntry(CHILD_BUILD_LOG_MARK, name);
            }
            BLog = Logs.Last();               
            return BLog;
        }

        public static string FinishArmatureBuildLog()
        {
            string name = BLog.name;
            BLog.FinishLog();
            BLog = BLog.Parent;
            return name;
        }

        public static void PrintBuildLog(string name)
        {
            string resultLog = "";
            foreach (ArmatureBuildLog log in Logs)
            {
                if (name == log.name) resultLog = GetBuildLogOutput(log);
            }

            LogMessage(resultLog);
        }

        private static string GetBuildLogOutput(ArmatureBuildLog log)
        {
            string resultLog = log.ToString();

            foreach (ArmatureBuildLog armatureBuildLog in log.Children)
            {
                resultLog = resultLog.Replace(CHILD_BUILD_LOG_MARK + armatureBuildLog.name, GetBuildLogOutput(armatureBuildLog));
            }
            
            return resultLog;
        }
        
        internal static void LogMessage(object message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            AddToLog(PREFIX + message, file, member, line);
        }

        private static void AddToLog(string message, string file, string member, int line)
        {
            message = Path.GetFileName(file) + $": {member}({line}): " + message;
            
            Log.Add(message);
        }

        internal static void Warn(object message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            AddToLog(PREFIX + message, file, member, line);
        }

        internal static void Assert(bool condition, string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            if(condition)
            {
                AddToLog(PREFIX + message, file, member, line);
            }
        }

        public static void Error(string error,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            AddToLog(PREFIX + error, file, member, line);
        }
    }
}