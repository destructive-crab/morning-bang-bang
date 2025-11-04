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
        
        public static ArmatureController Create(string armatureName, string projectName)
        {
            UnityArmatureRoot root = DB.Factory.UnityCreateArmature(armatureName, projectName, null);
            root.gameObject.AddComponent<ArmatureController>();

            return root.gameObject.AddComponent<ArmatureController>();
        }
        
        public static ArmatureController Edit(string armatureName, string projectName, ArmatureController controller)
        {
            UnityArmatureRoot root = DB.Factory.UnityCreateArmature(armatureName, projectName, controller.Root);

            return controller;
        }
    }
}