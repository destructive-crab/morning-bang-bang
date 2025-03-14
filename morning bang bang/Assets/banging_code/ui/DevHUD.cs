using MothDIed;
using TMPro;
using UnityEngine;

public class DevHUD : MonoBehaviour
{
    public TMP_Text pool;
    public TMP_Text health;
    public TMP_Text active;
    public TMP_Text passive;
    
    public void Update()
    {
        var text = Game.RunSystem.Data.ItemsPool.Name;

        foreach (var item in Game.RunSystem.Data.ItemsPool.All)
        {
            text += "\n" + item.ID;
        }

        pool.text = text;

        text = Game.RunSystem.Data.PlayerHealth.CurrentHealth.ToString();

        health.text = text;

if(Game.RunSystem.Data.Inventory.Main != null)        active.text = Game.RunSystem.Data.Inventory.Main.ID;

        text = "pvs current";

        foreach (var item in Game.RunSystem.Data.Inventory.PassiveItems)
        {
            text += "\n"+ item.ID;
        }

        passive.text = text;
    }
}
