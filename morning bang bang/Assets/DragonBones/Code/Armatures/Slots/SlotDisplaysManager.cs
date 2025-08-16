using System.Collections.Generic;
using System.Linq;

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
        private readonly Dictionary<string, IEngineChildArmatureSlotDisplay> childArmatureDisplays = new();

        public DisplayData CurrentDisplayData { get; private set; }
        public string CurrentName => CurrentDisplayData.Name;

        public bool HasVisibleDisplay { get; private set; }
        public IEngineChildArmatureSlotDisplay CurrentChildArmature { get; set; }

        public SlotDisplaysManager(Slot belongsTo) { BelongsTo = belongsTo; }
        
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

        public DisplayData GetDisplay(string displayName) => displays[displayName];
        public DisplayData[] GetAllData() => allDisplays;

        public virtual bool SwapCurrentDisplay(string displayName)
        {
            if (displays.TryGetValue(displayName, out DisplayData newDisplayData) &
                ((CurrentDisplayData != null && CurrentDisplayData.Name != displayName) || CurrentDisplayData == null ))
            {
                //check if previous display was child armature
                if (CurrentDisplayData != null && CurrentDisplayData.Type == DisplayType.Armature)
                {
                    childArmatureDisplays[CurrentDisplayData.Name].Disable();
                }
                
                //if our new display is child armature, we should enable its engine display
                if (newDisplayData.Type == DisplayType.Armature)
                {
                    childArmatureDisplays[newDisplayData.Name].Enable();
                    CurrentChildArmature = childArmatureDisplays[newDisplayData.Name];
                }
                
                CurrentDisplayData = newDisplayData;
                
                DisplayDirty = true;
                HasVisibleDisplay = true;
                
                BelongsTo.RefreshData();
                
                return true;
            }

            return false;
        }

        public void Clear()
        {
        }

        public void AddChildArmatureDisplay(IEngineChildArmatureSlotDisplay childArmatureSlotDisplay)
        {
            DBLogger.BLog.AddEntry("Slot Displays Manager: Add Child Armature Display", childArmatureSlotDisplay.Data.Name);
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
                HasVisibleDisplay = false;
                return true;
            }
            if (CurrentDisplayData != null && DisplayIndexToName(index) == CurrentDisplayData.Name && !HasVisibleDisplay)
            {
                HasVisibleDisplay = true;
                return true;
            }
            
            return SwapCurrentDisplay(DisplayIndexToName(index));
        }


        public void RefreshCurrentDisplayWithIndex()
        {
            
        }
    }
}