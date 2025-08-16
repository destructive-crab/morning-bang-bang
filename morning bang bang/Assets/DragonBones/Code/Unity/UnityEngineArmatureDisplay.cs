using System;
using System.Collections.Generic;
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
                if (!slot.Displays.HasVisibleDisplay) { continue; }

                slot.UpdateZPosition();

                if (slot.IsDisplayingChildArmature())
                {
                    ((UnityEngineArmatureDisplay)slot.Displays.CurrentChildArmature.ArmatureDisplay).UpdateSlotsSorting();
                }
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
        public ArmatureMesh Combiner { get; private set; }
        public UnityEngineMeshSeparator Separator { get; private set; }
        public UnitySlotStateRegistry Registry { get; private set; }

        public void DBInit(Armature armature)
        {
            Armature = armature;

            if (Combiner == null)
            {
                Combiner = new ArmatureMesh(this);
            }

            if (Separator == null)
            {
                Separator = new UnityEngineMeshSeparator(this);
            }

            if (Registry == null)
            {
                Registry = new UnitySlotStateRegistry(Armature.Structure, Separator, Combiner);
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
                DBProjectData dragonBonesData = DBInitial.UnityDataLoader.LoadData(unityData, false);

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
            if (Armature == null || !Armature.Structure.SlotsBuilt) { return; }

            _flipX = Armature.flipX;
            _flipY = Armature.flipY;

            //combine startpoint. combine mesh if needed
            if (CombineMeshes && !Combiner.IsCombined)
            {
                Combiner.Combine();
                //Registry.CommitChanges();
                
            }
            else if (!CombineMeshes && !Separator.IsCreated)
            {
                Separator.Create();
                Registry.CommitChanges();
            }

//            foreach (Slot slot in Armature.Structure.Slots)
//            {
//                var changes = Registry.PullChanges(slot.Name);
//                
//                while (changes != UnitySlotStateRegistry.RegistryChange.None)
//                {
//                    //process changes
//                    switch (changes)
//                    {
//                        case UnitySlotStateRegistry.RegistryChange.ChildArmatureVisibility: break;
//                        case UnitySlotStateRegistry.RegistryChange.CombinedMeshVisibility: break;
//                        case UnitySlotStateRegistry.RegistryChange.SeparatedMeshVisibility: break;
//                        case UnitySlotStateRegistry.RegistryChange.DisplayChanged: break;
//                    }
//                    
//                    changes = Registry.PullChanges(slot.Name);
//                } 
//            }
           // Registry.CommitChanges();
            
            if (Combiner.IsCombined) Combiner.Update();
            if (Separator.IsCreated) Separator.Update();

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

        private bool IsDataSetupCorrectly()
        {
            return unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null;
        }

        public class UnitySlotStateRegistry
        {
            public enum DisplayType
            {
                ChildArmature,
                CombinedMesh,
                SeparatedMesh,
            }

            public enum RegistryChange
            {
                None,
                
                ChildArmatureVisibility,
                CombinedMeshVisibility,
                SeparatedMeshVisibility,
                
                DisplayChanged
            }
            
            private readonly Dictionary<string, DisplayType> states = new();
            private readonly Dictionary<string, string> currentDisplays = new();
            private readonly Dictionary<string, bool> visibilities = new();
            
            private ArmatureStructure structure;
            
            private UnityEngineMeshSeparator separator;
            private ArmatureMesh combiner;

            public UnitySlotStateRegistry(ArmatureStructure structure, UnityEngineMeshSeparator separator, ArmatureMesh combiner)
            {
                this.structure = structure;
                this.separator = separator;
                this.combiner = combiner;
            }

            public RegistryChange PullChanges(string name)
            {
                UnitySlot unitySlot = structure.GetSlot(name) as UnitySlot;

                var state = GetState(name);
                
                if (GetVisibility(name) != unitySlot.IsVisible)
                {
                    switch (state)
                    {
                        case DisplayType.ChildArmature: return RegistryChange.ChildArmatureVisibility;
                        case DisplayType.CombinedMesh: return RegistryChange.CombinedMeshVisibility; 
                        case DisplayType.SeparatedMesh: return RegistryChange.SeparatedMeshVisibility;
                    }
                }

                if (state == DisplayType.ChildArmature && !unitySlot.IsDisplayingChildArmature())
                    return RegistryChange.DisplayChanged;
                if (state != DisplayType.ChildArmature && unitySlot.IsDisplayingChildArmature())
                    return RegistryChange.DisplayChanged;

                //todo warnings
                return RegistryChange.None;
            }

            public void CommitChanges()
            {
                foreach (Slot slot in structure.Slots)
                {
                    if (combiner.Contains(slot.Name)) states[slot.Name] = DisplayType.CombinedMesh;
                    else if (separator.Contains(slot.Name)) states[slot.Name] = DisplayType.SeparatedMesh;
                    else if (slot.IsDisplayingChildArmature()) states[slot.Name] = DisplayType.ChildArmature;

                    visibilities[slot.Name] = slot.Visible.Value;
                    currentDisplays[slot.Name] = slot.Displays.CurrentName;
                }
            }
            
            private bool GetVisibility(string name) => visibilities[name];
            private string GetSlotDisplayName(UnitySlot unitySlot) => currentDisplays[unitySlot.Name];
            public DisplayType GetState(string name) => states[name];
            public void SetState(string name, DisplayType state) => states[name] = state;
        }
    }
}
