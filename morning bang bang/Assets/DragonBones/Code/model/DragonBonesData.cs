using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The DragonBones data.
    /// A DragonBones data contains multiple armature data.
    /// </summary>
    /// <see cref="DBKernel.ArmatureData"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class DragonBonesData : BaseObject
    {
        /// <private/>
        public bool autoSearch;
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

        public uint PixelsPerUnit = 512;
        
        public string name;
        /// <private/>
        public ArmatureData stage;
        /// <internal/>
        /// <private/>
        public readonly List<uint> frameIndices = new List<uint>();
        /// <internal/>
        /// <private/>
        public readonly List<float> cachedFrames = new List<float>();
        /// <summary>
        /// - All armature data names.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public readonly List<string> armatureNames = new List<string>();
        /// <private/>
        public readonly Dictionary<string, ArmatureData> armatures = new Dictionary<string, ArmatureData>();
        /// <internal/>
        /// <private/>
        internal byte[] binary;
        /// <internal/>
        /// <private/>
        internal short[] intArray;
        /// <internal/>
        /// <private/>
        internal float[] floatArray;
        /// <internal/>
        /// <private/>
        internal short[] frameIntArray;
        /// <internal/>
        /// <private/>
        internal float[] frameFloatArray;
        /// <internal/>
        /// <private/>
        internal short[] frameArray;
        /// <internal/>
        /// <private/>
        internal ushort[] timelineArray;
        /// <private/>
        internal UserData userData = null; // Initial value.

        /// <inheritDoc/>
        protected override void _OnClear()
        {
            foreach (var k in this.armatures.Keys)
            {
                this.armatures[k].ReturnToPool();
            }

            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.autoSearch = false;
            this.frameRate = 0;
            this.version = "";
            this.name = "";
            this.stage = null;
            this.frameIndices.Clear();
            this.cachedFrames.Clear();
            this.armatureNames.Clear();
            this.armatures.Clear();
            this.binary = null;
            this.intArray = null; //
            this.floatArray = null; //
            this.frameIntArray = null; //
            this.frameFloatArray = null; //
            this.frameArray = null; //
            this.timelineArray = null; //
            this.userData = null;
            this.PixelsPerUnit = 512;
        }

        /// <internal/>
        /// <private/>
        public void AddArmature(ArmatureData value)
        {
            if (this.armatures.ContainsKey(value.name))
            {
                Helper.Assert(false, "Same armature: " + value.name);
                this.armatures[value.name].ReturnToPool();
            }

            value.parent = this;
            this.armatures[value.name] = value;
            this.armatureNames.Add(value.name);
        }

        /// <summary>
        /// - Get a specific armature data.
        /// </summary>
        /// <param name="armatureName">- The armature data name.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public ArmatureData GetArmature(string armatureName)
        {
            return this.armatures.ContainsKey(armatureName) ? this.armatures[armatureName] : null;
        }
    }
}
