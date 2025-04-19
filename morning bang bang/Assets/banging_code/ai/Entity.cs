using System;
using System.Collections.Generic;
using MothDIed;
using UnityEngine;

namespace banging_code.ai
{
    public abstract class Entity : DepressedBehaviour
    {
        protected abstract IEnumerable<RequireIn> Require();

        public void SetupEntity()
        {
            var requirements = Require();

            foreach (var require in requirements)
            {
                var fullPath = require.path.Split("/");
                
                Transform current = transform;
                
                foreach(var next in fullPath)
                {
                    var step = current.Find(next);

                    if (step == null)
                    {
                        step = new GameObject(next).transform;
                        step.parent = current;
                    }

                    current = step;
                }

                List<Component> components = new();
                foreach (var component in require.components)
                {
                    components.Add(current.gameObject.AddComponent(component));
                    if (require.addToCached) CachedComponents.Register(components[0]);
                }
                
                require.configureEach?.Invoke(components.ToArray());
                require.final?.Invoke(current.gameObject);
            }
        }

        public void Awake()
        {
            SetupEntity();
        }

        public abstract void GoSleep();
        public abstract void WakeUp();
        public abstract void Tick();
        
        protected class RequireIn
        {
            public string path;
            public Type[] components;
            
            public Action<Component[]> configureEach;
            public Action<GameObject> final;
            
            public bool addToCached;

            public RequireIn PathToObject(string path)
            {
                this.path = path;
                return this;
            }
            
            public RequireIn WithComponents(bool addToCached, params Type[] components)
            {
                this.components = components;
                this.addToCached = addToCached;
                return this;
            }

            public RequireIn ForEachComponent(Action<Component[]> forEachComponent)
            {
                configureEach = forEachComponent;
                return this;
            }

            public RequireIn Final(Action<GameObject> finalConfiguration)
            {
                final = finalConfiguration;
                
                return this;
            }
            
        }
    }
}