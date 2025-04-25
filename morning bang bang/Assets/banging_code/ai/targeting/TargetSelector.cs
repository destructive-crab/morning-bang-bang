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
            sortedTargets.Sort();           
            if(previousBest != BestTarget) OnBestTargetChanged?.Invoke(previousBest);
        }

        private void OnTargetDisappear(TargetToEntities other)
        {
            sortedTargets.Remove(other);
            var previousBest = BestTarget;
            sortedTargets.Sort();
            
            if(previousBest != BestTarget) OnBestTargetChanged?.Invoke(previousBest);
        }
    }
}