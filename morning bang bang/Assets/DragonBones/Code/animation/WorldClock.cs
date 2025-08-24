using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class WorldClock
    {
        public float Time { get; private set; } = 0.0f; //in seconds
        private IAnimatable[] animatables;

        public float TimeScale
        {
            get { return timeScale; }
            set => timeScale = Mathf.Clamp(value, 0f, 1f);
        }
        private float timeScale = 1.0f;
        
        private float systemTime = 0.0f;

        public WorldClock(float time = -1.0f)
        {
            Time = time;
            systemTime = DateTime.Now.Ticks * 0.01f * 0.001f;
        }
        
        public void AdvanceTime(float passedTime)
        {
            if (float.IsNaN(passedTime)) { passedTime = 0.0f; }

            float currentTime = DateTime.Now.Ticks * 0.01f * 0.001f;
            if (passedTime < 0.0f) { passedTime = currentTime - systemTime; }
            systemTime = currentTime;
            
            passedTime *= TimeScale;

            if (passedTime == 0.0f) { return; }

            passedTime = Mathf.Abs(passedTime);
            Time += passedTime;
            
            if (DB.Registry.RegistryChangedOnPreviousFrame)
            {
                DBRegistry.DBID[] armatures = DB.Registry.GetAllRootArmatures();
                
                if(animatables == null || animatables.Length != armatures.Length)
                {
                    animatables = new IAnimatable[armatures.Length];
                }

                for (int i = 0; i < armatures.Length; i++)
                {
                    animatables[i] = DB.Registry.GetArmature(armatures[i]);
                }
            }
            
            if(animatables != null && animatables.Length > 0)
            {
                foreach (IAnimatable animatable in animatables)
                {
                    animatable.AdvanceTime(passedTime);
                }
            }

            DB.Registry.MarkAsUnchanged();
        }
    }
}