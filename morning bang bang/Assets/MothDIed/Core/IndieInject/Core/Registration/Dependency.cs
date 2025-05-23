using System;

namespace MothDIed.DI
{
    public sealed class Dependency : IDisposable
    {
        public readonly Type Type;
        public readonly bool IsSingleton;
        
        private readonly Func<object> fabric;
        private object instance;

        public Dependency(Type type, Func<object> fabric, bool isSingleton)
        {
            Type = type;
            this.fabric = fabric;
            IsSingleton = isSingleton;
        }

        public void Dispose()
        {
            if (IsSingleton && instance is IDisposable disposable)
            {
                disposable.Dispose();   
            }
        }

        public object GetInstance()
        {
            if (IsSingleton && instance == null)
            {
                instance = fabric.Invoke();
            }
            else if (IsSingleton)
            {
                if (instance == null)
                {
                    throw new DIProvideException($"Your provider of type {Type} returns null");
                }
                
                return instance;
            }

            object newInstance = fabric.Invoke();
            
            if(newInstance == null)
            {
                throw new DIProvideException($"Your provider of {Type} provides null");
            }

            return fabric.Invoke();
        }
    }
}