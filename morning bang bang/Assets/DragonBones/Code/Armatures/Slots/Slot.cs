namespace DragonBones
{
    public abstract class Slot : TransformObject
    {
        public string displayController;
        public readonly SlotDisplaysManager Displays;

        public string Name => SlotData.name;
        
        public SlotData SlotData { get; internal set; }
        public Bone Parent { get; protected set; }
        public BoundingBoxData BoundingBoxData { get; protected set; }

        public readonly Dirty<DBColor> Color = new(new DBColor());
        public DeformVertices DeformVertices;
        
        public float PivotX;
        public float PivotY;
        
        protected readonly DBMatrix LocalDBMatrix = new DBMatrix();
        protected TextureData TextureData;

        public bool TransformDirty { get; set; }
        public Dirty<bool> Visible { get; private set; } = new();
        public Dirty<int> ZOrder { get; private set; } = new();
        public Dirty<BlendMode> BlendMode { get; protected set; } = new();
 
        public Slot()
        {
            Displays = new SlotDisplaysManager(this);
        }

        public void StartSlotBuilding(SlotData data, Armature armature)
        {
            SlotData = data;
            
            Armature = armature;
            Parent = Armature.Structure.GetBone(data.parent.name);
        }

        public void EndSlotBuilding()
        {
            ZOrder.Set(SlotData.zOrder);
            BlendMode.Set(SlotData.blendMode);
            Color.GetAndChange().CopyFrom(SlotData.DBColor);
            
            Armature.Structure.AddSlot(this);
            RefreshData();
        }
        
        #region Animation Loop 
        public void ProcessDirtyDisplay()
        {
            if (Displays.DisplayDirty)
            {
                Displays.DisplayDirty = false;
                RefreshData();
                UpdateDisplay();
                if (TransformDirty) UpdateLocalMatrix();
            }
        }

        public void UpdateCache(AnimationData animation, DBFrameCacher cacher, int frameIndex)
        {
            ProcessCacheFrameIndex(animation, cacher, frameIndex);
        }
        public virtual void ProcessDirtyData()
        {
            if (Parent._childrenTransformDirty) TransformDirty = true;
            
            if(Color.IsDirty) EngineUpdateColor();
            if(Visible.IsDirty) EngineUpdateVisibility();
            if(ZOrder.IsDirty) EngineUpdateZOrder();
            if(BlendMode.IsDirty) EngineUpdateBlendMode();
            
            if (this.DeformVertices != null && this.DeformVertices.verticesData != null && !IsDisplayingChildArmature())
            {
                var isSkinned = this.DeformVertices.verticesData.weight != null;

                if (this.DeformVertices.verticesDirty ||
                    (isSkinned && this.DeformVertices.AreBonesDirty()))
                {
                    this.DeformVertices.verticesDirty = false;
                    EngineUpdateMesh();
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

            if (!IsDisplayingChildArmature())
            {
                EngineUpdateFrame();
            }
            else
            {
                
            }
        }

        private void UpdateGlobalTransformAndMatrix()
        {
            GlobalTransformDBMatrix.CopyFrom(LocalDBMatrix);
            GlobalTransformDBMatrix.Concat(Parent.GlobalTransformDBMatrix);

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
            DisplayData prevDisplayData = Displays.CurrentDisplayData;
            VerticesData prevVerticesData = DeformVertices != null ? DeformVertices.verticesData : null;
            TextureData prevTextureData = TextureData;

            DisplayData rawDisplayData = null;
            VerticesData currentVerticesData = null;

            Displays.RefreshCurrentDisplayWithIndex();
            
            BoundingBoxData = null;
            TextureData = null;
            
            if (Displays.CurrentDisplayData != null)
            {
                switch (Displays.CurrentDisplayData)
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

            if (Displays.CurrentDisplayData != prevDisplayData || currentVerticesData != prevVerticesData || TextureData != prevTextureData)
            {
                // Update pivot offset.
                if (currentVerticesData == null && TextureData != null)//it means that cVD null and TD not null can be provided only if it is image display 
                {
                    ImageDisplayData imageDisplayData = Displays.CurrentDisplayData as ImageDisplayData;
                    float scale = TextureData.parent.scale * Armature.ArmatureData.scale;
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
                    if (Displays.CurrentDisplayData != null && rawDisplayData != null && Displays.CurrentDisplayData != rawDisplayData)
                    {
                        rawDisplayData.DBTransform.ToMatrix(HelpDBMatrix);
                        HelpDBMatrix.Invert();
                        HelpDBMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        PivotX -= _helpPoint.x;
                        PivotY -= _helpPoint.y;

                        Displays.CurrentDisplayData.DBTransform.ToMatrix(HelpDBMatrix);
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
                
                if (Displays.CurrentDisplayData != null)
                {
                    // Compatible.
                    origin = Displays.CurrentDisplayData.DBTransform;
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

                    DeformVertices.Init(currentVerticesData, Armature);
                }
                else if (DeformVertices != null && TextureData != prevTextureData)
                {
                    // Update mesh after update frame.
                    DeformVertices.verticesDirty = true;
                }

                Displays.DisplayDirty = true;
                TransformDirty = true;
            }
        }
        #endregion
        #region Engine Implementation Region
        protected abstract void EngineUpdateDisplay();
        protected abstract void EngineUpdateZOrder();
        protected abstract void EngineUpdateMesh();
        protected abstract void EngineUpdateVisibility();
        protected abstract void EngineUpdateColor();
        protected abstract void EngineUpdateBlendMode();
        protected abstract void EngineUpdateTransform();
        protected abstract void EngineUpdateFrame();
        #endregion

        public void SetZOrder(int zOrder)
        {
            ZOrder.Set(zOrder);
        }

        public void InvalidUpdate()
        {
            TransformDirty = true;
            Displays.DisplayDirty = true;
        }

        public bool IsDisplayingChildArmature()
        {
            return Displays.ChildArmatureSlotDisplay != null && Displays.CurrentDisplayData.type == DisplayType.Armature;
        }
    }
}