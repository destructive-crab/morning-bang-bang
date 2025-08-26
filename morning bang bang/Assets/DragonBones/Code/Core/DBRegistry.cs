using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public sealed class DBRegistry
    {
        public bool RegistryChanged { get; private set; } = false;
        
        private void MarkAsChanged() => RegistryChanged = true;
        public void MarkAsUnchanged() => RegistryChanged = false;
        
        private readonly Dictionary<DBID, object> activeRegistry = new();
        
        //unity/
        private readonly Dictionary<DBID, IEngineArmatureRoot> activeRoots = new(); //armature id to root
        //meshes
        private readonly Dictionary<DBID, DBID> slotIDToMeshID = new();

        private readonly Dictionary<DBID, DBMeshBuffer> activeMeshes = new();
        // /unity
        
        private readonly Dictionary<DBID, DisplayData> displayDataRegistry = new();
        private readonly Dictionary<DBID, DBID> displayDataToChildArmature = new();
        
        private readonly Dictionary<DBID, DBID[]> armatureToContent = new();
        
        //animation runtime
        private readonly Dictionary<DBID, DBID> activeDisplays = new();
        private readonly Dictionary<DBID, DBID[]> drawOrders = new();
        private readonly Dictionary<DBID, bool> visibilities = new();

        private readonly Dictionary<DBID, DBID> activeDisplaysChanges = new();
        private readonly Dictionary<DBID, DBID[]> drawOrderChanges = new();
        private readonly Dictionary<DBID, bool> visibilitiesChanges = new();
 
        private const char SEPARATOR = ':';
        
        private const string BUILD = "build";
        private readonly Dictionary<DBID, Dictionary<DBID, object>> buildingRegistry = new();
        
        private const string ARMATURE = "armature";
        public const string BONE = "bone";
        private const string SLOT = "slot";
        private const string CONSTRAINT = "constraint";
        private const string CHILD_ARMATURE = "child_armature";
        private const string DISPLAY_DATA = "display_data";
        private const string DISPLAY_INDEX_MARK = "_[*]_";
        
        private const string SLOT_MESH_POSTFIX = "MESH";
        private const string DISPLAY_TO_CHILD_ARMATURE_CONNECTION_POSTFIX = "DISP_CA_CN";
        private const string ARMATURE_ROOT_CONNECTION_POSTFIX = "ROOT_CN";

        #region BUILDING
        public bool IsBuildIDValid(DBID buildID, bool throwError = true)
        {
            if (buildID.Has(BUILD) && buildingRegistry.ContainsKey(buildID))
            {
                return true;
            }
            
            if(throwError) DBLogger.Error($"Invalid Build ID {buildID.ToString()}");
            return false;
        }
        public object GetInBuild(DBID buildID, DBID id)
        {
            if (id.Equals(INVALID_ID) || buildID.Equals(INVALID_ID) || !buildingRegistry.ContainsKey(buildID) || !buildingRegistry[buildID].ContainsKey(id))
            {
                return null;
            }
            return buildingRegistry[buildID][id];
        }
        public DBID SearchInBuild(DBID buildID, params string[] keywords)
        {
            foreach (DBID key in buildingRegistry[buildID].Keys)
            {
                if (DoesLeafHas(key, keywords))
                {
                    return key;
                }
            }

            return INVALID_ID;
        }

        public DBID StartBuilding(string name)
        {
            DBID buildID = GenerateRootID(BUILD + "_"+ name);
            buildingRegistry.Add(buildID, new Dictionary<DBID, object>());
            return buildID;
        }
        public DBID RegisterArmature(DBID buildID, Armature armature, IEngineArmatureRoot root)
        {
            if (!IsBuildIDValid(buildID)) return INVALID_ID;
            
            if (armature.ID.Equals(EMPTY_ID))
            {
                armature.SetID(GenerateRootID(ARMATURE+"_"+armature.Name));
            }
            else
            {
                //push error
                return INVALID_ID;
            }
            
            RegisterEntryToBuild(buildID, armature);
            RegisterDataToBuild(buildID, PutPostfixInID(armature.ID, ARMATURE_ROOT_CONNECTION_POSTFIX), root);
            
            return armature.ID;
        }
        public DBID RegisterChildArmature(DBID buildID, DBID slotID, DBID displayDataID, ChildArmature childArmature)
        {
            if (!IsBuildIDValid(buildID)) return INVALID_ID;
            if (!IsSlot(slotID, true)) return INVALID_ID;
            
            childArmature.SetID(AppendLeaf(slotID, CHILD_ARMATURE + "_" + childArmature.Name));
            
            RegisterEntryToBuild(buildID, childArmature);
            RegisterDataToBuild(buildID, PutPostfixInID(displayDataID, DISPLAY_TO_CHILD_ARMATURE_CONNECTION_POSTFIX), childArmature.ID);
            
            return childArmature.ID;
        }
        public DBID RegisterBone(DBID buildID, DBID parentID, Bone bone)
        {
            if (!IsBuildIDValid(buildID)) return INVALID_ID;

            bone.SetID(AppendLeaf(parentID, BONE + "_" + bone.name));
            RegisterEntryToBuild(buildID, bone);
            return bone.ID;
        }
        public DBID RegisterSlot(DBID buildID, DBID boneID, Slot slot)
        {
            if (!IsBuildIDValid(buildID)) return INVALID_ID;
            if (!IsBone(boneID, true)) return INVALID_ID;
            
            if (boneID.Invalid())
            {
                DBLogger.Error($"[Register Slot]: Invalid Bone ID provided {boneID}");
                return INVALID_ID;
            }
            
            slot.SetID(AppendLeaf(boneID, SLOT + "_" + slot.Name));
            RegisterEntryToBuild(buildID, slot);
            return slot.ID;

        }
        public DBID RegisterConstraint(DBID buildID, DBID armatureID, Constraint constraint)
        {
            if (!IsBuildIDValid(buildID)) return INVALID_ID;

            constraint.SetID(AppendLeaf(armatureID, CONSTRAINT + "_" + constraint.name));
            RegisterEntryToBuild(buildID, constraint);
            return constraint.ID;
        }
        public DBID RegisterDisplay(DBID buildID, DBID slotID, DisplayData data, int index)
        {
            if (!IsBuildIDValid(buildID)) return INVALID_ID;
            if (!IsSlot(slotID)) return INVALID_ID;
            
            DBID id = AppendLeaf(slotID, DISPLAY_DATA + DISPLAY_INDEX_MARK.Replace("*", index.ToString()) + data.Name);
            buildingRegistry[buildID].Add(id, data);
            
            return id;
        }
        
        //buffers. only MUST be used in CompleteBuilding
        private readonly List<Tuple<DBID, object>> activeRegistryBuffer = new();
        private readonly List<Tuple<DBID, DisplayData>> displayDataBuffer = new();
        private readonly List<DBID> armaturesBuffer = new(); //0 - armature; 1 - engine root; 2...n - content
        public void CompleteBuilding(DBID buildingID)
        {
            int armaturesBufferCurrent = 0;
            foreach (KeyValuePair<DBID, object> entry in buildingRegistry[buildingID])
            {
                if (IsArmature(entry.Key))
                {
                    AddToArmaturesBuffer(entry.Key);
                    continue;
                }
                if (entry.Key.ID.Contains(ARMATURE_ROOT_CONNECTION_POSTFIX))
                {
                    
                }
            }

            buildingRegistry.Remove(buildingID);
            MarkAsChanged();

            void AddToArmaturesBuffer(DBID id)
            {
                if(armaturesBuffer.Count > armaturesBufferCurrent) armaturesBuffer[armaturesBufferCurrent] = id;
                else armaturesBuffer.Add(id);
                armaturesBufferCurrent++;
            }
        }
        
        private void RegisterEntryToBuild(DBID buildID, IRegistryEntry entry)
        {
            if (!IsBuildIDValid(buildID)) return;
            buildingRegistry[buildID].Add(entry.ID, entry);
        }
        private void RegisterDataToBuild(DBID buildID, DBID id, object data)
        {
            buildingRegistry[buildID].Add(id, data);
        }
        #endregion
        #region RUNTIME

        public bool HasMesh(DBID id)
        {
            return activeMeshes.ContainsKey(id);
        }

        public DBMeshBuffer CreateMesh(DBID id)
        {
            activeMeshes.TryAdd(id, new DBMeshBuffer());
            return activeMeshes[id];
        }
        
        public void UpdateMesh(DBID id, Mesh mesh, Material material)
        {
            activeMeshes.TryAdd(id, new DBMeshBuffer());
            activeMeshes[id].Material = material;
        }
        
        public void SetActiveDisplayFor(DBID slotID, DBID displayID)
        {
            activeDisplays[slotID] = displayID;
        }

        public void SetActiveDisplayForByIndex(DBID slotID, int index)
        {
            DBID[] ids = GetAllDisplaysDataOf(slotID);
            for (var i = 0; i < ids.Length; i++)
            { 
                if (ids[i].Has(DISPLAY_INDEX_MARK.Replace("*", index.ToString())))
                {
                    SetActiveDisplayFor(slotID, ids[i]);
                    return;
                }
            }
        }

        public void SortDrawOrder(DBID armatureID)
        {
            if(!armatureID.Has(ARMATURE)) return;

            DBID[] sortedOrder = drawOrders[armatureID];
            
            if(drawOrders[armatureID].Length < 20) InsertionSort(ref sortedOrder);
            else                                                   HeapSort(ref sortedOrder);

            drawOrders[armatureID] = sortedOrder;
        }
        
        
        private void HeapSort(ref DBID[] slots)
        {
            int n = slots.Length;
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(ref slots, i);

            for (int i = n - 1; i >= 0; i--) {
                Swap(ref slots, n, i );
                Heapify(ref slots, 0);
            }
            return;
            
            void Heapify(ref DBID[] slots, int i)
            {
                int n = slots.Length;
                int largest = i;
                int left = 2 * i + 1;
                int right = 2 * i + 2;

                if (left < n && GetSlot(slots[left]).DrawOrder.V > GetSlot(slots[largest]).DrawOrder.V)
                {
                    largest = left;
                }

                if (right < n && GetSlot(slots[right]).DrawOrder.V > GetSlot(slots[largest]).DrawOrder.V)
                {
                    largest = right;
                }

                if (largest != i) {
                    Swap(ref slots, i, largest);
                    Heapify(ref slots, largest);
                } 
            }

            void Swap(ref DBID[] slots, int a, int b)
            {
                (slots[a], slots[b]) = (slots[b], slots[a]);
            }
        }
        private void InsertionSort(ref DBID[] slots)
        {
            int i;
            int j;
            
            for (i = 1; i < slots.Length; i++) {
                DBID key = slots[i];
                j = i - 1;

                while (j >= 0 && GetSlot(slots[j]).DrawOrder.V > GetSlot(key).DrawOrder.V)
                {
                    slots[j + 1] = slots[j];
                    j = j - 1;
                }
                slots[j + 1] = key;
            }
        }

        #endregion
        #region GET INTERFACE

        public DBID Search(string keyword)
        {
            return INVALID_ID;
        }

        public DBID SearchAtArmature(DBID armatureID, string keyword)
        {
            for (int i = 0; i < armatureToContent[armatureID].Length; i++)
            {
                if (armatureToContent[armatureID][i].Has(keyword)) return armatureToContent[armatureID][i];
            }

            return INVALID_ID;
        }
        public DBID GetParent(DBID childID)
        {
            int childPartStart = childID.ID.LastIndexOf(SEPARATOR);

            if (childPartStart < 0)
            {
                return INVALID_ID;
            }

            return new DBID(childID.ID.Remove(childPartStart));
        }
        
        public TRoot GetEngineRoot<TRoot>(DBID armatureID)
            where TRoot : class, IEngineArmatureRoot
        {
            return activeRoots[armatureID] as TRoot;
        }
        
        public TEntry GetEntry<TEntry>(DBID id)
            where TEntry : class, IRegistryEntry
        {
            return Get(id) as TEntry;
        }
        
        public TEntry GetEntryInBuild<TEntry>(DBID buildID, DBID id)
            where TEntry : class, IRegistryEntry
        {
            return GetInBuild(buildID, id) as TEntry;
        }

        public ChildArmature GetChildArmature(DBID id) => Get(id) as ChildArmature;
        public ChildArmature GetChildArmatureFromDisplayID(DBID displayID) => Get(displayDataToChildArmature[displayID]) as ChildArmature;
        public ChildArmature GetChildArmatureInBuild(DBID buildID, DBID id) => GetInBuild(buildID, id) as ChildArmature;
        public ChildArmature SearchInBuildAndGetChildArmature(DBID buildId, string keyword) => GetChildArmatureInBuild(buildId, SearchInBuild(buildId, keyword));
        public Armature GetArmature(DBID id) => Get(id) as Armature;
        public Armature GetArmatureInBuild(DBID buildID, DBID id) => GetInBuild(buildID, id) as Armature;
        public Armature SearchInBuildAndGetArmature(DBID buildID, string keyword) => GetArmatureInBuild(buildID, SearchInBuild(buildID, keyword));
        public Bone GetBone(DBID id) => Get(id) as Bone;
        public Bone GetBoneInBuild(DBID buildID, DBID id) => GetInBuild(buildID, id) as Bone;
        public Bone SearchInBuildAndGetBone(DBID buildID, string keyword) => GetBoneInBuild(buildID, SearchInBuild(buildID, keyword));
        public Slot GetSlot(DBID id) => Get(id) as Slot;
        public Slot GetSlotInBuild(DBID buildID, DBID id) => GetInBuild(buildID, id) as Slot;
        public Slot SearchInBuildAndGetSlot(DBID buildID, string keyword) => GetSlotInBuild(buildID, SearchInBuild(buildID, keyword));
        public object Get(DBID id)
        {
            if (!activeRegistry.ContainsKey(id) || id.Equals(INVALID_ID))
            {
                return null;
            }

            return activeRegistry[id];
        }


        public DisplayData GetDisplayData(DBID id)
        {
            if (id.Equals(EMPTY_ID) || id.Equals(INVALID_ID))
            {
                DBLogger.Warn("Invalid ID was provided to get display data");
                return null;
            }
            return displayDataRegistry[id];
        }

        public DBID GetCurrentActiveDisplayOf(DBID slotID) => activeDisplays[slotID];

        public DBMeshBuffer GetMesh(DBID id)
        {
            if (id.Equals(EMPTY_ID) || id.Equals(INVALID_ID))
            {
                DBLogger.LogMessage("INVALID ID PROVIDED TO GET MESH");
                return null;
            }

            if (!activeMeshes.ContainsKey(id))
            {
                //DBLogger.LogMessage($"NO {id} FOUND IN ACTIVE MESHES REGISTRY");
                return null;
            }

            return activeMeshes[id];
        }

        private readonly List<DBID> helpIDList = new();
        private readonly object lockHelpList = new();

        public DBID[] GetAllChildEntries<TEntry>(DBID parentID, bool includeChildArmatures = false)
            where TEntry : class, IRegistryEntry
        {
            lock (lockHelpList)
            {
                string hasCheck = "";
                if (typeof(TEntry) == typeof(Slot)) { hasCheck = SLOT; }
                if (typeof(TEntry) == typeof(Bone)) { hasCheck = BONE; }
                if (typeof(TEntry) == typeof(ChildArmature)) { hasCheck = CHILD_ARMATURE; }
                if (typeof(TEntry) == typeof(Constraint)) { hasCheck = CONSTRAINT; }
                
                helpIDList.Clear();
                int childArmaturesInParentsPath = 0;
                if(!includeChildArmatures)
                {
                    childArmaturesInParentsPath = CountInPath(parentID, CHILD_ARMATURE);
                }
                foreach (KeyValuePair<DBID, object> pair in activeRegistry)
                {
                    if(pair.Key.Has(parentID) && Is(pair.Key, hasCheck) && (includeChildArmatures || childArmaturesInParentsPath == CountInPath(pair.Key, CHILD_ARMATURE)))
                    {
                        helpIDList.Add(pair.Key);
                    }
                }
                return helpIDList.ToArray();   
            }
        }
        
        
        public DBID[] GetAllRootArmatures()
        {
            lock (lockHelpList)
            {
                helpIDList.Clear();
                foreach (KeyValuePair<DBID, object> pair in activeRegistry)
                {
                    if (IsArmature(pair.Key))
                    {
                        helpIDList.Add(pair.Key);
                    }
                }   
                return helpIDList.ToArray();
            }
        }
        
        //as parentID can be provided any parent of search target
        public DBID[] GetChildSlotsOf(DBID parentID, bool includeChildArmatures = false)
        {
            lock (lockHelpList)
            {
                helpIDList.Clear();
                int childArmaturesInParentsPath = 0;
                if(!includeChildArmatures)
                {
                    childArmaturesInParentsPath = CountInPath(parentID, CHILD_ARMATURE);
                }
                foreach (KeyValuePair<DBID, object> pair in activeRegistry)
                {
                    if(pair.Key.Has(parentID) && DoesLeafHas(pair.Key, SLOT) && (includeChildArmatures || childArmaturesInParentsPath == CountInPath(pair.Key, CHILD_ARMATURE)))
                    {
                        helpIDList.Add(pair.Key);
                    }
                }
                return helpIDList.ToArray();   
            }
        }
        public DBID[] GetAllDisplaysDataOf(DBID parentID)
        {
            lock (lockHelpList)
            {
                helpIDList.Clear();
                foreach (KeyValuePair<DBID, DisplayData> pair in displayDataRegistry)
                {
                    if(pair.Key.Has(parentID) && DoesLeafHas(pair.Key, DISPLAY_DATA))
                        helpIDList.Add(pair.Key);
                }
                return helpIDList.ToArray();   
            }
        }
        public DBID[] GetAllChildArmaturesOf(DBID parentID)
        {
            lock (lockHelpList)
            {
                helpIDList.Clear();
                foreach (KeyValuePair<DBID, object> pair in activeRegistry)
                {
                    if(pair.Key.Has(parentID) && pair.Key.Has(CHILD_ARMATURE))
                        helpIDList.Add(pair.Key);
                }
                return helpIDList.ToArray();   
            }
        }
        public DBID[] GetBones(DBID parentID, bool includeChildArmatures = false)
        {
            lock (lockHelpList)
            {
                helpIDList.Clear();
                int childArmaturesInParentsPath = 0;
                if(!includeChildArmatures)
                {
                    childArmaturesInParentsPath = CountInPath(parentID, CHILD_ARMATURE);
                }

                foreach (KeyValuePair<DBID, object> pair in activeRegistry)
                {
                    if(pair.Key.Has(parentID) && DoesLeafHas(pair.Key, BONE) && (includeChildArmatures || childArmaturesInParentsPath == CountInPath(pair.Key, CHILD_ARMATURE)))
                    {
                        helpIDList.Add(pair.Key);
                    }
                }
                return helpIDList.ToArray();   
            }
        }
        #endregion
        #region ID
        public struct DBID : IEquatable<DBID>
        {
            public string ID;

            public bool Has(string part)
            {
                if (string.IsNullOrEmpty(part)) return false;
                return ID.Contains(part);
            }
            
            public bool Has(DBID part)
            {
                if (part.Equals(null)) return false;
                if (part.Equals(EMPTY_ID)) return false;
                if (part.Equals(INVALID_ID)) return false;
                
                return Has(part.ID);
            }

            public DBID(string id)
            {
                if(string.IsNullOrEmpty(id))
                {
                    ID = EMPTY_ID.ID;
                }

                ID = id;
            }

            public override string ToString()
            {
                return ID;
            }

            public bool Equals(DBID other) => ID == other.ID;
            public override bool Equals(object obj)
            {
                return obj is DBID other && Equals(other);
            }

            public override int GetHashCode() => (ID != null ? ID.GetHashCode() : 0);

            public bool Invalid()
            {
                return Equals(INVALID_ID) || Equals(EMPTY_ID);
            }
        }
        public static readonly DBID INVALID_ID = new("INVALID_ID");
        public static readonly DBID EMPTY_ID = new("EMPTY_ID");
        
        private void GoFromRootTo(DBID id, Action<DBID, int> callback)
        {
            DBID current = id;
            DBID[] all = new DBID[CountElements(current)];
            
            int i = 0;
            while (!current.Invalid())
            {
                all[all.Length - i - 1] = current;
                current = GetParent(current);
            }

            for (i = 0; i < all.Length; i++)
            {
                DBID next = all[i];
                callback?.Invoke(all[i], i);
            }
        }
        private void GoToRootOf(DBID id, Action<DBID, int> callback)
        {
            DBID current = id;
            DBID[] all = new DBID[CountElements(current)];
            
            int i = 0;
            while (!current.Invalid())
            {
                all[all.Length - i - 1] = current;
                callback?.Invoke(current, i);
                
                current = GetParent(current);
            }
        }
        private string GetLeaf(DBID id)
        {
            if (id.Invalid())
            {
                DBLogger.Error($"Invalid ID({id}) provided, can not get leaf");
                return string.Empty;
            }
            return id.ID.Substring(id.ID.LastIndexOf(SEPARATOR));
        }

        private DBID GetRootOf(DBID id)
        {
            if (id.Invalid())
            {
                DBLogger.Error($"Invalid ID({id}) provided, can not get leaf");
                return INVALID_ID;
            }
            return new DBID(id.ID.Substring(0, id.ID.IndexOf(SEPARATOR)));
        }


        private bool DoesLeafHas(DBID id, string key) => GetLeaf(id).Contains(key);
        private bool DoesLeafHas(DBID id, params string[] keys)
        {
            string leaf = GetLeaf(id);
            foreach (string key in keys)
            {
                if (!leaf.Contains(key)) return false;
            }

            return true;
        }
        private bool DoesLeafHasAny(DBID id, params string[] keys)
        {
            string leaf = GetLeaf(id);
            foreach (string key in keys)
            {
                if (leaf.Contains(key)) return true;
            }

            return false;
        }
        public bool Is(DBID id, string key, bool throwError = false)
        {
            if (DoesLeafHas(id, key)) return true;
            
            if(throwError)
            {
                DBLogger.Error($"ID({id.ToString()}) is not {key} as supposed to be");
            }

            return false;
        }

        public bool IsArmature(DBID id, bool throwError = false) => Is(id, ARMATURE, throwError) && !DoesLeafHasAny(id, CHILD_ARMATURE, ARMATURE_ROOT_CONNECTION_POSTFIX);
        public bool IsBone(DBID id, bool throwError = false) => Is(id, BONE, throwError);
        public bool IsSlot(DBID id, bool throwError = false) => Is(id, SLOT, throwError) && !DoesLeafHas(id, SLOT_MESH_POSTFIX);
        public bool IsChildArmature(DBID id, bool throwError = false) => Is(id, CHILD_ARMATURE, throwError) && !DoesLeafHas(id, DISPLAY_TO_CHILD_ARMATURE_CONNECTION_POSTFIX);
        public bool IsDisplayData(DBID id, bool throwError = false) => Is(id, DISPLAY_DATA, throwError);
        public bool IsConstraint(DBID id, bool throwError = false) => Is(id, CONSTRAINT, throwError);

        private DBID AppendLeaf(DBID parent, string leaf)
        {
            DBID newID;
            newID.ID = parent.ID + SEPARATOR + leaf + "_" + GetRandomID();
            
            return newID;
        }
        private DBID PutPostfixInID(DBID id, string postfix)
        {
            DBID newID;
            newID.ID = id.ID + "_" + postfix;
            
            return newID;
        }
        private int CountElements(DBID id) => id.ID.Count((c => c == SEPARATOR));
        private int CountInPath(DBID id, string key)
        {
            int c = 0;

            void Count(DBID dbid, int i)
            {
                if (i == 0) return;
                if (DoesLeafHas(dbid, key)) c++;
            }

            GoToRootOf(id, Count);
            return c;
        }
        private DBID GenerateRootID(string name)
        {
            DBID newID = new(SEPARATOR + name + "_" + GetRandomID());
            return newID;
        }
        private static string GetRandomID()
        {
            string ID = "";
            
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);
            ID += UnityEngine.Random.Range(0, 10);

            return ID;
        }
        #endregion
        #region DEBUG
        public void PrintCurrentState()
        {
            DBLogger.StartNewArmatureBuildLog("DB Registry State");

            DBLogger.BLog.StartSection("Active Register");
            int i = 0;
            foreach (KeyValuePair<DBID, object> entry in activeRegistry)
            {
                DBLogger.BLog.AddEntry($"[{i}]", entry.Key + " " + entry.Value.ToString());
                i++;
            }
            DBLogger.BLog.EndSection();
            
            DBLogger.BLog.StartSection("Active Displays");
            i = 0;
            foreach (KeyValuePair<DBID, DBID> entry in activeDisplays)
            {
                DBLogger.BLog.AddEntry($"[{i}]", entry.Key + " -> " + entry.Value.ToString());
                i++;
            }
            DBLogger.BLog.EndSection();           
            
            DBLogger.BLog.StartSection("Active Meshes");
            i = 0;
            foreach (KeyValuePair<DBID, DBMeshBuffer> entry in activeMeshes)
            {
                DBLogger.BLog.AddEntry($"[{i}]", entry.Key + " -> " + entry.Value.ToString(), $"VC: {entry.Value.GeneratedMesh.vertexCount}; Mat: {entry.Value.Material.name}");
                i++;
            }
            DBLogger.BLog.EndSection();           
            
            DBLogger.BLog.StartSection("Display Data Registry");
            i = 0;
            foreach (KeyValuePair<DBID, DisplayData> entry in displayDataRegistry)
            {
                DBLogger.BLog.AddEntry($"[{i}]", entry.Key + " " + entry.Value);
                i++;
            }
            DBLogger.BLog.EndSection();
            
            DBLogger.BLog.StartSection("Structures Registry");
            i = 0;
            foreach (KeyValuePair<DBID, DBID[]> entry in armatureToContent)
            {
                DBLogger.BLog.StartSection($"Structure {i}: {entry.Key}");
                foreach (DBID dbid in entry.Value)
                {
                    DBLogger.BLog.AddEntry($"Part:", dbid.ToString());
                }
                DBLogger.BLog.EndSection();
                i++;
            }
            DBLogger.BLog.EndSection();
            
            DBLogger.BLog.StartSection("Draw Orders");
            i = 0;
            foreach (KeyValuePair<DBID, DBID[]> entry in drawOrders)
            {
                DBLogger.BLog.StartSection($"{i} Order Of : {entry.Key}");
                for (var order = 0; order < entry.Value.Length; order++)
                {
                    var dbid = entry.Value[order];
                    DBLogger.BLog.AddEntry($"{order}", dbid.ToString());
                }

                DBLogger.BLog.EndSection();
                i++;

            }
            DBLogger.BLog.EndSection();           
            
            DBLogger.PrintBuildLog(DBLogger.FinishArmatureBuildLog());
        }
        #endregion
    }
}