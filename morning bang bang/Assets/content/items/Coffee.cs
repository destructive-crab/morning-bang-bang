using banging_code.inventory;
using banging_code.player_logic;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    [CreateAssetMenu(menuName = "Items/Coffee")]
    public class Coffee : PassiveItem
    {
        public override string ID => "Coffee";

        public override void OnPickedUp(Inventory inventory, PlayerRoot playerInstance)
        {
            Game.RunSystem.Data.Speed += 1;
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.RunSystem.Data.Speed -= 1;
        }
    }
}