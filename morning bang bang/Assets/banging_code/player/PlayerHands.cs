using System;
using System.Collections.Generic;
using banging_code.items;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace banging_code.player_logic
{
    public class PlayerHands : Extension
    {
        private Transform root;
        private Dictionary<Type, InHandsItemInstance> collectedFromRoot = new();
        private Type current;

        public PlayerHands(Transform root)
        {
            this.root = root;

            InHandsItemInstance[] foundItems = this.root.GetComponentsInChildren<InHandsItemInstance>(true);

            foreach (InHandsItemInstance itemInstance in foundItems)
            {
                collectedFromRoot.Add(itemInstance.GetType(), itemInstance); 
            }
        }

        public T EnableItem<T>() where T : InHandsItemInstance
        {
            if(current != null) DisableItem(current);
            
            collectedFromRoot[typeof(T)].gameObject.SetActive(true);
            current = typeof(T);
            
            return (T)collectedFromRoot[typeof(T)];
        }
        public void DisableItem<T>() where T : InHandsItemInstance
        {
            collectedFromRoot[typeof(T)].gameObject.SetActive(false);
        }
        
        public void DisableItem(Type type) 
        {
            collectedFromRoot[type].gameObject.SetActive(false);
        }
    }
}