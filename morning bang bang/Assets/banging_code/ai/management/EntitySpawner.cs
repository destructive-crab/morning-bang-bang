using MothDIed;
using UnityEngine;

namespace banging_code.ai
{
    public abstract class EntitySpawner : MonoBehaviour
    {
        public abstract MonoEntity[] Spawn();
    }
}