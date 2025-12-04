using System.Reflection;
using leditor.UI;

namespace leditor.root;

public abstract class CustomDataDrawer<TData>
    where TData : LEditorDataUnit, new()
{
    public event Action<FieldInfo> OnChanged;

    public abstract AUIElement Build();
}

public sealed class TextureDataDrawer : CustomDataDrawer<TextureData>
{
    private UIEntry idEntry;

    private AxisBox pathContainer;
    private UIEntry pathEntry;
    private UIButton choosePathButton;

    private AxisBox startContainer;
    private UIEntry startXEntry;
    private UIEntry startYEntry;

    private AxisBox sizeContainer;
    private UIEntry widthEntry;
    private UIEntry heightEntry;

    private AxisBox pivotBox;
    private UIEntry pivotXEntry;
    private UIEntry pivotYEntry;
    
    private readonly UIHost host;
    private readonly TextureData data;

    public TextureDataDrawer(UIHost host, TextureData data)
    {
        this.host = host;
        this.data = data;
    }

    public override AUIElement Build()
    {
        AxisBox r = new(host, UIAxis.Vertical);
        
        //texture.ID
        idEntry = new UIEntry(host, new UIVar<string>(data.ID, (value) =>
        {
            data.ID = value;
            DataChange();
        }));
        pathContainer.AddChild(idEntry);
        
        //texture.Path
        pathEntry = new UIEntry(host, new UIVar<string>(data.PathToTexture, 
            (value) =>
            {
                data.PathToTexture = value;
                DataChange();
            }), 215);
        
        choosePathButton = new UIButton(host, "Choose", () =>
        {
            string path = UTLS.OpenChooseFileDialog();
            if (path != String.Empty)
            {
                pathEntry.Var.Value = path;
            }
            DataChange();
        });
        
        pathContainer = new AxisBox(host, UIAxis.Horizontal, pathEntry, choosePathButton);
        r.AddChild(pathContainer);

        int axisWidth = 120;
        //texture.StartPixels
        startXEntry = new UIEntry(host, new UIVar<string>(data.StartX.ToString(), (value) =>
        {
            data.StartX = Int32.Parse(value);
            DataChange();
        }), axisWidth);
        startYEntry = new UIEntry(host, new UIVar<string>(data.StartY.ToString(), (value) =>
        {
            data.StartY = Int32.Parse(value);
            DataChange();
        }), axisWidth);

        r.AddChild(BuildXYEntry(host, "Texture Start", startXEntry, startYEntry));

        widthEntry = new UIEntry(host, new UIVar<string>(data.Width.ToString(), (value) =>
        {
            data.Width = Int32.Parse(value);
            DataChange();
        }), axisWidth);
        
        heightEntry = new UIEntry(host, new UIVar<string>(data.Height.ToString(), (value) =>
        {
            data.Height = Int32.Parse(value);
            DataChange();
        }), axisWidth);

        r.AddChild(BuildXYEntry(host, "Texture Size", widthEntry, heightEntry));
        
        pivotXEntry= new UIEntry(host, new UIVar<string>(data.PivotX.ToString(), (value) =>
        {
            data.PivotX = Int32.Parse(value);
            DataChange();
        }), axisWidth);
        
        pivotYEntry = new UIEntry(host, new UIVar<string>(data.PivotY.ToString(), (value) =>
        {
            data.PivotY = Int32.Parse(value);
            DataChange();
        }), axisWidth);

        r.AddChild(BuildXYEntry(host, "Pivot", pivotXEntry, pivotYEntry));
        
        return null;
    }

    private AUIElement BuildXYEntry(UIHost host, string label, UIEntry x, UIEntry y)
    {
        AxisBox root = new(host, UIAxis.Vertical);
        root.AddChild(new UILabel(host, label));
        root.AddChild(new AxisBox(host, UIAxis.Horizontal, 
            new UILabel(host, "X:"), x,
            new UILabel(host, "Y:"), y));

        return root;
    }

    private void DataChange()
    {
        data.ValidateExternalDataChange();
    }
}