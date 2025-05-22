using System;
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

        public static Vector2 MousePosition => Game.CurrentScene.GetCamera().ScreenToWorldPoint(Input.mousePosition);
        public static Vector2 Movement { get; private set; }
        public static event Action OnPlayerInteract;
        public static event Action OnUseMainItem;
        
        private static InputActions actionsMap;

        public static void Initialize()
        {
            if (actionsMap != null) throw new WarningException("[INPUT SERVICE : INITIALIZE] Actions map already initialized.");
            
            actionsMap = new InputActions();
            
            actionsMap.Player.Interact.performed += OnInteract;
            actionsMap.Player.UseItem.started += OnUseItem;
            
            actionsMap.Enable();
        }

        private static void OnUseItem(InputAction.CallbackContext obj)
        {
            OnUseMainItem?.Invoke();
        }

        private static void OnInteract(InputAction.CallbackContext obj)
        {
            OnPlayerInteract?.Invoke();
        }

        public static void DisableAll()
        {
            actionsMap.Player.Disable();
            actionsMap.UI.Disable();
        }

        public static void EnterPlayerMode()
        {
            DisableAll();
            EnablePlayerInputs();
        }

        public static void EnterUIMode()
        {
            DisableAll();
            EnableUIInputs();
        }

        public static void EnableUIInputs()
        {
            actionsMap.UI.Enable();
        }

        public static void EnablePlayerInputs()
        {
            actionsMap.Player.Enable();
        }

        public static void Tick()
        {
            if (actionsMap.Player.enabled)
            {
                Movement = actionsMap.Player.Move.ReadValue<Vector2>();
            }
        }
    }
}