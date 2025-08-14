using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public class SlotDisplaysManager
    {
        public bool DisplayDirty { get; set; } = true;
        public readonly Slot BelongsTo;
        
        //displays management
        private readonly Dictionary<string, DisplayData> displays = new();
        private readonly List<ChildArmatureDisplayData> childArmatures = new();

        private DisplayData[] allDisplays;

        public DisplayData CurrentDisplayDataRaw { get; private set; }
        private DisplayData[] rawDisplaysData;
        
        public IEngineChildArmatureSlotDisplay[] GetChildArmaturesDisplays => childArmatureDisplays.Values.ToArray();
        private readonly Dictionary<string, IEngineChildArmatureSlotDisplay> childArmatureDisplays = new();
        public IEngineSlotDisplay MeshDisplay { get; private set; }

        public DisplayData CurrentDisplayData { get; private set; }
        public IEngineSlotDisplay CurrentEngineDisplay { get; private set; }
        
        public IEngineChildArmatureSlotDisplay ChildArmatureSlotDisplay => CurrentEngineDisplay as IEngineChildArmatureSlotDisplay;
        public bool MeshDisplayInitialized => MeshDisplay != null;

        public SlotDisplaysManager(Slot belongsTo)
        {
            BelongsTo = belongsTo;
        }
        
        public virtual bool AddDisplay(DisplayData data)
        {
            if (data is ChildArmatureDisplayData childArmatureData && !childArmatures.Contains(data))
            {
                DBLogger.BLog.AddEntry("Add Child Armature Display", data.Name);
                childArmatures.Add(childArmatureData);
            }
            
            if(!displays.ContainsKey(data.Name))
            {
                DBLogger.BLog.AddEntry("Add Display", data.Name);
                displays.Add(data.Name, data);
            }

            return true;
        }

        public void SetAllDisplays(DisplayData[] data)
        {
            allDisplays = data;
            foreach (DisplayData displayData in data)
            {
                AddDisplay(displayData);
            }
            
            DBLogger.BLog.AddEntry("Slot Displays Manager: Displays Array Set",  data.ToString(), $"count: {data.Length}");
            BelongsTo.RefreshData();
        }

        public DisplayData GetDisplay(string displayName)
        {
            return displays[displayName];
        }

        public DisplayData[] GetAllData()
        {
            return displays.Values.ToArray();
        }
        
        public virtual bool SwapCurrentDisplay(string displayName)
        {
            if (displays.TryGetValue(displayName, out DisplayData newDisplayData) &&
                ((CurrentDisplayData != null && CurrentDisplayData.Name != displayName) || CurrentDisplayData == null))
            {
                IEngineSlotDisplay newDisplay = null;

                if (newDisplayData.type == DisplayType.Armature)
                {
                    newDisplay = childArmatureDisplays[displayName];
                }
                else if (newDisplayData.type == DisplayType.Image || newDisplayData.type == DisplayType.Mesh)
                {
                    newDisplay = MeshDisplay;
                }

                if (CurrentEngineDisplay != newDisplay && CurrentDisplayData != null)
                {
                    CurrentEngineDisplay.Disable();
                }
                
                CurrentEngineDisplay = newDisplay;
                CurrentDisplayData = newDisplayData;
                
                CurrentEngineDisplay?.Enable();
                
                DisplayDirty = true;
                BelongsTo.RefreshData();
                return true;
            }

            return false;
        }

        public void Clear()
        {
        }

        public void InitMeshDisplay(IEngineSlotDisplay display)
        {
            DBLogger.BLog.AddEntry("SlotDisplaysManager Init Mesh Display", display.Data.Name);
            MeshDisplay = display;
        }

        public void AddChildArmatureDisplay(IEngineChildArmatureSlotDisplay childArmatureSlotDisplay)
        {
            DBLogger.BLog.AddEntry("Slot Displays Manager: Add Child Armature Display", childArmatureSlotDisplay.ChildArmatureDisplayData.Name);
            childArmatureDisplays.Add(childArmatureSlotDisplay.Data.Name, childArmatureSlotDisplay);
        }

        public string DisplayIndexToName(int index)
        {
            return allDisplays[index].Name;
        }

        public DisplayData GetEngineDisplayByIndex(int index)
        {
            return GetDisplay(DisplayIndexToName(index));
        }

        public bool SwapDisplaysByIndex(int index)
        {
            if (index == -1)
            {
                CurrentEngineDisplay.Disable();
                return true;
            }
            
            return SwapCurrentDisplay(DisplayIndexToName(index));
        }

        public void RefreshCurrentDisplayWithIndex()
        {
            
        }
    }
}