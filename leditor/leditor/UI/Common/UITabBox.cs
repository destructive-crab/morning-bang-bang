using leditor.root;
using leditor.UI;
using SFML.Graphics;
using SFML.System;

namespace deadlauncher.Other.UI;

public class UITabBox : AUIBox
{
    private readonly List<AUIElement> children = new();
    private readonly Dictionary<AUIElement, string> tabNamesMap = new();
    
    private readonly List<ClickArea> tabSwitchers = new();
    private readonly Dictionary<AUIElement, ClickArea> tabSwitchersMap = new();

    private AUIElement activeElement;
    
    private RectangleShape backgroundOutline;
    private RectangleShape background;
    private RectangleShape separator;
    
    private RectangleShape tabBackground;
    private Text tabText;
    
    public UITabBox(UIHost host, params KeyValuePair<AUIElement, string>[] children) : base(host, default)
    {
        backgroundOutline = new RectangleShape();
        background        = new RectangleShape();
        separator         = new RectangleShape();
        tabText           = new Text();
        tabBackground     = new RectangleShape();

        backgroundOutline.FillColor = App.UIHost.Style.OutlineColor();
        background.       FillColor = App.UIHost.Style.FirstBackgroundColor();
        separator.        FillColor = App.UIHost.Style.OutlineColor();

        tabText.          FillColor = App.UIHost.Style.NormalButton().TopColor;
        
        tabText.Font  = App.UIHost.Style.Font();
        tabText.Style = Text.Styles.Bold;
        tabText.CharacterSize = (uint)(App.UIHost.Style.TabLineHeight() - 6);
        
        foreach (KeyValuePair<AUIElement, string> element in children)
        {
            AddChild(element.Key, element.Value);
        }
        
        SetActive(this.children[0]);
        UpdateLayoutP();
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(backgroundOutline);
        target.Draw(background);
        
        foreach (KeyValuePair<AUIElement,string> pair in tabNamesMap)
        {
            AUIElement c = pair.Key;
            string     n = pair.Value;

            ClickArea switcher = tabSwitchersMap[c];

            tabBackground.Position = switcher.Rect.Position;
            tabBackground.Size = switcher.Rect.Size;

            if (pair.Key == activeElement)
            {
                tabBackground.FillColor = Host.Style.FirstBackgroundColor();
            }
            else
            {
                tabBackground.FillColor = Host.Style.SecondBackgroundColor();
            }
            
            separator.Position = tabBackground.Position + new Vector2f(tabBackground.Size.X, 0);

            tabText.DisplayedString = pair.Value;
            tabText.Position   = switcher.Rect.Position + new Vector2f(switcher.Rect.Size.X / 2 - tabText.GetGlobalBounds().Size.X / 2, 0);
            
            target.Draw(tabBackground);
            target.Draw(separator);
            target.Draw(tabText);
        }
        
        activeElement.Draw(target);
    }

    public override void ProcessClicks()
    {
        activeElement.ProcessClicks();
        
        foreach (ClickArea switcher in tabSwitchers)
        {
            Host.Areas.Process(switcher);
        }
    }

    public void SetActive(AUIElement element)
    {
        if (children.Contains(element))
        {
            activeElement = element;
        }
        
        UpdateLayoutP();
    }

    protected override void UpdateLayout()
    {
        int outline = App.UIHost.Style.BaseOutline();
        
        backgroundOutline.Position = Rect.Position - new Vector2f(outline, outline);
        backgroundOutline.Size     = Rect.Size     + new Vector2f(outline * 2, outline * 2);

        background.Position = Rect.Position;
        background.Size     = Rect.Size;
        
        int tabSwitcherWidth = (int)((Rect.Width - outline * (children.Count-1)) / children.Count);

        for (var i = 0; i < children.Count; i++)
        {
            FloatRect rect = new FloatRect(
                Rect.Position + new Vector2f((tabSwitcherWidth + outline)*i, 0),
                new Vector2f(tabSwitcherWidth, Host.Style.TabLineHeight()));

            ClickArea switcher = tabSwitchersMap[children[i]];
            switcher.Rect = rect;
            
        }

        separator.Size = new Vector2f(outline, Host.Style.TabLineHeight());

        if(activeElement != null)
        {
            activeElement.SetRect(new FloatRect(
                Rect.Position.X + outline, Rect.Position.Y + Host.Style.TabLineHeight() + outline, 
                Rect.Size.X - outline * 2, Rect.Size.Y - Host.Style.TabLineHeight()));
        }
    }

    public void AddChild(AUIElement child, string tabName)
    {
        children   .Add(child);
        tabNamesMap.Add(child, tabName);

        ClickArea switcher = new ClickArea(default, true);
        tabSwitchers.Add(switcher);
        tabSwitchersMap.Add(child, switcher);
        
        switcher.OnRightMouseButtonClick += () =>
        {
            SetActive(child);
        };
        
        if(activeElement != null) UpdateLayout();
    }

    public override void RemoveChild(AUIElement child)
    {
        children.Remove(child);
        tabNamesMap.Remove(child);
        
        if(activeElement != null) UpdateLayout();
    }

    public void Rename(AUIElement child, string tabName)
    {
        if (!children.Contains(child)) return;
            
        tabNamesMap[child] = tabName;
    }

    public override IEnumerable<AUIElement> GetChildren() => children.ToArray();
    protected override void UpdateMinimalSize() { }
}