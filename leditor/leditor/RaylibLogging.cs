using System.Runtime.InteropServices;
using Raylib_cs;

namespace leditor.root;

public static class RaylibLogging
{
    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static unsafe void LogCustom(int logLevel, sbyte* text, sbyte* args)
    {
        var message = Logging.GetLogMessage(new IntPtr(text), new IntPtr(args));
        
        // where is my matches... ;(
        switch ((TraceLogLevel)logLevel)
        {
            case TraceLogLevel.Warning: Logger.Warn(message, "raylib", -1); break;
            
            case TraceLogLevel.Error:
            case TraceLogLevel.Fatal: Logger.Error(message, "raylib", -1); break;
            
            case TraceLogLevel.Trace:
            case TraceLogLevel.Debug: Logger.Debug(message, "raylib", -1); break;
        };
    }

    public static void Setup()
    {
        unsafe
        {
            Raylib.SetTraceLogCallback(&LogCustom);
        }
    }
}