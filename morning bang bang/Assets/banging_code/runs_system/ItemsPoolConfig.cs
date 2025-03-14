using banging_code.items;
using UnityEngine;

namespace banging_code.runs_system
{
    [CreateAssetMenu(menuName = "Create ItemsPoolConfig")]
    public sealed class ItemsPoolConfig : ScriptableObject
    {
        public GameItem[] items;    
    }
}