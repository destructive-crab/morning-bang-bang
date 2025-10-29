using MothDIed;
using UnityEngine;

namespace banging_code.pause
{
    public class PauseMenu : MonoBehaviour
    {
        private void Start()
        {
            if(!Game.G<PauseSystem>().IsPaused) gameObject.SetActive(false);
        }

        public void ContinueButton() => Game.G<PauseSystem>().Unpause();
    }
}