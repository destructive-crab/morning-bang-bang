using DragonBones;
using UnityEngine;

namespace banging_code.player_logic
{
    [CreateAssetMenu(menuName = "Configs/Player Animator Bundle")]
    public class PlayerAnimatorBundle : ScriptableObject
    {
        [field: SerializeField] public UnityDragonBonesData side { get; private set; }
        [field: SerializeField] public UnityDragonBonesData up { get; private set; }
        [field: SerializeField] public UnityDragonBonesData down { get; private set; }
    }
}