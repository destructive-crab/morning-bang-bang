using MothDIed;

namespace banging_code.dev
{
    public class ConsoleDevStartPoint : GameStartPoint
    {
        protected override void StartGame()
        {
            Game.SwitchTo(new CommonScene("ConsoleScene"));
        }
    }
}