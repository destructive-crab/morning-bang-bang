namespace banging_code.common.rooms
{
    public interface IConnectedWithRoom<TRoom>
        where TRoom : Room
    {
        TRoom InRoom { get; set; }
    }
}