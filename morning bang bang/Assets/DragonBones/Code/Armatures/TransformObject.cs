namespace DragonBones
{
    /// <summary>
    /// - The base class of the transform object.
    /// </summary>
    /// <see cref="DBKernel.Transform"/>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    public abstract class TransformObject : DBObject
    {
        protected static readonly DBMatrix HelpDBMatrix  = new DBMatrix();
        protected static readonly DBTransform HelpDBTransform  = new DBTransform();
        protected static readonly Point _helpPoint = new Point();
        
        /// <summary>
        /// - A matrix relative to the armature coordinate system.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public readonly DBMatrix GlobalTransformDBMatrix = new DBMatrix();
        
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
        /// <see cref="DragonBones.Bone.InvalidUpdate()"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public readonly DBTransform offset = new DBTransform();
        
        public DBTransform origin;
        public object userData;
        protected bool _globalDirty;

        public override void OnReleased()
        {
            this.GlobalTransformDBMatrix.Identity();
            this.global.Identity();
            this.offset.Identity();
            this.origin = null; 
            this.userData = null;

            this._globalDirty = false;
            this.ParentArmature = null; 
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
                this.global.FromMatrix(this.GlobalTransformDBMatrix);
            }
        }
        
        /// <summary>
        /// - The armature to which it belongs.
        /// </summary>
        public Armature ParentArmature { get; internal set; }
    }
}
