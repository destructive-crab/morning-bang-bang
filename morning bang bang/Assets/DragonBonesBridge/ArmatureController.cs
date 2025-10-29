using System;
using DragonBones;
using UnityEngine;

namespace DragonBonesBridge
{
    [RequireComponent(typeof(UnityArmatureRoot))]
    public class ArmatureController : MonoBehaviour
    {
        public UnityArmatureRoot Root { get; private set; }

        private void Awake()
        {
            Root = GetComponent<UnityArmatureRoot>();
        }
    }
}