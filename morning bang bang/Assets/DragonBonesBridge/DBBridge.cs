using DragonBones;
using UnityEngine;

namespace DragonBonesBridge
{
    public static class DBBridge
    {
        private static string preloadFolderPath = "dbpreload/";
        
        public static void Preload()
        {
            UnityDragonBonesData[] all = Resources.LoadAll<UnityDragonBonesData>(preloadFolderPath);
            
            foreach (UnityDragonBonesData data in all)
            {
                DB.UnityDataLoader.LoadData(data);
            }
        }
        
        public static UnityArmatureRoot Create(string armatureName, string projectName, ArmatureController armatureController)
        {
            return DB.Factory.UnityCreateArmature(armatureName, projectName, armatureController.Root);
        }
    }
}