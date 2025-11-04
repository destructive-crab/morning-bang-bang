using DragonBones;
using UnityEngine;
using UnityEngine.Rendering;

namespace DragonBonesBridge
{
    [RequireComponent(typeof(UnityArmatureRoot))]
    [RequireComponent(typeof(SortingGroup))]
    public class ArmatureController : MonoBehaviour
    {
        public string CurrentAnimation => Root.AnimationPlayer.lastAnimationName;
        public SortingGroup SortingGroup;
        internal UnityArmatureRoot Root;

        private void Awake()
        {
            Root = GetComponent<UnityArmatureRoot>();
            SortingGroup = GetComponent<SortingGroup>();
        }

        public void Play(string animationName)
        {
            Root.AnimationPlayer.Play(animationName);
        }
        
        public void SetOrderInLayer(int order)
        {
            SortingGroup.sortingOrder = order;
        }

        public void SetLayer(string layer)
        {
            SortingGroup.sortingLayerName = layer;
        }
    }
}