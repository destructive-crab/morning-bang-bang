using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The DragonBones data.
    /// A DragonBones data contains multiple armature data
    /// </summary>
    /// <see cref="ArmatureData"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public sealed class DBProjectData : DBObject
    {
        /// <summary>
        /// - The animation frame rate.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public uint frameRate;
        
        /// <summary>
        /// - The data version.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string version;

        /// <summary>
        /// - The DragonBones data name.
        /// The name is consistent with the DragonBones project name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name;
        
        public ArmatureData stage;
        public readonly List<uint> frameIndices = new();
        public readonly List<float> cachedFrames = new();

        /// <summary>
        /// - All armature data names.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string[] ArmatureNames => armatureNames.ToArray();
        
        private readonly List<string> armatureNames = new();
        private readonly Dictionary<string, ArmatureData> armatures = new();

        internal byte[] binary;
        internal short[] intArray;
        internal float[] floatArray;
        internal short[] frameIntArray;
        internal float[] frameFloatArray;
        internal short[] frameArray;
        internal ushort[] timelineArray;
        
        internal UserData userData = null; // Initial value.

        public override void OnReleased()
        {
            foreach (var k in armatures.Keys)
            {
                armatures[k].ReleaseThis();
            }

            if (userData != null)
            {
                userData.ReleaseThis();
            }

            frameRate = 0;
            version = "";
            name = "";
            stage = null;
            frameIndices.Clear();
            cachedFrames.Clear();
            armatureNames.Clear();
            armatures.Clear();
            binary = null;
            intArray = null; 
            floatArray = null; 
            frameIntArray = null; 
            frameFloatArray = null; 
            frameArray = null; 
            timelineArray = null; 
            userData = null;
        }

        public void AddArmature(ArmatureData value)
        {
            if (armatures.ContainsKey(value.name))
            {
                armatures[value.name].ReleaseThis();
            }

            value.belongsToProject = this;
            armatures[value.name] = value;
            armatureNames.Add(value.name);
        }

        /// <summary>
        /// - Get a specific armature data.
        /// </summary>
        /// <param name="armatureName">- The armature data name.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public ArmatureData GetArmatureData(string armatureName)
        {
            if (armatures.ContainsKey(armatureName)) return armatures[armatureName];
            else return null;
        }

        public bool TryGetArmatureData(string armatureName, out ArmatureData data) => armatures.TryGetValue(armatureName, out data);
    }
}
