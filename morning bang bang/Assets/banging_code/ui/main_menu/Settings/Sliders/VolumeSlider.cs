using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace banging_code.ui.main_menu
{
    public class VolumeSlider : MonoBehaviour
    {
        private Slider slider;
        private TMP_Text label;
        private string pattern;

        private void Awake()
        {
            slider = GetComponentInChildren<Slider>(true);
            label = GetComponentInChildren<TMP_Text>(true);

            pattern = label.text;
            
            slider.minValue = 0;
            slider.maxValue = 100;

            slider.wholeNumbers = true;
        }

        public int Current()
        {
            return (int)slider.value;
        }

        public void Set(int volume)
        {
            slider.value = volume;
            label.text = pattern.Replace("*", volume.ToString());
        }

        private void Update()
        {
            label.text = pattern.Replace("*", slider.value.ToString());
        }
    }
}