using banging_code.interactions;
using UnityEngine;

namespace banging_code.common.rooms
{
    public class Door : MonoBehaviour, IInteraction, IOnBreakIntoRoom
    {
        public BangRoom InRoom { get; set; }

        public void OnBreak()
        {
            GetComponent<SpriteRenderer>().color = Color.magenta;
            Destroy(GetComponent<Collider2D>());
        }

        public void Interact()
        {
            InRoom.BreakIntoRoom(new BreakArg());
        }

        public void OnInteractorNear() { }
    }
}