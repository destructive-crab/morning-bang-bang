using banging_code.runs_system;
using MothDIed;
using UnityEngine;

namespace banging_code.dev
{
    public class ButtonMethods : MonoBehaviour
    {
        public void StartDevRun() => Game.G<RunSystem>().StartDevRun();
        public void Exit() => Game.QuitGame();
        
    }
}