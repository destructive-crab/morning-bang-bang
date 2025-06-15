using System;
using banging_code.inventory;
using banging_code.items;
using banging_code.level;
using MothDIed;

namespace banging_code.runs_system
{
    public class RunData
    {
        public LevelScene Level => Game.SceneSwitcher.CurrentScene as LevelScene;
        public ItemsPool ItemsPool { get; private set; } = new();
        public Inventory Inventory { get; private set; } = new();
        public PlayerHealth PlayerHealth { get; private set; } = new();
        public float Speed { get; set; } = 4;
        public float DamageMultiplier { get; set; } = 1;
        public int Money { get; set; } = 3;

        public RunData(GameItem[] items)
        {
            ItemsPool.LoadFrom(items, "dev items");
        }
    }

    public class PlayerHealth
    {
        public int CurrentHealth { get; private set; } = 10;
        public int MaximumHealth { get; private set; } = 10;

        public event Action<int, int> OnChanged;

        public void Add(int current, int maximum)
        {
            CurrentHealth += current;
            MaximumHealth += maximum;

            if (CurrentHealth > maximum) CurrentHealth = maximum;
            
            OnChanged?.Invoke(CurrentHealth, MaximumHealth);
        }

        public bool Remove(int current, int maximum)
        {
            CurrentHealth -= current;
            MaximumHealth -= maximum;

            if (CurrentHealth > MaximumHealth) CurrentHealth = maximum;
            if (CurrentHealth <= 0) return true;

            OnChanged?.Invoke(CurrentHealth, MaximumHealth);
            return false;
        }
    }
}