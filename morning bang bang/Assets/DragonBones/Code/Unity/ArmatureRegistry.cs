using System;
using System.Collections.Generic;

namespace DragonBones
{
    public sealed class ArmatureRegistry
    {
        public readonly UnityArmatureRoot BelongsTo;
        public enum RegistryChange
        {
            Visibility,
            Display,
            DrawOrder
        }

        private readonly List<ValueTuple<RegistryChange, string>> changesBuffer = new();
        private int bufferSize = 0;
            
        private readonly Dictionary<string, DisplayType> states = new();
            
        private readonly Dictionary<string, DisplayData> currentDisplays = new();
        private readonly Dictionary<string, bool> visibilities = new();
        private readonly Dictionary<string, int> drawOrder = new();

        public ArmatureRegistry(UnityArmatureRoot belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public Tuple<ValueTuple<RegistryChange, string>[], int> PullChanges(string id)
        {
            UnitySlot unitySlot = BelongsTo.Armature.Structure.GetSlot(id) as UnitySlot;

            bufferSize = 0;
                
            if(GetDrawOrder(id) != unitySlot.DrawOrder.V)
            {
                AddChange(RegistryChange.DrawOrder, id);
            }

            if (GetVisibility(id) != unitySlot.Visible.V)
            {
                AddChange(RegistryChange.Visibility, id);
            }

            if (GetSlotDisplay(id) != unitySlot.Display.V)
            {
                AddChange(RegistryChange.Display, id);
            }
            
            if (bufferSize != 0) return new Tuple<ValueTuple<RegistryChange, string>[], int>(changesBuffer.ToArray(), bufferSize);
            //todo warnings
            return null;

            void AddChange(RegistryChange change, string slot)
            {
                bufferSize++;
                    
                if (changesBuffer.Count > bufferSize)
                {
                    (RegistryChange, string) tuple = changesBuffer[bufferSize - 1];
                    tuple.Item1 = change;
                    tuple.Item2 = slot;
                    changesBuffer[bufferSize - 1] = tuple;
                }
                else
                {
                    changesBuffer.Add(new ValueTuple<RegistryChange, string>(change, slot));
                }
            }
        }

        public void CommitChanges()
        {
            foreach (Slot slot in BelongsTo.Armature.Structure.Slots)
            {
                visibilities[slot.Name] = slot.Visible.V;
                currentDisplays[slot.Name] = slot.Display.V;
                drawOrder[slot.Name] = slot.DrawOrder.V;
            }
        }

        private bool GetVisibility(string id) => visibilities[id];
        private int GetDrawOrder(string id) => drawOrder[id];
        private DisplayData GetSlotDisplay(string id) => currentDisplays[id];
        
        public void Clear()
        {
            
        }
    }
}