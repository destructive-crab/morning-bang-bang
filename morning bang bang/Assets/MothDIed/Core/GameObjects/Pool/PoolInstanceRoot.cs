using System.Collections.Generic;
using banging_code.common;
using UnityEngine;

namespace MothDIed.Core.GameObjects.Pool
{
    [DisallowMultipleComponent]
    public sealed class PoolInstanceRoot : MonoBehaviour
    {
        private string poolName;

        public bool Setup(string poolName)
        {
            this.poolName = poolName;
            gameObject.name = poolName;
            
            return true;
        }
    }
}