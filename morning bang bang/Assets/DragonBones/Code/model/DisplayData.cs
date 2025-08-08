using System.Collections.Generic;

namespace DragonBones
{
    /// <internal/>
    /// <private/>
    public class VerticesData
    {
        public bool isShared;
        public bool inheritDeform;
        public int offset;
        public DBProjectData data;
        public WeightData weight; // Initial value.

        public void Clear()
        {
            if (!this.isShared && this.weight != null)
            {
                this.weight.ReturnToPool();
            }

            this.isShared = false;
            this.inheritDeform = false;
            this.offset = 0;
            this.data = null;
            this.weight = null;
        }

        public void ShareFrom(VerticesData value)
        {
            this.isShared = true;
            this.offset = value.offset;
            this.weight = value.weight;
        }
    }

    public abstract class DisplayData : DBObject
    {
        public DisplayType type;
        public string Name;
        public string path;
        public SkinData parent;
        public readonly DBTransform DBTransform = new DBTransform();

        protected override void ClearObject()
        {
            this.Name = "";
            this.path = "";
            this.DBTransform.Identity();
            this.parent = null; //
        }
    }

    /// <internal/>
    /// <private/>
    public class ImageDisplayData : DisplayData
    {
        public readonly Point pivot = new Point();
        public TextureData texture = null;

        protected override void ClearObject()
        {
            base.ClearObject();

            this.type = DisplayType.Image;
            this.pivot.Clear();
            this.texture = null;
        }
    }

    public class ChildArmatureDisplayData : DisplayData
    {
        public bool inheritAnimation;
        public readonly List<ActionData> actions = new List<ActionData>();
        public ArmatureData armature = null;

        protected override void ClearObject()
        {
            base.ClearObject();

            foreach (var action in this.actions)
            {
                action.ReturnToPool();
            }

            this.type = DisplayType.Armature;
            this.inheritAnimation = false;
            this.actions.Clear();
            this.armature = null;
        }

        internal void AddAction(ActionData value)
        {
            this.actions.Add(value);
        }
    }

    /// <internal/>
    /// <private/>
    public class MeshDisplayData : DisplayData
    {
        public readonly VerticesData vertices = new VerticesData();
        public TextureData texture;

        protected override void ClearObject()
        {
            base.ClearObject();

            this.type = DisplayType.Mesh;
            this.vertices.Clear();
            this.texture = null;
        }
    }

    /// <internal/>
    /// <private/>
    public class BoundingBoxDisplayData : DisplayData
    {
        public BoundingBoxData boundingBox = null; // Initial value.

        protected override void ClearObject()
        {
            base.ClearObject();

            if (this.boundingBox != null)
            {
                this.boundingBox.ReturnToPool();
            }

            this.type = DisplayType.BoundingBox;
            this.boundingBox = null;
        }
    }
    
    /// <internal/>
    /// <private/>
    public class PathDisplayData : DisplayData
    {
        public bool closed;
        public bool constantSpeed;
        public readonly VerticesData vertices = new VerticesData();
        public readonly List<float> curveLengths = new List<float>();

        protected override void ClearObject()
        {
            base.ClearObject();

            this.type = DisplayType.Path;
            this.closed = false;
            this.constantSpeed = false;
            this.vertices.Clear();
            this.curveLengths.Clear();
        }
    }

    /// <internal/>
    /// <private/>
    public class WeightData : DBObject
    {
        public int count;
        public int offset; // IntArray.
        public readonly List<BoneData> bones = new List<BoneData>();

        protected override void ClearObject()
        {
            this.count = 0;
            this.offset = 0;
            this.bones.Clear();
        }

        internal void AddBone(BoneData value)
        {
            this.bones.Add(value);
        }
    }
}
