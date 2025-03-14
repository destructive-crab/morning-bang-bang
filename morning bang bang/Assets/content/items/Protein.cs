using banging_code.inventory;
using banging_code.player_logic;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    [CreateAssetMenu]
    public class Protein : PassiveItem
    {
        public override string ID => "Protein";
        
        public override void OnPickedUp(Inventory inventory, PlayerRoot playerInstance)
        {
            Game.RunSystem.Data.DamageMultiplier += 0.1f;
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.RunSystem.Data.DamageMultiplier -= 0.1f;
        }
    }
}