using banging_code.settings;
using banging_code.ui.main_menu.Buttons;
using MohDIed.Audio;
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
            masterSlider.Set(Game.G<AudioSystem>().AudioMixerHandler.GetMasterVolume());
            musicSlider.Set(Game.G<AudioSystem>().AudioMixerHandler.GetMusicVolume());
            soundsSlider.Set(Game.G<AudioSystem>().AudioMixerHandler.GetSoundsVolume());
        }

        private void Update()
        {
            bool hasChanges = enableDebug.IsChecked != Game.G<GameSettings>().Data.EnableDebugFeatures;
            
            hasChanges = hasChanges || Game.G<GameSettings>().Data.MasterVolume != masterSlider.Current();
            hasChanges = hasChanges || Game.G<GameSettings>().Data.MusicVolume != musicSlider.Current();
            hasChanges = hasChanges || Game.G<GameSettings>().Data.SoundsVolume != soundsSlider.Current();
            
            Game.G<AudioSystem>().AudioMixerHandler.SetVolumeForMaster(masterSlider.Current());
            Game.G<AudioSystem>().AudioMixerHandler.SetVolumeForMusic(musicSlider.Current());
            Game.G<AudioSystem>().AudioMixerHandler.SetVolumeForSounds(soundsSlider.Current());

            applyChangesButton.interactable = hasChanges;
            revertChangesButton.interactable = hasChanges;
        }

        public void ApplyChanges()
        {
            Game.G<GameSettings>().Data.EnableDebugFeatures = enableDebug.IsChecked;
            Game.G<GameSettings>().ApplyVolumes(masterSlider.Current(), musicSlider.Current(), soundsSlider.Current());
        }

        public void RevertChanges()
        {
            enableDebug.ResetToSettings();
            
            masterSlider.Set(Game.G<GameSettings>().Data.MasterVolume);
            musicSlider.Set(Game.G<GameSettings>().Data.MusicVolume);
            soundsSlider.Set(Game.G<GameSettings>().Data.SoundsVolume);
        }
    }
}