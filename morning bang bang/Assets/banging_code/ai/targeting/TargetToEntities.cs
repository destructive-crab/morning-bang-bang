using System;
using UnityEngine;

namespace banging_code.ai.targeting
{
    [RequireComponent(typeof(Collider2D))]
    public class TargetToEntities : MonoBehaviour, IComparable<TargetToEntities>
    {
        public Vector3 Position => transform.position;
        public int Priority = 0;
        
        public int CompareTo(TargetToEntities other)
        {
            if (other == null) return 1;
            
            if (other.Priority < Priority) return 1;
            if (other.Priority > Priority) return -1;

            return 0;
        }
    }
}