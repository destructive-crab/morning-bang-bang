using DragonBones;
using UnityEngine;

namespace DragonBonesBridge
{
    [RequireComponent(typeof(UnityArmatureRoot))]
    public class ArmatureController : MonoBehaviour
    {
        internal UnityArmatureRoot Root;

        private void Awake()
        {
            Root = GetComponent<UnityArmatureRoot>();
        }
    }
}