using System;
using System.Collections.Generic;
using MothDIed.ServiceLocators;
using UnityEngine;

namespace MothDIed.ExtensionSystem
{
    public sealed class ExtensionContainer : IServiceLocator
    {
        private DepressedBehaviour owner;

        private readonly Dictionary<Type, List<Extension>> extensions = new();

        private bool containerStarted = false;

        public void SetOwner(DepressedBehaviour owner)
        {
            if(owner == null) return;

            this.owner = owner;
        }
        
        public void StartContainer()
        {
            foreach (var extensionPair in extensions)
            {
                extensionPair.Value.ForEach((extension) =>
                {
                    Debug.Log(owner);
                    Game.DIKernel.InjectWithBaseAnd(extension, owner.CachedComponents, this);
                    extension.StartExtension();
                    extension.Enable();
                });
            }
            containerStarted = true;
        }

        ~ExtensionContainer()
        {
            foreach (var extension in extensions)
            {
                extension.Value.ForEach(extension => extension.Dispose());
            }
        }

        #region Checks

        public TExtension Get<TExtension>()
            where TExtension : Extension
        {
            return GetBlind(typeof(TExtension)) as TExtension;
        }

        public TExtension[] GetAllOf<TExtension>()
            where TExtension : Extension
        {
            return extensions[typeof(TExtension)].ToArray() as TExtension[];
        }

        public bool Contains<TExtension>() where TExtension : Extension
        {
            return Contains(typeof(TExtension));
        }

        public bool Contains(Type serviceType)
        {
            return extensions.ContainsKey(serviceType);
        }

        public object GetBlind(Type extensionType)
        {
            if (!extensions.ContainsKey(extensionType))
            {
                return null;
            }
            
            return extensions[extensionType][0];
        }
        
        #endregion

        #region Extensions Managment

        public void AddExtension(Extension extension)
        {
            if (Attribute.GetCustomAttribute(extension.GetType(), typeof(DisallowMultipleExtensionsAttribute)) != null)
            {
                if (extensions.ContainsKey(extension.GetType()))
                {
#if UNITY_EDITOR
                    Debug.Log("YOU TRIED TO ADD MULTIPLE EXTENSIONS OF TYPE " + extension.GetType());
#endif
                    return;
                }
            }
            
            extensions.TryAdd(extension.GetType(), new List<Extension>());
            extensions[extension.GetType()].Add(extension);

            extension.Initialize(owner);
            
            if (containerStarted)
            {
                Game.DIKernel.InjectWithBaseAnd(extension, owner.CachedComponents, this);
                
                extension.StartExtension();
                extension.Enable();
            }
        }

        public void RemoveExtension<TExtension>()
            where TExtension : Extension
        {
            Type extension = typeof(TExtension);

            if (extensions.ContainsKey(extension))
            {
                extensions[extension][0].Dispose();
                extensions[extension].RemoveAt(0);

                if (extensions[extension].Count == 0)
                    extensions.Remove(extension);
            }
        }

        public void RemoveAllExtensions<TExtension>()
            where TExtension : Extension
        {
            while (extensions.ContainsKey(typeof(Extension)))
            {
                RemoveExtension<TExtension>();
            }
        }
        
        public void RemoveExtensions<TExtension>(int count)
            where TExtension : Extension
        {
            for(int i = 0; i < count; i++)
            {
                RemoveExtension<TExtension>();
            }
        }

        #endregion

        #region Event Methods For Extensions

        public void EnableAll()
        {
            foreach (var extensionPair in extensions)
            {
                extensionPair.Value.ForEach(extension => extension.Enable());
            }
        }
        public void DisableAll()
        {
            foreach (var extensionPair in extensions)
            {
                extensionPair.Value.ForEach(extension => extension.Disable());
            }
        }
        public void UpdateContainer()
        {
            foreach (var extensionPair in extensions)
            {
                extensionPair.Value.ForEach(extension =>
                {
                    if (extension.Enabled) extension.Update();
                });
            }
        }
        public void FixedUpdateContainer()
        {
            foreach (var extensionPair in extensions)
            {
                extensionPair.Value.ForEach(extension =>
                {
                    if (extension.Enabled) extension.FixedUpdate();
                });
            }
        }

        #endregion
    }
}