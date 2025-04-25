using banging_code.interactions;
using UnityEngine;

namespace banging_code.common.rooms
{
    public class Door : MonoBehaviour, IInteraction, IOnBreakIntoRoom
    {
        public BangRoom InRoom { get; set; }

        public void OnBreak()
        {
            Destroy(GetComponent<Collider2D>());
        }

        public void Interact()
        {
            InRoom.BreakIntoRoom(new BreakArg());
            transform.Find("Closed").gameObject.SetActive(false);
            transform.Find("Opened").gameObject.SetActive(true);
        }

        public void OnInteractorNear() { }
    }
}