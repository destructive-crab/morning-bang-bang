namespace banging_code.player_logic.standard
{
    public sealed class StandardStateFactory<TState> : PlayerStateFactory
        where TState : PlayerState, new()
    {
        public override PlayerState GetState()
        {
            return new TState();
        }
    }
}