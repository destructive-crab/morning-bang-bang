namespace banging_code.level.rooms
{
    public interface IOnBreakIntoRoom : IConnectedWithRoom<BangRoom>
    {
        public void OnBreak();
    }
}