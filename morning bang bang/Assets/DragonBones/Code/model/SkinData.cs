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
        /// <private/>
        public readonly Dictionary<string, List<DisplayData>> displays = new();
        /// <private/>
        public ArmatureData parent;

        /// <inheritDoc/>
        protected override void ClearObject()
        {
            foreach (var list in this.displays.Values)
            {
                foreach (var display in list)
                {
                    display.ReturnToPool();
                }
            }

            this.name = "";
            this.displays.Clear();
            this.parent = null;
        }

        /// <internal/>
        /// <private/>
        public void AddDisplay(string slotName, DisplayData value)
        {
            if (!string.IsNullOrEmpty(slotName) && value != null && !string.IsNullOrEmpty(value.name))
            {
                if (!this.displays.ContainsKey(slotName))
                {
                    this.displays[slotName] = new List<DisplayData>();
                }

                if (value != null)
                {
                    value.parent = this;
                }

                var slotDisplays = this.displays[slotName]; // TODO clear prev
                slotDisplays.Add(value);
            }
        }
        /// <private/>
        public DisplayData GetDisplay(string slotName, string displayName)
        {
            List<DisplayData> slotDisplays = this.GetDisplays(slotName);
            
            if (slotDisplays != null)
            {
                foreach (var display in slotDisplays)
                {
                    if (display != null && display.name == displayName)
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
            if (string.IsNullOrEmpty(slotName) || !this.displays.ContainsKey(slotName))
            {
                return null;
            }

            return this.displays[slotName];
        }

    }
}
