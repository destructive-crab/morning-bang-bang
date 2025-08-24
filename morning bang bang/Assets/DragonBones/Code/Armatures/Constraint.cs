using System;

namespace DragonBones
{
    /// <internal/>
    /// <private/>
    public abstract class Constraint : DBObject, IRegistryEntry
    {
        protected static readonly DBMatrix HelpDBMatrix = new DBMatrix();
        protected static readonly DBTransform HelpDBTransform = new DBTransform();
        protected static readonly Point _helpPoint = new Point();

        /// <summary>
        /// - For timeline state.
        /// </summary>
        /// <internal/>
        internal ConstraintData _constraintData;
        protected Armature _armature;

        /// <summary>
        /// - For sort bones.
        /// </summary>
        /// <internal/>
        internal Bone _target;
        /// <summary>
        /// - For sort bones.
        /// </summary>
        /// <internal/>
        internal Bone _root;
        internal Bone _bone;

        public override void OnReleased()
        {
            _armature = null;
            _target = null; //
            _root = null; //
            _bone = null; //
        }

        public abstract void Init(ConstraintData constraintData, Armature armature, Bone target, Bone root, Bone bone);
        public abstract void Update();
        public abstract void InvalidUpdate();

        public string name
        {
            get { return _constraintData.name; }
        }

        public DBRegistry.DBID ID { get; private set; }

        public void SetID(DBRegistry.DBID id)
        {
            ID = id;
        }
    }
    /// <internal/>
    /// <private/>
    public class IKConstraint : Constraint
    {
        internal bool _scaleEnabled; // TODO
        /// <summary>
        /// - For timeline state.
        /// </summary>
        /// <internal/>
        internal bool _bendPositive;
        /// <summary>
        /// - For timeline state.
        /// </summary>
        /// <internal/>
        internal float _weight;

        public override void OnReleased()
        {
            base.OnReleased();

            _scaleEnabled = false;
            _bendPositive = false;
            _weight = 1.0f;
            _constraintData = null;
        }

        private void _ComputeA()
        {
            var ikGlobal = _target.global;
            var global = _root.global;
            var globalTransformMatrix = _root.GlobalTransformDBMatrix;

            var radian = (float)Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x);
            if (global.scaleX < 0.0f)
            {
                radian += (float)Math.PI;
            }

            global.rotation += DBTransform.NormalizeRadian(radian - global.rotation) * _weight;
            global.ToMatrix(globalTransformMatrix);
        }

        private void _ComputeB()
        {
            var boneLength = _bone.boneData.length;
            var parent = _root as Bone;
            var ikGlobal = _target.global;
            var parentGlobal = parent.global;
            var global = _bone.global;
            var globalTransformMatrix = _bone.GlobalTransformDBMatrix;

            var x = globalTransformMatrix.a * boneLength;
            var y = globalTransformMatrix.b * boneLength;

            var lLL = x * x + y * y;
            var lL = (float)Math.Sqrt(lLL);

            var dX = global.x - parentGlobal.x;
            var dY = global.y - parentGlobal.y;
            var lPP = dX * dX + dY * dY;
            var lP = (float)Math.Sqrt(lPP);
            var rawRadian = global.rotation;
            var rawParentRadian = parentGlobal.rotation;
            var rawRadianA = (float)Math.Atan2(dY, dX);

            dX = ikGlobal.x - parentGlobal.x;
            dY = ikGlobal.y - parentGlobal.y;
            var lTT = dX * dX + dY * dY;
            var lT = (float)Math.Sqrt(lTT);

            var radianA = 0.0f;
            if (lL + lP <= lT || lT + lL <= lP || lT + lP <= lL)
            {
                radianA = (float)Math.Atan2(ikGlobal.y - parentGlobal.y, ikGlobal.x - parentGlobal.x);
                if (lL + lP <= lT)
                {
                }
                else if (lP < lL)
                {
                    radianA += (float)Math.PI;
                }
            }
            else
            {
                var h = (lPP - lLL + lTT) / (2.0f * lTT);
                var r = (float)Math.Sqrt(lPP - h * h * lTT) / lT;
                var hX = parentGlobal.x + (dX * h);
                var hY = parentGlobal.y + (dY * h);
                var rX = -dY * r;
                var rY = dX * r;

                var isPPR = false;
                var parentParent = parent.parent;
                if (parentParent != null)
                {
                    var parentParentMatrix = parentParent.GlobalTransformDBMatrix;
                    isPPR = parentParentMatrix.a * parentParentMatrix.d - parentParentMatrix.b * parentParentMatrix.c < 0.0f;
                }

                if (isPPR != _bendPositive)
                {
                    global.x = hX - rX;
                    global.y = hY - rY;
                }
                else
                {
                    global.x = hX + rX;
                    global.y = hY + rY;
                }

                radianA = (float)Math.Atan2(global.y - parentGlobal.y, global.x - parentGlobal.x);
            }

            var dR = DBTransform.NormalizeRadian(radianA - rawRadianA);
            parentGlobal.rotation = rawParentRadian + dR * _weight;
            parentGlobal.ToMatrix(parent.GlobalTransformDBMatrix);
            //
            var currentRadianA = rawRadianA + dR * _weight;
            global.x = parentGlobal.x + (float)Math.Cos(currentRadianA) * lP;
            global.y = parentGlobal.y + (float)Math.Sin(currentRadianA) * lP;
            //
            var radianB = (float)Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x);
            if (global.scaleX < 0.0f)
            {
                radianB += (float)Math.PI;
            }

            global.rotation = parentGlobal.rotation + rawRadian - rawParentRadian + DBTransform.NormalizeRadian(radianB - dR - rawRadian) * _weight;
            global.ToMatrix(globalTransformMatrix);
        }

        public override void Init(ConstraintData constraintData, Armature armature, Bone target, Bone root, Bone bone)
        {
            if (_constraintData != null) { return; }

            _constraintData = constraintData;
            _armature = armature;
            
            _target = target;
            _root = root;
            _bone = bone;

            {
                var ikConstraintData = _constraintData as IKConstraintData;
                //
                _scaleEnabled = ikConstraintData.scaleEnabled;
                _bendPositive = ikConstraintData.bendPositive;
                _weight = ikConstraintData.weight;
            }

            _root._hasConstraint = true;
        }

        public override void Update()
        {
            _root.UpdateByConstraint();

            if (_bone != null)
            {
                _bone.UpdateByConstraint();
                _ComputeB();
            }
            else
            {
                _ComputeA();
            }
        }

        public override void InvalidUpdate()
        {
            _root.InvalidUpdate();

            if (_bone != null)
            {
                _bone.InvalidUpdate();
            }
        }
    }
}
