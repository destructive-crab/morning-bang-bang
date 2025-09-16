using System;
using System.Collections.Generic;

namespace DragonBones
{
    public sealed class DBRegistry
    {
        public enum RegistryChange
        {
            Added,
            Removed
        }
        
        //buffer
        private readonly List<Tuple<RegistryChange, Armature>> changes = new();
        public bool IsBufferEmpty() => changes.Count == 0;
        
        //active lists
        private readonly List<Armature> allArmatures = new();
        private readonly List<ChildArmature> allChildArmatures = new();
        
        private readonly List<Armature> activeRegistry = new();
        
        //consts
        private const string SEPARATOR = "_";

        public void BufferToAddition(Armature armature)
        {
            if(armature == null || armature.ArmatureData == null) return;
            
            changes.Add(Tuple.Create(RegistryChange.Added, armature));

            IsBufferEmpty();
        }

        public void BufferToRemoval(Armature armature)
        {
            if (armature == null || armature.ArmatureData == null) return;
            
            if (armature is ChildArmature)
            {
                DBLogger.Warn("Cannot buffer child armature");
                return;
            }
            
            changes.Add(Tuple.Create(RegistryChange.Removed, armature));
        }
        
        private void RemoveFromRegistry(Armature armature)
        {
            foreach (ChildArmature childArmature in armature.Structure.ChildArmatures)
            {
                allChildArmatures.Remove(childArmature);
                activeRegistry.Remove(armature);
                childArmature.Dispose();
            }
            
            activeRegistry.Remove(armature);
            allArmatures.Remove(armature);
            
            armature.Dispose();
        }

        public void CommitRuntimeChanges()
        {
            foreach (Tuple<RegistryChange,Armature> change in changes)
            {
                switch (change.Item1)
                {
                    case RegistryChange.Added:
                        activeRegistry.Add(change.Item2);
                        
                        if (change.Item2 is ChildArmature ca)
                        {
                            allChildArmatures.Add(ca);
                        }
                        else
                        {
                            allArmatures.Add(change.Item2);
                        }
                        break;
                    case RegistryChange.Removed:
                        RemoveFromRegistry(change.Item2);
                        break;
                }
            }
            
            changes.Clear(); 
        }

        public Armature[] GetAllRootArmatures()
        {
            return allArmatures.ToArray();
        }

        public void ProcessChangedRoots(Action<RegistryChange, Armature> process)
        {
            foreach (Tuple<RegistryChange, Armature> change in changes)
            {
                if(change.Item2 is ChildArmature) continue;
                process.Invoke(change.Item1, change.Item2);
            }
        }
    }
}