using banging_code.inventory;
using banging_code.items;
using banging_code.level;
using MothDIed;

namespace banging_code.runs_system
{
    public class RunData
    {
        public LevelScene Level => Game.CurrentScene as LevelScene;
        public ItemsPool ItemsPool { get; private set; } = new();
        public Inventory Inventory { get; private set; } = new();
        public PlayerHealth PlayerHealth { get; private set; } = new();
        public float Speed { get; set; } = 4;
        public float DamageMultiplier { get; set; } = 1;

        public RunData(GameItem[] items)
        {
            ItemsPool.LoadFrom(items, "dev items");
        }
    }

    public class PlayerHealth
    {
        public int CurrentHealth { get; private set; }
        public int MaximumHealth { get; private set; }

        public PlayerHealth()
        {
            CurrentHealth = 10;
            MaximumHealth = 10;
        }

        public void Add(int current, int maximum)
        {
            CurrentHealth += current;
            MaximumHealth += maximum;

            if (CurrentHealth > maximum) CurrentHealth = maximum;
        }

        public bool Remove(int current, int maximum)
        {
            CurrentHealth -= current;
            MaximumHealth -= maximum;

            if (CurrentHealth > MaximumHealth) CurrentHealth = maximum;
            if (CurrentHealth <= 0) return true;

            return false;
        }
    }
}