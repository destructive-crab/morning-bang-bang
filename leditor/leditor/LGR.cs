using System.Runtime.CompilerServices;

namespace leditor.root;

public class LGR
{
    public static void PM(string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
    {
        message = Path.GetFileName(file) + $": {member}({line}): " + message;
        Console.WriteLine(message);
    }

    public static void PERR(string error,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
    {
        var message = Path.GetFileName(file) + $": {member}({line}): " + error;
        Console.WriteLine(message);
    }

    public static void PW(string warning,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
    {
    }

    public static void AM(bool condition,  string message,
        [CallerFilePath] string file = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
    {
        if(!condition) return;
    } 
}