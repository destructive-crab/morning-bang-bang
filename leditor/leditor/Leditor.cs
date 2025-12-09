using leditor.actions;
using leditor.UI;
using SFML.System;
using SFML.Window;

namespace leditor.root;

public sealed class Leditor
{
    public ProjectData Project => ProjectEnvironment.Project;
    public GridBuffer?  buffer;
    
    private readonly Dictionary<string, GridBuffer> buffers = new();

    private readonly HotkeysSystem Hotkeys = new();
    public EditorCommandsSystem    Commands;

    public EditorDisplay CurrentDisplay { get; private set; }
    
    public readonly HomeDisplay    HomeDisplay;
    public ProjectDisplay          ProjectDisplay;

    public ProjectEnvironment ProjectEnvironment { get; private set; } = new();

    private float currentZoom = 0;

    public Leditor()
    {
        Commands = new EditorCommandsSystem();
        Commands.BuildGUI();
        
        Hotkeys.Bind(WriteCurrentBufferChanges, Keyboard.Key.LControl, Keyboard.Key.S);
        
        //
        HomeDisplay = new HomeDisplay();
        CurrentDisplay = HomeDisplay;
        
        //_window.Closed += (_, _) => _window.Close();
        App.WindowHandler.window.Resized += 
            (_, args) => CurrentDisplay?.Host?.SetSize(new Vector2f(args.Width, args.Height));
 //       _window.KeyPressed +=
 //           (_, args) => _host.OnKeyPressed(args.Code);
 //       _window.TextEntered +=
 //           (_, args) => _host.OnTextEntered(args.Unicode);
 //       _window.MouseButtonReleased += (_, args) =>
 //       {
 //           if (args.Button != Mouse.Button.Left) return;
 //           _host.OnMouseClick(new Vector2f(args.X, args.Y));
 //       }; 
    }

    public GridBuffer OpenBuffer(string tag)
    {
        if (buffers.ContainsKey(tag))
        {
            buffer = buffers[tag];
            return buffer;
        }
        else
        {
            GridBuffer newBuffer = new(tag);
            
            buffers.Add(tag, newBuffer);
            buffer = newBuffer;
    
            return buffer;            
        }
    }
    
    public bool OpenBuffer(string tag, out GridBuffer buffer)
    {
        bool isNewlyCreated = false;
        
        if (buffers.ContainsKey(tag))
        {
            isNewlyCreated = false;
        }
        else
        {
            isNewlyCreated = true;
        }
        
        buffer = OpenBuffer(tag);
        return isNewlyCreated;
    }
    
    private void WriteCurrentBufferChanges()
    {
        foreach (KeyValuePair<string, GridBuffer> pair in buffers)
        {
            //todo
 //           ProjectEnvironment.GetMap(pair.Value.Tag).RewriteWith(pair.Value.Get);
        }
        ProjectEnvironment.SaveProject();
    }

    public void OpenProject(string path)
    {
        CloseProject();
        
        if(path != string.Empty) ProjectEnvironment.OpenProjectAtPath(path);
        else                     ProjectEnvironment.OpenEmptyProject();
    }

    public void CloseProject()
    {
        if (!ProjectEnvironment.IsProjectAvailable) return;

        ProjectEnvironment.ClearEnvironment();
        ClearBuffers();
        ProjectDisplay = null;
        CurrentDisplay = HomeDisplay;
    }

    private void ClearBuffers()
    {
        buffers.Clear();
        buffer = null;
    }

    public void DoLoop()
    {
        while (App.WindowHandler.IsOpen)
        {
            if(ProjectEnvironment.IsProjectAvailable) ProjectEnvironment.Project.PullEdits();
            
            App.WindowHandler.BeginFrame();
            {
                if (App.WindowHandler.InputsHandler.IsKeyDown(Keyboard.Key.Space))
                {
                    Commands.OpenInvokeMenu();
                }
        
                Hotkeys.Update();
                CurrentDisplay.Host.SetSize(new Vector2f(App.WindowHandler.Width, App.WindowHandler.Height));
                CurrentDisplay.Host.Update(App.WindowHandler.window);
                CurrentDisplay.Tick();
                
                //drawing
                App.WindowHandler.BeginDrawing();
                {
                    if (Project != null && buffer != null)
                    {
                        buffer.UpdateBufferOutput(ProjectEnvironment);
                    }
                    if (Project != null && ProjectDisplay == null)
                    {
                        ProjectDisplay = new ProjectDisplay(ProjectEnvironment);

                        CurrentDisplay = ProjectDisplay;
                    }
                    else if(Project == null)
                    {
                        CurrentDisplay = HomeDisplay;
                    }
                            
                    App.WindowHandler.BeginGUIMode();
                    {
                        CurrentDisplay.Host.Draw(App.WindowHandler.window);
                    }
                    App.WindowHandler.CompleteGUIMode();
                }
                App.WindowHandler.CompleteDrawing();   
            }
            App.WindowHandler.CompleteFrame();
        }
    }
}