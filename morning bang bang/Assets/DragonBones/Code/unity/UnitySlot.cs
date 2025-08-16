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

        public MeshBuffer MeshBuffer;

        public UnityTextureAtlasData CurrentTextureAtlasData
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

        public UnityEngineArmatureDisplay ArmatureDisplay { get; private set; }

        public bool IsVisible { get; private set; } = false;

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public override void OnReleased()
        {
            base.OnReleased();

            MeshBuffer?.Dispose();

            ArmatureDisplay = null;

            MeshBuffer = null;

            _skewed = false;

            BlendMode.Set(DragonBones.BlendMode.Normal);
            IsVisible = false;
            Displays.Clear();
        }

        internal void UpdateZPosition()
        {
            float z = -ZOrder.Value * (ArmatureDisplay._zSpace + Z_OFFSET);

            if (!IsDisplayingChildArmature()) return;

            UnityEngineArmatureDisplay childArmatureComp =
                (Displays.CurrentChildArmature as UnityEngineChildArmatureSlotDisplay).ArmatureDisplay as UnityEngineArmatureDisplay;

            childArmatureComp.transform.localPosition = new Vector3(childArmatureComp.transform.localPosition.x,
                childArmatureComp.transform.localPosition.y, z);

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

        public void StarUnitySlotBuilding(UnityEngineArmatureDisplay unityArmatureDisplay) => ArmatureDisplay = unityArmatureDisplay;
        public void EndUnitySlotBuilding()
        {
            if (MeshBuffer == null)
            {
                MeshBuffer = new MeshBuffer();
                MeshBuffer.Name = Name;
            }
            else
            {
                MeshBuffer.Clear();
            }
            EngineUpdateDisplay();
        }

        protected override void EngineUpdateFrame()
        {
            CreateMeshBuffer();
        }

        private bool CreateMeshBuffer()
        {
            if (MeshBuffer == null)
            {
                MeshBuffer = new MeshBuffer();
                MeshBuffer.Name = Name;
            }
            else
            {
                MeshBuffer.Clear();
            }

            VerticesData currentVerticesData = (DeformVertices != null && !IsDisplayingChildArmature()) ? DeformVertices.verticesData : null;
            UnityTextureData currentTextureData = TextureData as UnityTextureData;

            if (!IsDisplayingChildArmature() && !Displays.HasVisibleDisplay || currentTextureData == null) return false;

            Material currentTextureAtlas = CurrentTextureAtlasData.texture;

            if (currentTextureAtlas == null) return false;

            //if we display mesh, so we will do mesh stuff
            if (currentVerticesData != null)
            {
                FillMeshBufferWithVerticesData(currentVerticesData, currentTextureData);
            }
            //else, we will just create rectangle mesh just for image displaying
            else
            {
                CreateRectangleMeshForImageDisplay(currentTextureData);
            }

            MeshBuffer.Name = currentTextureAtlas.name;

            BlendMode.Set(DragonBones.BlendMode.Normal);
            BlendMode.MarkAsDirty();
            Color.MarkAsDirty();
            Visible.MarkAsDirty();

            return true;
        }

        private void FillMeshBufferWithVerticesData(VerticesData currentVerticesData, TextureData currentTextureData)
        {
            int textureAtlasWidth = CurrentTextureAtlasData.width > 0.0f ? (int)CurrentTextureAtlasData.width : CurrentTextureAtlasData.texture.mainTexture.width;
            int textureAtlasHeight = CurrentTextureAtlasData.height > 0.0f ? (int)CurrentTextureAtlasData.height : CurrentTextureAtlasData.texture.mainTexture.height;

            float textureScale = Armature.ArmatureData.scale * currentTextureData.parent.scale;
            float sourceX = currentTextureData.region.x;
            float sourceY = currentTextureData.region.y;
            float sourceWidth = currentTextureData.region.width;
            float sourceHeight = currentTextureData.region.height;

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

            MeshBuffer.uvBuffer = new Vector2[vertexCount];
            MeshBuffer.rawVertexBuffer = new Vector3[vertexCount];
            MeshBuffer.vertexBuffer = new Vector3[vertexCount];
            MeshBuffer.triangleBuffer = new int[triangleCount * 3];

            for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
            {
                MeshBuffer.uvBuffer[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                MeshBuffer.uvBuffer[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                MeshBuffer.rawVertexBuffer[i].x = floatArray[iV++] * textureScale;
                MeshBuffer.rawVertexBuffer[i].y = floatArray[iV++] * textureScale;

                MeshBuffer.vertexBuffer[i].x = MeshBuffer.rawVertexBuffer[i].x;
                MeshBuffer.vertexBuffer[i].y = MeshBuffer.rawVertexBuffer[i].y;
            }

            for (int i = 0; i < triangleCount * 3; ++i)
            {
                MeshBuffer.triangleBuffer[i] = intArray[meshOffset + (int)BinaryOffset.MeshVertexIndices + i];
            }
        }
        private void CreateRectangleMeshForImageDisplay(TextureData currentTextureData)
        {
            int textureAtlasWidth = CurrentTextureAtlasData.width > 0.0f ? (int)CurrentTextureAtlasData.width : CurrentTextureAtlasData.texture.mainTexture.width;
            int textureAtlasHeight = CurrentTextureAtlasData.height > 0.0f ? (int)CurrentTextureAtlasData.height : CurrentTextureAtlasData.texture.mainTexture.height;

            float textureScale = Armature.ArmatureData.scale * currentTextureData.parent.scale;
            float sourceX = currentTextureData.region.x;
            float sourceY = currentTextureData.region.y;
            float sourceWidth = currentTextureData.region.width;
            float sourceHeight = currentTextureData.region.height;

            if (MeshBuffer.rawVertexBuffer == null || MeshBuffer.rawVertexBuffer.Length != 4)
            {
                MeshBuffer.rawVertexBuffer = new Vector3[4];
                MeshBuffer.vertexBuffer = new Vector3[4];
            }

            if (MeshBuffer.uvBuffer == null || MeshBuffer.uvBuffer.Length != MeshBuffer.rawVertexBuffer.Length)
            {
                MeshBuffer.uvBuffer = new Vector2[MeshBuffer.rawVertexBuffer.Length];
            }

            if(MeshBuffer.color32Buffer == null) MeshBuffer.color32Buffer = new Color32[MeshBuffer.VertexCount];

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
                    MeshBuffer.uvBuffer[i].x = (sourceX + (1.0f - v) * sourceWidth) / textureAtlasWidth;
                    MeshBuffer.uvBuffer[i].y = 1.0f - (sourceY + u * sourceHeight) / textureAtlasHeight;
                }
                else
                {
                    //uv
                    MeshBuffer.uvBuffer[i].x = (sourceX + u * sourceWidth) / textureAtlasWidth;
                    MeshBuffer.uvBuffer[i].y = 1.0f - (sourceY + v * sourceHeight) / textureAtlasHeight;
                }

                //vertices
                MeshBuffer.rawVertexBuffer[i].x = u * scaleWidth - pivotX;
                MeshBuffer.rawVertexBuffer[i].y = (1.0f - v) * scaleHeight - pivotY;

                MeshBuffer.vertexBuffer[i].x = MeshBuffer.rawVertexBuffer[i].x;
                MeshBuffer.vertexBuffer[i].y = MeshBuffer.rawVertexBuffer[i].y;
            }

            MeshBuffer.triangleBuffer = TRIANGLES;
        }

        protected override void EngineUpdateDisplay()
        {
            ArmatureDisplay = Armature.Display as UnityEngineArmatureDisplay;


        }

        protected override void EngineUpdateZOrder()
        {
            UpdateZPosition();
        }

        protected override void EngineUpdateMesh()
        {
            if (MeshBuffer.sharedMesh == null || DeformVertices == null) { return; }

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
                    MeshBuffer.vertexBuffer[i].x = xG;
                    MeshBuffer.vertexBuffer[i].y = yG;
                }
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

                    MeshBuffer.rawVertexBuffer[i].x = rx;
                    MeshBuffer.rawVertexBuffer[i].y = -ry;

                    MeshBuffer.vertexBuffer[i].x = rx;
                    MeshBuffer.vertexBuffer[i].y = -ry;
                }
            }
        }
        protected override void EngineUpdateVisibility()
        {
            if(Parent.visible) Show();
            if (!Parent.visible) Hide();
        }

        protected override void EngineUpdateColor()
        {
            if (!IsDisplayingChildArmature())
            {
                DBColor proxyTrans = ArmatureDisplay._DBColor;

                for (int i = 0, l = MeshBuffer.vertexBuffer.Length; i < l; ++i)
                {
                    MeshBuffer.color32Buffer[i].r = (byte)(Color.Value.redMultiplier * proxyTrans.redMultiplier * 255);
                    MeshBuffer.color32Buffer[i].g = (byte)(Color.Value.greenMultiplier * proxyTrans.greenMultiplier * 255);
                    MeshBuffer.color32Buffer[i].b = (byte)(Color.Value.blueMultiplier * proxyTrans.blueMultiplier * 255);
                    MeshBuffer.color32Buffer[i].a = (byte)(Color.Value.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                }
            }
            else
            {
                //Set all childArmature color dirty
                ((UnityEngineArmatureDisplay)Displays.CurrentChildArmature.ArmatureDisplay).DBColor = Color.Value;
            }
        }

        protected override void EngineUpdateBlendMode()
        {
            if (BlendMode.NotDirty) return;

            if (!IsDisplayingChildArmature())
            {
                //TODO
                //CurrentAsMeshDisplay.MeshRenderer.sharedMaterial = ((UnityTextureData)TextureData).GetMaterial(BlendMode.Value);
            }
            else
            {
                foreach (Slot slot in Displays.CurrentChildArmature.ArmatureDisplay.Armature.Structure.Slots)
                {
                    slot.BlendMode.Set(BlendMode.Value);
                }
            }

            BlendMode.ResetDirty();
        }

        protected override void EngineUpdateTransform()
        {
            UpdateGlobalTransform(); // Update transform.

            if (IsDisplayingChildArmature())
            {
                UnityEngineChildArmatureSlotDisplay CurrentAsChildArmatureDisplay = Displays.CurrentChildArmature as UnityEngineChildArmatureSlotDisplay;
                UpdateGameObjectTransform(CurrentAsChildArmatureDisplay.transform);

                CurrentAsChildArmatureDisplay.ArmatureDisplay.Armature.flipX = Armature.flipX;
                CurrentAsChildArmatureDisplay.ArmatureDisplay.Armature.flipY = Armature.flipY;
            }
            else
            {
                UpdateMeshBufferTransform();
            }
        }

        private void UpdateMeshBufferTransform()
        {
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

            if (MeshBuffer == null || MeshBuffer.vertexBuffer == null) return;

            for (int i = 0, l = MeshBuffer.vertexBuffer.Length; i < l; i++)
            {
                rx = MeshBuffer.rawVertexBuffer[i].x;
                ry = -MeshBuffer.rawVertexBuffer[i].y;

                vx = rx * a + ry * c + tx;
                vy = rx * b + ry * d + ty;

                MeshBuffer.vertexBuffer[i].x = vx;
                MeshBuffer.vertexBuffer[i].y = vy;
                MeshBuffer.vertexBuffer[i].z = -ZOrder.Value * (ArmatureDisplay._zSpace + Z_OFFSET);
            }

            //MeshBuffer.InitMesh();
            //MeshBuffer.VertexDirty = true;
        }

        private void UpdateGameObjectTransform(Transform transform)
        {
            //localPosition
            bool flipX = Armature.flipX;
            bool flipY = Armature.flipY;

            _helpVector3.x = global.x;
            _helpVector3.y = global.y;
            _helpVector3.z = transform.localPosition.z;

            transform.localPosition = _helpVector3;

            //localEulerAngles
            if (!IsDisplayingChildArmature())
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
            if (!IsDisplayingChildArmature())
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

                    for (int i = 0, l = MeshBuffer.vertexBuffer.Length; i < l; ++i)
                    {
                        x = MeshBuffer.rawVertexBuffer[i].x;
                        y = MeshBuffer.rawVertexBuffer[i].y;

                        if (isPositive)
                        {
                            MeshBuffer.vertexBuffer[i].x = x + y * sin;
                        }
                        else
                        {
                            MeshBuffer.vertexBuffer[i].x = -x + y * sin;
                        }

                        MeshBuffer.vertexBuffer[i].y = y * cos;
                    }

                    // if (this.CurrentUnityDisplay.MeshRenderer && this.CurrentUnityDisplay.MeshRenderer.enabled)
                    {
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
