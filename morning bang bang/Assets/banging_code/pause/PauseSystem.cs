using UnityEngine;

namespace banging_code.pause
{
    public sealed class PauseSystem 
    {
        public bool IsPaused => Time.timeScale == 0;
        
        public void SwitchPause()
        {
            if(IsPaused) Unpause();
            else Pause();
        }

        public void Pause()
        {
            Time.timeScale = 0;
        }

        public void Unpause()
        {
            Time.timeScale = 1;
        }
    }
}