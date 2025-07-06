using UnityEngine;
using UnityEngine.UI;

namespace banging_code.ui.main_menu
{
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        public float Current()
        {
            return 0;
        }

        public int CurrentDB()
        {
            return 0;
        }
    }
}