using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DragonBones;

namespace DragonBones
{
    [System.Serializable]
    public class SlotItemData
    {
        public UnitySlot slot;
        public int sumLevel;
        public bool isSelected;
    }

    public class ShowSlotsWindow : EditorWindow
    {
        private const float WIDTH = 400.0f;
        private const float HEIGHT = 200.0f;
        
        private readonly List<SlotItemData> slotItems = new List<SlotItemData>();
        private UnityArmatureInstance unityArmatureComp;
        private Vector2 scrollPos;
        
        public static void OpenWindow(UnityArmatureInstance unityArmatureComp)
        {
            if (unityArmatureComp == null)
            {
                return;
            }

            var win = GetWindowWithRect(typeof(ShowSlotsWindow), new Rect(0.0f, 0.0f, WIDTH, HEIGHT), false) as ShowSlotsWindow;
            win.unityArmatureComp = unityArmatureComp;
            win.titleContent = new GUIContent("SlotList");
            win.Show();
        }

        void ColletSlotData(Armature armature, int sumLevel)
        {
            var slots = armature.GetSlots();

            foreach (UnitySlot slot in slots)
            {
                var slotItem = new SlotItemData();
                slotItem.slot = slot;
                slotItem.sumLevel = sumLevel;
                slotItem.isSelected = slot.isIgnoreCombineMesh || (slot.renderDisplay != null && slot.renderDisplay.activeSelf);

                this.slotItems.Add(slotItem);
                if (slot.childArmature != null)
                {
                    this.ColletSlotData(slot.childArmature, sumLevel + 1);
                }
            }
        }

        void OnGUI()
        {
            if (this.slotItems.Count == 0)
            {
                this.ColletSlotData(this.unityArmatureComp.Armature, 0);
            }

            //
            ShowSlots(this.unityArmatureComp.Armature);

            //
            if (GUILayout.Button("Apply"))
            {
                foreach (var slotItem in this.slotItems)
                {
                    var slot = slotItem.slot;
                    if (slotItem.isSelected && slot.renderDisplay != null && !slot.renderDisplay.activeSelf)
                    {
                        slot.DisallowCombineMesh();
                        var combineMeshs = (slot.armature.proxy as UnityArmatureInstance).GetComponent<UnityCombineMeshes>();
                        if (combineMeshs != null)
                        {
                            combineMeshs.BeginCombineMesh();
                        }
                    }
                }
                //
                this.Close();
            }
        }

        void ShowSlots(Armature armature)
        {
            var leftMargin = 20;
            var indentSpace = 50;
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            for (var i = 0; i < this.slotItems.Count; i++)
            {
                var slotItem = this.slotItems[i];
                var slot = slotItem.slot;

                //
                GUILayout.BeginHorizontal();
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.margin.left = slotItem.sumLevel * indentSpace + leftMargin;
                GUILayout.Label(slot.name,  GUILayout.Width(100.0f));
                
                if(slot.renderDisplay == null || !slot.renderDisplay.activeSelf)
                {
                    slotItem.isSelected = GUILayout.Toggle(slotItem.isSelected, "Active");
                }
                else
                {
                    GUILayout.Label("Activated");
                }
                
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}