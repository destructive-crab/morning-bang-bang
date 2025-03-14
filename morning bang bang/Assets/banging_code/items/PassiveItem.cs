using banging_code.inventory;
using banging_code.level;
using banging_code.player_logic;

namespace banging_code.items
{
    public abstract class PassiveItem : GameItem
    {
        public abstract void OnPickedUp(Inventory inventory, PlayerRoot playerInstance);
        public abstract void OnDropped(ItemPickUp droppedItems);

        public virtual void ApplyToPlayerInstance(PlayerRoot playerInstance) { }

        public virtual void WhileOnLevel(LevelScene scene) {}
    }
}