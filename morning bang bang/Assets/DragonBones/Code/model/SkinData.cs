using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The skin data, typically a armature data instance contains at least one skinData.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>

    public class SkinData : DBObject
    {
        /// <summary>
        /// - The skin name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name;
        
        public readonly Dictionary<string, List<DisplayData>> slotsAndTheirDisplays = new();
        public ArmatureData BelongsToArmature;

        public override void OnReleased()
        {
            foreach (var list in slotsAndTheirDisplays.Values)
            {
                foreach (var display in list)
                {
                    display.ReleaseThis();
                }
            }

            name = "";
            slotsAndTheirDisplays.Clear();
            BelongsToArmature = null;
        }

        public void AddDisplay(string slotName, DisplayData value)
        {
            if (!string.IsNullOrEmpty(slotName) && value != null && !string.IsNullOrEmpty(value.Name))
            {
                if (!slotsAndTheirDisplays.ContainsKey(slotName))
                {
                    slotsAndTheirDisplays[slotName] = new List<DisplayData>();
                }

                if (value != null)
                {
                    value.BelongsToSkin = this;
                }

                slotsAndTheirDisplays[slotName].Add(value); 
            }
        }
        public DisplayData GetDisplay(string slotName, string displayName)
        {
            List<DisplayData> slotDisplays = GetDisplays(slotName);
            
            if (slotDisplays != null)
            {
                foreach (var display in slotDisplays)
                {
                    if (display != null && display.Name == displayName)
                    {
                        return display;
                    }
                }
            }

            return null;
        }
        /// <private/>
        public List<DisplayData> GetDisplays(string slotName)
        {
            if (string.IsNullOrEmpty(slotName) || !slotsAndTheirDisplays.ContainsKey(slotName))
            {
                return null;
            }

            return slotsAndTheirDisplays[slotName];
        }

    }
}
