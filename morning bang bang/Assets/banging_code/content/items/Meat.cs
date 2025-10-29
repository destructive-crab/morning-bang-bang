using banging_code.inventory;
using banging_code.player_logic;
using banging_code.runs_system;
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
            Game.G<RunSystem>().Data.PlayerHealth.Add(2, 5);
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.G<RunSystem>().Data.PlayerHealth.Remove(0, 5);
        }
    }
}