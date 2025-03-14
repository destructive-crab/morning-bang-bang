namespace MothDIed.Scenes.SceneModules
{
    public abstract class SceneModule
    {
        //before OnSceneLoaded invocation
        public virtual void PrepareModule(Scene scene) { }
        
        //after OnSceneLoaded invocation
        public virtual void StartModule(Scene scene) { }
        
        public virtual void StopModule(Scene scene) { }
        
        public virtual void UpdateModule(Scene scene) { }
        
        public virtual void Dispose(Scene scene) { }
    }
}