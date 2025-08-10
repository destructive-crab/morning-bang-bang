using UnityEngine;

namespace DragonBones
{
    public class UnitySlot : Slot
    {
        internal const float Z_OFFSET = 0.001f;
        private static readonly int[] TRIANGLES = { 0, 1, 2, 0, 2, 3 };
        private static Vector3 _helpVector3;

        internal MeshBuffer meshBuffer;

        //combineMesh
        internal bool IgnoreCombineMesh { get; private set; } = true;
        internal bool IsCombineMesh;
        internal int _sumMeshIndex = -1;
        internal int _verticeOrder = -1;
        internal int _verticeOffset = -1;
        
        internal UnityCombineMeshes CombineMeshComponent { get; set; }

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

        private bool _skewed;
        private BlendMode _currentBlendMode;

        public override void OnReleased()
        {
            base.OnReleased();

            meshBuffer?.Dispose();

            ArmatureDisplay = null;

            meshBuffer = null;

            IgnoreCombineMesh = false;
            IsCombineMesh = false;
            CombineMeshComponent = null;
            _sumMeshIndex = -1;
            _verticeOrder = -1;
            _verticeOffset = -1;

            _skewed = false;

            _currentBlendMode = DragonBones.BlendMode.Normal;
            IsEnabled = false;
            Displays.Clear();
        }
        internal void UpdateZPosition(Vector3 zOrderPosition)
        {
            if (IsCombineMesh)
            {
                MeshBuffer meshBuffer = CombineMeshComponent.meshBuffers[_sumMeshIndex];
                meshBuffer.ZOrderDirty = true;
            }
            else
            {
                zOrderPosition.z = -ZOrder.Value * (ArmatureDisplay._zSpace + Z_OFFSET);

                if (UnityCurrentDisplay != null)
                {
                    UnityCurrentDisplay.transform.localPosition = zOrderPosition;
                    UnityCurrentDisplay.transform.SetSiblingIndex(ZOrder.Value);

                    if (ArmatureDisplay.isUGUI)
                    {
                        return;
                    }

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
            }
        }

        public void DisallowCombineMesh()
        {
            CancelCombineMesh();
            IgnoreCombineMesh = true;
        }

        internal void CancelCombineMesh()
        {
            if (IsCombineMesh)
            {
                IsCombineMesh = false;
                if (CurrentAsMeshDisplay.MeshFilter != null)
                {
                    CurrentAsMeshDisplay.MeshFilter.sharedMesh = meshBuffer.sharedMesh;
                    var isSkinnedMesh = DeformVertices != null && DeformVertices.verticesData != null && DeformVertices.verticesData.weight != null;
                    if (!isSkinnedMesh)
                    {
                        meshBuffer.rawVertextBuffers.CopyTo(meshBuffer.vertexBuffers, 0);
                    }

                    //
                    meshBuffer.UpdateVertices();
                    meshBuffer.UpdateColors();

                    if (isSkinnedMesh)
                    {
                        EngineUpdateMesh();
                        IdentityTransform();
                    }
                    else
                    {
                        EngineUpdateTransform();
                    }
                }

                meshBuffer.enabled = true;
            }

            if (UnityCurrentDisplay != null)
            {
                if (IsChildArmature())
                {
                    UnityCurrentDisplay.Enable();
                }
            }

            //
            IsCombineMesh = false;
            _sumMeshIndex = -1;
            _verticeOrder = -1;
            _verticeOffset = -1;
            // this._combineMesh = null;
        }

        //
        private void _CombineMesh()
        {
            //引起合并的条件,Display改变，混合模式改变，Visible改变，Zorder改变
            //已经关闭合并，不再考虑
            if (IgnoreCombineMesh || ArmatureDisplay.isUGUI)
            {
                return;
            }

            //已经合并过了，又触发合并，那么打断合并，用自己的网格数据还原
            if (IsCombineMesh)
            {
                //已经合并过，除非满足一下情况，否则都不能再合并, TODO
                CancelCombineMesh();
                IgnoreCombineMesh = true;
            }

            var combineMeshComp = ArmatureDisplay.GetComponent<UnityCombineMeshes>();
            //从来没有合并过，触发合并，那么尝试合并
            if (combineMeshComp != null)
            {
                combineMeshComp.MarkAsDirty();
            }
        }

        protected override void EngineUpdateFrame()
        {
            VerticesData currentVerticesData = (DeformVertices != null && !IsChildArmature()) ? DeformVertices.verticesData : null;
            UnityTextureData currentTextureData = TextureData as UnityTextureData;

            meshBuffer.Clear();
            Disable();
            
            if (UnityCurrentDisplay != null && currentTextureData != null)
            {
                var currentTextureAtlas = ArmatureDisplay.isUGUI ? currentTextureAtlasData.uiTexture : currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    Enable();
                    //
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? (int)currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? (int)currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

                    var textureScale = Armature.ArmatureData.scale * currentTextureData.parent.scale;
                    var sourceX = currentTextureData.region.x;
                    var sourceY = currentTextureData.region.y;
                    var sourceWidth = currentTextureData.region.width;
                    var sourceHeight = currentTextureData.region.height;

                    if (currentVerticesData != null)
                    {
                        var data = currentVerticesData.data;
                        var meshOffset = currentVerticesData.offset;
                        var intArray = data.intArray;
                        var floatArray = data.floatArray;
                        var vertexCount = intArray[meshOffset + (int)BinaryOffset.MeshVertexCount];
                        var triangleCount = intArray[meshOffset + (int)BinaryOffset.MeshTriangleCount];
                        int vertexOffset = intArray[meshOffset + (int)BinaryOffset.MeshFloatOffset];
                        if (vertexOffset < 0)
                        {
                            vertexOffset += 65536; // Fixed out of bouds bug. 
                        }

                        var uvOffset = vertexOffset + vertexCount * 2;
                        if (meshBuffer.uvBuffers == null || meshBuffer.uvBuffers.Length != vertexCount)
                        {
                            meshBuffer.uvBuffers = new Vector2[vertexCount];
                        }

                        if (meshBuffer.rawVertextBuffers == null || meshBuffer.rawVertextBuffers.Length != vertexCount)
                        {
                            meshBuffer.rawVertextBuffers = new Vector3[vertexCount];
                            meshBuffer.vertexBuffers = new Vector3[vertexCount];
                        }

                        meshBuffer.triangleBuffers = new int[triangleCount * 3];

                        for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
                        {
                            meshBuffer.uvBuffers[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                            meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                            meshBuffer.rawVertextBuffers[i].x = floatArray[iV++] * textureScale;
                            meshBuffer.rawVertextBuffers[i].y = floatArray[iV++] * textureScale;

                            meshBuffer.vertexBuffers[i].x = meshBuffer.rawVertextBuffers[i].x;
                            meshBuffer.vertexBuffers[i].y = meshBuffer.rawVertextBuffers[i].y;
                        }

                        for (int i = 0; i < triangleCount * 3; ++i)
                        {
                            meshBuffer.triangleBuffers[i] = intArray[meshOffset + (int)BinaryOffset.MeshVertexIndices + i];
                        }

                        var isSkinned = currentVerticesData.weight != null;
                        if (isSkinned)
                        {
                            IdentityTransform();
                        }
                    }
                    else
                    {
                        if (meshBuffer.rawVertextBuffers == null || meshBuffer.rawVertextBuffers.Length != 4)
                        {
                            meshBuffer.rawVertextBuffers = new Vector3[4];
                            meshBuffer.vertexBuffers = new Vector3[4];
                        }

                        if (meshBuffer.uvBuffers == null || meshBuffer.uvBuffers.Length != meshBuffer.rawVertextBuffers.Length)
                        {
                            meshBuffer.uvBuffers = new Vector2[meshBuffer.rawVertextBuffers.Length];
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

                            var scaleWidth = sourceWidth * textureScale;
                            var scaleHeight = sourceHeight * textureScale;
                            var pivotX = PivotX;
                            var pivotY = PivotY;

                            if (currentTextureData.rotated)
                            {
                                var temp = scaleWidth;
                                scaleWidth = scaleHeight;
                                scaleHeight = temp;

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
                            meshBuffer.rawVertextBuffers[i].x = u * scaleWidth - pivotX;
                            meshBuffer.rawVertextBuffers[i].y = (1.0f - v) * scaleHeight - pivotY;

                            meshBuffer.vertexBuffers[i].x = meshBuffer.rawVertextBuffers[i].x;
                            meshBuffer.vertexBuffers[i].y = meshBuffer.rawVertextBuffers[i].y;
                        }

                        meshBuffer.triangleBuffers = TRIANGLES;
                    }


                    CurrentAsMeshDisplay.MeshFilter.sharedMesh = meshBuffer.sharedMesh;
                    CurrentAsMeshDisplay.MeshRenderer.sharedMaterial = currentTextureAtlas;

                    meshBuffer.name = currentTextureAtlas.name;
                    meshBuffer.InitMesh();
                    _currentBlendMode = DragonBones.BlendMode.Normal;
                    BlendMode.MarkAsDirty();
                    Color.MarkAsDirty();// Relpace texture will override blendMode and color.
                    Visible.MarkAsDirty();

                    _CombineMesh();
                    return;
                }
            }

            CurrentAsMeshDisplay.MeshFilter.sharedMesh = null;
            CurrentAsMeshDisplay.MeshRenderer.sharedMaterial = null;

            _helpVector3.x = 0.0f;
            _helpVector3.y = 0.0f;
            _helpVector3.z = UnityCurrentDisplay.transform.localPosition.z;

            UnityCurrentDisplay.transform.localPosition = _helpVector3;

            if (IsCombineMesh)
            {
                _CombineMesh();
            }
        }

        protected void IdentityTransform()
        {
            Transform transform = UnityCurrentDisplay.transform;

            transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        private bool IsChildArmature()
        {
            return UnityCurrentDisplay is UnityEngineChildArmatureSlotDisplay;
        }

        public Mesh mesh
        {
            get
            {
                if (meshBuffer == null)
                {
                    return null;
                }

                return meshBuffer.sharedMesh;
            }
        }

        public MeshRenderer meshRenderer
        {
            get { return CurrentAsMeshDisplay.MeshRenderer; }
        }

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

        public void StarUnitySlotBuilding(UnityEngineArmatureDisplay unityArmatureDisplay)
        {
            ArmatureDisplay = unityArmatureDisplay;
        }

        public void EndUnitySlotBuilding()
        {
            EngineUpdateDisplay();
        }

        protected override void EngineUpdateDisplay()
        {
            ArmatureDisplay = Armature.Display as UnityEngineArmatureDisplay;

            if (meshBuffer == null)
            {
                meshBuffer = new MeshBuffer();
                meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                meshBuffer.sharedMesh.name = Name;
            }
        }

        protected override void EngineUpdateZOrder()
        {
            UpdateZPosition(UnityCurrentDisplay.transform.localPosition);

            if (IsChildArmature() || !IsEnabled)
            {
                _CombineMesh();
            } 
        }

        protected override void EngineUpdateMesh()
        {
            if (meshBuffer.sharedMesh == null || DeformVertices == null)
            {
                return;
            }
            
            var scale = Armature.ArmatureData.scale;
            var deformVertices = DeformVertices.vertices;
            var bones = DeformVertices.bones;
            var hasDeform = deformVertices.Count > 0;
            var verticesData = DeformVertices.verticesData;
            var weightData = verticesData.weight;

            var data = verticesData.data;
            var intArray = data.intArray;
            var floatArray = data.floatArray;
            var vertextCount = intArray[verticesData.offset + (int)BinaryOffset.MeshVertexCount];

            if (weightData != null)
            {
                int weightFloatOffset = intArray[weightData.offset + 1/*(int)BinaryOffset.MeshWeightOffset*/];
                if (weightFloatOffset < 0)
                {
                    weightFloatOffset += 65536; // Fixed out of bouds bug. 
                }

                MeshBuffer meshBuffer = null;
                if (IsCombineMesh)
                {
                    meshBuffer = CombineMeshComponent.meshBuffers[_sumMeshIndex];
                }
                int iB = weightData.offset + (int)BinaryOffset.WeightBoneIndices + weightData.bones.Count, iV = weightFloatOffset, iF = 0;
                for (int i = 0; i < vertextCount; ++i)
                {
                    var boneCount = intArray[iB++];
                    float xG = 0.0f, yG = 0.0f;
                    for (var j = 0; j < boneCount; ++j)
                    {
                        var boneIndex = intArray[iB++];
                        var bone = bones[boneIndex];
                        if (bone != null)
                        {
                            var matrix = bone.GlobalTransformDBMatrix;
                            var weight = floatArray[iV++];
                            var xL = floatArray[iV++] * scale;
                            var yL = floatArray[iV++] * scale;

                            if (hasDeform)
                            {
                                xL += deformVertices[iF++];
                                yL += deformVertices[iF++];
                            }

                            xG += (matrix.a * xL + matrix.c * yL + matrix.tx) * weight;
                            yG += (matrix.b * xL + matrix.d * yL + matrix.ty) * weight;
                        }
                    }
                    this.meshBuffer.vertexBuffers[i].x = xG;
                    this.meshBuffer.vertexBuffers[i].y = yG;

                    if (meshBuffer != null)
                    {
                        meshBuffer.vertexBuffers[i + _verticeOffset].x = xG;
                        meshBuffer.vertexBuffers[i + _verticeOffset].y = yG;
                    }
                }

                if (meshBuffer != null)
                {
                    meshBuffer.VertexDirty = true;
                }
                else
                {
                    // if (this.CurrentUnityDisplay.MeshRenderer && this.CurrentUnityDisplay.MeshRenderer.enabled)
                    {
                        this.meshBuffer.UpdateVertices();
                    }
                }
            }
            else if (deformVertices.Count > 0)
            {
                int vertexOffset = data.intArray[verticesData.offset + (int)BinaryOffset.MeshFloatOffset];
                if (vertexOffset < 0)
                {
                    vertexOffset += 65536; // Fixed out of bouds bug. 
                }
                //
                var a = GlobalTransformDBMatrix.a;
                var b = GlobalTransformDBMatrix.b;
                var c = GlobalTransformDBMatrix.c;
                var d = GlobalTransformDBMatrix.d;
                var tx = GlobalTransformDBMatrix.tx;
                var ty = GlobalTransformDBMatrix.ty;

                var index = 0;
                var rx = 0.0f;
                var ry = 0.0f;
                var vx = 0.0f;
                var vy = 0.0f;
                MeshBuffer meshBuffer = null;
                if (IsCombineMesh)
                {
                    meshBuffer = CombineMeshComponent.meshBuffers[_sumMeshIndex];
                }

                for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                {
                    rx = (data.floatArray[vertexOffset + (iV++)] * scale + deformVertices[iF++]);
                    ry = (data.floatArray[vertexOffset + (iV++)] * scale + deformVertices[iF++]);

                    this.meshBuffer.rawVertextBuffers[i].x = rx;
                    this.meshBuffer.rawVertextBuffers[i].y = -ry;

                    this.meshBuffer.vertexBuffers[i].x = rx;
                    this.meshBuffer.vertexBuffers[i].y = -ry;

                    if (meshBuffer != null)
                    {
                        index = i + _verticeOffset;
                        vx = (rx * a + ry * c + tx);
                        vy = (rx * b + ry * d + ty);

                        meshBuffer.vertexBuffers[index].x = vx;
                        meshBuffer.vertexBuffers[index].y = vy;
                    }
                }
                if (meshBuffer != null)
                {
                    meshBuffer.VertexDirty = true;
                }
                // else if (this.CurrentUnityDisplay.MeshRenderer && this.CurrentUnityDisplay.MeshRenderer.enabled)
                else
                {
                    this.meshBuffer.UpdateVertices();
                }
            }
        }

        protected override void EngineUpdateVisibility()
        {
            UnityCurrentDisplay.SetEnabled(false);

            if (IsCombineMesh && !Parent.visible)
            {
                _CombineMesh();
            }
        }

        protected override void EngineUpdateColor()
        {
            if (!IsChildArmature())
            {
                var proxyTrans = ArmatureDisplay._DBColor;
                if (IsCombineMesh)
                {
                    var meshBuffer = CombineMeshComponent.meshBuffers[_sumMeshIndex];
                    for (var i = 0; i < this.meshBuffer.vertexBuffers.Length; i++)
                    {
                        var index = _verticeOffset + i;
                        this.meshBuffer.color32Buffers[i].r = (byte)(Color.Value.redMultiplier * proxyTrans.redMultiplier * 255);
                        this.meshBuffer.color32Buffers[i].g = (byte)(Color.Value.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        this.meshBuffer.color32Buffers[i].b = (byte)(Color.Value.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        this.meshBuffer.color32Buffers[i].a = (byte)(Color.Value.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                        //
                        meshBuffer.color32Buffers[index] = this.meshBuffer.color32Buffers[i];
                    }

                    meshBuffer.UpdateColors();
                }
                else if (meshBuffer.sharedMesh != null)
                {
                    for (int i = 0, l = meshBuffer.sharedMesh.vertexCount; i < l; ++i)
                    {
                        meshBuffer.color32Buffers[i].r = (byte)(Color.Value.redMultiplier * proxyTrans.redMultiplier * 255);
                        meshBuffer.color32Buffers[i].g = (byte)(Color.Value.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        meshBuffer.color32Buffers[i].b = (byte)(Color.Value.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        meshBuffer.color32Buffers[i].a = (byte)(Color.Value.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                    }
                    //
                    meshBuffer.UpdateColors();
                }
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
            _CombineMesh();
        }

        protected override void EngineUpdateTransform()
        {
            if (IsCombineMesh)
            {
                var a = GlobalTransformDBMatrix.a;
                var b = GlobalTransformDBMatrix.b;
                var c = GlobalTransformDBMatrix.c;
                var d = GlobalTransformDBMatrix.d;
                var tx = GlobalTransformDBMatrix.tx;
                var ty = GlobalTransformDBMatrix.ty;

                var index = 0;
                var rx = 0.0f;
                var ry = 0.0f;
                var vx = 0.0f;
                var vy = 0.0f;
                var meshBuffer = CombineMeshComponent.meshBuffers[_sumMeshIndex];
                for (int i = 0, l = this.meshBuffer.vertexBuffers.Length; i < l; i++)
                {
                    index = i + _verticeOffset;
                    //vertices
                    rx = this.meshBuffer.rawVertextBuffers[i].x;
                    ry = -this.meshBuffer.rawVertextBuffers[i].y;

                    vx = rx * a + ry * c + tx;
                    vy = rx * b + ry * d + ty;

                    this.meshBuffer.vertexBuffers[i].x = vx;
                    this.meshBuffer.vertexBuffers[i].y = vy;

                    meshBuffer.vertexBuffers[index].x = vx;
                    meshBuffer.vertexBuffers[index].y = vy;
                }
                //
                meshBuffer.VertexDirty = true;
            }
            else
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
                            x = meshBuffer.rawVertextBuffers[i].x;
                            y = meshBuffer.rawVertextBuffers[i].y;

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

            if (UnityCurrentDisplay is UnityEngineChildArmatureSlotDisplay childArmatureSlot)
            {
                childArmatureSlot.ArmatureDisplay.Armature.flipX = Armature.flipX;
                childArmatureSlot.ArmatureDisplay.Armature.flipY = Armature.flipY;
            }
        }
    }
}