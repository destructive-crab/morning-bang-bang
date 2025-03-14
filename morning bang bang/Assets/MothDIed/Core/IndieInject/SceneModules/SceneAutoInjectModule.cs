using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace MothDIed.DI
{
    public class SceneAutoInjectModule : SceneModule
    {
        public override void StartModule(Scene scene)
        {
            var all = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var monoBehaviour in all)
            {
                Game.DIKernel.InjectWithBase(monoBehaviour);
            }
        }
    }
}