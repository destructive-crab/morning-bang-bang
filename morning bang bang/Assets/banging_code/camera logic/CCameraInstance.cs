using Unity.Cinemachine;
using UnityEngine;

namespace banging_code.camera_logic
{
    public class CCameraInstance : MonoBehaviour
    {
        /////////
        private static CCameraInstance currentInstance;
        /////////

        public Camera Camera;
        public CinemachineCamera ChillCamera;
        public CinemachineCamera BangCamera;
        public CinemachineCamera BuisnessCamera;


        private void Awake()
        {
            if (currentInstance == null) { currentInstance = this; }
            else                         { Destroy(gameObject);    }
            
            Camera = GetComponentInChildren<Camera>();
        }
    }
}