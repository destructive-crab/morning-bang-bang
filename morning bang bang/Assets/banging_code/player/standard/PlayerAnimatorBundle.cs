using UnityEngine;

namespace banging_code.player_logic
{
    [CreateAssetMenu(menuName = "Configs/Player Animator Bundle")]
    public class PlayerAnimatorBundle : ScriptableObject
    {
        [field: SerializeField] public RuntimeAnimatorController side { get; private set; }
        [field: SerializeField] public RuntimeAnimatorController up { get; private set; }
        [field: SerializeField] public RuntimeAnimatorController down { get; private set; }
    }
}