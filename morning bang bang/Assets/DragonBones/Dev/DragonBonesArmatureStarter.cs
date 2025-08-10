using DragonBones;
using UnityEngine;

public class DragonBonesArmatureStarter : MonoBehaviour
{
    private void Start()
    {
        DBInitial.UnityDataLoader.LoadDragonBonesData("animations/rat_gun_ske");
        DBInitial.UnityDataLoader.LoadTextureAtlasData("animations/rat_gun_tex");

        var a = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_side", "rat_gun");
        var b = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_down", "rat_gun");
        var c = DBInitial.UnityFactory.UnityCreateArmature("rat_gun_up", "rat_gun");

        b.transform.position += Vector3.up * 3;
        c.transform.position += Vector3.right * 3;

        a.AnimationPlayer.Play("run");
        b.AnimationPlayer.Play("run");
        c.AnimationPlayer.Play("run");
    }
}