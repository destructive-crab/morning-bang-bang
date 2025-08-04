using System.Threading.Tasks;
using MothDIed;

namespace banging_code.dev
{
    public class ConsoleDevStartPoint : GameStartPoint
    {
        protected override void StartGame()
        {
            Game.SceneSwitcher.SwitchTo(new CommonScene("ConsoleScene"));
        }
    }
}