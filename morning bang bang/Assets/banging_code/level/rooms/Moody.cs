using MothDIed;

namespace banging_code.common.rooms
{
    public abstract class Moody : DepressedBehaviour, IOnBreakIntoRoom
    {
        public abstract void PlayerBrokenIntoRoom(BreakArg breakArg);
        public BangRoom InRoom { get; set; }
        
        public void OnBreak()
        {
            
        }
    }
}