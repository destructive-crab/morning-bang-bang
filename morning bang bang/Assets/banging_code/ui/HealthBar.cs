using MothDIed;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthBar : MonoBehaviour
{
    private Image bar;

    private void Awake()
    {
        bar = GetComponent<Image>();
        Game.RunSystem.Data.PlayerHealth.OnChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int cur, int max)
    {
         bar.fillAmount = (float)cur/max;       
    }
}
