using System;
using UnityEngine;
using UnityEngine.UI;

namespace MothDIed.GUI
{
    [RequireComponent(typeof(Button))]
    public sealed class ReturnToPreviousButton : MonoBehaviour
    {
        private SceneGUIModule sceneGUIModule;
        
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(ReturnToPrevious);

            if (Game.SceneSwitcher.CurrentScene.Modules.TryGetModule(out SceneGUIModule sceneGUI))
            {
                this.sceneGUIModule = sceneGUI;
            }
#if UNITY_EDITOR
            else
            {
                throw new Exception($"No module of type {typeof(SceneGUIModule)}");
            }
#endif
        }

        public void ReturnToPrevious()
        {
            sceneGUIModule.BackToPrevious();
        }
    }

}