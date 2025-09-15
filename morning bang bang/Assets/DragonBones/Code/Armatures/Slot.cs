namespace DragonBones
{
    public abstract class Slot : TransformObject
    {
        public string displayController;

        public readonly Dirty<DisplayData> Display = new();
        private DisplayData DisplayData => Display.V;
        public bool HasVisibleDisplay => !Display.Eq(null);
        
        public string Name => SlotData.Name;

        public Bone ParentBone { get; private set; }
        
        public SlotData SlotData { get; internal set; }
        public BoundingBoxData BoundingBoxData { get; protected set; }

        public readonly Dirty<DBColor> Color = new(new DBColor());
        public DeformVertices DeformVertices;
        
        public float PivotX;
        public float PivotY;
        
        protected readonly DBMatrix LocalDBMatrix = new DBMatrix();
        protected TextureData TextureData;

        public bool TransformDirty { get; set; }
        public Dirty<bool> Visible { get; private set; } = new();
        public Dirty<int> DrawOrder { get; private set; } = new();
        public Dirty<BlendMode> BlendMode { get; protected set; } = new();

        public virtual void StartBuilding(SlotData data, Armature armature)
        {
            SlotData = data;
            ParentArmature = armature;
        }

        public virtual void SlotReady()
        {
            DrawOrder.Set(SlotData.zOrder);
            BlendMode.Set(SlotData.blendMode);
            Color.GetAndChange().CopyFrom(SlotData.DBColor);
            RefreshData();
        }
        
        #region Animation Loop 
        public void ProcessDirtyDisplay()
        {
            if (Display.IsDirty)
            {
                if (Display.pV != null && Display.pV.Type == DisplayType.Armature)
                {
                    ParentArmature.Structure.GetChildArmature(Display.pV).IsActive = false;
                }

                if (!HasVisibleDisplay)
                {
                    Visible.Set(false);
                    return;
                }  
                else if(!Visible.V)
                {
                    Visible.Set(true);
                }

                if (Display.V.Type == DisplayType.Armature)
                {
                    ParentArmature.Structure.GetChildArmature(Display.V).IsActive = true;
                }
                
                RefreshData();
                UpdateDisplay();
                if (TransformDirty) UpdateLocalMatrix();
                
                Display.ResetDirty();
            }
        }

        public void UpdateCache(AnimationData animation, DBFrameCacher cacher, int frameIndex)
        {
            ProcessCacheFrameIndex(animation, cacher, frameIndex);
        }
        
        public virtual void ProcessDirtyData()
        {
            if (ParentBone._childrenTransformDirty) TransformDirty = true;
            
            if(Color.IsDirty) EngineUpdateColor();
            if(Visible.IsDirty)
            {
                EngineUpdateVisibility();
            }

            //TODO            if(DrawOrder.IsDirty) EngineUpdateZOrder();
            if(BlendMode.IsDirty) EngineUpdateBlendMode();
            
            if (DeformVertices != null && DeformVertices.verticesData != null && !IsDisplayingChildArmature())
            {
                bool isSkinned = DeformVertices.verticesData.weight != null;

                if (DeformVertices.verticesDirty || (isSkinned && DeformVertices.AreBonesDirty()))
                {
                    DeformVertices.verticesDirty = false;
                    EngineUpdateDeformMesh();
                }

                if (isSkinned)
                {
                    // Compatible.
                    return;
                }
            }
            
            if (TransformDirty)
            {
                UpdateGlobalTransformAndMatrix();
                EngineUpdateTransform();
            }
        }

        private void ProcessCacheFrameIndex(AnimationData animation, DBFrameCacher cacher, int frameIndex)
        {
            if (cacher.IsFrameCached(animation, frameIndex))
            {
                cacher.GetCacheFrame(animation, Name, frameIndex, GlobalTransformDBMatrix, global);
            }
            else
            {
                UpdateGlobalTransformAndMatrix();
                
                cacher.SetCacheFrame(animation, Name, frameIndex, GlobalTransformDBMatrix, global);
            }
        }
        #endregion

        #region Update Stuff
        private void UpdateDisplay()
        {
            TransformDirty = true;
            
            Visible.MarkAsDirty();
            BlendMode.MarkAsDirty();
            Color.MarkAsDirty();

            EngineUpdateDisplay();
        }

        private void UpdateGlobalTransformAndMatrix()
        {
            GlobalTransformDBMatrix.CopyFrom(LocalDBMatrix);
            GlobalTransformDBMatrix.Concat(ParentBone.GlobalTransformDBMatrix);

            _globalDirty = true;
            UpdateGlobalTransform();
        }
        private void UpdateLocalMatrix()
        {
            // Update local matrix. (Only updated when both display and transform are dirty.)
            if (origin != null)
            {
                global.CopyFrom(origin).Add(offset).ToMatrix(LocalDBMatrix);
            }
            else
            {
                global.CopyFrom(offset).ToMatrix(LocalDBMatrix);
            }
        }

        public void RefreshData()
        {
            DisplayData prevDisplayData = DisplayData;
            VerticesData prevVerticesData = DeformVertices != null ? DeformVertices.verticesData : null;
            TextureData prevTextureData = TextureData;

            DisplayData rawDisplayData = null;
            VerticesData currentVerticesData = null;
            
            BoundingBoxData = null;
            TextureData = null;
            
            if (HasVisibleDisplay)
            {
                switch (DisplayData)
                {
                    case ImageDisplayData imageDisplayData:
                        TextureData = imageDisplayData.texture;
                        break;
                    case MeshDisplayData meshDisplayData:
                        currentVerticesData = meshDisplayData.vertices;
                        TextureData = meshDisplayData.texture;
                        break;
                    case BoundingBoxDisplayData boundingBoxData:
                        BoundingBoxData = boundingBoxData.boundingBox;
                        break;
                    case PathDisplayData pathDisplayData:
                        currentVerticesData = pathDisplayData.vertices;
                        break;
                }
            }

            if (DisplayData != prevDisplayData || currentVerticesData != prevVerticesData || TextureData != prevTextureData)
            {
                // Update pivot offset.
                if (currentVerticesData == null && TextureData != null)//it means that cVD null and TD not null can be provided only if it is image display 
                {
                    ImageDisplayData imageDisplayData = DisplayData as ImageDisplayData;
                    float scale = TextureData.parent.scale * ParentArmature.ArmatureData.scale;
                    Rectangle frame = TextureData.frame;

                    PivotX = imageDisplayData.pivot.x;
                    PivotY = imageDisplayData.pivot.y;

                    Rectangle rect = frame != null ? frame : TextureData.region;
                    float width = rect.width;
                    float height = rect.height;

                    if (TextureData.rotated && frame == null)
                    {
                        width = rect.height;
                        height = rect.width;
                    }

                    PivotX *= width * scale;
                    PivotY *= height * scale;

                    if (frame != null)
                    {
                        PivotX += frame.x * scale;
                        PivotY += frame.y * scale;
                    }

                    // Update replace pivot. TODO
                    if (DisplayData != null && rawDisplayData != null && DisplayData != rawDisplayData)
                    {
                        rawDisplayData.DBTransform.ToMatrix(HelpDBMatrix);
                        HelpDBMatrix.Invert();
                        HelpDBMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        PivotX -= _helpPoint.x;
                        PivotY -= _helpPoint.y;

                        DisplayData.DBTransform.ToMatrix(HelpDBMatrix);
                        HelpDBMatrix.Invert();
                        HelpDBMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        PivotX += _helpPoint.x;
                        PivotY += _helpPoint.y;
                    }

                    if (!DBKernel.IsNegativeYDown)
                    {
                        PivotY = (TextureData.rotated ? TextureData.region.width : TextureData.region.height) * scale - PivotY;
                    }
                }
                else
                {
                    PivotX = 0.0f;
                    PivotY = 0.0f;
                }
                
                if (HasVisibleDisplay)
                {
                    // Compatible.
                    origin = DisplayData.DBTransform;
                }
                else
                {
                    origin = null;
                }

                // Update vertices.
                if (currentVerticesData != prevVerticesData)
                {
                    if (DeformVertices == null)
                    {
                        DeformVertices = BorrowObject<DeformVertices>();
                    }

                    DeformVertices.Init(currentVerticesData, ParentArmature);
                }
                else if (DeformVertices != null && TextureData != prevTextureData)
                {
                    // Update mesh after update frame.
                    DeformVertices.verticesDirty = true;
                }

                TransformDirty = true;
            }
        }
        #endregion
        #region Engine Implementation Region
        protected abstract void EngineUpdateDisplay();
        protected abstract void EngineUpdateZOrder();
        protected abstract void EngineUpdateDeformMesh();
        protected abstract void EngineUpdateVisibility();
        protected abstract void EngineUpdateColor();
        protected abstract void EngineUpdateBlendMode();
        protected abstract void EngineUpdateTransform();
        #endregion

        public void SetDrawOrder(int drawOrder)
        {
            DrawOrder.Set(drawOrder);
        }

        public void InvalidUpdate()
        {
            TransformDirty = true;
            Display.MarkAsDirty(); //TODO: again, why i should to that
        }

        public bool IsDisplayingChildArmature()
        {
            return DisplayData != null && DisplayData.Type == DisplayType.Armature;
        }

        public bool WasDisplayingChildArmature()
        {
            return Display.pV != null && Display.pV.Type == DisplayType.Armature;
        }

        public bool WasInvisible()
        {
            return Display.pV == null && Display.V != null;
        }
        
        public void ApplyParentBone(Bone bone)
        {
            ParentBone = bone;
        }
    }
}