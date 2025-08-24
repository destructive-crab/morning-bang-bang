using System.Collections.Generic;

namespace DragonBones
{
    public class DBDataStorage
    {
        /// <summary>
        /// - If true, you can provide ""/null project name when build armature, armature data will be searched through all armature data loaded to storage
        /// </summary>
        public bool AutoSearch = false;
        
        //cash
        protected readonly Dictionary<string, DBProjectData> dragonBonesDataMap = new();
        protected readonly Dictionary<string, List<TextureAtlasData>> textureAtlasDataMap = new();

        protected readonly Dictionary<string, object> engineCache = new();
        
        //parsers
        protected readonly DataParser dataParser = null;
        
        protected static ObjectDataParser objectParser = null;
        protected static BinaryDataParser binaryParser = null;
        
        //option to set custom notbinary parser
        public DBDataStorage(DataParser dataParser = null)
        {
            if (objectParser == null)
            {
                objectParser = new ObjectDataParser();
            }

            if (binaryParser == null)
            {
                binaryParser = new BinaryDataParser();
            }

            if (dataParser != null)
            {
                this.dataParser = dataParser;
            }
            else
            {
                this.dataParser = objectParser;
            }
        }

        /// <summary>
        /// Refresh the Armature textureAtlas data.
        /// </summary>
        /// <param name="unityArmature">UnityArmatureComponent</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public TData GetCachedEngineData<TData>(string dataName)
            where TData : class
        {
            return engineCache[dataName] as TData;
        }
        
        public bool CacheEngineData(string dataName, object data, bool replace = false)
        {
            //todo add logs
            
            if (engineCache.ContainsKey(dataName) && replace)
            {
                engineCache[dataName] = data;
                return true;
            }
            else if(!replace)
            {
                return false;
            }
            
            engineCache.Add(dataName, data);
            return true;
        }

         /// <summary>
        /// - Parse the raw data to a DragonBonesData instance and cache it to the factory.
        /// </summary>
        /// <param name="rawData">- The raw data.</param>
        /// <param name="name">- Specify a cache name for the instance so that the instance can be obtained through name. (If not set, use the instance name instead)</param>
        /// <param name="scale">- Specify a scaling value for all armatures. (Default: 1.0)</param>
        /// <returns>DragonBonesData instance</returns>
        /// <see cref="GetDragonBonesData()"/>
        /// <see cref="AddDragonBonesData()"/>
        /// <see cref="RemoveDragonBonesData()"/>
        /// <see cref="DBProjectData"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public DBProjectData ParseAndAddDragonBonesData(object rawData, string name = null, float scale = 1.0f)
        {
            DataParser parser;
            
            if (rawData is byte[])
            {
                parser = binaryParser;
            }
            else
            {
                parser = dataParser;
            }

            DBProjectData dbProjectData = parser.ParseDragonBonesData(rawData, scale);

            //idk what that code means lol ;todo
            while (true)
            {
                TextureAtlasData textureAtlasData = DB.Kernel.Factory.BuildTextureAtlasData(null, null);
                
                if (parser.ParseTextureAtlasData(null, textureAtlasData, scale))
                {
                    AddTextureAtlasData(textureAtlasData, name);
                }
                else
                {
                    textureAtlasData.ReleaseThis();
                    break;
                }
            }

            if (dbProjectData != null)
            {
                AddDragonBonesData(dbProjectData, name);
            }

            return dbProjectData;
        }
        /// <summary>
        /// - Parse the raw texture atlas data and the texture atlas object to a TextureAtlasData instance and cache it to the factory.
        /// </summary>
        /// <param name="rawData">- The raw texture atlas data.</param>
        /// <param name="textureAtlas">- The texture atlas object.</param>
        /// <param name="name">- Specify a cache name for the instance so that the instance can be obtained through name. (If not set, use the instance name instead)</param>
        /// <param name="scale">- Specify a scaling value for the map set. (Default: 1.0)</param>
        /// <returns>TextureAtlasData instance</returns>
        /// <see cref="GetTextureAtlasData()"/>
        /// <see cref="AddTextureAtlasData()"/>
        /// <see cref="RemoveTextureAtlasData()"/>
        /// <see cref="TextureAtlasData"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public TextureAtlasData ParseAndAddTextureAtlasData(Dictionary<string, object> rawData, object textureAtlas, string name = null, float scale = 1.0f)
        {
            var textureAtlasData = DB.Kernel.Factory.BuildTextureAtlasData(null, null);
            dataParser.ParseTextureAtlasData(rawData, textureAtlasData, scale);
            DB.Kernel.Factory.BuildTextureAtlasData(textureAtlasData, textureAtlas);
            AddTextureAtlasData(textureAtlasData, name);

            return textureAtlasData;
        }
        
        public void UpdateTextureAtlasData(string name, List<object> textureAtlases)
        {
            var textureAtlasDatas = GetTextureAtlasData(name);
            if (textureAtlasDatas != null)
            {
                for (int i = 0, l = textureAtlasDatas.Count; i < l; ++i)
                {
                    if (i < textureAtlases.Count)
                    {
                        DB.Kernel.Factory.BuildTextureAtlasData(textureAtlasDatas[i], textureAtlases[i]);
                    }
                }
            }
        }
        
        public TextureData GetTextureData(string textureAtlasName, string textureName)
        {
            if (textureAtlasDataMap.ContainsKey(textureAtlasName))
            {
                foreach (var textureAtlasData in textureAtlasDataMap[textureAtlasName])
                {
                    var textureData = textureAtlasData.GetTexture(textureName);
                    if (textureData != null)
                    {
                        return textureData;
                    }
                }
            }

            foreach (var values in textureAtlasDataMap.Values)
            {
                foreach (var textureAtlasData in values)
                {
                    if (textureAtlasData.autoSearch)
                    {
                        var textureData = textureAtlasData.GetTexture(textureName);
                        if (textureData != null)
                        {
                            return textureData;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// - Get a specific DragonBonesData instance.
        /// </summary>
        /// <param name="name">- The DragonBonesData instance cache name.</param>
        /// <returns>DragonBonesData instance</returns>
        /// <see cref="ParseAndAddDragonBonesData"/>
        /// <see cref="AddDragonBonesData()"/>
        /// <see cref="RemoveDragonBonesData()"/>
        /// <see cref="DBProjectData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public DBProjectData GetDragonBonesData(string name)
        {
            return dragonBonesDataMap.ContainsKey(name) ? dragonBonesDataMap[name] : null;
        }
        /// <summary>
        /// - Cache a DragonBonesData instance to the factory.
        /// </summary>
        /// <param name="data">- The DragonBonesData instance.</param>
        /// <param name="name">- Specify a cache name for the instance so that the instance can be obtained through name. (if not set, use the instance name instead)</param>
        /// <see cref="ParseAndAddDragonBonesData"/>
        /// <see cref="GetDragonBonesData()"/>
        /// <see cref="RemoveDragonBonesData()"/>
        /// <see cref="DBProjectData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void AddDragonBonesData(DBProjectData data, string name = null)
        {
            name = !string.IsNullOrEmpty(name) ? name : data.name;
            if (dragonBonesDataMap.ContainsKey(name))
            {
                if (dragonBonesDataMap[name] == data)
                {
                    return;
                }

                DBLogger.Assert(false, "Can not add same name data: " + name);
                return;
            }

            dragonBonesDataMap[name] = data;
        }
        /// <summary>
        /// - Remove a DragonBonesData instance.
        /// </summary>
        /// <param name="name">- The DragonBonesData instance cache name.</param>
        /// <param name="disposeData">- Whether to dispose data. (Default: true)</param>
        /// <see cref="ParseAndAddDragonBonesData"/>
        /// <see cref="GetDragonBonesData()"/>
        /// <see cref="AddDragonBonesData()"/>
        /// <see cref="DBProjectData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public virtual void RemoveDragonBonesData(string name, bool disposeData = true)
        {
            if (dragonBonesDataMap.ContainsKey(name))
            {
                if (disposeData)
                {
                    DB.Kernel.BufferObject(dragonBonesDataMap[name]);
                }

                dragonBonesDataMap.Remove(name);
            }
        }
        /// <summary>
        /// - Get a list of specific TextureAtlasData instances.
        /// </summary>
        /// <param name="name">- The TextureAtlasData cahce name.</param>
        /// <see cref="ParseAndAddTextureAtlasData"/>
        /// <see cref="AddTextureAtlasData()"/>
        /// <see cref="RemoveTextureAtlasData()"/>
        /// <see cref="TextureAtlasData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public List<TextureAtlasData> GetTextureAtlasData(string name)
        {
            return textureAtlasDataMap.ContainsKey(name) ? textureAtlasDataMap[name] : null;
        }
        /// <summary>
        /// - Cache a TextureAtlasData instance to the factory.
        /// </summary>
        /// <param name="data">- The TextureAtlasData instance.</param>
        /// <param name="name">- Specify a cache name for the instance so that the instance can be obtained through name. (if not set, use the instance name instead)</param>
        /// <see cref="ParseAndAddTextureAtlasData"/>
        /// <see cref="GetTextureAtlasData()"/>
        /// <see cref="RemoveTextureAtlasData()"/>
        /// <see cref="TextureAtlasData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void AddTextureAtlasData(TextureAtlasData data, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = data.name;
            }

            if (textureAtlasDataMap.TryAdd(name, new List<TextureAtlasData>()) || !textureAtlasDataMap[name].Contains(data))
            {
                textureAtlasDataMap[name].Add(data);
            }
        }
        /// <summary>
        /// - Remove a TextureAtlasData instance.
        /// </summary>
        /// <param name="name">- The TextureAtlasData instance cache name.</param>
        /// <param name="disposeData">- Whether to dispose data.</param>
        /// <see cref="ParseAndAddTextureAtlasData"/>
        /// <see cref="GetTextureAtlasData()"/>
        /// <see cref="AddTextureAtlasData()"/>
        /// <see cref="TextureAtlasData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public virtual void RemoveTextureAtlasData(string name, bool disposeData = true)
        {
            if (textureAtlasDataMap.ContainsKey(name))
            {
                var textureAtlasDataList = textureAtlasDataMap[name];
                if (disposeData)
                {
                    foreach (var textureAtlasData in textureAtlasDataList)
                    {
                        DB.Kernel.BufferObject(textureAtlasData);
                    }
                }

                textureAtlasDataMap.Remove(name);
            }
        }
        /// <summary>
        /// - Get a specific armature data.
        /// </summary>
        /// <param name="name">- The armature data name.</param>
        /// <param name="dragonBonesName">- The cached name for DragonbonesData instance.</param>
        /// <see cref="ArmatureData"/>
        /// <version>DragonBones 5.1</version>
        /// <language>en_US</language>
        public virtual ArmatureData GetArmatureData(string name, string dragonBonesName = "")
        {
            var dataPackage = new BuildArmaturePackage();
            if (!FillBuildArmaturePackage(dataPackage, dragonBonesName, name, "", ""))
            {
                return null;
            }

            return dataPackage.ArmatureData;
        }
        /// <summary>
        /// - Clear all cached DragonBonesData instances and TextureAtlasData instances.
        /// </summary>
        /// <param name="disposeData">- Whether to dispose data.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public virtual void Clear(bool disposeData = true)
        {
            if (disposeData)
            {
                foreach (var dragonBoneData in dragonBonesDataMap.Values)
                {
                    DB.Kernel.BufferObject(dragonBoneData);
                }

                foreach (var textureAtlasDatas in textureAtlasDataMap.Values)
                {
                    foreach (var textureAtlasData in textureAtlasDatas)
                    {
                        DB.Kernel.BufferObject(textureAtlasData);
                    }
                }
            }

            dragonBonesDataMap.Clear();
            textureAtlasDataMap.Clear();
        }

        public bool FillBuildArmaturePackage(BuildArmaturePackage dataPackage,
                                                string dbProjectName,
                                                string armatureName,
                                                string skinName,
                                                string textureAtlasName)
        {
            DBProjectData dbProjectData = null;
            ArmatureData armatureData = null;

            bool isAvailableDBProjectName = !string.IsNullOrEmpty(dbProjectName);
            
            //trying to find data by names
            if (isAvailableDBProjectName)
            {
                if (dragonBonesDataMap.TryGetValue(dbProjectName, out dbProjectData))
                {
                    if (!dbProjectData.TryGetArmatureData(armatureName, out armatureData))
                    {
                        DBLogger.Warn($"No {nameof(ArmatureData)} with name {armatureName} found");
                    }
                }
                else
                {
                    DBLogger.Warn($"No {nameof(DBProjectData)} with name {dbProjectName} found");
                    return false;
                }
            }
            else 
            {
                armatureData = SearchForArmature(armatureName);
                
                if (armatureData == null)
                {
                    DBLogger.Warn($"Auto search failed. No {nameof(ArmatureData)} with name {armatureName} found");
                }
                
                dbProjectData = armatureData.parent;
            }

            //when data found, we can fill build data package
            
            dataPackage.DataName = dbProjectName;
            dataPackage.TextureAtlasName = textureAtlasName;
            
            dataPackage.DBProjectData = dbProjectData;
            dataPackage.ArmatureData = armatureData;
            
            dataPackage.Skin = null;

            if (!string.IsNullOrEmpty(skinName))
            {
                dataPackage.Skin = armatureData.GetSkin(skinName);

                if (dataPackage.Skin == null)
                {
                    DBLogger.Warn($"No {nameof(SkinData)} with name {skinName} found. Default skin will be used");
                    dataPackage.Skin = armatureData.defaultSkin;
                }
            }

            if (dataPackage.Skin == null)
            {
                dataPackage.Skin = armatureData.defaultSkin;
            }
            return true;
        }

        private ArmatureData SearchForArmature(string armatureName)
        {
            foreach (DBProjectData anotherDBProjectData in dragonBonesDataMap.Values)
            {
                if (anotherDBProjectData.TryGetArmatureData(armatureName, out ArmatureData armatureData))
                {
                    return armatureData;
                }
            }

            return null;
        }

        public Dictionary<string, List<TextureAtlasData>> GetAllTextureAtlases()
        {
            return textureAtlasDataMap;
        }

        public Dictionary<string, DBProjectData> GetAllDragonBonesData()
        {
            return dragonBonesDataMap;
        }
    }
}