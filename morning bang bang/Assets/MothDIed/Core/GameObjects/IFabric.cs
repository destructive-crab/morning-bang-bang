using UnityEngine;

namespace MothDIed.Scenes
{
    public interface IFabric
    {
        TObject Instantiate<TObject>(TObject original)
            where TObject : Object;
        
        TObject Instantiate<TObject>(TObject original, Vector3 position)
            where TObject : Object;

        TObject Instantiate<TObject>(TObject original, Vector3 position, Transform parent)
            where TObject : Object;

        TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation)
            where TObject : Object;

        TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation,
            Transform parent)
            where TObject : Object;

        void Destroy(Object toDestroy);
    }
}