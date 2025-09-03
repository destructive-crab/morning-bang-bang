using System.Collections.Generic;

namespace DragonBones
{
    public sealed class DBRegistry
    {
        public bool ActiveRegistryChanged { get; private set; } = false;
        public bool RuntimeRegistryChanged { get; private set; } = false;
        
        private void MarkActiveAsChanged() => ActiveRegistryChanged = true;
        private void MarkRuntimeAsChanged() => RuntimeRegistryChanged = true;
        public void MarkActiveAsUnchanged() => ActiveRegistryChanged = false;
        public void MarkRuntimeAsUnchanged() => RuntimeRegistryChanged = false;
        
        private readonly Dictionary<string, object> activeRegistry = new();
        
        //unity/
        private readonly Dictionary<string, IEngineArmatureRoot> activeRoots = new(); //armature id to root
        
        private const string ARMATURE = "armature";

        #region DEBUG
        public void PrintCurrentState()
        {
            
        }
        #endregion

        public void Register(Armature armature)
        {
            int hashCode = armature.GetHashCode();
            string id = armature.Name + "_" + hashCode.ToString();
            
            activeRegistry.Add(id, armature);
        }

        public void CommitRuntimeChanges()
        {
            throw new System.NotImplementedException();
        }

        public Armature[] GetAllRootArmatures()
        {
            return null;
        }
    }
}