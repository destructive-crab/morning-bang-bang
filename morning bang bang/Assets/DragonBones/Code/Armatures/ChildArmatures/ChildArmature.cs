namespace DragonBones.ChildArmatures
{
    public class ChildArmature : Armature
    {
        /// <summary>
        /// - Get the parent slot which the armature belongs to. 
        /// </summary>
        /// <see cref="Slot"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public Slot Parent { get; internal set; }

        public override void OnReleased()
        {
            base.OnReleased();

            Parent = null;
        }
    }
}