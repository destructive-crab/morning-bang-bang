using UnityEngine;

namespace DragonBones
{
    [DisallowMultipleComponent]
    public sealed class UnityArmatureRoot : UnityEventDispatcher, IEngineArmatureRoot
    {
        [HideInInspector] public bool CombineMeshes = true;

        public readonly DBColor Color = new DBColor();

        public Armature Armature { get; private set; }
        public AnimationPlayer AnimationPlayer => Armature != null ? Armature.AnimationPlayer : null;
        
        public UnityArmatureMeshRoot MeshRoot { get; private set; }
        
        public ArmatureRegistry Registry { get; private set; }

        public void DBConnect(Armature armature)
        {
            Armature = armature;
        }

        public void DBInit(Armature armature)
        {
            Armature = armature;

            if (MeshRoot == null)
            {
                MeshRoot = new UnityArmatureMeshRoot(Armature, gameObject.AddComponent<MeshFilter>(), gameObject.AddComponent<MeshRenderer>());
            }
            if (Registry == null)
            {
                //TODO
                //Registry = new ArmatureRegistry(Armature);
            }
        }
        
        public void DBClear()
        {
            Armature?.Dispose();
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
//                Registry.CommitChanges();
            }

            //foreach (DBRegistry.DBID id in DB.Registry.GetChildSlotsOf(Armature.ID))
            //{
            //    Tuple<(ArmatureRegistry.RegistryChange, DBRegistry.DBID)[], int> changes = Registry.PullChanges(id);
//
            //    if(changes == null) continue;
//
            //    for(int i = 0; i < changes.Item2; i++)
            //    {
            //        MeshRoot.SendChange(changes.Item1[i].Item1, changes.Item1[i].Item2);
            //    }
            //}
//
            //Registry.CommitChanges();
            
            if (MeshRoot.IsCombined) MeshRoot.Update();
        }


        private void BuildMeshes()
        {

        }

        //maybe we should not clear all stuff, maybe we want to store armature somewhere
        private void OnDestroy() => DBClear();
        
        public void SetColor(DBColor value)
        {
            return;
            Color.CopyFrom(value);
        }
    }
}
