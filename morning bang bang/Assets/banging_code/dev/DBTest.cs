using System;
using DragonBonesBridge;
using UnityEngine;

namespace banging_code.dev
{
    public class DBTest : MonoBehaviour
    {
        private void Awake()
        {
            var test = DBBridge.Create("rat_gun_side", "rat_gun");
            test.Play("run");
        }
    }
}