using banging_code.common;
using banging_code.player_logic;
using Cysharp.Threading.Tasks;
using MothDIed;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using Unity.Cinemachine;
using UnityEngine;
using System;

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
            if(Stopped)
            {
                return;
            }

            currentTarget = target;
            instance.ChillCamera.Follow = currentTarget;
        }

        public void EnterBangCamera(PolygonCollider2D roomCollider)
        {
            if(Stopped) return;
            
            SwitchCamera(instance.BangCamera);
            current.Follow = currentTarget;
            current.GetComponent<CinemachineConfiner2D>().BoundingShape2D = roomCollider;
        }
        
        public void EnterChillCamera()
        {
            if(Stopped) return;
            SwitchCamera(instance.ChillCamera);
            current.Follow = currentTarget;
        }
        public async void Shake(float intensity, float time)
        {
            if(Stopped) return;
            
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
            instance = Game.G<SceneSwitcher>().CurrentScene.Fabric.Instantiate(prefab, Vector2.zero, null);
        }

        public void EnterBuisnessCamera(params Transform[] targets)
        {
            if(Stopped) return;
            
            SwitchCamera(instance.BuisnessCamera);
            CinemachineTargetGroup targetGroup = current.GetComponent<CinemachineTargetGroup>();
            targetGroup.Targets.Clear();

            foreach (Transform target in targets)
            {
                CinemachineTargetGroup.Target newTarget = new CinemachineTargetGroup.Target();
                newTarget.Object = target;

                if (target.GetComponent<PlayerRoot>()) newTarget.Weight = 0.2f;
                
                targetGroup.Targets.Add(newTarget);
            }
        }

        public void SwitchCamera(CinemachineVirtualCameraBase to)
        {
            if(current != null)
            {
                current.gameObject.SetActive(false);
            }

            current = to;
            current.gameObject.SetActive(true);
        }
    }
}