using System.Runtime.CompilerServices;

namespace leditor.root;

public static class Logger
{
    public enum Level
    {
        Error = 3,
        Warn = 2,
        Info = 1,
        Debug = 0
    }

    public static Level MinimumLevel = Level.Info;

    private static void Log(
        Level level, string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
        )
    {
        if (level < MinimumLevel)
            return;
        
        // ERROR path/to/file:32
        //       <message>
        //  WARN path/to/file:32
        //       <message>
        //  INFO path/to/file:32
        //       <message>
        // DEBUG path/to/file:32
        //       <message>

        Console.ForegroundColor = level switch
        {
            Level.Error => ConsoleColor.Red,
            Level.Warn  => ConsoleColor.Yellow,
            Level.Info  => ConsoleColor.Blue,
            Level.Debug => ConsoleColor.DarkCyan,
            _           => ConsoleColor.Magenta
        };

        Console.Write(level switch
        {
            Level.Error => "ERROR",
            Level.Warn  => " WARN",
            Level.Info  => " INFO",
            Level.Debug => "DEBUG",
            _           => "  ???"
        });

        Console.ForegroundColor = ConsoleColor.DarkGray;
        
        const string GAP = "      ";
        
        var lineNumberString = lineNumber == -1 ? "--" : lineNumber.ToString();
        Console.WriteLine($" {filePath}:{lineNumberString}");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(GAP);
        Console.WriteLine(message.ReplaceLineEndings($"\n{GAP}"));
        
    }

    public static void Debug(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
    )
    {
        Log(Level.Debug, message, filePath, lineNumber);
    }
    
    public static void Info(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
    )
    {
        Log(Level.Info, message, filePath, lineNumber);
    }
    
    public static void Warn(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
    )
    {
        Log(Level.Warn, message, filePath, lineNumber);
    }
    
    public static void Error(
        string message,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0
    )
    {
        Log(Level.Error, message, filePath, lineNumber);
    }
}