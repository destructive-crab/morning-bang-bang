using MothDIed;
using UnityEngine;

namespace banging_code.pause
{
    public class PauseMenu : MonoBehaviour
    {
        private void Start()
        {
            if(!Game.PauseSystem.IsPaused) gameObject.SetActive(false);
        }

        public void ContinueButton() => Game.PauseSystem.Unpause();
    }
}