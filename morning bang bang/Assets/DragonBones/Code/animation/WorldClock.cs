using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class WorldClock
    {
        public float Time { get; private set; } = 0.0f; //in seconds
        private readonly List<IAnimatable> rootArmatures = new();

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
            
            ProcessRegistry();
            
            if(rootArmatures != null && rootArmatures.Count > 0)
            {
                foreach (IAnimatable armature in rootArmatures)
                {
                    armature.AdvanceTime(passedTime);
                }
            }
            
            DB.Registry.CommitRuntimeChanges();
        }

        private void ForceToClear()
        {
            rootArmatures.Clear();
        }
        
        private void ProcessRegistry()
        {
            if (!DB.Registry.IsBufferEmpty())
            {
                DB.Registry.ProcessChangedRoots((change, armature) =>
                {
                    switch (change)
                    {
                        case DBRegistry.RegistryChange.Added:
                            rootArmatures.Add(armature);
                            break;
                        case DBRegistry.RegistryChange.Removed:
                            rootArmatures.Remove(armature);
                            break;
                    }
                });
                
                DB.Registry.CommitRuntimeChanges();
            }
        }
    }
}