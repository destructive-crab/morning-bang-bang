using banging_code.common;
using banging_code.runs_system;
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
            slot.SetItem(Game.G<RunSystem>().Data.ItemsPool.GetNew(UTLS.RandomElement(Game.G<RunSystem>().Data.ItemsPool.AllIDs)));
        }
    }
    
}
