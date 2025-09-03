using System.Collections.Generic;

namespace DragonBones
{
    public sealed class ArmatureStructure
    {
        public Bone[] Bones;
        public Slot[] Slots;
        public Constraint[] Constraints;
        
        private Dictionary<string, Bone> bones;
        private Dictionary<string, Slot> slots;
        private Dictionary<string, Constraint> constraints;

        private Dictionary<string, string> boneToSlot;
        
        private Dictionary<Slot, DisplayData[]> displayData;
        private Dictionary<Slot, DisplayData> activeDisplays;

        private List<DBMeshBuffer> currentMeshes;

        public ChildArmature[] ChildArmatures;
        private Dictionary<DisplayData, ChildArmature> ChildArmaturesMap; 
        
        public Bone GetBone(string name)
        {
            if (bones.TryGetValue(name, out Bone bone))
            {
                return bone;
            }

            return null;
        }
        
        public Slot GetSlot(string name)
        {
            return slots[name];
        }

        public DisplayData GetDisplayByIndex(Slot slot, int index)
        {
            return displayData[slot][index];
        }
        
        public void RegisterBone(Bone bone)
        {
            bones.Add(bone.Name, bone);
        }
        public void RegisterSlot(Slot slot)
        {
            slots.Add(slot.Name, slot);
        }
        
        public void RegisterDisplayData(Slot slot, DisplayData[] data, ChildArmature[] childArmatures)
        {
            displayData.Add(slot, data);

            ChildArmatures = childArmatures;
            
            foreach (ChildArmature childArmature in childArmatures)
            {
                ChildArmaturesMap.Add(childArmature.DisplayData, childArmature);
            }
        }

        public void RegisterConstraint(IKConstraint constraint)
        {
            constraints.Add(constraint.Name, constraint);
        }

        public void CompleteBuilding()
        {
            Bones = new Bone[bones.Count];

            int i = 0;
            foreach (KeyValuePair<string, Bone> pair in bones)
            {
                Bones[i] = pair.Value;
                i++;
            }
           
            Slots = new Slot[bones.Count];

            i = 0;
            foreach (KeyValuePair<string, Slot> pair in slots)
            {
                Slots[i] = pair.Value;
                i++;
            }
        }

        public ChildArmature GetChildArmature(DisplayData data)
        {
            return ChildArmaturesMap[data];
        }

        public DisplayData[] GetDisplayData(Slot slot)
        {
            return null;
        }
    }
}