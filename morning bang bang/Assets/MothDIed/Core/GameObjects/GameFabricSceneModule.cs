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

        public virtual void OnGameObjectInstantiated(GameObject instance) { }

        public virtual void OnDestroyed(Object toDestroy) { }
        public virtual void BeforeGameObjectDestroyed(GameObject instance) { }
    }
}
