using banging_code.common.rooms;
using MothDIed;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.level.entity_locating
{
    public sealed class RoomLocationTrigger : Trigger<MonoEntity>
    {
        [Inject] private LocationManager location;
        private Room room;
        
        private void Start()
        {
            room = transform.parent.GetComponent<Room>();
            
            OnEnter += OnEntityEnter;
        }

        private void OnEntityEnter(MonoEntity entity)
        {
            Debug.Log(entity.name);
            location.ChangeLocationOf(entity, room.ID);
        }
    }
}