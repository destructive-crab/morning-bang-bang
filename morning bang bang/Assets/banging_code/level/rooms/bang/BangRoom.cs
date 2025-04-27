using System.Collections.Generic;
using banging_code.ai;
using banging_code.camera_logic;
using banging_code.level.light;
using MothDIed.DI;

namespace banging_code.common.rooms
{
    public class BangRoom : Room
    {
        public List<Moody> Moodys;
        private List<Entity> entities = new();

        [Inject] private LightManager lightManager;
        [Inject] private CCamera camera;

        //callbacks
        private IOnBreakIntoRoom[] onBreakIntoRoomSubs;
        
        public void BreakIntoRoom(BreakArg arg)
        {
            for (var i = 0; i < onBreakIntoRoomSubs.Length; i++)
            {
                onBreakIntoRoomSubs[i].OnBreak();
            }
            
            //???
            var entities = GetComponentsInChildren<Entity>();
            this.entities = new List<Entity>(entities);

            foreach (var entity in entities)
            {
                entity.WakeUp();
                entity.Health.OnDie += OnEntityDie;
            }
            
            lightManager.TurnOn(ID);
            camera.EnterBangCamera(RoomShapeCollider);
        }

        private void OnEntityDie(Entity entity)
        {
            if (entities.Contains(entity))
            {
                entities.Remove(entity);
            }

            if (entities.Count == 0)
            {
                camera.EnterChillCamera();
            }
        }

        public override void ProcessContentInRoom()
        {
            base.ProcessContentInRoom();
            
            foreach (IConnectedWithRoom<BangRoom> obj in ContentRoot.GetComponentsInChildren<IConnectedWithRoom<BangRoom>>())
            {
                obj.InRoom = this;
            }
            
            onBreakIntoRoomSubs = ContentRoot.GetComponentsInChildren<IOnBreakIntoRoom>();
            
            Moodys = new List<Moody>(GetComponentsInChildren<Moody>());
        }
    }
}