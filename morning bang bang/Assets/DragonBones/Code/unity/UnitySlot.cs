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

        public DBMeshBuffer MeshBufferBuffer => DB.Registry.GetMesh(ID);

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

        public UnityArmatureRoot ArmatureRoot { get; private set; }


        public override void OnReleased()
        {
            base.OnReleased();           
            ArmatureRoot = null;
            _skewed = false;
            BlendMode.Set(DragonBones.BlendMode.Normal);
        }

        internal void UpdateZPosition()
        {
            return;
            //float z = -DrawOrder.Value * (ArmatureDisplay._zSpace + Z_OFFSET);
//
            //if (!IsDisplayingChildArmature()) return;
//
            //UnityEngineArmatureDisplay childArmatureComp =
            //    (Displays.CurrentChildArmature as UnityEngineChildArmatureSlotDisplay).ArmatureDisplay as UnityEngineArmatureDisplay;
//
            //childArmatureComp.transform.localPosition = new Vector3(childArmatureComp.transform.localPosition.x,
            //    childArmatureComp.transform.localPosition.y, z);
//
            //childArmatureComp._sortingMode = ArmatureDisplay._sortingMode;
            //childArmatureComp._sortingLayerName = ArmatureDisplay._sortingLayerName;
//
            //if (ArmatureDisplay._sortingMode == SortingMode.SortByOrder)
            //{
            //    childArmatureComp.sortingOrder = DrawOrder.Value * UnityEngineArmatureDisplay.ORDER_SPACE;
            //}
            //else
            //{
            //    childArmatureComp.sortingOrder = ArmatureDisplay._sortingOrder;
            //}
        }

        public void StarUnitySlotBuilding(UnityArmatureRoot unityArmatureRoot)
        {
            ArmatureRoot = unityArmatureRoot;
        }

        public override void SlotReady()
        {
            base.SlotReady();

            EngineUpdateDisplay();
        }

        protected override void EngineUpdateFrame()
        {
            CreateMeshBuffer();
        }

        private bool CreateMeshBuffer()
        {
            if (!DB.Registry.HasMesh(ID))
            {
                DB.Registry.CreateMesh(ID);
            }

            if (IsDisplayingChildArmature())
            {
                return true;
            }

            VerticesData currentVerticesData = (DeformVertices != null && !IsDisplayingChildArmature()) ? DeformVertices.verticesData : null;
            UnityTextureData currentTextureData = TextureData as UnityTextureData;

            if (!IsDisplayingChildArmature() && (!HasVisibleDisplay || currentTextureData == null)) return false;

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

            BlendMode.Set(DragonBones.BlendMode.Normal);
            BlendMode.MarkAsDirty();
            Color.MarkAsDirty();
            Visible.MarkAsDirty();
            DB.Registry.GetMesh(ID).Material = currentTextureAtlas;

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

            MeshBufferBuffer.uvBuffer = new Vector2[vertexCount];
            MeshBufferBuffer.rawVertexBuffer = new Vector3[vertexCount];
            MeshBufferBuffer.vertexBuffer = new Vector3[vertexCount];
            MeshBufferBuffer.triangleBuffer = new int[triangleCount * 3];

            for (int i = 0, iV = vertexOffset, iU = uvOffset, l = vertexCount; i < l; ++i)
            {
                MeshBufferBuffer.uvBuffer[i].x = (sourceX + floatArray[iU++] * sourceWidth) / textureAtlasWidth;
                MeshBufferBuffer.uvBuffer[i].y = 1.0f - (sourceY + floatArray[iU++] * sourceHeight) / textureAtlasHeight;

                MeshBufferBuffer.rawVertexBuffer[i].x = floatArray[iV++] * textureScale;
                MeshBufferBuffer.rawVertexBuffer[i].y = floatArray[iV++] * textureScale;

                MeshBufferBuffer.vertexBuffer[i].x = MeshBufferBuffer.rawVertexBuffer[i].x;
                MeshBufferBuffer.vertexBuffer[i].y = MeshBufferBuffer.rawVertexBuffer[i].y;
            }

            for (int i = 0; i < triangleCount * 3; ++i)
            {
                MeshBufferBuffer.triangleBuffer[i] = intArray[meshOffset + (int)BinaryOffset.MeshVertexIndices + i];
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

            if (MeshBufferBuffer.rawVertexBuffer == null || MeshBufferBuffer.rawVertexBuffer.Length != 4)
            {
                MeshBufferBuffer.rawVertexBuffer = new Vector3[4];
                MeshBufferBuffer.vertexBuffer = new Vector3[4];
            }

            if (MeshBufferBuffer.uvBuffer == null || MeshBufferBuffer.uvBuffer.Length != MeshBufferBuffer.rawVertexBuffer.Length)
            {
                MeshBufferBuffer.uvBuffer = new Vector2[MeshBufferBuffer.rawVertexBuffer.Length];
            }

            if(MeshBufferBuffer.color32Buffer == null) MeshBufferBuffer.color32Buffer = new Color32[MeshBufferBuffer.VertexCount];

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
                    MeshBufferBuffer.uvBuffer[i].x = (sourceX + (1.0f - v) * sourceWidth) / textureAtlasWidth;
                    MeshBufferBuffer.uvBuffer[i].y = 1.0f - (sourceY + u * sourceHeight) / textureAtlasHeight;
                }
                else
                {
                    //uv
                    MeshBufferBuffer.uvBuffer[i].x = (sourceX + u * sourceWidth) / textureAtlasWidth;
                    MeshBufferBuffer.uvBuffer[i].y = 1.0f - (sourceY + v * sourceHeight) / textureAtlasHeight;
                }

                //vertices
                MeshBufferBuffer.rawVertexBuffer[i].x = u * scaleWidth - pivotX;
                MeshBufferBuffer.rawVertexBuffer[i].y = (1.0f - v) * scaleHeight - pivotY;

                MeshBufferBuffer.vertexBuffer[i].x = MeshBufferBuffer.rawVertexBuffer[i].x;
                MeshBufferBuffer.vertexBuffer[i].y = MeshBufferBuffer.rawVertexBuffer[i].y;
            }

            MeshBufferBuffer.triangleBuffer = TRIANGLES;
        }

        protected override void EngineUpdateOutput()
        {
            if (IsDisplayingChildArmature()) return;
        }

        protected override void EngineUpdateDisplay()
        {
            ArmatureRoot = Armature.Root as UnityArmatureRoot;


        }

        protected override void EngineUpdateZOrder()
        {
            UpdateZPosition();
        }

        protected override void EngineUpdateDeformMesh()
        {
            if (MeshBufferBuffer.GeneratedMesh == null || DeformVertices == null) { return; }

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
                    MeshBufferBuffer.vertexBuffer[i].x = xG;
                    MeshBufferBuffer.vertexBuffer[i].y = yG;
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

                    MeshBufferBuffer.rawVertexBuffer[i].x = rx;
                    MeshBufferBuffer.rawVertexBuffer[i].y = -ry;

                    MeshBufferBuffer.vertexBuffer[i].x = rx;
                    MeshBufferBuffer.vertexBuffer[i].y = -ry;
                }
            }
        }
        protected override void EngineUpdateVisibility()
        {
        }

        protected override void EngineUpdateColor()
        {
            if (!IsDisplayingChildArmature())
            {
                DBColor proxyTrans = ArmatureRoot.Color;

                return;
                for (int i = 0, l = MeshBufferBuffer.vertexBuffer.Length; i < l; ++i)
                {
                    MeshBufferBuffer.color32Buffer[i].r = (byte)(Color.V.redMultiplier * proxyTrans.redMultiplier * 255);
                    MeshBufferBuffer.color32Buffer[i].g = (byte)(Color.V.greenMultiplier * proxyTrans.greenMultiplier * 255);
                    MeshBufferBuffer.color32Buffer[i].b = (byte)(Color.V.blueMultiplier * proxyTrans.blueMultiplier * 255);
                    MeshBufferBuffer.color32Buffer[i].a = (byte)(Color.V.alphaMultiplier * proxyTrans.alphaMultiplier * 255);
                }
            }
            else
            {
                //Set all childArmature color dirty TODO
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
                //TODO
                //foreach (Slot slot in Displays.CurrentChildArmature.Armature.Structure.Slots)
                //{
                //    slot.BlendMode.Set(BlendMode.V);
                //}
            }

            BlendMode.ResetDirty();
        }

        protected override void EngineUpdateTransform()
        {
            UpdateGlobalTransform(); // Update transform.
            UpdateMeshBufferTransform();

            return;
            if (IsDisplayingChildArmature() && MeshBufferBuffer.GeneratedMesh== null)
            {
            }
            else
            {
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

            if (MeshBufferBuffer == null || MeshBufferBuffer.vertexBuffer == null) return;

            for (int i = 0, l = MeshBufferBuffer.vertexBuffer.Length; i < l; i++)
            {
                rx = MeshBufferBuffer.rawVertexBuffer[i].x;
                ry = -MeshBufferBuffer.rawVertexBuffer[i].y;

                vx = rx * a + ry * c + tx;
                vy = rx * b + ry * d + ty;

                MeshBufferBuffer.vertexBuffer[i].x = vx * 0.16f;
                MeshBufferBuffer.vertexBuffer[i].y = vy * 0.16f;
            }
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

                    for (int i = 0, l = MeshBufferBuffer.vertexBuffer.Length; i < l; ++i)
                    {
                        x = MeshBufferBuffer.rawVertexBuffer[i].x;
                        y = MeshBufferBuffer.rawVertexBuffer[i].y;

                        if (isPositive)
                        {
                            MeshBufferBuffer.vertexBuffer[i].x = x + y * sin;
                        }
                        else
                        {
                            MeshBufferBuffer.vertexBuffer[i].x = -x + y * sin;
                        }

                        MeshBufferBuffer.vertexBuffer[i].y = y * cos;
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
