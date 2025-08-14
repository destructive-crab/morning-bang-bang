using System.Collections;
using DragonBones;
using UnityEngine;

public class DragonBonesArmatureStarter : MonoBehaviour
{
    private void Start()
    {
        DBInitial.UnityDataLoader.LoadDragonBonesData("animations/rat_gun_ske");
        DBInitial.UnityDataLoader.LoadTextureAtlasData("animations/rat_gun_tex");

        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        for(int i = 0; i < 10; i++)
        {
            var a = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_side", "rat_gun");
            var b = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_down", "rat_gun");
            var c = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_up", "rat_gun");

            b.transform.position += Vector3.up * i;
            c.transform.position += Vector3.right * i;

            a.AnimationPlayer.Play("run");
            b.AnimationPlayer.Play("run");
            c.AnimationPlayer.Play("run");

            yield return new WaitForSeconds(1);
        }
    }
}