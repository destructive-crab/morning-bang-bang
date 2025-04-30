using banging_code.common;
using content.trader;
using MothDIed;
using UnityEngine;

public class TraderLogic : MonoBehaviour
{
    private TraderSlot[] slots;
    
    private void Start()
    {
        slots = GetComponentsInChildren<TraderSlot>();

        foreach (var slot in slots)
        {
            slot.SetItem(Game.RunSystem.Data.ItemsPool.GetNew(UTLS.RandomElement(Game.RunSystem.Data.ItemsPool.AllIDs)));
        }
    }
    
}
