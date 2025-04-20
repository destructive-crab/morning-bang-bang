using System.Collections;
using banging_code.ai.pathfinding;
using banging_code.ai.targeting;
using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BasicEntityToTargetMovement : Extension
    {
        [Inject] private TargetSelector targetSelector;
        [Inject] private Rigidbody2D rigidbody;
        
        private Pathfinder pathfinder;
        private Pathfinder.Path currentPath;
        private float speed;

        private const float PATH_UPDATE_RATE = 0.01f;
        private Coroutine pathUpdateCoroutine;

        public void StartMoving()
        {
            if(targetSelector.BestTarget == null)
            {
                Debug.LogWarning("[BASIC ENTITY TO TARGET MOVEMENT : START MOVING] CAN NOT START MOVING WHEN THERE IS NO TARGET");
                return;
            }

            currentPath = pathfinder.FindPath(Owner.transform.position, targetSelector.BestTarget.Position);
            pathUpdateCoroutine = Owner.StartCoroutine(UpdatePath());
        }

        public void StopMoving()
        {
            if (currentPath == null)
            {                
                Debug.LogWarning("[BASIC ENTITY TO TARGET MOVEMENT : STOP MOVING] ENTITY IS NOT MOVING");
                return;
            }
            
            Owner.StopCoroutine(pathUpdateCoroutine);
            currentPath = null;
        }

        public override void Update()
        {
            if (currentPath != null && !currentPath.Completed)
            {
                rigidbody.MovePosition(Vector3.MoveTowards(rigidbody.position, currentPath.CurrentPoint, Time.deltaTime *  speed));

                if (Vector3.Distance(rigidbody.position, currentPath.CurrentPoint) < 0.01f)
                {
                    currentPath.Next();
                }
            }
        }

        private IEnumerator UpdatePath()
        {
            while (Owner !=null && Owner.enabled)
            {
                yield return new WaitForSeconds(PATH_UPDATE_RATE);
                currentPath = pathfinder.FindPath(Owner.transform.position, targetSelector.BestTarget.Position);
            }
        }
        
    }
}