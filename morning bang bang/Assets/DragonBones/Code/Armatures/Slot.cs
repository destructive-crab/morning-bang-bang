using System;
using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The slot attached to the armature, controls the display status and properties of the display object.
    /// A bone can contain multiple slots.
    /// A slot can contain multiple display objects, displaying only one of the display objects at a time,
    /// but you can toggle the display object into frame animation while the animation is playing.
    /// The display object can be a normal texture, or it can be a display of a child armature, a grid display object,
    /// and a custom other display object.
    /// </summary>
    /// <see cref="DragonBones.Armature"/>
    /// <see cref="DragonBones.Bone"/>
    /// <see cref="DragonBones.SlotData"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public abstract class Slot : TransformObject
    {
        /// <summary>
        /// - Displays the animated state or mixed group name controlled by the object, set to null to be controlled by all animation states.
        /// </summary>
        /// <default>null</default>
        /// <see cref="DragonBones.AnimationState.displayControl"/>
        /// <see cref="DragonBones.AnimationState.name"/>
        /// <see cref="DragonBones.AnimationState.group"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public string displayController;
        
        protected bool _displayDirty;
        protected bool _zOrderDirty;
        protected bool _visibleDirty;
        protected bool _blendModeDirty;
        internal bool _colorDirty;
        internal bool _transformDirty;

        protected bool visible;
        internal BlendMode _blendMode;
        
        protected int displayIndex;
        protected int _animationDisplayIndex;

        protected int _cachedFrameIndex;

        internal int _zOrder;
        internal float _pivotX;
        internal float _pivotY;
        
        protected readonly Matrix _localMatrix = new Matrix();

        internal readonly ColorTransform _colorTransform = new ColorTransform();
        internal readonly List<DisplayData> _displayDatas = new List<DisplayData>();
        protected readonly List<object> displayList = new();

        protected List<DisplayData> allDisplaysData;

        protected DisplayData _displayData;
        protected TextureData _textureData;
        public DeformVertices _deformVertices;
        protected object display;
        protected Armature childArmature;

        internal List<int> _cachedFrameIndices = new List<int>();
        
        protected override void ClearObject()
        {
            base.ClearObject();

            var disposeDisplayList = new List<object>();
            for (int i = 0, l = displayList.Count; i < l; ++i)
            {
                var eachDisplay = displayList[i];
                if (eachDisplay != RawDisplay && eachDisplay != MeshDisplay && !disposeDisplayList.Contains(eachDisplay))
                {
                    disposeDisplayList.Add(eachDisplay);
                }
            }

            for (int i = 0, l = disposeDisplayList.Count; i < l; ++i)
            {
                var eachDisplay = disposeDisplayList[i];
                if (eachDisplay is Armature)
                {
                    (eachDisplay as Armature).Dispose();
                }
                else
                {
                    _DisposeDisplay(eachDisplay, true);
                }
            }

            if (_deformVertices != null)
            {
                _deformVertices.ReturnToPool();
            }

            if (MeshDisplay != null && MeshDisplay != RawDisplay)
            {
                // May be _meshDisplay and _rawDisplay is the same one.
                _DisposeDisplay(MeshDisplay, false);
            }

            if (RawDisplay != null)
            {
                _DisposeDisplay(RawDisplay, false);
            }

            displayController = null;

            _displayDirty = false;
            _zOrderDirty = false;
            _blendModeDirty = false;
            _colorDirty = false;
            _transformDirty = false;
            visible = true;
            _blendMode = BlendMode.Normal;
            displayIndex = -1;
            _animationDisplayIndex = -1;
            _zOrder = 0;
            _cachedFrameIndex = -1;
            _pivotX = 0.0f;
            _pivotY = 0.0f;
            _localMatrix.Identity();
            _colorTransform.Identity();
            displayList.Clear();
            _displayDatas.Clear();
            SlotData = null; //
            allDisplaysData = null; //
            _displayData = null;
            BoundingBoxData = null;
            _textureData = null;
            _deformVertices = null;
            RawDisplay = null;
            MeshDisplay = null;
            display = null;
            childArmature = null;
            Parent = null;
            _cachedFrameIndices = null;
        }

        protected abstract void _InitDisplay(object value, bool isRetain);
        protected abstract void _DisposeDisplay(object value, bool isRelease);
        protected abstract void _OnUpdateDisplay();
        protected abstract void _AddDisplay();
        protected abstract void _ReplaceDisplay(object value);
        protected abstract void _RemoveDisplay();
        protected abstract void _UpdateZOrder();
        internal abstract void _UpdateVisible();
        internal abstract void _UpdateBlendMode();
        protected abstract void _UpdateColor();
        protected abstract void _UpdateFrame();
        protected abstract void _UpdateMesh();
        protected abstract void _UpdateTransform();
        protected abstract void _IdentityTransform();

        /// <summary>
        /// - Support default skin data.
        /// </summary>
        /// <private/>
        protected DisplayData _GetDefaultRawDisplayData(int displayIndex)
        {
            var defaultSkin = Armature.ArmatureData.defaultSkin;
            if (defaultSkin != null)
            {
                var defaultRawDisplayDatas = defaultSkin.GetDisplays(SlotData.name);
                if (defaultRawDisplayDatas != null)
                {
                    return displayIndex < defaultRawDisplayDatas.Count ? defaultRawDisplayDatas[displayIndex] : null;
                }
            }

            return null;
        }

        /// <private/>
        protected void _UpdateDisplayData()
        {
            var prevDisplayData = _displayData;
            var prevVerticesData = _deformVertices != null ? _deformVertices.verticesData : null;
            var prevTextureData = _textureData;

            DisplayData rawDisplayData = null;
            VerticesData currentVerticesData = null;

            _displayData = null;
            BoundingBoxData = null;
            _textureData = null;

            if (displayIndex >= 0)
            {
                if (allDisplaysData != null)
                {
                    rawDisplayData = displayIndex < allDisplaysData.Count ? allDisplaysData[displayIndex] : null;
                }

                if (rawDisplayData == null)
                {
                    rawDisplayData = _GetDefaultRawDisplayData(displayIndex);
                }

                if (displayIndex < _displayDatas.Count)
                {
                    _displayData = _displayDatas[displayIndex];
                }
            }

            // Update texture and mesh data.
            if (_displayData != null)
            {
                if (_displayData.type == DisplayType.Mesh)
                {
                    currentVerticesData = (_displayData as MeshDisplayData).vertices;
                }
                else if (_displayData.type == DisplayType.Path)
                {
                    currentVerticesData = (_displayData as PathDisplayData).vertices;
                }
                else if (rawDisplayData != null)
                {
                    if (rawDisplayData.type == DisplayType.Mesh)
                    {
                        currentVerticesData = (rawDisplayData as MeshDisplayData).vertices;
                    }
                    else if (rawDisplayData.type == DisplayType.Path)
                    {
                        currentVerticesData = (rawDisplayData as PathDisplayData).vertices;
                    }
                }

                if (_displayData.type == DisplayType.BoundingBox)
                {
                    BoundingBoxData = (_displayData as BoundingBoxDisplayData).boundingBox;
                }
                else if (rawDisplayData != null)
                {
                    if (rawDisplayData.type == DisplayType.BoundingBox)
                    {
                        BoundingBoxData = (rawDisplayData as BoundingBoxDisplayData).boundingBox;
                    }
                }

                if (_displayData.type == DisplayType.Image)
                {
                    _textureData = (_displayData as ImageDisplayData).texture;
                }
                else if (_displayData.type == DisplayType.Mesh)
                {
                    _textureData = (_displayData as MeshDisplayData).texture;
                }
            }

            if (_displayData != prevDisplayData || currentVerticesData != prevVerticesData || _textureData != prevTextureData)
            {
                // Update pivot offset.
                if (currentVerticesData == null && _textureData != null)
                {
                    var imageDisplayData = _displayData as ImageDisplayData;
                    var scale = _textureData.parent.scale * Armature.ArmatureData.scale;
                    var frame = _textureData.frame;

                    _pivotX = imageDisplayData.pivot.x;
                    _pivotY = imageDisplayData.pivot.y;

                    var rect = frame != null ? frame : _textureData.region;
                    var width = rect.width;
                    var height = rect.height;

                    if (_textureData.rotated && frame == null)
                    {
                        width = rect.height;
                        height = rect.width;
                    }

                    _pivotX *= width * scale;
                    _pivotY *= height * scale;

                    if (frame != null)
                    {
                        _pivotX += frame.x * scale;
                        _pivotY += frame.y * scale;
                    }

                    // Update replace pivot. 
                    if (_displayData != null && rawDisplayData != null && _displayData != rawDisplayData)
                    {
                        rawDisplayData.DBTransform.ToMatrix(_helpMatrix);
                        _helpMatrix.Invert();
                        _helpMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        _pivotX -= _helpPoint.x;
                        _pivotY -= _helpPoint.y;

                        _displayData.DBTransform.ToMatrix(_helpMatrix);
                        _helpMatrix.Invert();
                        _helpMatrix.TransformPoint(0.0f, 0.0f, _helpPoint);
                        _pivotX += _helpPoint.x;
                        _pivotY += _helpPoint.y;
                    }

                    if (!DBKernel.IsNegativeYDown)
                    {
                        _pivotY = (_textureData.rotated ? _textureData.region.width : _textureData.region.height) * scale - _pivotY;
                    }
                }
                else
                {
                    _pivotX = 0.0f;
                    _pivotY = 0.0f;
                }

                // Update original transform.
                if (rawDisplayData != null)
                {
                    // Compatible.
                    origin = rawDisplayData.DBTransform;
                }
                else if (_displayData != null)
                {
                    // Compatible.
                    origin = _displayData.DBTransform;
                }
                else
                {
                    origin = null;
                }

                // Update vertices.
                if (currentVerticesData != prevVerticesData)
                {
                    if (_deformVertices == null)
                    {
                        _deformVertices = BorrowObject<DeformVertices>();
                    }

                    _deformVertices.init(currentVerticesData, Armature);
                }
                else if (_deformVertices != null && _textureData != prevTextureData)
                {
                    // Update mesh after update frame.
                    _deformVertices.verticesDirty = true;
                }

                _displayDirty = true;
                _transformDirty = true;
            }
        }

        protected void _UpdateDisplay()
        {
            var prevDisplay = display != null ? display : RawDisplay;
            var prevChildArmature = childArmature;

            // Update display and child armature.
            if (displayIndex >= 0 && displayIndex < displayList.Count)
            {
                display = displayList[displayIndex];
                if (display != null && display is Armature)
                {
                    childArmature = display as Armature;
                    display = childArmature.Display;
                }
                else
                {
                    childArmature = null;
                }
            }
            else
            {
                display = null;
                childArmature = null;
            }

            // Update display.
            var currentDisplay = display != null ? display : RawDisplay;
            if (currentDisplay != prevDisplay)
            {
                _OnUpdateDisplay();
                _ReplaceDisplay(prevDisplay);

                _transformDirty = true;
                _visibleDirty = true;
                _blendModeDirty = true;
                _colorDirty = true;
            }

            // Update frame.
            if (currentDisplay == RawDisplay || currentDisplay == MeshDisplay)
            {
                _UpdateFrame();
            }

            // Update child armature.
            if (childArmature != prevChildArmature)
            {
                if (prevChildArmature != null)
                {
                    // Update child armature parent.
                    prevChildArmature.Parent = null;
                    prevChildArmature.Clock = null;
                    if (prevChildArmature.inheritAnimation)
                    {
                        prevChildArmature.AnimationPlayer.Reset();
                    }
                }

                if (childArmature != null)
                {
                    // Update child armature parent.
                    childArmature.Parent = this;
                    childArmature.Clock = Armature.Clock;
                    if (childArmature.inheritAnimation)
                    {
                        // Set child armature cache frameRate.
                        if (childArmature.cacheFrameRate == 0)
                        {
                            var cacheFrameRate = Armature.cacheFrameRate;
                            if (cacheFrameRate != 0)
                            {
                                childArmature.cacheFrameRate = cacheFrameRate;
                            }
                        }

                        // Child armature action.
                        List<ActionData> actions = null;
                        if (_displayData != null && _displayData.type == DisplayType.Armature)
                        {
                            actions = (_displayData as ArmatureDisplayData).actions;
                        }
                        else if (displayIndex >= 0 && allDisplaysData != null)
                        {
                            var rawDisplayData = displayIndex < allDisplaysData.Count ? allDisplaysData[displayIndex] : null;

                            if (rawDisplayData == null)
                            {
                                rawDisplayData = _GetDefaultRawDisplayData(displayIndex);
                            }

                            if (rawDisplayData != null && rawDisplayData.type == DisplayType.Armature)
                            {
                                actions = (rawDisplayData as ArmatureDisplayData).actions;
                            }
                        }

                        if (actions != null && actions.Count > 0)
                        {
                            foreach (var action in actions)
                            {
                                var eventObject = BorrowObject<EventObject>();
                                EventObject.ActionDataToInstance(action, eventObject, Armature);
                                eventObject.slot = this;
                                Armature.BufferAction(eventObject, false);
                            }
                        }
                        else
                        {
                            childArmature.AnimationPlayer.Play();
                        }
                    }
                }
            }
        }

        /// <private/>
        protected void _UpdateGlobalTransformMatrix(bool isCache)
        {
            globalTransformMatrix.CopyFrom(_localMatrix);
            globalTransformMatrix.Concat(Parent.globalTransformMatrix);
            if (isCache)
            {
                global.FromMatrix(globalTransformMatrix);
            }
            else
            {
                _globalDirty = true;
            }
        }
        /// <internal/>
        /// <private/>
        internal bool _SetDisplayIndex(int value, bool isAnimation = false)
        {
            if (isAnimation)
            {
                if (_animationDisplayIndex == value)
                {
                    return false;
                }

                _animationDisplayIndex = value;
            }

            if (displayIndex == value)
            {
                return false;
            }

            displayIndex = value;
            _displayDirty = true;

            _UpdateDisplayData();

            return _displayDirty;
        }

        internal bool SetZOrder(int value)
        {
            if (_zOrder == value)
            {
                //return false;
            }

            _zOrder = value;
            _zOrderDirty = true;

            return _zOrderDirty;
        }

        /// <internal/>
        /// <private/>
        internal bool _SetColor(ColorTransform value)
        {
            _colorTransform.CopyFrom(value);
            _colorDirty = true;

            return _colorDirty;
        }
        /// <internal/>
        /// <private/>
        internal bool _SetDisplayList(List<object> value)
        {
            if (value != null && value.Count > 0)
            {
                if (displayList.Count != value.Count)
                {
                    displayList.ResizeList(value.Count);
                }

                for (int i = 0, l = value.Count; i < l; ++i)
                {
                    // Retain input render displays.
                    var eachDisplay = value[i];
                    if (eachDisplay != null &&
                        eachDisplay != RawDisplay &&
                        eachDisplay != MeshDisplay &&
                        !(eachDisplay is Armature) && displayList.IndexOf(eachDisplay) < 0)
                    {
                        _InitDisplay(eachDisplay, true);
                    }

                    displayList[i] = eachDisplay;
                }
            }
            else if (displayList.Count > 0)
            {
                displayList.Clear();
            }

            if (displayIndex >= 0 && displayIndex < displayList.Count)
            {
                _displayDirty = display != displayList[displayIndex];
            }
            else
            {
                _displayDirty = display != null;
            }

            _UpdateDisplayData();

            return _displayDirty;
        }

        /// <internal/>
        /// <private/>
        internal virtual void Init(SlotData slotData, Armature armatureValue, object rawDisplay, object meshDisplay)
        {
            if (this.SlotData != null)
            {
                return;
            }

            this.SlotData = slotData;
            //
            _visibleDirty = true;
            _blendModeDirty = true;
            _colorDirty = true;
            _blendMode = this.SlotData.blendMode;
            _zOrder = this.SlotData.zOrder;
            _colorTransform.CopyFrom(this.SlotData.color);
            this.RawDisplay = rawDisplay;
            this.MeshDisplay = meshDisplay;

            Armature = armatureValue;

            var slotParent = Armature.Structure.GetBone(this.SlotData.parent.name);
            if (slotParent != null)
            {
                Parent = slotParent;
            }
            else
            {
                // Never;
            }

            Armature.Structure.AddSlot(this);

            //
            _InitDisplay(this.RawDisplay, false);
            if (this.RawDisplay != this.MeshDisplay)
            {
                _InitDisplay(this.MeshDisplay, false);
            }

            _OnUpdateDisplay();
            _AddDisplay();

            //this.rawDisplayDatas = _displayDatas; 
        }

        internal void Update(int cacheFrameIndex)
        {
            if (_displayDirty)
            {
                _displayDirty = false;
                _UpdateDisplay();

                if (_transformDirty)
                {
                    // Update local matrix. (Only updated when both display and transform are dirty.)
                    if (origin != null)
                    {
                        global.CopyFrom(origin).Add(offset).ToMatrix(_localMatrix);
                    }
                    else
                    {
                        global.CopyFrom(offset).ToMatrix(_localMatrix);
                    }
                }
            }

            if (this._zOrderDirty)
            {
                this._zOrderDirty = false;
                this._UpdateZOrder();
            }

            if (cacheFrameIndex >= 0 && _cachedFrameIndices != null)
            {
                var cachedFrameIndex = _cachedFrameIndices[cacheFrameIndex];

                if (cachedFrameIndex >= 0 && _cachedFrameIndex == cachedFrameIndex)
                {
                    // Same cache.
                    _transformDirty = false;
                }
                else if (cachedFrameIndex >= 0)
                {
                    // Has been Cached.
                    _transformDirty = true;
                    _cachedFrameIndex = cachedFrameIndex;
                }
                else if (_transformDirty || Parent._childrenTransformDirty)
                {
                    // Dirty.
                    _transformDirty = true;
                    _cachedFrameIndex = -1;
                }
                else if (_cachedFrameIndex >= 0)
                {
                    // Same cache, but not set index yet.
                    _transformDirty = false;
                    _cachedFrameIndices[cacheFrameIndex] = _cachedFrameIndex;
                }
                else
                {
                    // Dirty.
                    _transformDirty = true;
                    _cachedFrameIndex = -1;
                }
            }
            else if (_transformDirty || Parent._childrenTransformDirty)
            {
                // Dirty.
                cacheFrameIndex = -1;
                _transformDirty = true;
                _cachedFrameIndex = -1;
            }

            if (display == null)
            {
                return;
            }

            if (_visibleDirty)
            {
                _visibleDirty = false;
                _UpdateVisible();
            }

            if (_blendModeDirty)
            {
                _blendModeDirty = false;
                _UpdateBlendMode();
            }

            if (_colorDirty)
            {
                _colorDirty = false;
                _UpdateColor();
            }

            if (_deformVertices != null && _deformVertices.verticesData != null && display == MeshDisplay)
            {
                var isSkinned = _deformVertices.verticesData.weight != null;

                if (_deformVertices.verticesDirty ||
                    (isSkinned && _deformVertices.isBonesUpdate()))
                {
                    _deformVertices.verticesDirty = false;
                    _UpdateMesh();
                }

                if (isSkinned)
                {
                    // Compatible.
                    return;
                }
            }

            if (_transformDirty)
            {
                _transformDirty = false;

                if (_cachedFrameIndex < 0)
                {
                    var isCache = cacheFrameIndex >= 0;
                    _UpdateGlobalTransformMatrix(isCache);

                    if (isCache && _cachedFrameIndices != null)
                    {
                        _cachedFrameIndex = _cachedFrameIndices[cacheFrameIndex] = Armature.ArmatureData.SetCacheFrame(globalTransformMatrix, global);
                    }
                }
                else
                {
                    Armature.ArmatureData.GetCacheFrame(globalTransformMatrix, global, _cachedFrameIndex);
                }

                _UpdateTransform();
            }
        }

        public void UpdateTransformAndMatrix()
        {
            if (_transformDirty)
            {
                _transformDirty = false;
                _UpdateGlobalTransformMatrix(false);
            }
        }

        internal void ReplaceDisplayData(DisplayData value, int displayIndex = -1)
        {
            if (displayIndex < 0)
            {
                if (this.displayIndex < 0)
                {
                    displayIndex = 0;
                }
                else
                {
                    displayIndex = this.displayIndex;
                }
            }

            if (_displayDatas.Count <= displayIndex)
            {
                _displayDatas.ResizeList(displayIndex + 1);

                for (int i = 0, l = _displayDatas.Count; i < l; ++i)
                {
                    // Clean undefined.
                    _displayDatas[i] = null;
                }
            }

            _displayDatas[displayIndex] = value;
        }

        /// <summary>
        /// - Check whether a specific point is inside a custom bounding box in the slot.
        /// The coordinate system of the point is the inner coordinate system of the armature.
        /// Custom bounding boxes need to be customized in Dragonbones Pro.
        /// </summary>
        /// <param name="x">- The horizontal coordinate of the point.</param>
        /// <param name="y">- The vertical coordinate of the point.</param>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public bool ContainsPoint(float x, float y)
        {
            if (BoundingBoxData == null)
            {
                return false;
            }

            UpdateTransformAndMatrix();

            _helpMatrix.CopyFrom(globalTransformMatrix);
            _helpMatrix.Invert();
            _helpMatrix.TransformPoint(x, y, _helpPoint);

            return BoundingBoxData.ContainsPoint(_helpPoint.x, _helpPoint.y);
        }

        /// <summary>
        /// - Check whether a specific segment intersects a custom bounding box for the slot.
        /// The coordinate system of the segment and intersection is the inner coordinate system of the armature.
        /// Custom bounding boxes need to be customized in Dragonbones Pro.
        /// </summary>
        /// <param name="xA">- The horizontal coordinate of the beginning of the segment.</param>
        /// <param name="yA">- The vertical coordinate of the beginning of the segment.</param>
        /// <param name="xB">- The horizontal coordinate of the end point of the segment.</param>
        /// <param name="yB">- The vertical coordinate of the end point of the segment.</param>
        /// <param name="intersectionPointA">- The first intersection at which a line segment intersects the bounding box from the beginning to the end. (If not set, the intersection point will not calculated)</param>
        /// <param name="intersectionPointB">- The first intersection at which a line segment intersects the bounding box from the end to the beginning. (If not set, the intersection point will not calculated)</param>
        /// <param name="normalRadians">- The normal radians of the tangent of the intersection boundary box. [x: Normal radian of the first intersection tangent, y: Normal radian of the second intersection tangent] (If not set, the normal will not calculated)</param>
        /// <returns>Intersection situation. [1: Disjoint and segments within the bounding box, 0: Disjoint, 1: Intersecting and having a nodal point and ending in the bounding box, 2: Intersecting and having a nodal point and starting at the bounding box, 3: Intersecting and having two intersections, N: Intersecting and having N intersections]</returns>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public int IntersectsSegment(float xA, float yA, float xB, float yB,
                                    Point intersectionPointA = null,
                                    Point intersectionPointB = null,
                                    Point normalRadians = null)
        {
            if (BoundingBoxData == null)
            {
                return 0;
            }

            UpdateTransformAndMatrix();
            _helpMatrix.CopyFrom(globalTransformMatrix);
            _helpMatrix.Invert();
            _helpMatrix.TransformPoint(xA, yA, _helpPoint);
            xA = _helpPoint.x;
            yA = _helpPoint.y;
            _helpMatrix.TransformPoint(xB, yB, _helpPoint);
            xB = _helpPoint.x;
            yB = _helpPoint.y;

            var intersectionCount = BoundingBoxData.IntersectsSegment(xA, yA, xB, yB, intersectionPointA, intersectionPointB, normalRadians);
            if (intersectionCount > 0)
            {
                if (intersectionCount == 1 || intersectionCount == 2)
                {
                    if (intersectionPointA != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointA.x, intersectionPointA.y, intersectionPointA);

                        if (intersectionPointB != null)
                        {
                            intersectionPointB.x = intersectionPointA.x;
                            intersectionPointB.y = intersectionPointA.y;
                        }
                    }
                    else if (intersectionPointB != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointB.x, intersectionPointB.y, intersectionPointB);
                    }
                }
                else
                {
                    if (intersectionPointA != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointA.x, intersectionPointA.y, intersectionPointA);
                    }

                    if (intersectionPointB != null)
                    {
                        globalTransformMatrix.TransformPoint(intersectionPointB.x, intersectionPointB.y, intersectionPointB);
                    }
                }

                if (normalRadians != null)
                {
                    globalTransformMatrix.TransformPoint((float)Math.Cos(normalRadians.x), (float)Math.Sin(normalRadians.x), _helpPoint, true);
                    normalRadians.x = (float)Math.Atan2(_helpPoint.y, _helpPoint.x);

                    globalTransformMatrix.TransformPoint((float)Math.Cos(normalRadians.y), (float)Math.Sin(normalRadians.y), _helpPoint, true);
                    normalRadians.y = (float)Math.Atan2(_helpPoint.y, _helpPoint.x);
                }
            }

            return intersectionCount;
        }

        /// <summary>
        /// - Forces the slot to update the state of the display object in the next frame.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public void InvalidUpdate()
        {
            _displayDirty = true;
            _transformDirty = true;
        }
        
        /// <summary>
        /// - The visible of slot's display object.
        /// </summary>
        /// <default>true</default>
        /// <version>DragonBones 5.6</version>
        /// <language>en_US</language>
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible == value)
                {
                    return;
                }

                visible = value;
                _UpdateVisible();
            }
        }
        
        /// <summary>
        /// - The index of the display object displayed in the display list.
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let slot = armature.getSlot("weapon");
        ///     slot.displayIndex = 3;
        ///     slot.displayController = "none";
        /// </pre>
        /// </example>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public int DisplayIndex
        {
            get => displayIndex;
            set
            {
                if (_SetDisplayIndex(value))
                {
                    Update(-1);
                }
            }
        }

        /// <summary>
        /// - The slot name.
        /// </summary>
        /// <see cref="DBKernel.SlotData.name"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name
        {
            get { return SlotData.name; }
        }

        /// <summary>
        /// - Contains a display list of display objects or child armatures.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public List<object> DisplayList
        {
            get => new(displayList.ToArray());
            set
            {
                var backupDisplayList = displayList.ToArray(); // Copy.
                var disposeDisplayList = new List<object>();

                if (_SetDisplayList(value))
                {
                    Update(-1);
                }

                // Release replaced displays.
                foreach (var eachDisplay in backupDisplayList)
                {
                    if (eachDisplay != null &&
                        eachDisplay != RawDisplay &&
                        eachDisplay != MeshDisplay &&
                        displayList.IndexOf(eachDisplay) < 0 &&
                        disposeDisplayList.IndexOf(eachDisplay) < 0)
                    {
                        disposeDisplayList.Add(eachDisplay);
                    }
                }

                foreach (var eachDisplay in disposeDisplayList)
                {
                    if (eachDisplay is Armature)
                    {
                        // (eachDisplay as Armature).Dispose();
                    }
                    else
                    {
                        _DisposeDisplay(eachDisplay, true);
                    }
                }
            }
        }
        
        /// <summary>
        /// - The slot data.
        /// </summary>
        /// <see cref="DragonBones.SlotData"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public SlotData SlotData { get; internal set; }

        public List<DisplayData> AllDisplaysData
        {
            get => allDisplaysData;
            set
            {
                if (allDisplaysData == value)
                {
                    return;
                }

                _displayDirty = true;
                allDisplaysData = value;

                if (allDisplaysData != null)
                {
                    _displayDatas.ResizeList(allDisplaysData.Count);
                    for (int i = 0, l = _displayDatas.Count; i < l; ++i)
                    {
                        var rawDisplayData = allDisplaysData[i];

                        if (rawDisplayData == null)
                        {
                            rawDisplayData = _GetDefaultRawDisplayData(i);
                        }

                        _displayDatas[i] = rawDisplayData;
                    }
                }
                else
                {
                    _displayDatas.Clear();
                }
            }
        }
        
        /// <summary>
        /// - The custom bounding box data for the slot at current time.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public BoundingBoxData BoundingBoxData { get; protected set; }

        public object RawDisplay { get; protected set; }
        public object MeshDisplay { get; protected set; }

        /// <summary>
        /// - The display object that the slot displays at this time.
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let slot = armature.getSlot("text");
        ///     slot.display = new yourEngine.TextField();
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public object Display
        {
            get => display;
            set
            {
                if (display == value)
                {
                    return;
                }

                var displayListLength = displayList.Count;
                if (displayIndex < 0 && displayListLength == 0)
                {
                    // Emprty.
                    displayIndex = 0;
                }

                if (displayIndex < 0)
                {
                    return;
                }
                else
                {
                    var replaceDisplayList = DisplayList; // Copy.
                    if (displayListLength <= displayIndex)
                    {
                        replaceDisplayList.ResizeList(displayIndex + 1);
                    }

                    replaceDisplayList[displayIndex] = value;
                    DisplayList = replaceDisplayList;
                }
            }
        }
        /// <summary>
        /// - The child armature that the slot displayed at current time.
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let slot = armature.getSlot("weapon");
        ///     let prevChildArmature = slot.childArmature;
        ///     if (prevChildArmature) {
        ///         prevChildArmature.dispose();
        ///     }
        ///     slot.childArmature = factory.buildArmature("weapon_2", "weapon_2_project");
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Armature ChildArmature
        {
            get { return childArmature; }
            set
            {
                if (childArmature == value)
                {
                    return;
                }

                childArmature = value;
                Display = value.Display;
            }
        }

        /// <summary>
        /// - The parent bone to which it belongs.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Bone Parent { get; protected set; }
    }
}
