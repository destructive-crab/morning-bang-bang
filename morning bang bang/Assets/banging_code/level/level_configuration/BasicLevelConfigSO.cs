using UnityEngine;

namespace banging_code.common.rooms
{
    [CreateAssetMenu(menuName = "Level/Basic Level Config")]
    public sealed class BasicLevelConfigSO : ScriptableObject
    {
        public BasicLevelConfig Data;
    }
}