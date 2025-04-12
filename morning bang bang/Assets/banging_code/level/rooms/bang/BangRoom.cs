using System.Collections.Generic;

namespace banging_code.common.rooms
{
    public class BangRoom : Room
    {
        public List<Moody> Moodys;

        //callbacks
        private IOnBreakIntoRoom[] onBreakIntoRoomSubs;
        
        public void BreakIntoRoom(BreakArg arg)
        {
            for (var i = 0; i < onBreakIntoRoomSubs.Length; i++)
            {
                onBreakIntoRoomSubs[i].OnBreak();
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