using System.Collections;
using banging_code.ai.pathfinding;
using banging_code.ai.targeting;
using banging_code.common;
using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BasicMovementSystem : Extension
    {
        [Inject] private TargetSelector targetSelector;
        [Inject] private Rigidbody2D rigidbody;
        [Inject] private Flipper flipper;

        public bool IsMoving => pathUpdateCoroutine != null;
        
        public float Speed = 7;
        
        private readonly Pathfinder pathfinder = new();
        private Pathfinder.Path currentPath;

        private const float PATH_UPDATE_RATE = 0.01f;
        private Coroutine pathUpdateCoroutine = null;

        public BasicMovementSystem()
        {
            Speed = UnityEngine.Random.Range(450, 650)/100f;
        }

        public void StartMoving()
        {
#if UNITY_EDITOR
             if(targetSelector.BestTarget == null)
             {
                 Debug.LogWarning("[BASIC ENTITY TO TARGET MOVEMENT : START MOVING] CAN NOT START MOVING WHEN THERE IS NO TARGET");
                 return;
             }           
#endif

            currentPath = pathfinder.FindPath(Owner.transform.position, targetSelector.BestTarget.Position);
            pathUpdateCoroutine = Owner.StartCoroutine(UpdatePath());
        }

        public void StopMoving()
        {
#if UNITY_EDITOR
            if (currentPath == null)
            {                
                Debug.LogWarning("[BASIC ENTITY TO TARGET MOVEMENT : STOP MOVING] ENTITY IS NOT MOVING");
            }
#endif
            Owner.StopCoroutine(pathUpdateCoroutine);
            pathUpdateCoroutine = null;
            currentPath = null;
        }

        public override void Update()
        {
            if (currentPath != null && !currentPath.Completed)
            {
                flipper?.FaceToPoint(currentPath.CurrentPoint);
                rigidbody.MovePosition(Vector3.MoveTowards(rigidbody.position, currentPath.CurrentPoint, Time.deltaTime *  Speed));

                if (Vector3.Distance(rigidbody.position, currentPath.CurrentPoint) < 0.01f)
                {
                    currentPath.Next();
                }
            }
        }

        private IEnumerator UpdatePath()
        {
            while (Owner != null && Owner.enabled)
            {
                yield return new WaitForSeconds(PATH_UPDATE_RATE);
                
                if(targetSelector.BestTarget == null)
                {
                    currentPath = null;
                    continue;
                }

                currentPath = pathfinder.FindPath(Owner.transform.position, targetSelector.BestTarget.Position);
            }
        }
        
    }
}