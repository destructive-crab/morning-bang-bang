using System;
using System.Collections;
using System.Collections.Generic;
using DragonBones;
using UnityEngine;

public class DragonBonesArmatureStarter : MonoBehaviour
{
    private List<UnityEngineArmatureDisplay> armatures=new();
    
    private void Start()
    {
        DBInitial.UnityDataLoader.LoadDragonBonesData("animations/rat_gun_ske");
        DBInitial.UnityDataLoader.LoadTextureAtlasData("animations/rat_gun_tex");

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
        }        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var unityEngineArmatureDisplay in armatures)
            {
                unityEngineArmatureDisplay.AnimationPlayer.Play("idle");
            }
        }
    }

    private IEnumerator Spawn()
    {
        for(int i = 0; i < 10; i++)
        {
            var a = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_side", "rat_gun");
            var b = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_down", "rat_gun");
            var c = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_up", "rat_gun");

            a.transform.position += Vector3.down * (2 * i);
            b.transform.position += Vector3.up * (2 * i);
            c.transform.position += Vector3.right * (2 * i);

            a.AnimationPlayer.Play("idle");
            b.AnimationPlayer.Play("idle");
            c.AnimationPlayer.Play("idle");
            
            armatures.Add(a);
            armatures.Add(b);
            armatures.Add(c);

            yield return new WaitForSeconds(1);
        }
    }
}