namespace banging_code.common.rooms
{
    public interface IOnBreakIntoRoom : IConnectedWithRoom<BangRoom>
    {
        public void OnBreak();
    }
}