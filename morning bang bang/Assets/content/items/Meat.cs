using banging_code.inventory;
using banging_code.player_logic;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    [CreateAssetMenu(menuName = "Create Meat", fileName = "Meat")]
    public class Meat : PassiveItem
    {
        public override string ID => "meat";
        public override void OnPickedUp(Inventory inventory, PlayerRoot playerInstance)
        {
            Game.RunSystem.Data.PlayerHealth.Add(2, 5);
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.RunSystem.Data.PlayerHealth.Remove(0, 5);
        }
    }
}