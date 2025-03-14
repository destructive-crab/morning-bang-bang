using MothDIed;
using MothDIed.InputsHandling;

namespace banging_code
{
    public class BangBoot : GameStartPoint
    {
        protected override void StartGame()
        {
            InputService.Initialize();
            
            Game.InnerLoop();
            Game.SwitchTo(new CommonScene("Menu"));
        }
    }
}