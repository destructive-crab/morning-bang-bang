using System;
using banging_code;
using banging_code.interactions;
using banging_code.items;
using banging_code.player_logic;
using MothDIed;
using MothDIed.InputsHandling;
using TMPro;
using UnityEngine;

namespace content.trader
{
    [RequireComponent(typeof(Collider2D))]
    public class TraderSlot : Trigger<PlayerRoot>
    {
        private TMP_Text text;
        private GameItem solding;
        private SpriteRenderer itemSprite;
        public bool Selected = false;
        
        private void Awake()
        {
            text = GetComponentInChildren<TMP_Text>();
            itemSprite = GetComponentInChildren<SpriteRenderer>();

            OnEnter += Highlight;
            OnExit += Deselect;

            InputService.OnPlayerInteract += Sold;
        }

        private void OnDestroy()
        {
            InputService.OnPlayerInteract -= Sold;
        }

        private void Deselect(PlayerRoot obj)
        {
            Selected = false;
        }

        private void Highlight(PlayerRoot obj)
        {
            Selected = true;
        }

        public void SetItem(GameItem item)
        {
            itemSprite.sprite = item.InstanceSprite;
            text.text = item.Cost + " $";
            solding = item;
        }
        
        public void Sold()
        {
            if(!Selected) return;
            if (solding == null) return;

            if (Game.RunSystem.Data.Money >= solding.Cost)
            {
                Game.RunSystem.Data.Money -= solding.Cost;
                text.text = "SOLD";
                itemSprite.enabled = false;
                Game.RunSystem.Data.Inventory.Add(solding);
                solding = null;
            }
        }
    }
}