using System.Collections.Generic;
using MothDIed.DI;
using MothDIed.ExtensionSystem;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.interactions
{
    public sealed class Interactor : Extension
    {
        [Inject] private InteractorFoV fov;

        private readonly List<IInteraction> current = new();
        
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

        private void OnEnterInFoV(Collider2D other)
        {
            if (other.TryGetComponent(out IInteraction interaction))
            {
                current.Add(interaction);
            }
        }

        private void OnExitFromFoV(Collider2D other)
        {
            if (other.TryGetComponent(out IInteraction interaction))
            {
                current.Remove(interaction);
            }
        }
    }
}