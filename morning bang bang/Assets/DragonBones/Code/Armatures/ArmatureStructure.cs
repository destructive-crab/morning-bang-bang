using System.Collections.Generic;

namespace DragonBones
{
    public sealed class ArmatureStructure
    {
        public Bone[] Bones;
        public Slot[] Slots;
        public Constraint[] Constraints;

        public Slot[] CurrentDrawOrder;

        public bool IsCurrentDrawOrderOriginal { get; private set; } = true;
        
        private readonly Dictionary<string, Bone> bones = new();
        private readonly Dictionary<string, Slot> slots = new();
        private readonly Dictionary<string, Constraint> constraints = new();

        private readonly Dictionary<string, string> boneToSlot = new();
        
        private readonly Dictionary<Slot, DisplayData[]> displayData = new();
        private readonly Dictionary<Slot, DisplayData> activeDisplays = new();

        public ChildArmature[] ChildArmatures;
        private readonly Dictionary<DisplayData, ChildArmature> ChildArmaturesMap = new();

        public Armature BelongsTo;

        public ArmatureStructure(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

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
            if (index == -1) return null;
            
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
           
            Slots = new Slot[slots.Count];

            i = 0;
            foreach (KeyValuePair<string, Slot> pair in slots)
            {
                Slots[i] = pair.Value;
                i++;
            }

            CurrentDrawOrder = new Slot[slots.Count];
            Slots.CopyTo(CurrentDrawOrder, 0);

            Constraints = new Constraint[constraints.Count];

            i = 0;
            foreach (KeyValuePair<string, Constraint> pair in constraints)
            {
                Constraints[i] = pair.Value;
                i++;
            }

        }

        public ChildArmature GetChildArmature(DisplayData data)
        {
            return ChildArmaturesMap[data];
        }

        public DisplayData[] GetDisplayData(Slot slot)
        {
            return displayData[slot];
        }

        public void SortDrawOrder(short[] indices, int offset)
        {
            if (indices == null && !IsCurrentDrawOrderOriginal)
            {
                for (int index = 0; index < Slots.Length; index++)
                {
                    CurrentDrawOrder[index] = Slots[index];
                    CurrentDrawOrder[index].SetDrawOrder(index);
                }

                IsCurrentDrawOrderOriginal = true;
            }
            else if(indices != null)
            {
                for (int i = 0; i < Slots.Length; i++)
                {
                    short index = indices[i+offset];
                    
                    CurrentDrawOrder[i] = Slots[index];
                    CurrentDrawOrder[i].SetDrawOrder(i);
                }

                IsCurrentDrawOrderOriginal = false;
            }
        }
    }
}