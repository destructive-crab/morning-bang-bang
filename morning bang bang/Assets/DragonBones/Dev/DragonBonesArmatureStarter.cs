using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class DragonBonesArmatureStarter : MonoBehaviour
{
    private List<UnityArmatureRoot> armatures = new();
    
    private void Start()
    {
        DB.UnityDataLoader.LoadDragonBonesData("animations/mecha/mecha_1502b_ske");
        DB.UnityDataLoader.LoadTextureAtlasData("animations/mecha/mecha_1502b_tex");
        
        DB.UnityDataLoader.LoadDragonBonesData("animations/mecha/skin_1502b_ske");
        DB.UnityDataLoader.LoadTextureAtlasData("animations/mecha/skin_1502b_tex");
        
        DB.UnityDataLoader.LoadDragonBonesData("animations/mecha/weapon_1000_ske");
        DB.UnityDataLoader.LoadTextureAtlasData("animations/mecha/weapon_1000_tex");

        DB.UnityDataLoader.LoadDragonBonesData("animations/rat_gun_ske");
        DB.UnityDataLoader.LoadTextureAtlasData("animations/rat_gun_tex");
        
        StartCoroutine(Spawn());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var unityEngineArmatureDisplay in armatures)
            {
                unityEngineArmatureDisplay.AnimationPlayer.Play("run");
            }
        }        
        if (Input.GetKeyDown(KeyCode.J))
        {
            foreach (var unityEngineArmatureDisplay in armatures)
            {
                unityEngineArmatureDisplay.AnimationPlayer.Play("idle");
            }
        }
    }

    private IEnumerator Spawn()
    {
        for (int x = 0; x < 30; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                UnityArmatureRoot a = null;
                if(x%2 ==0) a = DB.Factory.UnityCreateArmature("rat_gun_side", "rat_gun"); 
                else a= DB.Factory.UnityCreateArmature("rat_gun_down", "rat_gun");

                a.transform.position = new Vector3(x, y, 0); 

                armatures.Add(a);

                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}