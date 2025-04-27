using banging_code.interactions;
using banging_code.player_logic.rat;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace banging_code.common.rooms
{
    public class Door : MonoBehaviour, IInteraction, IOnBreakIntoRoom
    {
        public BangRoom InRoom { get; set; }
        private DoorTrigger doorTrigger;

        private void Awake()
        {
            GetComponentInChildren<DoorTrigger>().OnExit += OnExit;
        }

        private void OnExit(RatBody obj)
        {
            GetComponent<Collider2D>().enabled = true;
        }

        public void OnBreak()
        {
            GetComponentInChildren<Light2D>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
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