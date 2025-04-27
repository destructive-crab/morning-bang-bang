using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace banging_code.items
{
    public sealed class ItemsPool
    {
        public GameItem[] All => all.Values.ToArray();
        private Dictionary<string, GameItem> all = new();

        private readonly List<string> passives = new();
        private readonly List<string> mains = new();
        public string Name { get; set; }

        public void LoadFrom(GameItem[] items, string name)
        {
            foreach (var item in items)
            {
                all.Add(item.ID, item);
                
                switch (item)
                {
                    case MainItem:     passives.Add(item.ID); break;
                    case PassiveItem:  mains   .Add(item.ID); break;
                }
            }
            Name = name;
        }

        public GameItem GetNew(string id) 
            => ScriptableObject.Instantiate(all[id]);

        public void Remove(string id)
        {
            if(!all.ContainsKey(id)) return;
            
            switch (all[id])
            {
                case MainItem mainItem:       passives.Remove(id); break;
                case PassiveItem passiveItem: mains.Remove(id);    break;
            }

            all.Remove(id);
        }
    }
}