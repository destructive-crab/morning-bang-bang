using System.Collections.Generic;
using UnityEngine;

namespace MohDIed.Tilemaps
{
    public sealed class TilesReplacerSettings : ScriptableObject
    {
        public List<TilesReplacerWindow.ReplacementRule> rules = new();
    }
}