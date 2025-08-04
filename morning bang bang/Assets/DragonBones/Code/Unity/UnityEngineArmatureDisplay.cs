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
        SortByZ,
        /// <summary>
        /// Renderer's order within a sorting layer.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
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
                    _sortingGroup = GetComponent<SortingGroup>();
                    if (_sortingGroup == null)
                    {
                        _sortingGroup = gameObject.AddComponent<SortingGroup>();
                    }
                }
                else
                {
                    _sortingGroup = GetComponent<SortingGroup>();

                    if (_sortingGroup != null)
                    {
                        DestroyImmediate(_sortingGroup);
                    }
                }
#endif

                UpdateSlotsSorting();
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

                UpdateSlotsSorting();
            }
        } 
        [SerializeField]
        
        internal SortingGroup _sortingGroup;
        
        private void UpdateSlotsSorting()
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
            foreach (UnitySlot slot in Armature.Structure.Slots)
            {
                var display = slot.RenderDisplay;
                if (display == null)
                {
                    continue;
                }

                slot.UpdateZPosition(new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (_zSpace + 0.001f)));

                if (slot.ChildArmature != null)
                {
                    (slot.ChildArmature.Display as UnityEngineArmatureDisplay).UpdateSlotsSorting();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && slot.meshRenderer != null)
                {
                    EditorUtility.SetDirty(slot.meshRenderer);
                }
#endif
            }
        }
        
        public const int ORDER_SPACE = 10;
        public UnityDragonBonesData unityData = null;

        public string armatureName = null;

        public bool isUGUI = false;

        internal readonly ColorTransform _colorTransform = new ColorTransform();

        public string animationName = null;

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

                UpdateSlotsSorting();
            }
        }
        //default open combineMeshs
        [SerializeField] protected bool _closeCombineMeshs;

        private bool _hasSortingGroup = false;

        public Armature Armature { get; internal set; } = null;
        public AnimationPlayer AnimationPlayer => Armature != null ? Armature.AnimationPlayer : null;

        ///
        public void DBInit(Armature armature)
        {
            Armature = armature;
        }

        public void DBClear(bool disposeDisplay = false)
        {
            if (disposeDisplay)
            {
                DBUnityFactory.Helper.DestroyUnityObject(gameObject);
            }
            
            unityData = null;
            armatureName = null;
            animationName = null;
            isUGUI = false;

            Armature = null;
            _colorTransform.Identity();
            _playTimes = 0;
            _timeScale = 1.0f;
            _zSpace = 0.0f;
            _flipX = false;
            _flipY = false;

            _hasSortingGroup = false;

            _closeCombineMeshs = false;
        }

        public void DBUpdate()
        {

        }

        public void Dispose(bool disposeDisplay = true)
        {
            if (Armature != null)
            {
                Armature.Dispose();
            }
        }

        public ColorTransform color
        {
            get { return _colorTransform; }
            set
            {
                _colorTransform.CopyFrom(value);

                foreach (var slot in Armature.Structure.Slots)
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
                    DBInitial.UnityFactory.CreateNewArmature(armatureName, unityData.dataName, this,null, null);
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
            if (_closeCombineMeshs)
            {
                CloseCombineMeshs();
            }
            else
            {
                OpenCombineMeshs();
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

            var hasSortingGroup = GetComponent<SortingGroup>() != null;
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
                var armature = Armature;
                Armature = null;

                armature.Dispose();

                if (!Application.isPlaying)
                {
                    DBInitial.Kernel.AdvanceTime(0.0f);
                }
            }

            Armature = null;
        }

        private void OpenCombineMeshs()
        {
            if (isUGUI)
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

            if (Armature == null)
            {
                return;
            }

            var slots = Armature.Structure.Slots;
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
            _closeCombineMeshs = true;
            //
            var cm = gameObject.GetComponent<UnityCombineMeshes>();
            if (cm != null)
            {
                DestroyImmediate(cm);
            }

            if (Armature == null)
            {
                return;
            }

            //
            var slots = Armature.Structure.Slots;
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