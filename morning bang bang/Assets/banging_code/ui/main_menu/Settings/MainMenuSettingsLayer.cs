using System;
using MothDIed;
using MothDIed.GUI;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace banging_code.ui.main_menu
{
    public class MainMenuSettingsLayer : CommonLayer
    {
        private EnableDebugFeaturesCheckbox enableDebug;

        private MasterVolumeSlider masterSlider;
        private MusicVolumeSlider musicSlider;
        private SoundsVolumeSlider soundsSlider;
        private UIVolumeSlider uiVolume;
        
        private void Start()
        {
            soundsSlider = FindObjectOfType<SoundsVolumeSlider>(true);
            musicSlider = FindObjectOfType<MusicVolumeSlider>(true);
            masterSlider = FindObjectOfType<MasterVolumeSlider>(true);
            enableDebug = transform.GetComponentInChildren<EnableDebugFeaturesCheckbox>();
        }

        protected override void OnShown()
        {
            base.OnShown();
            soundsSlider = FindObjectOfType<SoundsVolumeSlider>(true);
            musicSlider = FindObjectOfType<MusicVolumeSlider>(true);
            masterSlider = FindObjectOfType<MasterVolumeSlider>(true);
            masterSlider.GetComponent<Slider>().value = Game.AudioSystem.AudioMixerHandler.GetMasterVolume();
            musicSlider.GetComponent<Slider>().value = Game.AudioSystem.AudioMixerHandler.GetMusicVolume();
            soundsSlider.GetComponent<Slider>().value = Game.AudioSystem.AudioMixerHandler.GetSoundsVolume();
        }

        private void Update()
        {
            bool hasChanges = false;
            
            if (enableDebug.IsChecked != Game.Settings.Data.EnableDebugFeatures)
            {
                hasChanges = true;
            }
            Game.AudioSystem.AudioMixerHandler.SetVolumeForMaster((int)(masterSlider.GetComponent<Slider>().value * 100));
            Game.AudioSystem.AudioMixerHandler.SetVolumeForMusic((int)(musicSlider.GetComponent<Slider>().value * 100));
            Game.AudioSystem.AudioMixerHandler.SetVolumeForSounds((int)(soundsSlider.GetComponent<Slider>().value * 100));
        }

        public void ApplyChanges()
        {
            Game.Settings.Data.EnableDebugFeatures = enableDebug.IsChecked;
            
            Game.Settings.Data.MasterVolume = (ushort)(masterSlider.GetComponent<Slider>().value * 100);
            Game.Settings.Data.MusicVolume = (ushort)(musicSlider.GetComponent<Slider>().value * 100);
            Game.Settings.Data.SoundsVolume = (ushort)(soundsSlider.GetComponent<Slider>().value * 100);
        }

        public void DiscardChanges()
        {
            
        }
    }
}