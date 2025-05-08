using banging_code.common;
using UnityEngine;

namespace banging_code.ai.targeting
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class EntityFieldOfView : TriggerCollector<TargetToEntities>
    {
    }
}