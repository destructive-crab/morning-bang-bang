using System;
using System.IO;
using banging_code.json;
using Cysharp.Threading.Tasks;

namespace banging_code.settings
{
    public sealed class GameSettings
    {
        public GeneralSettingsData Data;

        private static GameSettings instance;

        public GameSettings()
        {
            if (instance != null)
            {
                throw new Exception("GAME SETTINGS OBJECT ALREADY EXISTS");
                return;
            }

            instance = this;
        }

        public async UniTask LoadFromFile()
        {
            Data = await SaveManager.Load<GeneralSettingsData>(GeneralSettingsData.SaveName, "");
            
            if (Data == null)
            {
                Data = new GeneralSettingsData();
                ResetToDefault();
                await SaveSettings();
            }
        }

        public UniTask SaveSettings()
        {
            return SaveManager.SaveImmediatly(new SaveManager.QueueRegistry(Data, typeof(GeneralSettingsData), Data.GetSaveName(), Data.GetDirectory()));
        }

        public void ResetToDefault()
        {
            Data.ResetToDefaults();
        }
        
        [Serializable]
        public sealed class GeneralSettingsData : SavableData
        { 
            public bool EnableDebugFeatures;

            public ushort MasterVolume;
            public ushort MusicVolume;
            public ushort SoundsVolume;
            public ushort UIVolume;

            public static string SaveName = "general_settings";

            public GeneralSettingsData() => ResetToDefaults();

            public void ResetToDefaults()
            {
                EnableDebugFeatures = false;
                
                MasterVolume = 100;
                MusicVolume = 100;
                SoundsVolume = 100;
                UIVolume = 100;
            }

            public override string GetSaveName() => SaveName;
            public override string GetDirectory() => "";
        }
    }
}