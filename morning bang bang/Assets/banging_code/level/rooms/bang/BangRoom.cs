using System.Collections.Generic;
using banging_code.ai;
using banging_code.level.light;
using MothDIed.DI;

namespace banging_code.common.rooms
{
    public class BangRoom : Room
    {
        public List<Moody> Moodys;

        [Inject] private LightManager lightManager;

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

            foreach (var entity in entities)
            {
                entity.WakeUp();
            }
            
            lightManager.TurnOn(ID);
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