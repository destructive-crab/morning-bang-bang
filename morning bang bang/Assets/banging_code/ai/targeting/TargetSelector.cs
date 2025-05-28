using System;
using System.Collections.Generic;
using banging_code.common;
using banging_code.level.entity_locating;
using MothDIed;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.ai.targeting
{
    public class TargetSelector : MonoSystem
    {
        [Inject] private LocationManager locationManager;
        
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

        public override bool EnableOnStart() => true;

        public override void Enable()
        {
            locationManager.OnLocationEntitesChanged += CheckLocation;
        }

        public override void Dispose()
        {
            locationManager.OnLocationEntitesChanged -= CheckLocation;
        }

        private void CheckLocation(ID locationID)
        {
            if (locationID != locationManager.GetLocationOf(Owner)) return;
            
            var entities = locationManager.GetEntitesFrom(locationID);
            var previousBest = BestTarget;
            sortedTargets.Clear();
            
            foreach (MonoEntity entity in entities)
            {
                if (entity.TryGetComponent<TargetToEntities>(out TargetToEntities target))
                {
                    Debug.Log(target.name);
                    sortedTargets.Add(target);
                }
            }
            if(sortedTargets.Count > 1) sortedTargets.Sort();           
            if(previousBest != BestTarget) OnBestTargetChanged?.Invoke(BestTarget);
        }
    }
}