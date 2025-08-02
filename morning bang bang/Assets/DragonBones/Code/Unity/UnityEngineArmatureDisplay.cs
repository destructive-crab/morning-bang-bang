using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{

    public enum SortingMode
    {
        /// <summary>
        /// Sort by Z values
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 按照插槽显示对象的z值排序
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        SortByZ,
        /// <summary>
        /// Renderer's order within a sorting layer.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// 在同一层sorting layer中插槽按照sortingOrder排序
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        SortByOrder
    } 
    [ExecuteInEditMode, DisallowMultipleComponent]
    [RequireComponent(typeof(SortingGroup))]
    public class UnityEngineArmatureDisplay : UnityEventDispatcher, IEngineArmatureDisplay
    {
        public SortingMode sortingMode
        {
            get { return _sortingMode; }
            set
            {
                if (_sortingMode == value)
                {
                    return;
                }

#if UNITY_5_6_OR_NEWER
                var isWarning = false;
#else
                var isWarning = value == SortingMode.SortByOrder;
#endif


                _sortingMode = value;

                //
#if UNITY_5_6_OR_NEWER
                if (_sortingMode == SortingMode.SortByOrder)
                {
                    _sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
                    if (_sortingGroup == null)
                    {
                        _sortingGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                    }
                }
                else
                {
                    _sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();

                    if (_sortingGroup != null)
                    {
                        DestroyImmediate(_sortingGroup);
                    }
                }
#endif

                _UpdateSlotsSorting();
            }
        } 
        [SerializeField]
        internal SortingMode _sortingMode = SortingMode.SortByZ;
        [SerializeField]
        internal string _sortingLayerName = "Default";
        [SerializeField]
        internal int _sortingOrder = 0;
        public int sortingOrder
        {
            get { return _sortingOrder; }
            set
            {
                if (_sortingOrder == value)
                {
                    //return;
                }

                _sortingOrder = value;

                _UpdateSlotsSorting();
            }
        } 
        [SerializeField]
        
        internal UnityEngine.Rendering.SortingGroup _sortingGroup;
        
        private void _UpdateSlotsSorting()
        {
            if (Armature == null)
            {
                return;
            }

            if (!isUGUI)
            {
#if UNITY_5_6_OR_NEWER
                if (_sortingGroup)
                {
                    _sortingMode = SortingMode.SortByOrder;
                    _sortingGroup.sortingLayerName = _sortingLayerName;
                    _sortingGroup.sortingOrder = _sortingOrder;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(_sortingGroup);
                    }
#endif
                }
#endif
            }

            //
            foreach (UnitySlot slot in Armature.GetSlots())
            {
                var display = slot._renderDisplay;
                if (display == null)
                {
                    continue;
                }

                slot.SetZOrder(new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (_zSpace + 0.001f)));

                if (slot.ChildArmature != null)
                {
                    (slot.ChildArmature.Display as UnityEngineArmatureDisplay)._UpdateSlotsSorting();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && slot.meshRenderer != null)
                {
                    EditorUtility.SetDirty(slot.meshRenderer);
                }
#endif
            }
        }

        private bool _IsPrefab()
        {
            return PrefabUtility.GetPrefabParent(gameObject) == null
                   && PrefabUtility.GetPrefabObject(gameObject) != null;
        } 
        
        
        
        public const int ORDER_SPACE = 10;
        public UnityDragonBonesData unityData = null;

        public string armatureName = null;

        public bool isUGUI = false;

        internal readonly ColorTransform _colorTransform = new ColorTransform();

        public string animationName = null;

        private bool _disposeDisplay = true;

        [Tooltip("0 : Loop")] [Range(0, 100)] [SerializeField]
        protected int _playTimes = 0;

        [Range(-2f, 2f)] [SerializeField] protected float _timeScale = 1.0f;

        [SerializeField] protected bool _flipX = false;

        [SerializeField] protected bool _flipY = false;
        internal float _zSpace = 0.0f;
        
        public float zSpace
        {
            get { return _zSpace; }
            set
            {
                if (value < 0.0f || float.IsNaN(value))
                {
                    value = 0.0f;
                }

                if (_zSpace == value)
                {
                    return;
                }

                _zSpace = value;

                _UpdateSlotsSorting();
            }
        }
        //default open combineMeshs
        [SerializeField] protected bool _closeCombineMeshs;

        private bool _hasSortingGroup = false;

        public Armature Armature { get; internal set; } = null;
        public AnimationPlayer AnimationPlayer => Armature != null ? Armature.AnimationPlayer : null;
        public void ApplyPPU(uint armatureDataPixelsPerUnit)
        {
            transform.localScale = new Vector3(100f / armatureDataPixelsPerUnit, 100f / armatureDataPixelsPerUnit, 1);
        }

        public void DBClear()
        {
            if (this.Armature != null)
            {
                this.Armature = null;
                if (this._disposeDisplay)
                {
                    //DBUnityFactory.UnityFactoryHelper.DestroyUnityObject(gameObject);
                }
            }

            this.unityData = null;
            this.armatureName = null;
            this.animationName = null;
            this.isUGUI = false;

            this._disposeDisplay = true;
            this.Armature = null;
            this._colorTransform.Identity();
            this._playTimes = 0;
            this._timeScale = 1.0f;
            this._zSpace = 0.0f;
            this._flipX = false;
            this._flipY = false;

            this._hasSortingGroup = false;

            this._closeCombineMeshs = false;
        }

        ///
        public void DBInit(Armature armature)
        {
            Armature = armature;
        }

        public void DBUpdate()
        {

        }

        public void Dispose(bool disposeDisplay = true)
        {
            _disposeDisplay = disposeDisplay;

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
                var dragonBonesData = DBInitial.UnityDataLoader.LoadData(unityData, isUGUI);

                if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
                {
                    DBInitial.UnityFactory.BuildArmatureComponent(armatureName, unityData.dataName, this,null, null,
                         isUGUI);
                }
            }

            if (Armature != null)
            {
                Armature.flipX = _flipX;
                Armature.flipY = _flipY;

                Armature.AnimationPlayer.timeScale = _timeScale;

                if (!string.IsNullOrEmpty(animationName))
                {
                    Armature.AnimationPlayer.Play(animationName, _playTimes);
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

            var hasSortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>() != null;
            if (hasSortingGroup != _hasSortingGroup)
            {
                _hasSortingGroup = hasSortingGroup;

            }
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
                    DBInitial.Kernel.AdvanceTime(0.0f);
                }
            }

            _disposeDisplay = true;
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
                if (slot.ChildArmature != null)
                {
                    (slot.ChildArmature.Display as UnityEngineArmatureDisplay).OpenCombineMeshs();
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
                if (slot.ChildArmature != null)
                {
                    (slot.ChildArmature.Display as UnityEngineArmatureDisplay).CloseCombineMeshs();
                }
            }
        }
    }
}