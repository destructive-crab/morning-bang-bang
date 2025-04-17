using Cysharp.Threading.Tasks;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace MothDIed.Core.GameObjects
{
    public class GameFabricSceneModule : SceneModule
    {
        public override void PrepareModule(Scene scene)
            => scene.Fabric.RefreshModules();

        public virtual void OnInstantiated<TObject>(TObject instance) where TObject : Object { }
        public virtual void OnWillBeDestroyed(Object toDestroy) { }
    }
}
