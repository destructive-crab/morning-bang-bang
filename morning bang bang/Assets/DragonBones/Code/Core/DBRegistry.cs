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
        
        private readonly List<IEngineArmatureRoot> allRoots = new();
        private readonly List<Armature> allArmatures = new();
        private readonly List<ChildArmature> allChildArmatures = new();
        
        private readonly Dictionary<string, object> activeRegistry = new();
        private readonly Dictionary<string, IEngineArmatureRoot> activeRoots = new(); //armature id to root
        
        private const string SEPARATOR = "_";

        public void Register(Armature armature)
        {
            int hashCode = armature.GetHashCode();
            string id = armature.Name + SEPARATOR + hashCode.ToString();
            
            activeRegistry.Add(id, armature);

            if (armature is ChildArmature ca)
            {
                allChildArmatures.Add(ca);
            }
            else
            {
                allArmatures.Add(armature);
            }

            ActiveRegistryChanged = true;
        }

        public void RegisterRoot(IEngineArmatureRoot root)
        {
            allRoots.Add(root);
        }

        public void CommitRuntimeChanges()
        {
            ActiveRegistryChanged = true;
            return;
        }

        public Armature[] GetAllRootArmatures()
        {
            return allArmatures.ToArray();
        }
    }
}