using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnitySlot : Slot
    {
        internal const float Z_OFFSET = 0.001f;
        
        private static readonly int[] TRIANGLES = { 0, 1, 2, 0, 2, 3 };
        private static Vector3 _helpVector3;

        private bool _skewed;
        private BlendMode _currentBlendMode;

        public MeshBuffer meshBuffer;
        
        public MeshRenderer MeshRenderer => CurrentAsMeshDisplay.MeshRenderer;
        public UnityTextureAtlasData currentTextureAtlasData
        {
            get
            {
                if (TextureData == null || TextureData.parent == null)
                {
                    return null;
                }

                return TextureData.parent as UnityTextureAtlasData;
            }
        }

        public UnityEngineSlotDisplay UnityCurrentDisplay => Displays.CurrentEngineDisplay as UnityEngineSlotDisplay;
        public UnityEngineMeshSlotDisplay CurrentAsMeshDisplay => UnityCurrentDisplay as UnityEngineMeshSlotDisplay;
        
        public UnityEngineChildArmatureSlotDisplay CurrentAsChildArmatureDisplay => UnityCurrentDisplay as UnityEngineChildArmatureSlotDisplay;
        public UnityEngineArmatureDisplay ArmatureDisplay { get; private set; }

        public bool IsEnabled { get; private set; } = false;

        public void Enable()
        {
            IsEnabled = true;
            UnityCurrentDisplay?.Enable();
        }

        public void Disable()
        {
            IsEnabled = false;
            UnityCurrentDisplay?.Disable();
        }

        public override void OnReleased()
        {
            base.OnReleased();

            meshBuffer?.Dispose();

            ArmatureDisplay = null;

            meshBuffer = null;

            _skewed = false;

            _currentBlendMode = DragonBones.BlendMode.Normal;
            IsEnabled = false;
            Displays.Clear();
        }
        
        internal void UpdateZPosition(Vector3 zOrderPosition)
        {
            zOrderPosition.z = -ZOrder.Value * (ArmatureDisplay._zSpace + Z_OFFSET);

            if (UnityCurrentDisplay == null) return;

            UnityCurrentDisplay.transform.localPosition = zOrderPosition;
            UnityCurrentDisplay.transform.SetSiblingIndex(ZOrder.Value);
                
            if (!IsChildArmature())
            {
                CurrentAsMeshDisplay.MeshRenderer.sortingLayerName = LayerMask.LayerToName(ArmatureDisplay.gameObject.layer);
                    
                if (ArmatureDisplay.sortingMode == SortingMode.SortByOrder)
                {
                    CurrentAsMeshDisplay.MeshRenderer.sortingOrder = ZOrder.Value * UnityEngineArmatureDisplay.ORDER_SPACE;
                }
                else
                {
                    CurrentAsMeshDisplay.MeshRenderer.sortingOrder = ArmatureDisplay._sortingOrder;
                }
            }
            else
            {
                UnityEngineArmatureDisplay childArmatureComp = Displays.ChildArmatureSlotDisplay.ArmatureDisplay as UnityEngineArmatureDisplay;
                childArmatureComp._sortingMode = ArmatureDisplay._sortingMode;
                childArmatureComp._sortingLayerName = ArmatureDisplay._sortingLayerName;
                    
                if (ArmatureDisplay._sortingMode == SortingMode.SortByOrder)
                {
                    childArmatureComp.sortingOrder = ZOrder.Value * UnityEngineArmatureDisplay.ORDER_SPACE;
                }
                else
                {
                    childArmatureComp.sortingOrder = ArmatureDisplay._sortingOrder;
                }
            }
        }
        protected void ResetTransform()
        {
            Transform transform = UnityCurrentDisplay.transform;

            transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
        }
        private bool IsChildArmature() => UnityCurrentDisplay is UnityEngineChildArmatureSlotDisplay;

        public void StarUnitySlotBuilding(UnityEngineArmatureDisplay unityArmatureDisplay) => ArmatureDisplay = unityArmatureDisplay;
        public void EndUnitySlotBuilding() => EngineUpdateDisplay();

        protected override void EngineUpdateFrame()
        {
            if (FillMeshBuffer()) return;

            //TODO:
            CurrentAsMeshDisplay.MeshFilter.sharedMesh = null;
            CurrentAsMeshDisplay.MeshRenderer.sharedMaterial = null;

            _helpVector3.x = 0.0f;
            _helpVector3.y = 0.0f;
            _helpVector3.z = UnityCurrentDisplay.transform.localPosition.z;

            UnityCurrentDisplay.transform.localPosition = _helpVector3;
        }

        private bool FillMeshBuffer()
        {
            VerticesData currentVerticesData = (DeformVertices != null && !IsChildArmature()) ? DeformVertices.verticesData : null;
            UnityTextureData currentTextureData = TextureData as UnityTextureData;

            meshBuffer.Clear();

            if (UnityCurrentDisplay == null || currentTextureData == null) return false;
            
            Material currentTextureAtlas = currentTextureAtlasData.texture;

            if (currentTextureAtlas == null) return false;
            
            int textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? (int)currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
            int textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? (int)currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

            float textureScale = Armature.ArmatureData.scale * currentTextureData.parent.scale;
            float sourceX = currentTextureData.region.x;
            float sourceY = currentTextureData.region.y;
            float sourceWidth = currentTextureData.region.width;
            float sourceHeight = currentTextureData.region.height;

            if (currentVerticesData != null)
            {
                DBProjectData data = currentVerticesData.data;
                int meshOffset = currentVerticesData.offset;
                short[] intArray = data.intArray;
                float[] floatArray = data.floatArray;
                short vertexCount = intArray[meshOffset + (int)BinaryOffset.MeshVertexCount];
                short triangleCount = intArray[meshOffset + (int)BinaryOffset.MeshTriangleCount];
                int vertexOffset = intArray[meshOffset + (int)BinaryOffset.MeshFloatOffset];
                        
                if (vertexOffset < 0)
                {
                    vertexOffset += 65536; // Fixed out of bouds bug. 
                }

                int uvOffset = vertexOffset + vertexCount * 2;

                meshBuffer.uvBuffers = new Vector2[vertexCount];
                meshBuffer.rawVertexBuffers = new Vector3[vertexCount];
                meshBuffer.vertexBuffers = new Vector3[vertexCount];
                meshBuffer.triangleBuffers = new int[triangleCount * 3];

                for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
                {
                    meshBuffer.uvBuffers[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                    meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                    meshBuffer.rawVertexBuffers[i].x = floatArray[iV++] * textureScale;
                    meshBuffer.rawVertexBuffers[i].y = floatArray[iV++] * textureScale;

                    meshBuffer.vertexBuffers[i].x = meshBuffer.rawVertexBuffers[i].x;
                    meshBuffer.vertexBuffers[i].y = meshBuffer.rawVertexBuffers[i].y;
                }

                for (int i = 0; i < triangleCount * 3; ++i)
                {
                    meshBuffer.triangleBuffers[i] = intArray[meshOffset + (int)BinaryOffset.MeshVertexIndices + i];
                }

                bool isSkinned = currentVerticesData.weight != null;
                        
                if (isSkinned)
                {
                    ResetTransform();
                }
            }
            else
            {
                if (meshBuffer.rawVertexBuffers == null || meshBuffer.rawVertexBuffers.Length != 4)
                {
                    meshBuffer.rawVertexBuffers = new Vector3[4];
                    meshBuffer.vertexBuffers = new Vector3[4];
                }

                if (meshBuffer.uvBuffers == null || meshBuffer.uvBuffers.Length != meshBuffer.rawVertexBuffers.Length)
                {
                    meshBuffer.uvBuffers = new Vector2[meshBuffer.rawVertexBuffers.Length];
                }

                // Normal texture.                        
                for (int i = 0, l = 4; i < l; ++i)
                {
                    var u = 0.0f;
                    var v = 0.0f;

                    switch (i)
                    {
                        case 0:
                            break;

                        case 1:
                            u = 1.0f;
                            break;

                        case 2:
                            u = 1.0f;
                            v = 1.0f;
                            break;

                        case 3:
                            v = 1.0f;
                            break;
                    }

                    float scaleWidth = sourceWidth * textureScale;
                    float scaleHeight = sourceHeight * textureScale;
                    float pivotX = PivotX;
                    float pivotY = PivotY;

                    if (currentTextureData.rotated)
                    {
                        (scaleWidth, scaleHeight) = (scaleHeight, scaleWidth);

                        pivotX = scaleWidth - PivotX;
                        pivotY = scaleHeight - PivotY;
                        //uv
                        meshBuffer.uvBuffers[i].x = (sourceX + (1.0f - v) * sourceWidth) / textureAtlasWidth;
                        meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + u * sourceHeight) / textureAtlasHeight;
                    }
                    else
                    {
                        //uv
                        meshBuffer.uvBuffers[i].x = (sourceX + u * sourceWidth) / textureAtlasWidth;
                        meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + v * sourceHeight) / textureAtlasHeight;
                    }

                    //vertices
                    meshBuffer.rawVertexBuffers[i].x = u * scaleWidth - pivotX;
                    meshBuffer.rawVertexBuffers[i].y = (1.0f - v) * scaleHeight - pivotY;

                    meshBuffer.vertexBuffers[i].x = meshBuffer.rawVertexBuffers[i].x;
                    meshBuffer.vertexBuffers[i].y = meshBuffer.rawVertexBuffers[i].y;
                }

                meshBuffer.triangleBuffers = TRIANGLES;
            }

            meshBuffer.name = currentTextureAtlas.name;
            meshBuffer.InitMesh();
            
            _currentBlendMode = DragonBones.BlendMode.Normal;
            BlendMode.MarkAsDirty();
            Color.MarkAsDirty();
            Visible.MarkAsDirty();

            return true;
        }

        protected override void EngineUpdateDisplay()
        {
            ArmatureDisplay = Armature.Display as UnityEngineArmatureDisplay;

            if (meshBuffer == null)
            {
                meshBuffer = new MeshBuffer();
                meshBuffer.sharedMesh = MeshBuffer.GetNewMesh();
                meshBuffer.sharedMesh.name = Name;
            }
        }
        protected override void EngineUpdateZOrder()
        {
            UpdateZPosition(UnityCurrentDisplay.transform.localPosition);
        }
        protected override void EngineUpdateMesh()
        {
            if (meshBuffer.sharedMesh == null || DeformVertices == null) { return; }
            
            float scale = Armature.ArmatureData.scale;
            List<float> deformVertices = DeformVertices.vertices;
            List<Bone> bones = DeformVertices.bones;
            VerticesData verticesData = DeformVertices.verticesData;
            WeightData weightData = verticesData.weight;
            
            bool isDeformated = deformVertices.Count > 0;

            DBProjectData data = verticesData.data;
            
            short[] intArray = data.intArray;
            float[] floatArray = data.floatArray;
            short vertextCount = intArray[verticesData.offset + (int)BinaryOffset.MeshVertexCount];

            if (weightData != null)
            {
                int weightFloatOffset = intArray[weightData.offset + 1/*(int)BinaryOffset.MeshWeightOffset*/];//wtf lol; TODO
                if (weightFloatOffset < 0)
                {
                    weightFloatOffset += 65536; // Fixed out of bouds bug. 
                }

                int iB = weightData.offset + (int)BinaryOffset.WeightBoneIndices + weightData.bones.Count, iV = weightFloatOffset, iF = 0;
                for (int i = 0; i < vertextCount; ++i)
                {
                    short boneCount = intArray[iB++];
                    float xG = 0.0f, yG = 0.0f;
                    for (var j = 0; j < boneCount; ++j)
                    {
                        short boneIndex = intArray[iB++];
                        Bone bone = bones[boneIndex];
                        if (bone != null)
                        {
                            DBMatrix matrix = bone.GlobalTransformDBMatrix;
                            float weight = floatArray[iV++];
                            float xL = floatArray[iV++] * scale;
                            float yL = floatArray[iV++] * scale;

                            if (isDeformated)
                            {
                                xL += deformVertices[iF++];
                                yL += deformVertices[iF++];
                            }

                            xG += (matrix.a * xL + matrix.c * yL + matrix.tx) * weight;
                            yG += (matrix.b * xL + matrix.d * yL + matrix.ty) * weight;
                        }
                    }
                    meshBuffer.vertexBuffers[i].x = xG;
                    meshBuffer.vertexBuffers[i].y = yG;
                }

                meshBuffer.UpdateVertices();
            }
            else if (deformVertices.Count > 0)
            {
                int vertexOffset = data.intArray[verticesData.offset + (int)BinaryOffset.MeshFloatOffset];
                
                if (vertexOffset < 0)
                {
                    vertexOffset += 65536; // Fixed out of bouds bug. 
                }

                int index = 0;
                float rx = 0.0f;
                float ry = 0.0f;

                for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                {
                    rx = (data.floatArray[vertexOffset + (iV++)] * scale + deformVertices[iF++]);
                    ry = (data.floatArray[vertexOffset + (iV++)] * scale + deformVertices[iF++]);

                    meshBuffer.rawVertexBuffers[i].x = rx;
                    meshBuffer.rawVertexBuffers[i].y = -ry;

                    meshBuffer.vertexBuffers[i].x = rx;
                    meshBuffer.vertexBuffers[i].y = -ry;
                }

                meshBuffer.UpdateVertices();
            }
        }
        protected override void EngineUpdateVisibility()
        {
            UnityCurrentDisplay.SetEnabled(Parent.visible);
        }
        
        protected override void EngineUpdateColor()
        {
            if (!IsChildArmature())
            {
                DBColor proxyTrans = ArmatureDisplay._DBColor;

                for (int i = 0, l = meshBuffer.sharedMesh.vertexCount; i < l; ++i)
                {
                    meshBuffer.color32Buffers[i].r = (byte)(Color.Value.redMultiplier * proxyTrans.redMultiplier * 255);
                    meshBuffer.color32Buffers[i].g = (byte)(Color.Value.greenMultiplier * proxyTrans.greenMultiplier * 255);
                    meshBuffer.color32Buffers[i].b = (byte)(Color.Value.blueMultiplier * proxyTrans.blueMultiplier * 255);
                    meshBuffer.color32Buffers[i].a = (byte)(Color.Value.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                }
                
                meshBuffer.UpdateColors();
            }
            else
            {
                //Set all childArmature color dirty
                ((UnityEngineArmatureDisplay)Displays.ChildArmatureSlotDisplay.ArmatureDisplay).DBColor = Color.Value;
            }
        }
        
        protected override void EngineUpdateBlendMode()
        {
            if (_currentBlendMode == BlendMode.Value)
            {
                return;
            }

            if (!IsChildArmature())
            {
                CurrentAsMeshDisplay.MeshRenderer.sharedMaterial = (TextureData as UnityTextureData).GetMaterial(BlendMode.Value);
            }
            else
            {
                foreach (var slot in Displays.ChildArmatureSlotDisplay.ArmatureDisplay.Armature.Structure.Slots)
                {
                    slot.BlendMode.Set(BlendMode.Value);
                }
            }

            _currentBlendMode = BlendMode.Value;
        }
        
        protected override void EngineUpdateTransform()
        {
            UpdateMeshBufferTransform();
            UpdateGameObjectTransform();

            if (UnityCurrentDisplay is UnityEngineChildArmatureSlotDisplay childArmatureSlot)
            {
                childArmatureSlot.ArmatureDisplay.Armature.flipX = Armature.flipX;
                childArmatureSlot.ArmatureDisplay.Armature.flipY = Armature.flipY;
            }
        }

        private void UpdateMeshBufferTransform()
        {
            if (Displays.CurrentDisplayData.type != DisplayType.Mesh || Displays.CurrentDisplayData.type != DisplayType.Path) return;
            
            float a = GlobalTransformDBMatrix.a;
            float b = GlobalTransformDBMatrix.b;
            float c = GlobalTransformDBMatrix.c;
            float d = GlobalTransformDBMatrix.d;
            float tx = GlobalTransformDBMatrix.tx;
            float ty = GlobalTransformDBMatrix.ty;

            float rx = 0.0f;
            float ry = 0.0f;
            float vx = 0.0f;
            float vy = 0.0f;
            
            for (int i = 0, l = meshBuffer.vertexBuffers.Length; i < l; i++)
            {
                rx = meshBuffer.rawVertexBuffers[i].x;
                ry = -meshBuffer.rawVertexBuffers[i].y;

                vx = rx * a + ry * c + tx;
                vy = rx * b + ry * d + ty;

                meshBuffer.vertexBuffers[i].x = vx;
                meshBuffer.vertexBuffers[i].y = vy;

            }
            //
            meshBuffer.VertexDirty = true;
        }

        private void UpdateGameObjectTransform()
        {
            UpdateGlobalTransform(); // Update transform.

            //localPosition
            bool flipX = Armature.flipX;
            bool flipY = Armature.flipY;
            Transform transform = UnityCurrentDisplay.transform;

            _helpVector3.x = global.x;
            _helpVector3.y = global.y;
            _helpVector3.z = transform.localPosition.z;

            transform.localPosition = _helpVector3;

            //localEulerAngles
            if (!IsChildArmature())
            {
                _helpVector3.x = flipY ? 180.0f : 0.0f;
                _helpVector3.y = flipX ? 180.0f : 0.0f;
                _helpVector3.z = global.rotation * DBTransform.RAD_DEG;
            }
            else
            {
                //If the childArmature is not null,
                //X, Y axis can not flip in the container of the childArmature container,
                //because after the flip, the Z value of the child slot is reversed,
                //showing the order is wrong, only in the child slot to deal with X, Y axis flip 
                _helpVector3.x = 0.0f;
                _helpVector3.y = 0.0f;
                _helpVector3.z = global.rotation * DBTransform.RAD_DEG;

                //这里这样处理，是因为子骨架的插槽也要处理z值,那就在容器中反一下，子插槽再正过来
                if (flipX != flipY)
                {
                    _helpVector3.z = -_helpVector3.z;
                }
            }

            if (flipX || flipY)
            {
                if (flipX && flipY)
                {
                    _helpVector3.z += 180.0f;
                }
                else
                {
                    if (flipX)
                    {
                        _helpVector3.z = 180.0f - _helpVector3.z;
                    }
                    else
                    {
                        _helpVector3.z = -_helpVector3.z;
                    }
                }
            }

            transform.localEulerAngles = _helpVector3;

            //Modify mesh skew. // TODO child armature skew.
            if (UnityCurrentDisplay is not UnityEngineChildArmatureSlotDisplay && meshBuffer.sharedMesh != null)
            {
                var skew = global.skew;
                var dSkew = skew;
                if (flipX && flipY)
                {
                    dSkew = -skew + DBTransform.PI;
                }
                else if (!flipX && !flipY)
                {
                    dSkew = -skew - DBTransform.PI;
                }

                var skewed = dSkew < -0.01f || 0.01f < dSkew;
                if (_skewed || skewed)
                {
                    _skewed = skewed;

                    var isPositive = global.scaleX >= 0.0f;
                    var cos = Mathf.Cos(dSkew);
                    var sin = Mathf.Sin(dSkew);

                    var x = 0.0f;
                    var y = 0.0f;
                        
                    for (int i = 0, l = meshBuffer.vertexBuffers.Length; i < l; ++i)
                    {
                        x = meshBuffer.rawVertexBuffers[i].x;
                        y = meshBuffer.rawVertexBuffers[i].y;

                        if (isPositive)
                        {
                            meshBuffer.vertexBuffers[i].x = x + y * sin;
                        }
                        else
                        {
                            meshBuffer.vertexBuffers[i].x = -x + y * sin;
                        }

                        meshBuffer.vertexBuffers[i].y = y * cos;
                    }

                    // if (this.CurrentUnityDisplay.MeshRenderer && this.CurrentUnityDisplay.MeshRenderer.enabled)
                    {
                        meshBuffer.UpdateVertices();
                    }
                }
            }

            //localScale
            _helpVector3.x = global.scaleX;
            _helpVector3.y = global.scaleY;
            _helpVector3.z = 1.0f;

            transform.localScale = _helpVector3;
        }
    }
}