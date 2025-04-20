using System.Collections.Generic;
using MothDIed.DI;
using MothDIed.ExtensionSystem;

namespace banging_code.ai.targeting
{
    public class TargetSelector : Extension
    {
        [Inject] private EntityFieldOfView fov;
        
        public TargetToEntities BestTarget => sortedTargets[0];
        private List<TargetToEntities> sortedTargets = new();

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

        private void OnTargetDisappear(TargetToEntities other)
        {
            sortedTargets.Add(other);
            sortedTargets.Sort();
        }

        private void OnNewTargetAppear(TargetToEntities other)
        {
             sortedTargets.Remove(other);
             sortedTargets.Sort();           
        }
    }
}