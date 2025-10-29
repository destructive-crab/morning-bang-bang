using Cysharp.Threading.Tasks;
using DragonBones;
using MothDIed;
using MothDIed.Scenes;
using UnityEngine;

public class DragonBonesMothDIedModule : IGMModuleBoot
{
    public async UniTask Boot()
    {
        await DB.InitializeDragonBones();
        UnityDragonBonesData[] allData = Resources.LoadAll<UnityDragonBonesData>("dbpreload/");
        
        foreach (UnityDragonBonesData data in allData)
        {
            DB.UnityDataLoader.LoadData(data);
        }
           
        Game.G<SceneSwitcher>().OnSwitchingFromCurrent += DisposeArmatures;

        void DisposeArmatures(Scene obj)
        {
            foreach (Armature armature in DB.Registry.GetAllRootArmatures())
            {
                DB.Factory.UnityDestroyArmature(armature);
            }
                
            DB.Registry.CommitRuntimeChanges();
        }
    }
}
