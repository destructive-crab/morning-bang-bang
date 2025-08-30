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
            LGR.PM(Time.deltaTime.ToString());
            text.text = "FPS: " + (1f / Time.deltaTime).ToString();
        }

        public void Hide()
        {
            Hidden = true;
            text.text = "";
        }
    }
}