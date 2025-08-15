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
    [DisallowMultipleComponent]
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

        public bool CombineMeshes = true;
        
        private void UpdateSlotsSorting()
        {
            if (Armature == null)
            {
                return;
            }
            
            foreach (UnitySlot slot in Armature.Structure.Slots)
            {
                var display = slot.UnityCurrentDisplay;
                if (display == null)
                {
                    continue;
                }

                slot.UpdateZPosition(new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot.ZOrder.Value * (_zSpace + 0.001f)));

                if (slot.IsDisplayingChildArmature())
                {
                    ((UnityEngineArmatureDisplay)slot.Displays.ChildArmatureSlotDisplay.ArmatureDisplay).UpdateSlotsSorting();
                }

#if UNITY_EDITOR
                if (!Application.isPlaying && slot.MeshRenderer != null)
                {
                    EditorUtility.SetDirty(slot.MeshRenderer);
                }
#endif
            }
        }
        
        public const int ORDER_SPACE = 10;
        public UnityDragonBonesData unityData = null;

        public string armatureName = null;

        internal readonly DBColor _DBColor = new DBColor();

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
        private bool _hasSortingGroup = false;

        public Armature Armature { get; internal set; }
        public AnimationPlayer AnimationPlayer => Armature != null ? Armature.AnimationPlayer : null;
        public UnityEngineMeshCombiner Combiner { get; private set; }
        
        public void DBInit(Armature armature)
        {
            Armature = armature;
            
            if(Combiner == null)
            {
                Combiner = new UnityEngineMeshCombiner(this);
            }
        }

        public void DBClear(bool disposeDisplay = false)
        {
            if (disposeDisplay)
            {
                //TODO DBUnityFactory.Helper.DestroyUnityObject(gameObject);
            }
            
            unityData = null;
            armatureName = null;
            animationName = null;

            Armature = null;
            _DBColor.Identity();
            _playTimes = 0;
            _timeScale = 1.0f;
            _zSpace = 0.0f;
            _flipX = false;
            _flipY = false;

            _hasSortingGroup = false;
            Combiner?.Clear();
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

        public DBColor DBColor
        {
            get { return _DBColor; }
            set
            {
                _DBColor.CopyFrom(value);

                foreach (var slot in Armature.Structure.Slots)
                {
                    slot.Color.MarkAsDirty();
                }
            }
        }


        private void Awake()
        {
            if (IsDataSetupCorrectly())
            {
                var dragonBonesData = DBInitial.UnityDataLoader.LoadData(unityData, false);

                if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
                {
                    DBInitial.UnityFactory.UnityCreateArmature(armatureName, unityData.dataName, this,null, null);
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

        private void LateUpdate()
        {
            if (Armature == null) { return; }

            _flipX = Armature.flipX;
            _flipY = Armature.flipY;
 
            if (CombineMeshes && !Combiner.IsCombined)
            {
                Combiner.Combine();
            }

            if (Combiner.IsCombined) Combiner.Update();
            
            foreach (Slot slot in Armature.Structure.Slots)
            {
                UnitySlot unitySlot = slot as UnitySlot;

                if (slot.IsDisplayingChildArmature()) continue;
                
                if (CombineMeshes && unitySlot.IsEnabled)
                {
                    unitySlot.Disable();
                    continue;
                }
                
                
                UpdateSlotGO(unitySlot);
            }
            
            //TODO
            
            var hasSortingGroup = GetComponent<SortingGroup>() != null;
            if (hasSortingGroup != _hasSortingGroup)
            {
                _hasSortingGroup = hasSortingGroup;
            }
        }

        private void OnDestroy()
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

        private static void UpdateSlotGO(UnitySlot unitySlot)
        {
            unitySlot.CurrentAsMeshDisplay.MeshFilter.sharedMesh = unitySlot.meshBuffer.sharedMesh;
            unitySlot.CurrentAsMeshDisplay.MeshRenderer.sharedMaterial = unitySlot.currentTextureAtlasData.texture;
        }

        private bool IsDataSetupCorrectly()
        {
            return unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null;
        }
    }
}