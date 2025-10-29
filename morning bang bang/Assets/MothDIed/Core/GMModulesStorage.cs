using System;
using System.Collections.Generic;
using MothDIed.ServiceLocators;

namespace MothDIed
{
    public sealed class GMModulesStorage : IServiceLocator
    {
        public int Count => map.Count;
        public Dictionary<Type, object> map = new();

        public List<object> all = new();

        public List<IGMModuleBoot> boots = new();
        public List<IGMModuleTick> ticks = new();
        public List<IGMModuleQuit> quits = new();

        public TModule Get<TModule>()
            where TModule : class
        {
            if (!map.ContainsKey(typeof(TModule)))
            {
                return null;
            }

            return map[typeof(TModule)] as TModule;
        }

        public void AutoRegister<TModule>(object module)
            where TModule : class
        {
            if(map.ContainsKey(typeof(TModule))) return;
            
            switch (module)
            {
                case IGMModuleBoot boot:
                    boots.Add(boot);
                    break;
                case IGMModuleTick tick:
                    ticks.Add(tick);
                    break;
                case IGMModuleQuit quit:
                    quits.Add(quit);
                    break;
            }

            all.Add(module);
            map.Add(typeof(TModule), module);
        }

        public void RegisterBootable(IGMModuleBoot boot)
        {
            boots.Add(boot);
        }

        public bool Contains(Type serviceType)
        {
            return map.ContainsKey(serviceType);
        }
        public object GetBlind(Type serviceType)
        {
            if(!map.ContainsKey(serviceType)) return null;
            return map[serviceType];
        }
    }
}