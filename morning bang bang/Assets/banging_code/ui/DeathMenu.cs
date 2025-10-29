using banging_code;
using banging_code.runs_system;
using MothDIed;
using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    public void RestartButton()
    {
        Game.G<RunSystem>().ClearData();
        Game.G<SceneSwitcher>().SwitchTo(new CommonScene("Menu"));
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
