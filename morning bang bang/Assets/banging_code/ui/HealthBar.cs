using banging_code.health;
using banging_code.player_logic.rat;
using MothDIed;
using MothDIed.DI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthBar : MonoBehaviour
{
    private Image bar;

    [Inject]
    private void Inject(HitsHandler hitsHandler)
    {
        hitsHandler.OnHit += OnHit;
    }

    private void OnHit(HitableBody arg1, HitData arg2)
    {
        if (arg1 is RatBody)
        {
            bar.fillAmount = (float)Game.RunSystem.Data.PlayerHealth.CurrentHealth /
                             Game.RunSystem.Data.PlayerHealth.MaximumHealth;
        }
    }

    private void Awake()
    {
        bar = GetComponent<Image>();
    }
}
