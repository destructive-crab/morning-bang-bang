using UnityEngine;

namespace banging_code.common.rooms
{
    public class StartRoom : Room 
    {
        [field: SerializeField] public Transform PlayerSpawnPoint { get; private set; }

        private void Reset()
        {
            PlayerSpawnPoint = new GameObject("[PLAYER START POINT]").transform;
            PlayerSpawnPoint.transform.parent = transform;
        }

        public override void ProcessContentInRoom()
        {
            base.ProcessContentInRoom();
        }
    }
}