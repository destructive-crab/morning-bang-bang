using System;
using System.Collections.Generic;
using banging_code;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MothDIed.InputsHandling
{
    public static class InputService
    {
        public static bool Enabled { get; private set; } = false;
        private static InputActions actionsMap;

        public static void Setup()
        {
            if (actionsMap != null)
            {
#if UNITY_EDITOR
                Debug.LogError("[INPUT SERVICE : INITIALIZE] Actions map already initialized.");
                return;
#endif
            }

            actionsMap = new InputActions();

            //hooks
            actionsMap.Player.Interact.performed += OnInteract;
            actionsMap.Player.UseItem.started += OnUseItem;
            actionsMap.InRun.Pause.performed += OnPause;
            actionsMap.Debug.ConsoleSwitch.performed += ConsoleSwitch;
            
            //modes
            SetupModeSwitchers();
            
            //disable all inputs. enable inputs that you need in gamestartpoint
            actionsMap.Disable();
            Enabled = true;
        }
        
        public static void EnableInputs()
        {
            if(Enabled) return;
            
            ModeSwitchers[CurrentMode].Enter.Invoke();
            foreach (var inputActionMap in currentParallel)
            {
               inputActionMap.Enable();
            }

            Enabled = true;
        }

        public static void DisableInputs()
        {
            if(!Enabled) return;
            
            ModeSwitchers[CurrentMode].Exit.Invoke();
            foreach (var inputActionMap in currentParallel)
            {
               inputActionMap.Disable();
            }

            Enabled = false;
        }

        //interface && hooks
        public static Vector3 MousePosition => Game.SceneSwitcher.CurrentScene.GetCamera().ScreenToWorldPoint(Input.mousePosition);
        public static Vector2 Movement { get; private set; }

        public static void Tick()
        {
            Movement = actionsMap.Player.Move.ReadValue<Vector2>();
        }

        public static event Action OnPlayerInteract;
        private static void OnInteract(InputAction.CallbackContext obj) => OnPlayerInteract?.Invoke();

        public static event Action OnUseMainItem;
        private static void OnUseItem(InputAction.CallbackContext obj) => OnUseMainItem?.Invoke();

        public static event Action OnPauseButtonPressed;
        private static void OnPause(InputAction.CallbackContext obj) => OnPauseButtonPressed?.Invoke();

        public static event Action OnConsoleSwitchPressed;
        private static void ConsoleSwitch(InputAction.CallbackContext obj) => OnConsoleSwitchPressed?.Invoke();

        //parallel maps
        private static readonly List<InputActionMap> currentParallel = new();

        private static void EnableParallelMap(InputActionMap map)
        {
             if(map.enabled && !Enabled) return;
             
             map.Enable();
             currentParallel.Add(map);           
        }
         private static void DisableParallelMap(InputActionMap map)
         {
              if(!map.enabled && !Enabled) return;
              
              map.Disable();
              currentParallel.Remove(map);           
         }

         public static void EnableDebugInputs() => EnableParallelMap(actionsMap.Debug);
         public static void DisableDebugInputs() => DisableParallelMap(actionsMap.Debug);

         public static void EnableInRunInputs() => EnableParallelMap(actionsMap.InRun);
         public static void DisableInRunInputs() => DisableParallelMap(actionsMap.InRun);
        
        //mode system
        private static Dictionary<Mode, ModeSwitch> ModeSwitchers;
        
        private static Mode CurrentMode;
        private static Mode PreviousMode;
        
        // interface
        public static bool BackToPreviousMode()
        {
            if (PreviousMode == CurrentMode) return false;
            
            return SwitchTo(PreviousMode);
        }

        public static bool SwitchTo(Mode mode)
        {
            if (mode == CurrentMode && !Enabled) return false;

            PreviousMode = CurrentMode;
            
            ModeSwitchers[CurrentMode].Exit.Invoke();
            
            CurrentMode = mode;
            
            ModeSwitchers[mode].Enter.Invoke();

            return true;
        }
        
        //logic
        private static void SetupModeSwitchers()
        {
            ModeSwitchers = new Dictionary<Mode, ModeSwitch>()
            {
                {Mode.Player, new ModeSwitch(Mode.Player, EnterPlayerMode, ExitPlayerMode)},
                {Mode.UI , new ModeSwitch(Mode.UI, EnterUIMode, ExitUIMode)}
            };
        }

        //player
        private static void EnterPlayerMode()
        {
            actionsMap.Player.Enable();
        }

        private static void ExitPlayerMode()
        {
            actionsMap.Player.Disable();
        }

        //ui
        private static void EnterUIMode()
        {
            actionsMap.UI.Enable();
        }

        private static void ExitUIMode()
        {
            actionsMap.UI.Disable();
        }       

        public class ModeSwitch
        {
            public readonly Mode Key;
            
            public readonly Action Enter;
            public readonly Action Exit;

            public ModeSwitch(Mode key, Action enter, Action exit)
            {
                Key = key;
                Enter = enter;
                Exit = exit;
            }
        }
        public enum Mode
        {
            Player,
            UI
        }
    }
}