using System;
using banging_code.common;
using Cysharp.Threading.Tasks;
using MothDIed;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using Unity.Cinemachine;
using UnityEngine;

namespace banging_code.camera_logic
{
    public sealed class CCamera : SceneModule
    {
        private CCameraInstance instance;
        private Transform currentTarget;

        private CinemachineVirtualCameraBase current;
        public override void PrepareModule(Scene scene)
        {
            CreateInstance();
        }

        public void SetTarget(Transform target)
        {
            currentTarget = target;
            instance.ChillCamera.Follow = currentTarget;
        }

        public void EnterBangCamera(PolygonCollider2D roomCollider)
        {
            if(current != null) current.gameObject.SetActive(false);
            
            current = instance.BangCamera;
            current.Follow = currentTarget;
            current.GetComponent<CinemachineConfiner2D>().BoundingShape2D = roomCollider;
            current.gameObject.SetActive(true);
        }
        
        public void EnterChillCamera()
        {
            if(current != null) current.gameObject.SetActive(false);
            
            current = instance.ChillCamera;
            current.Follow = currentTarget;
            current.gameObject.SetActive(true);
        }
        public async void Shake(float intensity, float time)
        {
            var shake = current.GetComponent<CinemachineBasicMultiChannelPerlin>();

            shake.AmplitudeGain = intensity;
            shake.FrequencyGain = 1;

            await UniTask.Delay(TimeSpan.FromSeconds(time));

            shake.AmplitudeGain = 0;
            shake.FrequencyGain = 0;
        }
        
        private void CreateInstance()
        {
            if (instance != null) return;
            
            CCameraInstance prefab = Resources.Load<CCameraInstance>(PTH.CCameraInstance);
            instance = Game.CurrentScene.Fabric.Instantiate(prefab, Vector2.zero, null);
        }
    }
}