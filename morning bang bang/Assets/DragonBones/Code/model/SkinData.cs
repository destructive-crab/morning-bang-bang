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
        public readonly Dictionary<string, List<DisplayData>> slotsAndTheirDisplays = new();
        /// <private/>
        public ArmatureData parent;

        /// <inheritDoc/>
        protected override void ClearObject()
        {
            foreach (var list in this.slotsAndTheirDisplays.Values)
            {
                foreach (var display in list)
                {
                    display.ReturnToPool();
                }
            }

            this.name = "";
            this.slotsAndTheirDisplays.Clear();
            this.parent = null;
        }

        /// <internal/>
        /// <private/>
        public void AddDisplay(string slotName, DisplayData value)
        {
            if (!string.IsNullOrEmpty(slotName) && value != null && !string.IsNullOrEmpty(value.Name))
            {
                if (!this.slotsAndTheirDisplays.ContainsKey(slotName))
                {
                    this.slotsAndTheirDisplays[slotName] = new List<DisplayData>();
                }

                if (value != null)
                {
                    value.parent = this;
                }

                var slotDisplays = this.slotsAndTheirDisplays[slotName]; // TODO clear prev
                slotDisplays.Add(value);
            }
        }
        public DisplayData GetDisplay(string slotName, string displayName)
        {
            List<DisplayData> slotDisplays = this.GetDisplays(slotName);
            
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
            if (string.IsNullOrEmpty(slotName) || !this.slotsAndTheirDisplays.ContainsKey(slotName))
            {
                return null;
            }

            return this.slotsAndTheirDisplays[slotName];
        }

    }
}
