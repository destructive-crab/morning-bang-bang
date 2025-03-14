using banging_code.dev;
using banging_code.interactions;
using content.commuter_basement_II;
using MothDIed;
using UnityEngine;

public class CommuterIITransition : MonoBehaviour, IInteraction
{
    public void Interact()
    {
        Game.RunSystem.EnterNewLevel(new CommuterBasementII());
    }

    public void OnInteractorNear()
    {
    }
}
