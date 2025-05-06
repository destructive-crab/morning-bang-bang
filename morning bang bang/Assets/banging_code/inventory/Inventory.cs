using System.Collections.Generic;
using banging_code.items;
using banging_code.level;
using banging_code.player_logic;
using MothDIed;

namespace banging_code.inventory
{
    public class Inventory
    {
        private List<PassiveItem> passiveItems = new List<PassiveItem>();
        private MainItem currentMainItem;

        public MainItem Main => currentMainItem;
        public PassiveItem[] PassiveItems => passiveItems.ToArray();

        public void Add(GameItem item)
        {
            switch (item)
            {
                case MainItem mainItem:       SetMainItem(mainItem);       break;
                case PassiveItem passiveItem: AddPassiveItem(passiveItem); break;
            }
        }
        
        public void AddPassiveItem(PassiveItem item)
        {
            passiveItems.Add(item);
            item.OnPickedUp(this, Game.RunSystem.Data.Level.PlayerInstance);
        }

        public void AddPassiveItem(string name)
        {
            //todo
        }

        public void SetMainItem(MainItem newItem)
        {
            if (newItem == null)
            {
                Game.RunSystem.Data.Level.PlayerInstance.Systems.Get<PlayerHands>().EnableItem<EmptyHands>();
                currentMainItem = null;
                
                return;
            }
            
            
            if(currentMainItem != null)
            {
                //TODO
                currentMainItem.OnDropped(null);
            }
            
            newItem.OnPickedUp(this, Game.RunSystem.Data.Level.PlayerInstance);
            currentMainItem = newItem;
            currentMainItem.PutOnPlayerInstance(Game.RunSystem.Data.Level.PlayerInstance);
        }

        public void WhileOnLevel(LevelScene scene)
        {
            for (var i = 0; i < passiveItems.Count; i++)
            {
                var item = passiveItems[i];

                item.WhileOnLevel(scene);
            }
        }
    }
}