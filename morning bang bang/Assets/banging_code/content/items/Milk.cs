using banging_code.inventory;
using banging_code.player_logic;
using banging_code.runs_system;
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
            Game.G<RunSystem>().Data.PlayerHealth.Add(0, 6);
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.G<RunSystem>().Data.PlayerHealth.Remove(0, 6);
        }
    }
}