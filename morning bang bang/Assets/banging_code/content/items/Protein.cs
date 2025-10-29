using banging_code.inventory;
using banging_code.player_logic;
using banging_code.runs_system;
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
            Game.G<RunSystem>().Data.DamageMultiplier += 0.1f;
        }

        public override void OnDropped(ItemPickUp droppedItems)
        {
            Game.G<RunSystem>().Data.DamageMultiplier -= 0.1f;
        }
    }
}