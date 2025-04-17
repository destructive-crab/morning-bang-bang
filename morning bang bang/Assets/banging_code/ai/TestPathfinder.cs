using System;
using System.Collections;
using System.Collections.Generic;
using banging_code.ai.pathfinding;
using MothDIed;
using UnityEngine;

namespace banging_code.ai
{
    public class TestPathfinder : DepressedBehaviour
    {
        private readonly Pathfinder pathfinder = new();
        private List<Vector2> path;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                path = pathfinder.FindPath(transform.position, Game.RunSystem.Data.Level.PlayerInstance.transform.position);
                StartCoroutine(MOVE());
            }
        }

        private IEnumerator MOVE()
        {
            while (path.Count > 0)
            {
                var to = path[0];
                
                while (transform.position != new Vector3(to.x, to.y, 0))
                {
                    transform.position = Vector3.MoveTowards(transform.position,
                        new Vector3(to.x, to.y, 0), 5 * Time.deltaTime);
    
                    yield return new WaitForFixedUpdate();
                }
                
                path.RemoveAt(0);
            }


        }
    }
}