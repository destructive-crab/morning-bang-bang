using TMPro;
using UnityEngine;

namespace banging_code.debug
{
    public class FPS
    {
        public bool Hidden = true;
        private TMP_Text text;

        public FPS(TMP_Text fpsText)
        {
            text = fpsText;
        }

        public void Tick()
        {
            Hidden = false;
            text.text = "FPS: " + (Time.timeScale / Time.deltaTime).ToString();
        }

        public void Hide()
        {
            Hidden = true;
            text.text = "";
        }
    }
}