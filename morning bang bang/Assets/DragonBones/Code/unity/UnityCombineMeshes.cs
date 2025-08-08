using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngineArmatureDisplay))]
    public class UnityCombineMeshes : MonoBehaviour
    {
        [HideInInspector] public List<string> slotNames = new List<string>();
        [HideInInspector] public MeshBuffer[] meshBuffers;
        
        public bool Dirty { get; private set; }
        public void MarkAsDirty() => Dirty = true;

        private UnityEngineArmatureDisplay armatureDisplay;
        
        private int _subSlotCount;
        private int _verticeOffset;

        private bool CanCombineMesh = false;

        private void Start()
        {
            armatureDisplay = GetComponent<UnityEngineArmatureDisplay>();
            
            CanCombineMesh = true;
            MarkAsDirty();
        }

        private void OnDestroy()
        {
            if (armatureDisplay != null)
            {
                RestoreArmature(armatureDisplay.Armature);
            }

            if (meshBuffers != null)
            {
                for (var i = 0; i < meshBuffers.Length; i++)
                {
                    var meshBuffer = meshBuffers[i];
                    meshBuffer.Dispose();
                }
            }

            meshBuffers = null;
            Dirty = false;

            armatureDisplay = null;
            _subSlotCount = 0;
            _verticeOffset = -1;

            CanCombineMesh = false;
        }

        private void LateUpdate()
        {
            if (Dirty)
            {
                BeginCombineMesh();
                Dirty = false;
            }

            UpdateMeshBuffers();
        }

        public void BeginCombineMesh()
        {
            if (!CanCombineMesh || armatureDisplay.isUGUI) { return; }
            
            _verticeOffset = 0;
            _subSlotCount = 0;
            slotNames.Clear();

            if (meshBuffers != null)
            {
                for (var i = 0; i < meshBuffers.Length; i++)
                {
                    var meshBuffer = meshBuffers[i];
                    meshBuffer.Dispose();
                }

                meshBuffers = null;
            }

            List<CombineMeshInfo> combineSlots = new List<CombineMeshInfo>();
            //
            CollectMesh(armatureDisplay.Armature, combineSlots);

            //
            meshBuffers = new MeshBuffer[combineSlots.Count];
            for (var i = 0; i < combineSlots.Count; i++)
            {
                var combineSlot = combineSlots[i];
                //
                UnitySlot proxySlot = combineSlot.proxySlot;
                MeshBuffer meshBuffer = new MeshBuffer();
                meshBuffer.name = proxySlot.meshBuffer.name;
                meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                meshBuffer.sharedMesh.Clear();

                meshBuffer.CombineMeshes(combineSlot.combines.ToArray());
                meshBuffer.VertexDirty = true;
                //
                proxySlot.UnityDisplay.MeshFilter.sharedMesh = meshBuffer.sharedMesh;

                meshBuffers[i] = meshBuffer;

                //
                _verticeOffset = 0;
                for (int j = 0; j < combineSlot.slots.Count; j++)
                {
                    var slot = combineSlot.slots[j];

                    slot.IsCombineMesh = true;
                    slot._sumMeshIndex = i;
                    slot._verticeOrder = j;
                    slot._verticeOffset = _verticeOffset;
                    slot.CombineMeshComponent = this;
                    slot.meshBuffer.enabled = false;

                    if (slot.UnityDisplay != null)
                    {
                        slot.UnityDisplay.Disable();
                        slot.UnityDisplay.hideFlags = HideFlags.HideInHierarchy;

                        var transform = slot.UnityDisplay.transform;

                        transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
                        transform.localEulerAngles = Vector3.zero;
                        transform.localScale = Vector3.one;
                    }
                    //
                    if(slot.DeformVertices != null)
                    {
                        slot.DeformVertices.verticesDirty = true;
                    }
                    
                    slot.TransformDirty = true;
                    
                    slot.ProcessDirtyDisplay();
                    slot.ProcessDirtyData();

                    //
                    meshBuffer.combineSlots.Add(slot);

                    slotNames.Add(slot.Name);

                    _verticeOffset += slot.meshBuffer.vertexBuffers.Length;
                    _subSlotCount++;
                }

                if (proxySlot.UnityDisplay != null)
                {
                    proxySlot.UnityDisplay.Enable();
                    proxySlot.UnityDisplay.hideFlags = HideFlags.None;
                }
            }
        }

        private void RestoreArmature(Armature armature)
        {
            if (armature == null)  return; 
            
            foreach (Slot slot in armature.Structure.Slots)
            {
                UnitySlot unitySlot = (UnitySlot)slot;
                
                if(unitySlot == null || slot == null) continue;
                
                if (!unitySlot.IsDisplayingChildArmature())
                {
                    unitySlot.CancelCombineMesh();
                }
            }
        }

        private void UpdateMeshBuffers()
        {
            if (meshBuffers == null)
            {
                return;
            }

            for (var i = 0; i < meshBuffers.Length; i++)
            {
                var meshBuffer = meshBuffers[i];
                
                if (meshBuffer.ZOrderDirty)
                {
                    meshBuffer.UpdateOrder();
                    meshBuffer.ZOrderDirty = false;
                }
                else if (meshBuffer.VertexDirty)//dont quite understand why they used else if here. maybe vertices update included in UpdateOrder
                {
                    meshBuffer.UpdateVertices();
                    meshBuffer.VertexDirty = false;
                }
            }
        }

        public void CollectMesh(Armature armature, List<CombineMeshInfo> combineSlots)
        {
            if (armature == null)
            {
                DBLogger.LogWarning($"Trying to collect mesh but armature is not initialized in UnityCombineMeshes({gameObject.name})");
                return;
            }

            if (armature.Structure.Slots.Length == 0) return; 
            
            List<Slot> slots = new List<Slot>(armature.Structure.Slots);
            
            bool isBreakCombineMesh = false;
            bool isSameMaterial = false;
            
            UnitySlot slotMeshProxy = null;
            GameObject slotDisplay = null;
            
            for (var i = 0; i < slots.Count; i++)
            {
                UnitySlot slot = slots[i] as UnitySlot;

                slot.CancelCombineMesh();

                slotDisplay = slot.UnityDisplay.gameObject;

                if (slotMeshProxy != null)
                {
                    if (slot.meshBuffer.name == string.Empty)
                    {
                        isSameMaterial = true;
                    }
                    else
                    {
                        isSameMaterial = slotMeshProxy.meshBuffer.name == slot.meshBuffer.name;
                    }
                }
                else
                {
                    isSameMaterial = slotMeshProxy == null;
                }

                //First check if this slot will interrupt the grid merge
                isBreakCombineMesh = slot.IsDisplayingChildArmature() || slot.IgnoreCombineMesh || slot.BlendMode.Value != BlendMode.Normal || !isSameMaterial;

                //If it will be interrupted, then merge once first
                if (isBreakCombineMesh)
                {
                    if (combineSlots.Count > 0)
                    {
                        if (combineSlots[combineSlots.Count - 1].combines.Count == 1)
                        {
                            combineSlots.RemoveAt(combineSlots.Count - 1);
                        }
                    }

                    slotMeshProxy = null;
                }
                //
                if (slotMeshProxy == null && !isBreakCombineMesh && slotDisplay != null && slotDisplay.activeSelf)
                {
                    CombineMeshInfo combineSlot = new CombineMeshInfo();
                    combineSlot.proxySlot = slot;
                    combineSlot.combines = new List<CombineInstance>();
                    combineSlot.slots = new List<UnitySlot>();
                    combineSlots.Add(combineSlot);

                    slotMeshProxy = slot;
                }

                if (slot.IsDisplayingChildArmature())
                {
                    continue;
                }

                if (slotMeshProxy != null && slotDisplay != null && slotDisplay.activeSelf && !slot.IgnoreCombineMesh)
                {
                    CombineInstance com = new CombineInstance();
                    com.mesh = slot.meshBuffer.sharedMesh;

                    com.transform = slotMeshProxy.UnityDisplay.transform.worldToLocalMatrix * slotDisplay.transform.localToWorldMatrix;

                    combineSlots[combineSlots.Count - 1].combines.Add(com);
                    combineSlots[combineSlots.Count - 1].slots.Add(slot);
                }
                
                if (i != slots.Count - 1)
                {
                    continue;
                }
                
                if (combineSlots.Count > 0)
                {
                    if (combineSlots[combineSlots.Count - 1].combines.Count == 1)
                    {
                        combineSlots.RemoveAt(combineSlots.Count - 1);
                    }
                }
                
                slotMeshProxy = null;
            }
        }
    }

    public struct CombineMeshInfo
    {
        public UnitySlot proxySlot;
        public List<CombineInstance> combines;
        public List<UnitySlot> slots;
    }
}