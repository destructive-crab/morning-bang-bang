using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace banging_code.level.light
{
    [RequireComponent(typeof(Light2D))]
    public class BasementLamp : MonoBehaviour, IControllableLight
    {
        private Light2D light2D;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
        }

        public void TurnOn()
        {
            light2D.intensity = 1;
        }

        public void TurnOff()
        {
            light2D.intensity = 0;
        }
    }
}