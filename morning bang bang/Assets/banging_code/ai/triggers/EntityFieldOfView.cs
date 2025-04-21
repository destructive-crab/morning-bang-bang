using System.Collections.Generic;
using UnityEngine;

namespace banging_code.ai.targeting
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class EntityFieldOfView : Trigger<TargetToEntities>
    {
        public TargetToEntities[] CurrentTargets => currentTargets.ToArray();
        private readonly List<TargetToEntities> currentTargets = new();
        public bool Contains(TargetToEntities target) => currentTargets.Contains(target);
        
        public void SetRadius(float radius) => ((CircleCollider2D)TriggerCollider()).radius = radius;

        private void Start()
        {
            OnEnter += (other) =>
            {
                currentTargets.Add(other);
            };
            
            OnExit += (other) =>
            {
                currentTargets.Remove(other);
            };
        }
    }
}