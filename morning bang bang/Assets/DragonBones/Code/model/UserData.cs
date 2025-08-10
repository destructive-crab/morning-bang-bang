using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The user custom data.
    /// </summary>
    /// <version>DragonBones 5.0</version>
    /// <language>en_US</language>
    public class UserData : DBObject
    {
        /// <summary>
        /// - The custom int numbers.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public readonly List<int> ints = new List<int>();
        /// <summary>
        /// - The custom float numbers.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public readonly List<float> floats = new List<float>();
        /// <summary>
        /// - The custom strings.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public readonly List<string> strings = new List<string>();

        /// <inheritDoc/>
        public override void OnReleased()
        {
            this.ints.Clear();
            this.floats.Clear();
            this.strings.Clear();
        }

        /// <internal/>
        /// <private/>
        internal void AddInt(int value)
        {
            this.ints.Add(value);
        }
        /// <internal/>
        /// <private/>
        internal void AddFloat(float value)
        {
            this.floats.Add(value);
        }
        /// <internal/>
        /// <private/>
        internal void AddString(string value)
        {
            this.strings.Add(value);
        }

        /// <summary>
        /// - Get the custom int number.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public int GetInt(int index = 0)
        {
            return index >= 0 && index < this.ints.Count ? this.ints[index] : 0;
        }
        /// <summary>
        /// - Get the custom float number.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float GetFloat(int index = 0)
        {
            return index >= 0 && index < this.floats.Count ? this.floats[index] : 0.0f;
        }
        /// <summary>
        /// - Get the custom string.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public string GetString(int index = 0)
        {
            return index >= 0 && index < this.strings.Count ? this.strings[index] : string.Empty;
        }
    }

    /// <internal/>
    /// <private/>
    public class ActionData : DBObject
    {
        public ActionType type;
        // Frame event name | Sound event name | Animation name
        public string name; 
        public BoneData bone;
        public SlotData slot;
        public UserData data;

        public override void OnReleased()
        {
            if (this.data != null)
            {
                this.data.ReleaseThis();
            }

            this.type = ActionType.Play;
            this.name = "";
            this.bone = null;
            this.slot = null;
            this.data = null;
        }
    }
}
