using System;
using DragonBones;
using UnityEngine;

public class DragonBonesArmatureStarter : MonoBehaviour
{
    private void Start()
    {
        DBInitial.UnityDataLoader.LoadDragonBonesData("animations/rat_gun_ske");
        DBInitial.UnityDataLoader.LoadTextureAtlasData("animations/rat_gun_tex");

        DBInitial.UnityFactory.BuildArmatureComponent("rat_gun_side", "rat_gun");
        DBInitial.UnityFactory.BuildArmatureComponent("rat_gun_down", "rat_gun").transform.position += Vector3.right*3;
        DBInitial.UnityFactory.BuildArmatureComponent("rat_gun_up", "rat_gun").transform.position += Vector3.up*3;
    }
}