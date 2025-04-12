using System;
using System.Collections.Generic;
using banging_code.common;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace destructive_code.Tilemaps
{
    public sealed class TilesReplacerWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Tiles Replacer")]
        private static void OpenWindow()
        {
            GetWindow<TilesReplacerWindow>().Show();
        }

        public ReplacementRule[] Rules;

        [Sirenix.OdinInspector.Button]
        public void InvokeEnabled()
        {
            foreach (var replacementRule in Rules)
            {
                if(!replacementRule.Enabled) continue;
                
                foreach (var tilemap in replacementRule.InTilemaps)
                {
                    Replace(replacementRule, tilemap);
                }

                foreach (var target in replacementRule.WhereToSearch)
                {
                    foreach (var name in replacementRule.Names)
                    {
                        Transform res = target.Find(name);
                            
                        if(res != null && res.TryGetComponent(out Tilemap tilemap))
                        {
                            Replace(replacementRule, tilemap);
                            EditorUtility.SetDirty(target.gameObject);
                        }
                    }
                }
            }
        }

        private static void Replace(ReplacementRule replacementRule, Tilemap tilemap)
        {
            foreach (TileBase tileBase in replacementRule.WhichTilesWillBeReplaced)
            {
                tilemap.Replace(tileBase, replacementRule.Replacement);
                EditorUtility.SetDirty(tilemap);
            }
        }

        [Serializable]
        public class ReplacementRule
        {
            public bool Enabled;
            
            public List<TileBase> WhichTilesWillBeReplaced = new();
            public TileBase Replacement;
            public List<Tilemap> InTilemaps = new();

            public List<Transform> WhereToSearch = new();  
            public List<string> Names = new();
        }
    }
}

