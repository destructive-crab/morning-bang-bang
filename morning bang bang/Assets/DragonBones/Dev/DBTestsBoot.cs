using banging_code;
using MothDIed;

public class DBTestsBoot : GameStartPoint 
{
    protected override async void StartGame()
    {
        await Game.StartGame(Arguments);
        Game.SceneSwitcher.SwitchTo(new CommonScene("DBTEST"));
    }
}
