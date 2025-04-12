using System.Collections.Generic;
using UnityEngine;

namespace destructive_code.Tilemaps
{
    public sealed class TilesReplacerSettings : ScriptableObject
    {
        public List<TilesReplacerWindow.ReplacementRule> rules = new();
    }
}