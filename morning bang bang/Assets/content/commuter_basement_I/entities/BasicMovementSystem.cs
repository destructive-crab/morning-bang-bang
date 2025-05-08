using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using banging_code.common;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BasicMovementSystem : MonoSystem, IEntityAISystem
    {
        [Inject] private Rigidbody2D rigidbody;
        [Inject] private Flipper flipper;
        [Inject] private EntityPath currentPath;
        
        public float Speed = 7;

        public BasicMovementSystem()
        {
            Speed = UnityEngine.Random.Range(450, 650)/100f;
        }

        public override bool EnableOnStart()
        {
            return true;
        }

        public override void Update()
        {
            if (currentPath.Path != null && !currentPath.Path.Completed)
            {
                flipper?.FaceToPoint(currentPath.Path.CurrentPoint);
                rigidbody.MovePosition(Vector3.MoveTowards(rigidbody.position, currentPath.Path.CurrentPoint, Time.deltaTime *  Speed));

                if (Vector3.Distance(rigidbody.position, currentPath.Path.CurrentPoint) < 0.01f)
                {
                    currentPath.Path.Next();
                }
            }
        }
    }
}