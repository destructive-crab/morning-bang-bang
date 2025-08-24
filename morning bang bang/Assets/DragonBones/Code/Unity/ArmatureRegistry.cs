using System;
using System.Collections.Generic;

namespace DragonBones
{
    public sealed class ArmatureRegistry
    {
        public Armature BelongsTo;
        public enum RegistryChange
        {
            Visibility,
            Display,
            DrawOrder
        }

        private readonly List<ValueTuple<RegistryChange, DBRegistry.DBID>> changesBuffer = new();
        private int bufferSize = 0;
            
        private readonly Dictionary<DBRegistry.DBID, DisplayType> states = new();
            
        private readonly Dictionary<DBRegistry.DBID, DBRegistry.DBID> currentDisplays = new();
        private readonly Dictionary<DBRegistry.DBID, bool> visibilities = new();
        private readonly Dictionary<DBRegistry.DBID, int> drawOrder = new();

        public ArmatureRegistry(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public Tuple<ValueTuple<RegistryChange, DBRegistry.DBID>[], int> PullChanges(DBRegistry.DBID id)
        {
            UnitySlot unitySlot = DB.Registry.GetSlot(id) as UnitySlot;

            bufferSize = 0;
                
            if(GetDrawOrder(id) != unitySlot.DrawOrder.V)
                AddChange(RegistryChange.DrawOrder, id);

            if (GetVisibility(id) != unitySlot.Visible.V)
                AddChange(RegistryChange.Visibility, id);

            if (!GetSlotDisplayID(id).Equals(DB.Registry.GetCurrentActiveDisplayOf(id)))
                AddChange(RegistryChange.Display, id);
                
            //drawOrder

            if (bufferSize != 0) return new Tuple<ValueTuple<RegistryChange, DBRegistry.DBID>[], int>(changesBuffer.ToArray(), bufferSize);
            //todo warnings
            return null;

            void AddChange(RegistryChange change, DBRegistry.DBID slot)
            {
                bufferSize++;
                    
                if (changesBuffer.Count > bufferSize)
                {
                    (RegistryChange, DBRegistry.DBID) tuple = changesBuffer[bufferSize - 1];
                    tuple.Item1 = change;
                    tuple.Item2 = slot;
                    changesBuffer[bufferSize - 1] = tuple;
                }
                else
                {
                    changesBuffer.Add(new ValueTuple<RegistryChange, DBRegistry.DBID>(change, slot));
                }
            }
        }

        public void CommitChanges()
        {
            foreach (DBRegistry.DBID id in DB.Registry.GetChildSlotsOf(BelongsTo.ID))
            {
                visibilities[id] = DB.Registry.GetSlot(id).Visible.V;
                currentDisplays[id] = DB.Registry.GetCurrentActiveDisplayOf(id);
                drawOrder[id] = DB.Registry.GetSlot(id).DrawOrder.V;
            }
        }

        private bool GetVisibility(DBRegistry.DBID id) => visibilities[id];
        private int GetDrawOrder(DBRegistry.DBID id) => drawOrder[id];
        private DBRegistry.DBID GetSlotDisplayID(DBRegistry.DBID id) => currentDisplays[id];
            
        private DBRegistry.DBID GetSlotDisplayID(UnitySlot unitySlot) => currentDisplays[unitySlot.ID];
        public DisplayType GetState(DBRegistry.DBID id) => states[id];
        public void SetState(DBRegistry.DBID id, DisplayType state) => states[id] = state;
    }
}