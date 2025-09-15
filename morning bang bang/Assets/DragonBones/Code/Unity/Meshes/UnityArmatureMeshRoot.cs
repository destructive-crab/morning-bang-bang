using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshRoot 
    {
        public bool IsCombined { get; private set; } = false;

        public readonly UnityArmatureRoot BelongsTo;
        public Armature Armature => BelongsTo.Armature;

        private readonly List<UnityArmatureMeshPart> parts = new();

        private readonly List<DBMeshBuffer> meshes = new();
        private readonly Dictionary<string, DBMeshBuffer> meshesMap = new();

        private DBMeshBuffer[] drawOrder;
        
        public Mesh OutputMesh;
        private Material[] materials;
        
        private readonly MeshFilter filter;
        private readonly MeshRenderer renderer;
        
        public UnityArmatureMeshRoot(UnityArmatureRoot belongsTo, MeshFilter meshFilter, MeshRenderer meshRenderer)
        {
            BelongsTo = belongsTo;
            filter = meshFilter;
            renderer = meshRenderer;
        }

        public DBMeshBuffer GetMeshFor(Slot slot)
        {
            if (meshesMap.ContainsKey(GetSlotID(slot)))
            {
                return meshesMap[GetSlotID(slot)];
            }
            
            DBMeshBuffer mesh = DBMeshBuffer.BorrowObject<DBMeshBuffer>();
            
            mesh.Init(slot as UnitySlot);
            meshes.Add(mesh);
            meshesMap.Add(GetSlotID(slot), mesh);

            return mesh;
        }
       
        public void Update()
        {
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            
            List<int[]> triangles = new();

            bool updateTriangles = false;
            
            for (var index = 0; index < parts.Count; index++)
            {
                UnityArmatureMeshPart meshPart = parts[index];
                meshPart.UpdateVertices();
                
                if (meshPart.IsDirty)
                {
                    updateTriangles = true;
                    meshPart.IsDirty = false;
                }
                
                vertices.AddRange(meshPart.OutputMesh.vertices);
                uvs.AddRange(meshPart.OutputMesh.uv);
            }

            if (updateTriangles)
            {
                OutputMesh.Clear();
                OutputMesh.vertices = vertices.ToArray();
                OutputMesh.uv = uvs.ToArray();
            
                UpdateTriangles(triangles);
      
                for (int i = 0; i < OutputMesh.subMeshCount; i++)
                {
                    OutputMesh.SetTriangles(triangles[i], i);
                }               
                OutputMesh.RecalculateBounds();
                return;
            }
            
            OutputMesh.vertices = vertices.ToArray();
            OutputMesh.uv = uvs.ToArray();
            
            OutputMesh.RecalculateBounds();
        }

        private void UpdateTriangles(List<int[]> triangles)
        {
            int offset = 0;
            for (var index = 0; index < parts.Count; index++)
            {
                var meshPart = parts[index];
                triangles.Add(meshPart.OutputMesh.triangles);

                for (var i = 0; i < triangles.Last().Length; i++)
                {
                    triangles.Last()[i] += offset;
                }

                offset += OutputMesh.vertices.Length;
            }
            OutputMesh.subMeshCount = triangles.Count;
        }

        public void Combine(bool updateDrawOrder = true)
        {
            if(updateDrawOrder) BuildDrawOrder();
            
            CollectParts();
            
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int[]> triangles = new();
            
            foreach (UnityArmatureMeshPart meshPart in parts)
            {
                triangles.Add(meshPart.OutputMesh.triangles);
              
                for (var i = 0; i < triangles.Last().Length; i++)
                {
                    triangles.Last()[i] += vertices.Count;
                }
                
                vertices.AddRange(meshPart.OutputMesh.vertices);
                uvs.AddRange(meshPart.OutputMesh.uv);
            }

            if(OutputMesh == null)
            {
                OutputMesh = UnityDBFactory.GetEmptyMesh();
            }
            else
            {
                OutputMesh.Clear();
            }

            OutputMesh.vertices = vertices.ToArray();
            OutputMesh.uv = uvs.ToArray();

            OutputMesh.subMeshCount = triangles.Count;

            materials = new Material[parts.Count];
            for (var i = 0; i < triangles.Count; i++)
            {
                int[] triangle = triangles[i];
                OutputMesh.SetTriangles(triangle, i);
                materials[i] = (parts[i].Material);
            }

            OutputMesh.RecalculateBounds();

            filter.sharedMesh = OutputMesh;
            renderer.sharedMaterials = materials;
            
            IsCombined = true;
        }

        private void CollectParts()
        {
            parts.Clear();
            
            List<DBMeshBuffer> currentPart = new();
            Material currentMaterial = null;

            for (int i = 0; i < drawOrder.Length; i++)
            {
                DBMeshBuffer meshBuffer = drawOrder[i];
                
                if (meshBuffer.Material == currentMaterial || currentMaterial == null)
                {
                    currentPart.Add(meshBuffer);
                    currentMaterial = meshBuffer.Material;
                }
                else
                {
                    parts.Last().To = i;//prev part ends here
                    
                    parts.Add(new UnityArmatureMeshPart(Armature));
                    parts.Last().From = i;
                    parts.Last().Init(currentPart.ToArray(), currentMaterial, this);
                    currentPart.Clear();
                    currentMaterial = meshBuffer.Material;
                    
                    i--;
                }
            }

            if (currentPart.Count != 0)
            {
                parts.Add(new UnityArmatureMeshPart(Armature));
                parts.Last().Init(currentPart.ToArray(), currentMaterial, this);
                parts.Last().To = drawOrder.Length;
                currentPart.Clear();
            }
            
            foreach (UnityArmatureMeshPart part in parts)
            {
                part.Build();
            }
        }

        public UnityArmatureMeshPart GetPartWith(DBMeshBuffer buffer)
        {
            return parts.Find((part) => part.Contains(buffer));
        }

        public void SendChange(ArmatureRegistry.RegistryChange change, Slot slot)
        {
            string slotID = GetSlotID(slot);
            
            if (!meshesMap.ContainsKey(slotID)) return;
            
            DBMeshBuffer buffer = meshesMap[slotID];
            UnityArmatureMeshPart part = GetPartWith(meshesMap[slotID]);
            
            if (change == ArmatureRegistry.RegistryChange.DrawOrder && part.IsSingle && parts.Count != 1)
            {
                //recombine
                Combine();
                return;
            }
            
            if(change == ArmatureRegistry.RegistryChange.DrawOrder)
            {
                int previousDrawOrder = buffer.DrawOrder;
 
                BuildDrawOrder();
                
                if(buffer.DrawOrder >= part.From && buffer.DrawOrder < part.To)
                {
                    part.RebuildWithDifferentOrder();
                }
                else
                {
                    Combine(false);
                }
            }
            else
            {
                if(change == ArmatureRegistry.RegistryChange.Display)
                {
                    DBLogger.LogMessage("RECOMADJKASJD");
                    if (slot.IsDisplayingChildArmature() || slot.WasDisplayingChildArmature() || slot.WasInvisible())
                    {
                        DBLogger.LogMessage("RECOMBIUNE");
                        Combine(false);
                        return;
                    }
                }

                part.SendChange(change, buffer);
            }
        }

        private static string GetSlotID(Slot slot)
        {
            return slot.Name + "_" + slot.GetHashCode();
        }

        public void ReleaseMesh(UnitySlot unitySlot)
        {
            string slotID = GetSlotID(unitySlot);
            if(!meshesMap.ContainsKey(slotID))return;
            meshes.Remove(meshesMap[slotID]);
            meshesMap.Remove(slotID);
        }

        public void Clear()
        {
            
        }

        private void BuildDrawOrder()
        {
            drawOrder = new DBMeshBuffer[meshes.Count];
            int offset = 0;

            Dictionary<ArmatureStructure, int> continueOn = new();
            ArmatureStructure currentStructure = BelongsTo.Armature.Structure;

            while (currentStructure != null)
            {
                bool switchToNextLoopTrigger = false;
                int i = 0;
                if (continueOn.ContainsKey(currentStructure))
                    i = continueOn[currentStructure];
                
                for (; i < currentStructure.CurrentDrawOrder.Length; i++)
                {
                    Slot slot = currentStructure.CurrentDrawOrder[i];
                    
                    if (slot.IsDisplayingChildArmature())
                    {
                        continueOn[currentStructure] = i+1;
                        currentStructure = currentStructure.GetChildArmature(slot.Display.V).Structure;
                        
                        switchToNextLoopTrigger = true;
                        
                        break;
                    }
                    
                    if (meshesMap.TryGetValue(GetSlotID(slot), out DBMeshBuffer buffer))
                    {
                        AddToDrawOrder(buffer);
                    }                       
                }

                if (switchToNextLoopTrigger)
                {
                    continue;
                }

                if (currentStructure.BelongsTo is ChildArmature childArmature)
                {
                    currentStructure = childArmature.Parent.ParentArmature.Structure;
                }
                else
                {
                    break;
                }
            }    

            void AddToDrawOrder(DBMeshBuffer buffer)
            {
                drawOrder[offset] = buffer;
                buffer.DrawOrder = offset;
                offset++;
            }
        }

        public DBMeshBuffer GetAtDrawOrder(int i)
        {
            return drawOrder[i];
        }
    }
}