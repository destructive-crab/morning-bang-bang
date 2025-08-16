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
            if (!isShared && weight != null)
            {
                weight.ReleaseThis();
            }

            isShared = false;
            inheritDeform = false;
            offset = 0;
            data = null;
            weight = null;
        }

        public void ShareFrom(VerticesData value)
        {
            isShared = true;
            offset = value.offset;
            weight = value.weight;
        }
    }

    public abstract class DisplayData : DBObject
    {
        public DisplayType Type;
        public string Name;
        public string path;
        public SkinData BelongsToSkin;
        public readonly DBTransform DBTransform = new DBTransform();

        public override void OnReleased()
        {
            Name = "";
            path = "";
            DBTransform.Identity();
            BelongsToSkin = null; 
        }
    }

    /// <internal/>
    /// <private/>
    public class ImageDisplayData : DisplayData
    {
        public readonly Point pivot = new Point();
        public TextureData texture = null;

        public override void OnReleased()
        {
            base.OnReleased();

            Type = DisplayType.Image;
            pivot.Clear();
            texture = null;
        }
    }

    public class ChildArmatureDisplayData : DisplayData
    {
        public bool inheritAnimation;
        public readonly List<ActionData> actions = new List<ActionData>();
        public ArmatureData armature = null;

        public override void OnReleased()
        {
            base.OnReleased();

            foreach (var action in actions)
            {
                action.ReleaseThis();
            }

            Type = DisplayType.Armature;
            inheritAnimation = false;
            actions.Clear();
            armature = null;
        }

        internal void AddAction(ActionData value)
        {
            actions.Add(value);
        }
    }

    /// <internal/>
    /// <private/>
    public class MeshDisplayData : DisplayData
    {
        public readonly VerticesData vertices = new VerticesData();
        public TextureData texture;

        public override void OnReleased()
        {
            base.OnReleased();

            Type = DisplayType.Mesh;
            vertices.Clear();
            texture = null;
        }
    }

    /// <internal/>
    /// <private/>
    public class BoundingBoxDisplayData : DisplayData
    {
        public BoundingBoxData boundingBox = null; // Initial value.

        public override void OnReleased()
        {
            base.OnReleased();

            if (boundingBox != null)
            {
                boundingBox.ReleaseThis();
            }

            Type = DisplayType.BoundingBox;
            boundingBox = null;
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

        public override void OnReleased()
        {
            base.OnReleased();

            Type = DisplayType.Path;
            closed = false;
            constantSpeed = false;
            vertices.Clear();
            curveLengths.Clear();
        }
    }

    /// <internal/>
    /// <private/>
    public class WeightData : DBObject
    {
        public int count;
        public int offset; // IntArray.
        public readonly List<BoneData> bones = new List<BoneData>();

        public override void OnReleased()
        {
            count = 0;
            offset = 0;
            bones.Clear();
        }

        internal void AddBone(BoneData value)
        {
            bones.Add(value);
        }
    }
}
