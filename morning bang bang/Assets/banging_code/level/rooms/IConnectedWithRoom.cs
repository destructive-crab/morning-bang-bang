namespace banging_code.level.rooms
{
    public interface IConnectedWithRoom<TRoom>
        where TRoom : Room
    {
        TRoom InRoom { get; set; }
    }
}