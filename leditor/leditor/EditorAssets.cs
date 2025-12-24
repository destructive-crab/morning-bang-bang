using System.Reflection;

namespace leditor.root;

public static class EditorAssets
{
    private const string assetsFolder = "leditor.assets";
    private const string fontsFolder = assetsFolder + ".fonts";
    
    private const string iconssFolder = assetsFolder + ".icons";
    
    private static readonly HashSet<string> embeddedResources = new();

    public static void Initialize()
    {
        string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        foreach (string name in names)
        {
            embeddedResources.Add(name);
            Console.WriteLine(name);
        }
    }

    public static Stream? Load(string name)
    {
        if (!embeddedResources.Contains(name)) return null;
        
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        return stream;
    }

    public static Stream? LoadFont(string name) => Load(fontsFolder + $".{name}");
    public static Stream? LoadIcon(string name) => Load(iconssFolder + $".{name}");
}