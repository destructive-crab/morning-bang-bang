using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
    ///<inheritDoc/>
    [ExecuteInEditMode, DisallowMultipleComponent]
    [RequireComponent(typeof(SortingGroup))]
    public class UnityArmatureInstance : DragonBoneEventDispatcher, IArmatureProxy
    {
        public UnityDragonBonesData unityData = null;

        public string armatureName = null;

        public bool isUGUI = false;

        internal readonly ColorTransform _colorTransform = new ColorTransform();

        public string animationName = null;

        private bool _disposeProxy = true;

        [Tooltip("0 : Loop")] [Range(0, 100)] [SerializeField]
        protected int _playTimes = 0;

        [Range(-2f, 2f)] [SerializeField] protected float _timeScale = 1.0f;

        [SerializeField] protected bool _flipX = false;

        [SerializeField] protected bool _flipY = false;

        //default open combineMeshs
        [SerializeField] protected bool _closeCombineMeshs;

        private bool _hasSortingGroup = false;


        public Armature Armature { get; internal set; } = null;
        public Animation Animation => Armature != null ? Armature.animation : null;
        public void DBClear()
        {
            if (this.Armature != null)
            {
                this.Armature = null;
                if (this._disposeProxy)
                {
                    UnityFactoryHelper.DestroyUnityObject(gameObject);
                }
            }

            this.unityData = null;
            this.armatureName = null;
            this.animationName = null;
            this.isUGUI = false;

            this._disposeProxy = true;
            this.Armature = null;
            this._colorTransform.Identity();
            this._playTimes = 0;
            this._timeScale = 1.0f;
            this._flipX = false;
            this._flipY = false;

            this._hasSortingGroup = false;

            this._closeCombineMeshs = false;
        }

        ///
        public void DBInit(Armature armature)
        {
            this.Armature = armature;
        }

        public void DBUpdate()
        {

        }

        public void Dispose(bool disposeProxy = true)
        {
            _disposeProxy = disposeProxy;

            if (Armature != null)
            {
                Armature.Dispose();
            }
        }


        public ColorTransform color
        {
            get { return this._colorTransform; }
            set
            {
                this._colorTransform.CopyFrom(value);

                foreach (var slot in this.Armature.GetSlots())
                {
                    slot._colorDirty = true;
                }
            }
        }

#if UNITY_EDITOR
        private bool IsPrefab()
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null
                   && PrefabUtility.GetPrefabInstanceHandle(gameObject) != null;
        }
#endif

        /// <private/>
        void Awake()
        {
#if UNITY_EDITOR
            if (IsPrefab())
            {
                return;
            }
#endif
            if (IsDataSetupCorrectly())
            {
                var dragonBonesData = DBUnityFactory.factory.LoadData(unityData, isUGUI);

                if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
                {
                    DBUnityFactory.factory.BuildArmatureComponent(armatureName, unityData.dataName, null, null,
                        gameObject, isUGUI);
                }
            }

            if (Armature != null)
            {
                Armature.flipX = _flipX;
                Armature.flipY = _flipY;

                Armature.animation.timeScale = _timeScale;

                if (!string.IsNullOrEmpty(animationName))
                {
                    Armature.animation.Play(animationName, _playTimes);
                }
            }
        }

        private bool IsDataSetupCorrectly()
        {
            return unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null;
        }

        void Start()
        {
            // this._closeCombineMeshs = true;
            //默认开启合并
            if (this._closeCombineMeshs)
            {
                this.CloseCombineMeshs();
            }
            else
            {
                this.OpenCombineMeshs();
            }
        }

        void LateUpdate()
        {
            if (Armature == null)
            {
                return;
            }

            _flipX = Armature.flipX;
            _flipY = Armature.flipY;

#if UNITY_5_6_OR_NEWER
            var hasSortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>() != null;
            if (hasSortingGroup != _hasSortingGroup)
            {
                _hasSortingGroup = hasSortingGroup;

            }
#endif
        }

        /// <private/>
        void OnDestroy()
        {
            if (Armature != null)
            {
                var armature = this.Armature;
                this.Armature = null;

                armature.Dispose();

                if (!Application.isPlaying)
                {
                    DBUnityFactory.factory._dragonBones.AdvanceTime(0.0f);
                }
            }

            _disposeProxy = true;
            Armature = null;
        }

        private void OpenCombineMeshs()
        {
            if (this.isUGUI)
            {
                return;
            }

            //
            var cm = gameObject.GetComponent<UnityCombineMeshes>();
            if (cm == null)
            {
                cm = gameObject.AddComponent<UnityCombineMeshes>();
            }
            //

            if (this.Armature == null)
            {
                return;
            }

            var slots = this.Armature.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.childArmature != null)
                {
                    (slot.childArmature.proxy as UnityArmatureInstance).OpenCombineMeshs();
                }
            }
        }

        public void CloseCombineMeshs()
        {
            this._closeCombineMeshs = true;
            //
            var cm = gameObject.GetComponent<UnityCombineMeshes>();
            if (cm != null)
            {
                DestroyImmediate(cm);
            }

            if (this.Armature == null)
            {
                return;
            }

            //
            var slots = this.Armature.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.childArmature != null)
                {
                    (slot.childArmature.proxy as UnityArmatureInstance).CloseCombineMeshs();
                }
            }
        }
    }
}