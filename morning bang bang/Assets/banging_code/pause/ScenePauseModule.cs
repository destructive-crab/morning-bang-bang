using banging_code.common;
using MothDIed;
using MothDIed.InputsHandling;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace banging_code.pause
{
    public class ScenePauseModule : SceneModule
    {
        private PauseMenu pauseMenu;

        public override void StartModule(Scene scene)
        {
            base.StartModule(scene);
            
            if (pauseMenu == null)
            {
                pauseMenu = scene.Fabric.Instantiate(GetPauseMenuPrefab(), Vector3.zero);
            }
            
            InputService.OnPauseButtonPressed += Game.G<PauseSystem>().SwitchPause;
            
            Game.G<PauseSystem>().OnPauseSwitched += SwitchPause;
        }

        public override void StopModule(Scene scene)
        {
            InputService.OnPauseButtonPressed -= Game.G<PauseSystem>().SwitchPause;
            Game.G<PauseSystem>().OnPauseSwitched -= SwitchPause;
        }

        public void SwitchPause(bool isPaused)
        {
            pauseMenu.gameObject.SetActive(isPaused);
            
            switch (isPaused)
            {
                case true:
                    InputService.SwitchTo(InputService.Mode.UI);
                    break;
                case false:
                    InputService.BackToPreviousMode();
                    break;
            }
        }

        private PauseMenu GetPauseMenuPrefab()
        {
            return Resources.Load<PauseMenu>(PTH.PauseMenuPrefab);
        }
    }
}