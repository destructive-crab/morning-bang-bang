using MothDIed;
using UnityEngine;

namespace banging_code.dev
{
    public class ButtonMethods : MonoBehaviour
    {
        public void StartDevRun() => Game.RunSystem.StartDevRun();
        public void Exit() => Game.QuitGame();
        
    }
}