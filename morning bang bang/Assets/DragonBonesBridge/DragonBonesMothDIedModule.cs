using Cysharp.Threading.Tasks;
using DragonBones;
using DragonBonesBridge;
using MothDIed;
using MothDIed.Scenes;
using UnityEngine;

public class DragonBonesMothDIedModule : IGMModuleBoot, IGMModuleTick
{
    public async UniTask Boot()
    {
        await DB.InitializeDragonBones();
        DBBridge.Preload();
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

    public void Tick()
    {
        Debug.Log("ADFJALKD");
        DB.Kernel.AdvanceTime(Time.deltaTime);
    }
}
