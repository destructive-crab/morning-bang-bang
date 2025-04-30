using banging_code.inventory;
using banging_code.player_logic;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    [CreateAssetMenu(menuName = "Create Milk", fileName = "Milk" )]
    public class Milk : PassiveItem
    {
        public override string ID => "Milk";
        
        public override void OnPickedUp(Inventory inventory, PlayerRoot playerInstance)
        {
            Game.RunSystem.Data.PlayerHealth.Add(0, 6);
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.RunSystem.Data.PlayerHealth.Remove(0, 6);
        }
    }
}