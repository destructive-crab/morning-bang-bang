using System;
using Cysharp.Threading.Tasks;
using MothDIed.Core.GameObjects.Pool;
using UnityEngine;
using Object = UnityEngine.Object;

namespace banging_code.debug
{
    public class DebugPoolFabric : PoolFabric
    {
        public DebugPoolFabric() : base(true, false) { }

        public override TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation, Transform parent)
        {
            return GameObject.Instantiate(original, position, rotation, parent);
        }

        public override void Destroy(Object toDestroy)
        {
            GameObject.Destroy(toDestroy);
        }
    }
}