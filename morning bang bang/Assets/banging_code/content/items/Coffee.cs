using banging_code.inventory;
using banging_code.player_logic;
using banging_code.runs_system;
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
            Game.G<RunSystem>().Data.Speed += 1;
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.G<RunSystem>().Data.Speed -= 1;
        }
    }
}