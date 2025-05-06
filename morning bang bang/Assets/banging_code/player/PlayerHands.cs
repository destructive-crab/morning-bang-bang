using System;
using System.Collections.Generic;
using banging_code.common;
using banging_code.items;
using MothDIed.MonoSystems;
using UnityEngine;

namespace banging_code.player_logic
{
    public class PlayerHands : MonoSystem
    {
        private Transform currentRoot;
        
        private Transform sideRoot;
        private Transform upRoot;
        private Transform downRoot;
        
        private Dictionary<Type, List<InHandsItemInstance>> collectedFromRoot = new();
        private Type current;

        public override bool EnableOnStart()
        {
            return true;
        }

        public override void ContainerStarted()
        {
            base.ContainerStarted();
            
            InHandsItemInstance[] foundItems = Owner.transform.GetComponentsInChildren<InHandsItemInstance>(true);

            foreach (InHandsItemInstance itemInstance in foundItems)
            {
                collectedFromRoot.TryAdd(itemInstance.GetType(), new List<InHandsItemInstance>());
                collectedFromRoot[itemInstance.GetType()].Add(itemInstance);
            }
        }

        public void SetSideRoot(Transform root)
        {
            sideRoot = root;
        }
        public void SetDownRoot(Transform root)
        {
            downRoot = root;
        }
        
        public void SetUpRoot(Transform root)
        {
            upRoot = root;
        }

        public void RotateTo(GameDirection direction)
        {
            switch (direction)
            {
                case GameDirection.Left:
                    SwitchRoot(sideRoot);
                    break;
                case GameDirection.Right:
                    SwitchRoot(sideRoot);
                    break;
                case GameDirection.Top:
                    SwitchRoot(upRoot);
                    break;
                case GameDirection.Bottom:
                    SwitchRoot(downRoot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        
        public void EnableItem<T>() where T : InHandsItemInstance
        {
            if(current != null) DisableItem(current);
            
            SetActiveTo(typeof(T), true);
            current = typeof(T);
        }
        public void DisableItem<T>() where T : InHandsItemInstance
        {
            DisableItem(typeof(T));
        }
        
        public void DisableItem(Type type) 
        {
            SetActiveTo(type, false);
        }

        private void SetActiveTo(Type type, bool active)
        {
            var items = collectedFromRoot[type];

            foreach (var item in items)
            {
                if(item != null) item.gameObject.SetActive(active);
            }
        }
        
        private void SwitchRoot(Transform to)
        {
            if(currentRoot!=null) currentRoot.gameObject.SetActive(false);
            
            currentRoot = to;
            to.gameObject.SetActive(true);
        }
        
    }
}