using banging_code.interactions;
using banging_code.runs_system;
using content.commuter_basement_II;
using MothDIed;
using UnityEngine;

public class CommuterIITransition : MonoBehaviour, IInteraction
{
    public void Interact()
    {
        Game.G<RunSystem>().EnterNewLevel(new CommuterBasementII());
    }

    public void OnInteractorNear()
    {
    }
}
