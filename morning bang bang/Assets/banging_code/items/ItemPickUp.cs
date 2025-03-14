using banging_code.interactions;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class ItemPickUp : MonoBehaviour, IInteraction
    {
        private GameItem item;

        private void Awake()
        {
            GetComponent<SpriteRenderer>().sortingOrder = 2;
        }

        public void FromNew(string itemID)
        {
            item = Game.RunSystem.Data.ItemsPool.GetNew(itemID);
            Reload();
        }

        public void FromExisting(GameItem item)
        {
            this.item = item;
            Reload();
        }

        public void Reload()
        {
            gameObject.name = item.ID + " Instance";
            GetComponent<SpriteRenderer>().sprite = item.InstanceSprite;
        }

        public void Interact()
        {
            Game.RunSystem.Data.Inventory.Add(item);
            Destroy(gameObject);
        }

        public void OnInteractorNear()
        {
            Debug.Log(item.ID);
        }
    }
}