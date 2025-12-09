using System.Reflection;
using System.Runtime.InteropServices;
using leditor.UI;

namespace leditor.root;

public static class UTLS
{
    public static Rect FULL = new(0, 0, -1, -1);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }
    
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    public static string ShowOpenProjectDialog()
    {
        var ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        
        ofn.lpstrFilter = "Leditor Project Files (*.lep)\0*.lep\0All Files (*.*)\0*.*\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "Open Project...";
        
        if (GetOpenFileName(ref ofn))
            return ofn.lpstrFile;
        
        return string.Empty;
    }

    public static string OpenSaveProjectDialog()
    {
        var ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        
        ofn.lpstrFilter = "Leditor Project Files (*.lep)\0*.lep\0All Files (*.*)\0*.*\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "Save Project...";
        
        if (GetOpenFileName(ref ofn))
            return ofn.lpstrFile;
        
        return string.Empty;
    }

    public static string OpenChooseFileDialog()
    {
        var ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        
        ofn.lpstrFilter = "(*.png)\0*.png\0All Files (*.*)\0*.*\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "Choose...";
        
        if (GetOpenFileName(ref ofn))
            return ofn.lpstrFile;
        
        return string.Empty;
    }

    public static IOrderedEnumerable<FieldInfo> GetDeriveOrderedFields(Type type)
    {
        Dictionary<Type, int> lookup = new Dictionary<Type, int>();

        int order = 0;
        lookup[type] = order++;
        Type parent = type.BaseType;
        while (parent != null)
        {
            lookup[parent] = order;
            order++;
            parent = parent.BaseType;
        }

        return type.GetFields()
            .OrderByDescending(prop => lookup[prop.DeclaringType]); 
    }
    
    public static bool ValidateRegarding(LEditorDataUnit toValidate, LEditorDataUnit[] regard)
    {
        foreach (LEditorDataUnit other in regard)
        {
            if (other.ID == toValidate.ID && other != toValidate) return false;
        }
        
        return toValidate.ValidateExternalDataChange();
    }

    public static UISelectionList BuildLayersList(bool singleSelection, Action<string> selectAction)
    {
        UISelectionList layersChooser = new(App.UIHost);
        foreach (string layer in MapData.AllLayers)
        {
            layersChooser.AddChild(new UIOption(App.UIHost, layer, () =>
            {
                selectAction?.Invoke(layer);
            }, false));
        }
        layersChooser.IsSingleSelection = singleSelection;
        return layersChooser;
    }

    public static bool ValidString(string s)
    {
        if (s.Length == 0) return false;
        
        char[] availableSymbols = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_'];
        
        foreach (char c in s)
        {
            if (!availableSymbols.Contains(c))
            {
                return false;
            }
        }

        return true;
    }

    public static bool ValidStrings(params string[] ss)
    {
        foreach (string s in ss)
        {
            if (!ValidString(s)) return false;
        }

        return true;
    }
}