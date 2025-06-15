using System;
using System.Collections.Generic;
using System.ComponentModel;
using banging_code;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MothDIed.InputsHandling
{
    public static class InputService
    {
        //
        //i will add stuff only when i need it
        //

        public static Vector3 MousePosition => Game.SceneSwitcher.CurrentScene.GetCamera().ScreenToWorldPoint(Input.mousePosition);
        public static Vector2 Movement { get; private set; }
        
        //events
        public static event Action OnPlayerInteract;
        public static event Action OnUseMainItem;
        public static event Action OnPauseButtonPressed;
        
        //static
        private static InputActions actionsMap;
        
        //modes
        private static readonly Dictionary<Mode, Action> ModeSwitchers;
        private static Mode CurrentMode;
        private static Mode PreviousMode;

        static InputService()
        {
            ModeSwitchers = new Dictionary<Mode, Action>()
            {
                {Mode.Player, EnterPlayerMode},
                {Mode.UI , EnterUIMode}
            };
        }

        public static void Initialize()
        {
            if (actionsMap != null) throw new WarningException("[INPUT SERVICE : INITIALIZE] Actions map already initialized.");
            
            actionsMap = new InputActions();
            
            actionsMap.Player.Interact.performed += OnInteract;
            actionsMap.Player.UseItem.started += OnUseItem;
            actionsMap.Global.Pause.performed += OnPause;
            
            actionsMap.Enable();
            EnterUIMode();
        }

        public static void Tick()
        {
            Movement = actionsMap.Player.Move.ReadValue<Vector2>();
        }
        
        //hooks from actions map
        private static void OnPause(InputAction.CallbackContext obj) => OnPauseButtonPressed?.Invoke();
        private static void OnUseItem(InputAction.CallbackContext obj) => OnUseMainItem?.Invoke();
        private static void OnInteract(InputAction.CallbackContext obj) => OnPlayerInteract?.Invoke();

        //actions map controls
        public static void DisableAll()
        {
            actionsMap.Player.Disable();
            actionsMap.UI.Disable();
        }
        public static void EnableUIInputs()
        {
            actionsMap.UI.Enable();
        }

        public static void EnablePlayerInputs()
        {
            actionsMap.Player.Enable();
        }

        //mode switchers
        public static void EnterPlayerMode()
        {
            DisableAll();
            EnablePlayerInputs();
            PreviousMode = CurrentMode;
            CurrentMode = Mode.Player;
        }

        public static void EnterUIMode()
        {
            DisableAll();
            EnableUIInputs();
            PreviousMode = CurrentMode;
            CurrentMode = Mode.UI;
        }


        public enum Mode
        {
            Player,
            UI
        }

        public static void BackToPreviousMode()
        {
            ModeSwitchers[PreviousMode].Invoke();
        }
    }
}