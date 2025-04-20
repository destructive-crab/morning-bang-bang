using UnityEngine;

namespace banging_code.interactions
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class InteractorFoV : Trigger<IInteraction>
    { }
}