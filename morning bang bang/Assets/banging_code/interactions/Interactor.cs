using System.Collections.Generic;
using MothDIed.DI;
using MothDIed.MonoSystems;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.interactions
{
    public sealed class Interactor : MonoSystem
    {
        [Inject] private InteractorFoV fov;

        private readonly List<IInteraction> current = new();

        public override bool EnableOnStart()
        {
            return true;
        }

        public override void Enable()
        {
            base.Enable();
            
            fov.OnEnter += OnEnterInFoV;
            fov.OnExit += OnExitFromFoV;
            
            InputService.OnPlayerInteract += Interact;
        }

        private void Interact()
        {
            for (var i = 0; i < current.Count; i++)
            {
                IInteraction interactable = current[i];
                interactable.Interact();
            }
        }

        public override void Disable()
        {
            base.Disable();
            
            fov.OnEnter -= OnEnterInFoV;
            fov.OnExit -= OnExitFromFoV;
            
        }
        
        public override void Update()
        {

        }

        private void OnEnterInFoV(IInteraction interaction)
        { 
            current.Add(interaction);
        }

        private void OnExitFromFoV(IInteraction interaction)
        {
            current.Remove(interaction);
        }
    }
}