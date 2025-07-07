using banging_code.ui.main_menu.Buttons;
using MothDIed;
using MothDIed.GUI;

namespace banging_code.ui.main_menu
{
    public class MainMenuSettingsLayer : CommonLayer
    {
        private EnableDebugFeaturesCheckbox enableDebug;

        private MasterVolumeSlider masterSlider;
        private MusicVolumeSlider musicSlider;
        private SoundsVolumeSlider soundsSlider;

        private ApplyChangesButton applyChangesButton;
        private RevertChangesButton revertChangesButton;
        
        //private UIVolumeSlider uiVolume; TODO
        
        private void Awake()
        {
            soundsSlider = FindObjectOfType<SoundsVolumeSlider>(true);
            musicSlider = FindObjectOfType<MusicVolumeSlider>(true);
            masterSlider = FindObjectOfType<MasterVolumeSlider>(true);

            applyChangesButton = FindObjectOfType<ApplyChangesButton>(true);
            revertChangesButton = FindObjectOfType<RevertChangesButton>(true);
            
            enableDebug = transform.GetComponentInChildren<EnableDebugFeaturesCheckbox>();
        }

        protected override void OnShown()
        {
            base.OnShown();
            
            enableDebug.ResetToSettings();
            masterSlider.Set(Game.AudioSystem.AudioMixerHandler.GetMasterVolume());
            musicSlider.Set(Game.AudioSystem.AudioMixerHandler.GetMusicVolume());
            soundsSlider.Set(Game.AudioSystem.AudioMixerHandler.GetSoundsVolume());
        }

        private void Update()
        {
            bool hasChanges = enableDebug.IsChecked != Game.Settings.Data.EnableDebugFeatures;
            
            hasChanges = hasChanges || Game.Settings.Data.MasterVolume != masterSlider.Current();
            hasChanges = hasChanges || Game.Settings.Data.MusicVolume != musicSlider.Current();
            hasChanges = hasChanges || Game.Settings.Data.SoundsVolume != soundsSlider.Current();
            
            Game.AudioSystem.AudioMixerHandler.SetVolumeForMaster(masterSlider.Current());
            Game.AudioSystem.AudioMixerHandler.SetVolumeForMusic(musicSlider.Current());
            Game.AudioSystem.AudioMixerHandler.SetVolumeForSounds(soundsSlider.Current());

            applyChangesButton.interactable = hasChanges;
            revertChangesButton.interactable = hasChanges;
        }

        public void ApplyChanges()
        {
            Game.Settings.Data.EnableDebugFeatures = enableDebug.IsChecked;
            Game.Settings.ApplyVolumes(masterSlider.Current(), musicSlider.Current(), soundsSlider.Current());
        }

        public void RevertChanges()
        {
            enableDebug.ResetToSettings();
            
            masterSlider.Set(Game.Settings.Data.MasterVolume);
            musicSlider.Set(Game.Settings.Data.MusicVolume);
            soundsSlider.Set(Game.Settings.Data.SoundsVolume);
        }
    }
}