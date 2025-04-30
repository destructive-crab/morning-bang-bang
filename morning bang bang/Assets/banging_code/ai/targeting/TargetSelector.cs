using System;
using System.Collections.Generic;
using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace banging_code.ai.targeting
{
    public class TargetSelector : Extension
    {
        [Inject] private EntityFieldOfView fov;
        
        public TargetToEntities BestTarget
        {
            get
            {
                if (sortedTargets.Count == 0) return null;
                return sortedTargets[0];
            }
        }

        public event Action<TargetToEntities> OnBestTargetChanged;
        private readonly List<TargetToEntities> sortedTargets = new();
        
        public override void StartExtension()
        {
            fov.OnEnter += OnNewTargetAppear;
            fov.OnExit += OnTargetDisappear;
        }

        public override void Dispose()
        {
            fov.OnEnter -= OnNewTargetAppear;
            fov.OnExit -= OnTargetDisappear;
        }

        private void OnNewTargetAppear(TargetToEntities other)
        {
            var previousBest = BestTarget;
            sortedTargets.Add(other);
            if(sortedTargets.Count > 1) sortedTargets.Sort();           
            if(previousBest != BestTarget) OnBestTargetChanged?.Invoke(BestTarget);
        }

        private void OnTargetDisappear(TargetToEntities other)
        {
            var previousBest = BestTarget;
            
            sortedTargets.Remove(other);
            sortedTargets.Sort();
            
            if(previousBest != BestTarget) OnBestTargetChanged?.Invoke(BestTarget);
        }
    }
}