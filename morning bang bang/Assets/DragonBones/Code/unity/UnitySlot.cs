using UnityEngine;

namespace DragonBones
{
    public class UnitySlot : Slot
    {
        internal const float Z_OFFSET = 0.001f;
        private static readonly int[] TRIANGLES = { 0, 1, 2, 0, 2, 3 };
        private static Vector3 _helpVector3 = new Vector3();

        internal GameObject _renderDisplay;
        internal UnityUGUIDisplay _uiDisplay = null;

        internal MeshBuffer _meshBuffer;

        internal MeshRenderer _meshRenderer = null;
        internal MeshFilter _meshFilter = null;

        //combineMesh
        internal bool _isIgnoreCombineMesh;
        internal bool _isCombineMesh;
        internal int _sumMeshIndex = -1;
        internal int _verticeOrder = -1;
        internal int _verticeOffset = -1;
        internal UnityCombineMeshes _combineMesh = null;
        internal bool _isActive = false;

        private bool _skewed;
        private UnityEngineArmatureDisplay _proxy;
        private BlendMode _currentBlendMode;

        protected override void ClearObject()
        {
            base.ClearObject();

            _meshBuffer?.Dispose();

            _skewed = false;
            _proxy = null;

            _renderDisplay = null;
            _uiDisplay = null;

            _meshBuffer = null;

            _meshRenderer = null;
            _meshFilter = null;

            _isIgnoreCombineMesh = false;
            _isCombineMesh = false;
            _sumMeshIndex = -1;
            _verticeOrder = -1;
            _verticeOffset = -1;

            _combineMesh = null;

            _currentBlendMode = BlendMode.Normal;
            _isActive = false;
        }

        protected override void _InitDisplay(object value, bool isRetain)
        {

        }

        protected override void _DisposeDisplay(object value, bool isRelease)
        {
            if (!isRelease)
            {
                DBUnityFactory.UnityFactoryHelper.DestroyUnityObject(value as GameObject);
            }
        }

        protected override void _OnUpdateDisplay()
        {
            _renderDisplay = (display != null ? display : _rawDisplay) as GameObject;

            _proxy = _armature.Display as UnityEngineArmatureDisplay;
            if (_proxy.isUGUI)
            {
                _uiDisplay = _renderDisplay.GetComponent<UnityUGUIDisplay>();
                if (_uiDisplay == null)
                {
                    _uiDisplay = _renderDisplay.AddComponent<UnityUGUIDisplay>();
                    _uiDisplay.raycastTarget = false;
                }
            }
            else
            {
                _meshRenderer = _renderDisplay.GetComponent<MeshRenderer>();
                if (_meshRenderer == null)
                {
                    _meshRenderer = _renderDisplay.AddComponent<MeshRenderer>();
                }
                //
                _meshFilter = _renderDisplay.GetComponent<MeshFilter>();
                if (_meshFilter == null && _renderDisplay.GetComponent<TextMesh>() == null)
                {
                    _meshFilter = _renderDisplay.AddComponent<MeshFilter>();
                }
            }

            //init mesh
            if (_meshBuffer == null)
            {
                _meshBuffer = new MeshBuffer();
                _meshBuffer.sharedMesh = MeshBuffer.GenerateMesh();
                _meshBuffer.sharedMesh.name = name;
            }
        }

        protected override void _AddDisplay()
        {
            _proxy = _armature.Display as UnityEngineArmatureDisplay;
            var container = _proxy;
            if (_renderDisplay.transform.parent != container.transform)
            {
                _renderDisplay.transform.SetParent(container.transform);

                _helpVector3.Set(0.0f, 0.0f, 0.0f);
            }
        }
        /**
         * @private
         */
        protected override void _ReplaceDisplay(object value)
        {
            var container = _proxy;
            var prevDisplay = value as GameObject;
            int index = prevDisplay.transform.GetSiblingIndex();
            prevDisplay.SetActive(false);

            _renderDisplay.hideFlags = HideFlags.None;
            _renderDisplay.transform.SetParent(container.transform);
            _renderDisplay.SetActive(true);
            _renderDisplay.transform.SetSiblingIndex(index);

        }
        /**
         * @private
         */
        protected override void _RemoveDisplay()
        {
            _renderDisplay.transform.parent = null;
        }

        protected override void _UpdateZOrder()
        {
            SetZOrder(this._renderDisplay.transform.localPosition);

            //
            if (this.childArmature != null || !this._isActive)
            {
                this._CombineMesh();
            } 
        }
        internal void SetZOrder(Vector3 zorderPos)
        {
            if (this._isCombineMesh)
            {
                var meshBuffer = this._combineMesh.meshBuffers[this._sumMeshIndex];
                meshBuffer.zorderDirty = true;
            }

            {
                zorderPos.z = -this._zOrder * (this._proxy._zSpace + Z_OFFSET);

                if (_renderDisplay != null)
                {
                    _renderDisplay.transform.localPosition = zorderPos;
                    _renderDisplay.transform.SetSiblingIndex(_zOrder);

                    if (_proxy.isUGUI)
                    {
                        return;
                    }

                    if (childArmature == null)
                    {
                        _meshRenderer.sortingLayerName = LayerMask.LayerToName(_proxy.gameObject.layer);
                        if (_proxy.sortingMode == SortingMode.SortByOrder)
                        {
                            _meshRenderer.sortingOrder = _zOrder * UnityEngineArmatureDisplay.ORDER_SPACE;
                        }
                        else
                        {
                            _meshRenderer.sortingOrder = _proxy._sortingOrder;
                        }
                    }
                    else
                    {
                        UnityEngineArmatureDisplay childArmatureComp = childArmature.Display as UnityEngineArmatureDisplay;
                        childArmatureComp._sortingMode = _proxy._sortingMode;
                        childArmatureComp._sortingLayerName = _proxy._sortingLayerName;
                        if (_proxy._sortingMode == SortingMode.SortByOrder)
                        {
                            childArmatureComp.sortingOrder = _zOrder * UnityEngineArmatureDisplay.ORDER_SPACE;
                        }
                        else
                        {
                            childArmatureComp.sortingOrder = _proxy._sortingOrder;
                        }}
                    
                }
            }
        }
        /**
         * @private
         */

        public void DisallowCombineMesh()
        {
            CancelCombineMesh();
            _isIgnoreCombineMesh = true;
        }

        internal void CancelCombineMesh()
        {
            if (_isCombineMesh)
            {
                _isCombineMesh = false;
                if (_meshFilter != null)
                {
                    _meshFilter.sharedMesh = _meshBuffer.sharedMesh;
                    var isSkinnedMesh = _deformVertices != null && _deformVertices.verticesData != null && _deformVertices.verticesData.weight != null;
                    if (!isSkinnedMesh)
                    {
                        _meshBuffer.rawVertextBuffers.CopyTo(_meshBuffer.vertexBuffers, 0);
                    }

                    //
                    _meshBuffer.UpdateVertices();
                    _meshBuffer.UpdateColors();

                    if (isSkinnedMesh)
                    {
                        _UpdateMesh();
                        _IdentityTransform();
                    }
                    else
                    {
                        _UpdateTransform();
                    }
                }

                _meshBuffer.enabled = true;
            }

            if (_renderDisplay != null)
            {
                if (childArmature != null)
                {
                    _renderDisplay.SetActive(true);
                }
                else
                {
                    _renderDisplay.SetActive(_isActive);
                }
                //
                _renderDisplay.hideFlags = HideFlags.None;
            }

            //
            _isCombineMesh = false;
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
            if (_isIgnoreCombineMesh || _proxy.isUGUI)
            {
                return;
            }

            //已经合并过了，又触发合并，那么打断合并，用自己的网格数据还原
            if (_isCombineMesh)
            {
                //已经合并过，除非满足一下情况，否则都不能再合并, TODO
                CancelCombineMesh();
                _isIgnoreCombineMesh = true;
            }

            var combineMeshComp = _proxy.GetComponent<UnityCombineMeshes>();
            //从来没有合并过，触发合并，那么尝试合并
            if (combineMeshComp != null)
            {
                combineMeshComp.dirty = true;
            }
        }

        /**
         * @private
         */
        internal override void _UpdateVisible()
        {
            _renderDisplay.SetActive(_parent.visible);

            if (_isCombineMesh && !_parent.visible)
            {
                _CombineMesh();
            }
        }
        /**
         * @private
         */
        internal override void _UpdateBlendMode()
        {
            if (_currentBlendMode == _blendMode)
            {
                return;
            }

            if (childArmature == null)
            {
                if (_uiDisplay != null)
                {
                    _uiDisplay.material = (_textureData as UnityTextureData).GetMaterial(_blendMode, true);
                }
                else
                {
                    _meshRenderer.sharedMaterial = (_textureData as UnityTextureData).GetMaterial(_blendMode);
                }

                _meshBuffer.name = _uiDisplay != null ? _uiDisplay.material.name : _meshRenderer.sharedMaterial.name;
            }
            else
            {
                foreach (var slot in childArmature.GetSlots())
                {
                    slot._blendMode = _blendMode;
                    slot._UpdateBlendMode();
                }
            }

            _currentBlendMode = _blendMode;
            _CombineMesh();
        }
        /**
         * @private
         */
        protected override void _UpdateColor()
        {
            if (childArmature == null)
            {
                var proxyTrans = _proxy._colorTransform;
                if (_isCombineMesh)
                {
                    var meshBuffer = _combineMesh.meshBuffers[_sumMeshIndex];
                    for (var i = 0; i < _meshBuffer.vertexBuffers.Length; i++)
                    {
                        var index = _verticeOffset + i;
                        _meshBuffer.color32Buffers[i].r = (byte)(_colorTransform.redMultiplier * proxyTrans.redMultiplier * 255);
                        _meshBuffer.color32Buffers[i].g = (byte)(_colorTransform.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        _meshBuffer.color32Buffers[i].b = (byte)(_colorTransform.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        _meshBuffer.color32Buffers[i].a = (byte)(_colorTransform.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                        //
                        meshBuffer.color32Buffers[index] = _meshBuffer.color32Buffers[i];
                    }

                    meshBuffer.UpdateColors();
                }
                else if (_meshBuffer.sharedMesh != null)
                {
                    for (int i = 0, l = _meshBuffer.sharedMesh.vertexCount; i < l; ++i)
                    {
                        _meshBuffer.color32Buffers[i].r = (byte)(_colorTransform.redMultiplier * proxyTrans.redMultiplier * 255);
                        _meshBuffer.color32Buffers[i].g = (byte)(_colorTransform.greenMultiplier * proxyTrans.greenMultiplier * 255);
                        _meshBuffer.color32Buffers[i].b = (byte)(_colorTransform.blueMultiplier * proxyTrans.blueMultiplier * 255);
                        _meshBuffer.color32Buffers[i].a = (byte)(_colorTransform.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                    }
                    //
                    _meshBuffer.UpdateColors();
                }
            }
            else
            {
                //Set all childArmature color dirty
                (childArmature.Display as UnityEngineArmatureDisplay).color = _colorTransform;
            }

        }
        /**
         * @private
         */
        protected override void _UpdateFrame()
        {
            var currentVerticesData = (_deformVertices != null && display == _meshDisplay) ? _deformVertices.verticesData : null;
            var currentTextureData = _textureData as UnityTextureData;

            _meshBuffer.Clear();
            _isActive = false;
            if (_displayIndex >= 0 && display != null && currentTextureData != null)
            {
                var currentTextureAtlas = _proxy.isUGUI ? currentTextureAtlasData.uiTexture : currentTextureAtlasData.texture;
                if (currentTextureAtlas != null)
                {
                    _isActive = true;
                    //
                    var textureAtlasWidth = currentTextureAtlasData.width > 0.0f ? (int)currentTextureAtlasData.width : currentTextureAtlas.mainTexture.width;
                    var textureAtlasHeight = currentTextureAtlasData.height > 0.0f ? (int)currentTextureAtlasData.height : currentTextureAtlas.mainTexture.height;

                    var textureScale = _armature.armatureData.scale * currentTextureData.parent.scale;
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
                        if (_meshBuffer.uvBuffers == null || _meshBuffer.uvBuffers.Length != vertexCount)
                        {
                            _meshBuffer.uvBuffers = new Vector2[vertexCount];
                        }

                        if (_meshBuffer.rawVertextBuffers == null || _meshBuffer.rawVertextBuffers.Length != vertexCount)
                        {
                            _meshBuffer.rawVertextBuffers = new Vector3[vertexCount];
                            _meshBuffer.vertexBuffers = new Vector3[vertexCount];
                        }

                        _meshBuffer.triangleBuffers = new int[triangleCount * 3];

                        for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
                        {
                            _meshBuffer.uvBuffers[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                            _meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                            _meshBuffer.rawVertextBuffers[i].x = floatArray[iV++] * textureScale;
                            _meshBuffer.rawVertextBuffers[i].y = floatArray[iV++] * textureScale;

                            _meshBuffer.vertexBuffers[i].x = _meshBuffer.rawVertextBuffers[i].x;
                            _meshBuffer.vertexBuffers[i].y = _meshBuffer.rawVertextBuffers[i].y;
                        }

                        for (int i = 0; i < triangleCount * 3; ++i)
                        {
                            _meshBuffer.triangleBuffers[i] = intArray[meshOffset + (int)BinaryOffset.MeshVertexIndices + i];
                        }

                        var isSkinned = currentVerticesData.weight != null;
                        if (isSkinned)
                        {
                            _IdentityTransform();
                        }
                    }
                    else
                    {
                        if (_meshBuffer.rawVertextBuffers == null || _meshBuffer.rawVertextBuffers.Length != 4)
                        {
                            _meshBuffer.rawVertextBuffers = new Vector3[4];
                            _meshBuffer.vertexBuffers = new Vector3[4];
                        }

                        if (_meshBuffer.uvBuffers == null || _meshBuffer.uvBuffers.Length != _meshBuffer.rawVertextBuffers.Length)
                        {
                            _meshBuffer.uvBuffers = new Vector2[_meshBuffer.rawVertextBuffers.Length];
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

                                default:
                                    break;
                            }

                            var scaleWidth = sourceWidth * textureScale;
                            var scaleHeight = sourceHeight * textureScale;
                            var pivotX = _pivotX;
                            var pivotY = _pivotY;

                            if (currentTextureData.rotated)
                            {
                                var temp = scaleWidth;
                                scaleWidth = scaleHeight;
                                scaleHeight = temp;

                                pivotX = scaleWidth - _pivotX;
                                pivotY = scaleHeight - _pivotY;
                                //uv
                                _meshBuffer.uvBuffers[i].x = (sourceX + (1.0f - v) * sourceWidth) / textureAtlasWidth;
                                _meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + u * sourceHeight) / textureAtlasHeight;
                            }
                            else
                            {
                                //uv
                                _meshBuffer.uvBuffers[i].x = (sourceX + u * sourceWidth) / textureAtlasWidth;
                                _meshBuffer.uvBuffers[i].y = 1.0f - (sourceY + v * sourceHeight) / textureAtlasHeight;
                            }

                            //vertices
                            _meshBuffer.rawVertextBuffers[i].x = u * scaleWidth - pivotX;
                            _meshBuffer.rawVertextBuffers[i].y = (1.0f - v) * scaleHeight - pivotY;

                            _meshBuffer.vertexBuffers[i].x = _meshBuffer.rawVertextBuffers[i].x;
                            _meshBuffer.vertexBuffers[i].y = _meshBuffer.rawVertextBuffers[i].y;
                        }

                        _meshBuffer.triangleBuffers = TRIANGLES;
                    }

                    if (_proxy.isUGUI)
                    {
                        _uiDisplay.material = currentTextureAtlas;
                        _uiDisplay.texture = currentTextureAtlas.mainTexture;
                        _uiDisplay.sharedMesh = _meshBuffer.sharedMesh;
                    }
                    else
                    {
                        _meshFilter.sharedMesh = _meshBuffer.sharedMesh;
                        _meshRenderer.sharedMaterial = currentTextureAtlas;
                    }

                    _meshBuffer.name = currentTextureAtlas.name;
                    _meshBuffer.InitMesh();
                    _currentBlendMode = BlendMode.Normal;
                    _blendModeDirty = true;
                    _colorDirty = true;// Relpace texture will override blendMode and color.
                    _visibleDirty = true;

                    _CombineMesh();
                    return;
                }
            }

            _renderDisplay.SetActive(_isActive);
            if (_proxy.isUGUI)
            {
                _uiDisplay.material = null;
                _uiDisplay.texture = null;
                _uiDisplay.sharedMesh = null;
            }
            else
            {
                _meshFilter.sharedMesh = null;
                _meshRenderer.sharedMaterial = null;
            }

            _helpVector3.x = 0.0f;
            _helpVector3.y = 0.0f;
            _helpVector3.z = _renderDisplay.transform.localPosition.z;

            _renderDisplay.transform.localPosition = _helpVector3;

            if (_isCombineMesh)
            {
                _CombineMesh();
            }
        }

        protected override void _IdentityTransform()
        {
            var transform = _renderDisplay.transform;

            transform.localPosition = new Vector3(0.0f, 0.0f, transform.localPosition.z);
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        protected override void _UpdateMesh()
        {
            if (_meshBuffer.sharedMesh == null || _deformVertices == null)
            {
                return;
            }
            
            var scale = _armature.armatureData.scale;
            var deformVertices = _deformVertices.vertices;
            var bones = _deformVertices.bones;
            var hasDeform = deformVertices.Count > 0;
            var verticesData = _deformVertices.verticesData;
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
                if (_isCombineMesh)
                {
                    meshBuffer = _combineMesh.meshBuffers[_sumMeshIndex];
                }
                int iB = weightData.offset + (int)BinaryOffset.WeigthBoneIndices + weightData.bones.Count, iV = weightFloatOffset, iF = 0;
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
                            var matrix = bone.globalTransformMatrix;
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
                    _meshBuffer.vertexBuffers[i].x = xG;
                    _meshBuffer.vertexBuffers[i].y = yG;

                    if (meshBuffer != null)
                    {
                        meshBuffer.vertexBuffers[i + _verticeOffset].x = xG;
                        meshBuffer.vertexBuffers[i + _verticeOffset].y = yG;
                    }
                }

                if (meshBuffer != null)
                {
                    meshBuffer.vertexDirty = true;
                }
                else
                {
                    // if (this._meshRenderer && this._meshRenderer.enabled)
                    {
                        _meshBuffer.UpdateVertices();
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
                var a = globalTransformMatrix.a;
                var b = globalTransformMatrix.b;
                var c = globalTransformMatrix.c;
                var d = globalTransformMatrix.d;
                var tx = globalTransformMatrix.tx;
                var ty = globalTransformMatrix.ty;

                var index = 0;
                var rx = 0.0f;
                var ry = 0.0f;
                var vx = 0.0f;
                var vy = 0.0f;
                MeshBuffer meshBuffer = null;
                if (_isCombineMesh)
                {
                    meshBuffer = _combineMesh.meshBuffers[_sumMeshIndex];
                }

                for (int i = 0, iV = 0, iF = 0, l = vertextCount; i < l; ++i)
                {
                    rx = (data.floatArray[vertexOffset + (iV++)] * scale + deformVertices[iF++]);
                    ry = (data.floatArray[vertexOffset + (iV++)] * scale + deformVertices[iF++]);

                    _meshBuffer.rawVertextBuffers[i].x = rx;
                    _meshBuffer.rawVertextBuffers[i].y = -ry;

                    _meshBuffer.vertexBuffers[i].x = rx;
                    _meshBuffer.vertexBuffers[i].y = -ry;

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
                    meshBuffer.vertexDirty = true;
                }
                // else if (this._meshRenderer && this._meshRenderer.enabled)
                else
                {
                    _meshBuffer.UpdateVertices();
                }
            }
        }

        protected override void _UpdateTransform()
        {
            if (_isCombineMesh)
            {
                var a = globalTransformMatrix.a;
                var b = globalTransformMatrix.b;
                var c = globalTransformMatrix.c;
                var d = globalTransformMatrix.d;
                var tx = globalTransformMatrix.tx;
                var ty = globalTransformMatrix.ty;

                var index = 0;
                var rx = 0.0f;
                var ry = 0.0f;
                var vx = 0.0f;
                var vy = 0.0f;
                var meshBuffer = _combineMesh.meshBuffers[_sumMeshIndex];
                for (int i = 0, l = _meshBuffer.vertexBuffers.Length; i < l; i++)
                {
                    index = i + _verticeOffset;
                    //vertices
                    rx = _meshBuffer.rawVertextBuffers[i].x;
                    ry = -_meshBuffer.rawVertextBuffers[i].y;

                    vx = rx * a + ry * c + tx;
                    vy = rx * b + ry * d + ty;

                    _meshBuffer.vertexBuffers[i].x = vx;
                    _meshBuffer.vertexBuffers[i].y = vy;

                    meshBuffer.vertexBuffers[index].x = vx;
                    meshBuffer.vertexBuffers[index].y = vy;
                }
                //
                meshBuffer.vertexDirty = true;
            }
            else
            {
                UpdateGlobalTransform(); // Update transform.

                //localPosition
                var flipX = _armature.flipX;
                var flipY = _armature.flipY;
                var transform = _renderDisplay.transform;

                _helpVector3.x = global.x;
                _helpVector3.y = global.y;
                _helpVector3.z = transform.localPosition.z;

                transform.localPosition = _helpVector3;

                //localEulerAngles
                if (childArmature == null)
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
                if ((display == _rawDisplay || display == _meshDisplay) && _meshBuffer.sharedMesh != null)
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
                        for (int i = 0, l = _meshBuffer.vertexBuffers.Length; i < l; ++i)
                        {
                            x = _meshBuffer.rawVertextBuffers[i].x;
                            y = _meshBuffer.rawVertextBuffers[i].y;

                            if (isPositive)
                            {
                                _meshBuffer.vertexBuffers[i].x = x + y * sin;
                            }
                            else
                            {
                                _meshBuffer.vertexBuffers[i].x = -x + y * sin;
                            }

                            _meshBuffer.vertexBuffers[i].y = y * cos;
                        }

                        // if (this._meshRenderer && this._meshRenderer.enabled)
                        {
                            _meshBuffer.UpdateVertices();
                        }
                    }
                }

                //localScale
                _helpVector3.x = global.scaleX;
                _helpVector3.y = global.scaleY;
                _helpVector3.z = 1.0f;

                transform.localScale = _helpVector3;
            }

            if (childArmature != null)
            {
                childArmature.flipX = _armature.flipX;
                childArmature.flipY = _armature.flipY;
            }
        }

        public Mesh mesh
        {
            get
            {
                if (_meshBuffer == null)
                {
                    return null;
                }

                return _meshBuffer.sharedMesh;
            }
        }

        public MeshRenderer meshRenderer
        {
            get { return _meshRenderer; }
        }

        public UnityTextureAtlasData currentTextureAtlasData
        {
            get
            {
                if (_textureData == null || _textureData.parent == null)
                {
                    return null;
                }

                return _textureData.parent as UnityTextureAtlasData;
            }
        }

        public GameObject renderDisplay
        {
            get { return _renderDisplay; }
        }

        public UnityEngineArmatureDisplay proxy
        {
            get { return _proxy; }
        }

        public bool isIgnoreCombineMesh
        {
            get { return _isIgnoreCombineMesh; }
        }
    }
}