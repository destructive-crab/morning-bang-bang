namespace DragonBones
{
    /// <summary>
    /// - The base class of the transform object.
    /// </summary>
    /// <see cref="DBKernel.Transform"/>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    public abstract class TransformObject : BaseObject
    {
        /// <private/>
        protected static readonly Matrix _helpMatrix  = new Matrix();
        /// <private/>
        protected static readonly DBTransform HelpDBTransform  = new DBTransform();
        /// <private/>
        protected static readonly Point _helpPoint = new Point();
        /// <summary>
        /// - A matrix relative to the armature coordinate system.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public readonly Matrix globalTransformMatrix = new Matrix();
        /// <summary>
        /// - A transform relative to the armature coordinate system.
        /// </summary>
        /// <see cref="UpdateGlobalTransform()"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public readonly DBTransform global = new DBTransform();
        /// <summary>
        /// - The offset transform relative to the armature or the parent bone coordinate system.
        /// </summary>
        /// <see cref="dragonBones.Bone.InvalidUpdate()"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public readonly DBTransform offset = new DBTransform();
        /// <private/>
        public DBTransform origin;
        /// <private/>
        public object userData;
        /// <private/>
        protected bool _globalDirty;
        /// <internal/>
        /// <private/>
        internal Armature _armature;
        /// <private/>
        protected override void _OnClear()
        {
            this.globalTransformMatrix.Identity();
            this.global.Identity();
            this.offset.Identity();
            this.origin = null; //
            this.userData = null;

            this._globalDirty = false;
            this._armature = null; //
        }
        /// <summary>
        /// - For performance considerations, rotation or scale in the {@link #global} attribute of the bone or slot is not always properly accessible,
        /// some engines do not rely on these attributes to update rendering, such as Egret.
        /// The use of this method ensures that the access to the {@link #global} property is correctly rotation or scale.
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     bone.updateGlobalTransform();
        ///     let rotation = bone.global.rotation;
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void UpdateGlobalTransform()
        {
            if (this._globalDirty)
            {
                this._globalDirty = false;
                this.global.FromMatrix(this.globalTransformMatrix);
            }
        }
        /// <summary>
        /// - The armature to which it belongs.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Armature armature
        {
            get{ return this._armature; }
        }
    }
}
