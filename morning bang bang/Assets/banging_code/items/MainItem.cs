using banging_code.inventory;
using banging_code.player_logic;

namespace banging_code.items
{
    public abstract class MainItem : GameItem
    {
        public abstract void PutOnPlayerInstance(PlayerRoot playerRoot);
        
        public virtual void OnPickedUp(Inventory inventory, PlayerRoot playerInstance) {}
        public virtual void OnDropped(ItemPickUp itemPickUp) {}
    }
}