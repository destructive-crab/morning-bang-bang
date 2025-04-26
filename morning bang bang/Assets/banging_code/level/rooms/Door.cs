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

            PlayOpenAnimation();
        }

        private void PlayOpenAnimation()
        {
            var doors = GetComponentsInChildren<Animator>(true);

            foreach (var door in doors)
            {
                door.enabled = true;
            }
        }

        public void OnInteractorNear() { }
    }
}