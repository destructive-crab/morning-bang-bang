using System;
using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class DragonBonesArmatureStarter : MonoBehaviour
{
    private List<UnityArmatureRoot> armatures=new();
    
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
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var unityEngineArmatureDisplay in armatures)
            {
                unityEngineArmatureDisplay.AnimationPlayer.Play("idle");
            }
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            DB.Registry.PrintCurrentState();
        }
    }

    private IEnumerator Spawn()
    {
        // var a =DBI.Factory.UnityCreateArmature("mecha_1502b", "mecha_1502b");
        // a.AnimationPlayer.Play("walk");
        // yield return null;
        for(int i = 0; i < 25; i++)
        {
            var a = DB.Factory.UnityCreateArmature("rat_gun_side", "rat_gun");
//            var b = DBI.Factory.UnityCreateArmature("rat_gun_down", "rat_gun");
            var c = DB.Factory.UnityCreateArmature("rat_gun_up", "rat_gun");

            a.transform.position += Vector3.down * (2 * i);
            //b.transform.position += Vector3.up * (2 * i);
            c.transform.position += Vector3.right * (2 * i);

            armatures.Add(a);
            //armatures.Add(b);
            armatures.Add(c);

            yield return new WaitForSeconds(1);
        }
    }
}