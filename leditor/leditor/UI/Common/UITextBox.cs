using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UITextBox : AUIElement
{
    public string Text
    {
        get => textOriginal.DisplayedString;
        set
        {
            displayString = value;
            UpdateLayout();
        }
    }

    private Text textOriginal;
    private List<Text> totalLines = new();
    private int linesCount = -1;
    private string displayString;
    private readonly bool stretchWidth;

    public UITextBox(UIHost host, string text = "", bool stretchWidth = false) : 
        base(host, default)
    {
        this.textOriginal = host.Fabric.MakeText("X");
        displayString = text;
        this.stretchWidth = stretchWidth;
    }

    private int currentWidth;
    private string currentDisplaying;

    public override void UpdateLayout()
    {
        textOriginal.Position = Rect.Position;

        int textWidth = (int)Rect.Width;
        
        if (textWidth != currentWidth || currentDisplaying != displayString)
        {
            currentWidth = textWidth;
            BuildLines();
        }

        if (linesCount > 0 && totalLines[0].Position != Rect.Position || currentDisplaying != displayString)
        {
            currentDisplaying = displayString;
            UpdatePositions();
        }
    }

    private void BuildLines()
    {
        int textWidth = (int)Rect.Width;
        List<string> lines = new();
        string[] words = displayString.Split(" ");

        textOriginal.DisplayedString = words[0];
        for (var i = 1; i < words.Length; i++)
        {
            string word = words[i];
            string prevStr = textOriginal.DisplayedString;

            textOriginal.DisplayedString += " " + word;
            
            if (textOriginal.GetGlobalBounds().Size.X >= textWidth)
            {
                lines.Add(prevStr);
                textOriginal.DisplayedString = word;
            }
        }
        if(textOriginal.DisplayedString != "")
        {
            lines.Add(textOriginal.DisplayedString);
        }

        textOriginal.DisplayedString = "";

        Text prev = null;
        Vector2f minimalSize = new Vector2f(textWidth, 0);
        
        for (var i = 0; i < lines.Count; i++)
        {
            string line = lines[i];

            if (stretchWidth)
            {
                int spacesCount = line.Count((c) => c == ' ');
                string onlyWords = line.Replace(" ", "");
                textOriginal.DisplayedString = onlyWords;
                int onlyWordsWidth = (int)textOriginal.GetGlobalBounds().Size.X;
                int forOneSpace = (textWidth - onlyWordsWidth) / spacesCount;
                string resSpacing = " ";
                for (int j = 0; true; j++)
                {
                    string spacing = "";
                    for (int k = 0; k < j; k++)
                    {
                        spacing += " ";
                    }
                    textOriginal.DisplayedString = spacing;
                    if (textOriginal.GetGlobalBounds().Size.X > forOneSpace)
                    {
                        resSpacing = spacing;
                        break;
                    }
                }
                
                line = line.Replace(" ", resSpacing);
                for (var index = 0; index < line.Length; index++)
                {
                    var c = line[index];
                    if (c == ' ')
                    {
                        line = line.Remove(index, 1);
                        if (index + 1 < line.Length && line[index+1] == ' ')
                        {
                            line = line.Remove(index + 1, 1);
                        }
                        break;
                    }
                }
            }
            
            if (i >= totalLines.Count) totalLines.Add(new Text(textOriginal));

            totalLines[i].DisplayedString = line;
            
            prev = totalLines[i];
            minimalSize.Y += prev.GetGlobalBounds().Height;
        }

        linesCount = lines.Count;
        MinimalSize = minimalSize;
        Rect = new FloatRect(Rect.Position, MinimalSize);
        
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        Text prev = null;
        for (var i = 0; i < linesCount; i++)
        {
            if (prev == null)
            {
                totalLines[i].Position = Rect.Position;
            }
            else
            {
                int height = (int)(prev.GetGlobalBounds().Height);
                if (height == 0) height = (int)Host.Style.FontSize()* i;
                
                totalLines[i].Position = prev.Position + new Vector2f(0, height);
            }
            
            prev = totalLines[i];
        }
    }

    public override void Draw(RenderTarget target)
    {
        for (var i = 0; i < linesCount; i++)
        {
            var line = totalLines[i];
            target.Draw(line);
        }
    }
}