using banging_code.camera_logic;
using MothDIed;
using MothDIed.DI;
using MothDIed.InputsHandling;
using MothDIed.Optional.Extensions;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace banging_code.debug.Console
{
    public sealed class ConsoleCommandsContainer
    {
        [ConsoleCommandKey("all")]
        [ConsoleCommandDescription("Prints all commands in console")]
        public string All()
        {
            string output = "";

            foreach (ConsoleCommand consoleCommand in Game.G<BangDebugger>().Console.CommandList)
            {
                output += "\n";
                output += consoleCommand.GetCommandPattern() + ". " + consoleCommand.Description;
            }

            return output;
        }
                
        [ConsoleCommandKey("clear")] 
        [ConsoleCommandDescription("Clears console output")]
        public string Clear()
        {
            return "";
        } 
        
        [ConsoleCommandKey("echo")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public string Echo(string text)
        {
            return text;
        }      
        
        [ConsoleCommandKey("echo_error")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public string EchoError(string text)
        {
            return "[! ERROR OCCURED] " + text + " [! ERROR OCCURED]";
        }      
        
        [ConsoleCommandKey("echo_warning")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public string EchoWarning(string text)
        {
            return "[!] " + text + " [!]";
        }  
        
        [ConsoleCommandKey("show_paths")] 
        [ConsoleCommandDescription("Is debug path's visualisation enabled")]
        public string ShowPaths(bool enabled)
        {
            BangDebugger.Flags.ShowPaths = enabled;
            return "DebugPaths enabled";
        }

        [ConsoleCommandKey("draw_map")]
        [ConsoleCommandDescription("Draws map cells indicators")]
        public string DrawMap()
        {
            Game.G<BangDebugger>().Map.DrawMap();
            return "Map drawn";
        }
        
        [ConsoleCommandKey("hide_map")]
        [ConsoleCommandDescription("Draws map cells indicators")]
        public string HideMap()
        {
            Game.G<BangDebugger>().Map.HideMap();
            return "Map hidden";
        }

        [ConsoleCommandKey("time_scale")]
        public string TimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            return $"timeScale: {timeScale}";
        }
        
        [ConsoleCommandKey("db_registry_state")]
        public string RegistryState()
        {
            return "";
        }

        [ConsoleCommandKey("quit")]
        public string Quit()
        {
            Game.QuitGame();
            return "";
        }

        [ConsoleCommandKey("fps")]
        public string FPS()
        {
            BangDebugger.Flags.ShowFPS = !BangDebugger.Flags.ShowFPS;
            return $"Show FPS: {BangDebugger.Flags.ShowFPS}";
        }
        
         [ConsoleCommandKey("recam")]
         public string CameraRecontrollerSwitch()
         {
             Game.G<SceneSwitcher>().CurrentScene.Modules.StartModule<CameraRecontroller>();
             return $"Recontroller: {Game.G<SceneSwitcher>().CurrentScene.Modules.Get<CameraRecontroller>().Active}";
         }

         [ConsoleCommandKey("show_inputs")]
         public string InputLog()
         {
             string output = "";
             output += "Enabled: " + InputService.Enabled + "\n";
             output += "Current Mode: " + InputService.CurrentMode.ToString() + "\n";
             output += "Previous Mode: " + InputService.PreviousMode.ToString() + "\n";
             output += "Parallels: " + InputService.GetCurrentParallels() + "\n";
             return output;
         }
    }

    internal sealed class CameraRecontroller : SceneModule
    {
        public override void StartModule(Scene scene)
        {
            base.StartModule(scene);
            
            Game.G<SceneSwitcher>().CurrentScene.Modules.StopModule<CCamera>();
        }

        public override void StopModule(Scene scene)
        {
            base.StopModule(scene);
            
            Game.G<SceneSwitcher>().CurrentScene.Modules.StartModule<CCamera>();
        }

        public override void UpdateModule(Scene scene)
        {
            Camera.main.transform.AddPosition(InputService.DebugMovement * 0.1f);
            Camera.main.orthographicSize += InputService.DebugYScroll / -120;
        }
    }
}