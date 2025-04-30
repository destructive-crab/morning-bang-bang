using banging_code;
using MothDIed;
using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    public void RestartButton()
    {
        Game.RunSystem.ClearData();
        Game.SwitchTo(new CommonScene("Menu"));
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
