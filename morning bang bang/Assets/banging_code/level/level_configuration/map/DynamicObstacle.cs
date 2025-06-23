using banging_code.common;
using UnityEngine;

namespace banging_code.ai.pathfinding
{
    public class DynamicObstacle : MonoBehaviour
    {
        public ID ID;

        private void Awake()
        {
            ID = new ID("dyn_obs");
        }
    }
}