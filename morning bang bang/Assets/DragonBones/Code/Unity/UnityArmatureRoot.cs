using System;
using UnityEngine;

namespace DragonBones
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class UnityArmatureRoot : UnityEventDispatcher, IEngineArmatureRoot
    {
        [HideInInspector] public bool CombineMeshes = true;

        public readonly DBColor Color = new DBColor();

        public Armature Armature { get; private set; }
        public AnimationPlayer AnimationPlayer => Armature != null ? Armature.AnimationPlayer : null;
        
        public UnityArmatureMeshRoot MeshRoot { get; private set; }
        
        public ArmatureRegistry Registry { get; private set; }

        private void Awake()
        {
            MeshRoot = new UnityArmatureMeshRoot(this, gameObject.GetComponent<MeshFilter>(), gameObject.GetComponent<MeshRenderer>());
            Registry = new ArmatureRegistry(this);
        }

        //maybe we should not clear all stuff, maybe we want to store armature somewhere
        private void OnDestroy() => DBClear();
        
        public void DBConnect(Armature armature)
        {
            Armature = armature;
        }
        
        public void DBClear()
        {
            
            MeshRoot.Clear();
            Registry.Clear();
            Armature.Dispose();
            Armature = null;
            
            Color.Identity();
        }
        
        public void DBUpdate()
        {
            //combine startpoint. combine mesh if needed
            if (!CombineMeshes) return;
            if (CombineMeshes && !MeshRoot.IsCombined)
            {
                MeshRoot.Combine();
                Registry.CommitChanges();
            }

            foreach (Slot slot in Armature.Structure.Slots)
            {
                Tuple<(ArmatureRegistry.RegistryChange, string)[], int> changes = Registry.PullChanges(slot.Name);

                if(changes == null) continue;

                for(int i = 0; i < changes.Item2; i++)
                {
                    MeshRoot.SendChange(changes.Item1[i].Item1, slot);
                }
            }

            Registry.CommitChanges();
            
            if (MeshRoot.IsCombined) MeshRoot.Update();
        }
        
        public void SetColor(DBColor value)
        {
            Color.CopyFrom(value);
        }
    }
}
