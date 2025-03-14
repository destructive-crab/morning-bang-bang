using banging_code.common;
using MothDIed;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace banging_code.camera_logic
{
    public sealed class CCamera : SceneModule
    {
        private CCameraInstance instance;
        private Transform currentTarget;

        public override void PrepareModule(Scene scene)
        {
            CreateInstance();
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;
            instance.PlayerCamera.Follow = currentTarget;
        }

        private void CreateInstance()
        {
            if (instance != null) return;
            
            CCameraInstance prefab = Resources.Load<CCameraInstance>(PTH.CCameraInstance);
            instance = Game.CurrentScene.Fabric.Instantiate(prefab, Vector2.zero, null);
        }
    }
}